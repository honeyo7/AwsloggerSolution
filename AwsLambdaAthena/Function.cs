using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Athena;
using Amazon.Athena.Model;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AwsLambdaAthena
{
    public class Function
    {

      
        private const String ATHENA_TEMP_PATH = "s3://athenaoutputlogger7/";
        private const String ATHENA_DB = "db_logger";
        public async Task<string> FunctionHandler(ILambdaContext context)
        {
            string strResult = "";
            using (var client = new AmazonAthenaClient(Amazon.RegionEndpoint.APSouth1))
            {
                QueryExecutionContext qContext = new QueryExecutionContext();
                qContext.Database = ATHENA_DB;
                ResultConfiguration resConf = new ResultConfiguration();
                resConf.OutputLocation = ATHENA_TEMP_PATH;
                strResult = await run(client, qContext, resConf);
               
            }

            return strResult;
        }

        async static Task<string> run(IAmazonAthena client, QueryExecutionContext qContext, ResultConfiguration resConf)
        {
            /* Execute a simple query on a table */
            StartQueryExecutionRequest qReq = new StartQueryExecutionRequest()
            {

                QueryString = "SELECT * FROM tbl_logger7;",
                QueryExecutionContext = qContext,
                ResultConfiguration = resConf
            };

            try
            {
                /* Executes the query in an async manner */
                StartQueryExecutionResponse qRes = await client.StartQueryExecutionAsync(qReq);
                /* Call internal method to parse the results and return a list of key/value dictionaries */
                List<Dictionary<String, String>> items = await getQueryExecution(client, qRes.QueryExecutionId);

                string json = JsonConvert.SerializeObject(items);

                return json;
            }
            catch (InvalidRequestException e)
            {
                return "{\"Error\":\"" + e.Message + "\"}";
            }
        }
        async static Task<List<Dictionary<String, String>>> getQueryExecution(IAmazonAthena client, String id)
        {
            List<Dictionary<String, String>> items = new List<Dictionary<String, String>>();
            GetQueryExecutionResponse results = null;
            QueryExecution q = null;
            /* Declare query execution request object */
            GetQueryExecutionRequest qReq = new GetQueryExecutionRequest()
            {
                QueryExecutionId = id
            };
            /* Poll API to determine when the query completed */
            do
            {
                results = await client.GetQueryExecutionAsync(qReq);
                q = results.QueryExecution;

                await Task.Delay(5000); //Wait for 5sec before polling again

            } while (q.Status.State == "RUNNING" || q.Status.State == "QUEUED");

            Console.WriteLine("Data Scanned for {0}: {1} Bytes", id, q.Statistics.DataScannedInBytes);

            /* Declare query results request object */
            GetQueryResultsRequest resReq = new GetQueryResultsRequest()
            {
                QueryExecutionId = id,
                MaxResults = 10
            };

            GetQueryResultsResponse resResp = null;
            /* Page through results and request additional pages if available */
            do
            {
                resResp = await client.GetQueryResultsAsync(resReq);
                /* Loop over result set and create a dictionary with column name for key and data for value */
                foreach (Row row in resResp.ResultSet.Rows)
                {
                    Dictionary<String, String> dict = new Dictionary<String, String>();
                    for (var i = 0; i < resResp.ResultSet.ResultSetMetadata.ColumnInfo.Count; i++)
                    {
                        dict.Add(resResp.ResultSet.ResultSetMetadata.ColumnInfo[i].Name, row.Data[i].VarCharValue);
                    }
                    items.Add(dict);
                }

                if (resResp.NextToken != null)
                {
                    resReq.NextToken = resResp.NextToken;
                }
            } while (resResp.NextToken != null);

            /* Return List of dictionary per row containing column name and value */
            return items;
        }
    }
}
