using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using S3Upload.AppCode;
namespace S3Upload
{
    public partial class upload_log : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {

            Stream st = FileUpload1.PostedFile.InputStream;

            string name = Path.GetFileName(FileUpload1.FileName);
            string myBucketName = "logger7"; //your s3 bucket name goes here  
            string s3DirectoryName = "";
            string s3FileName = @name;
          
            clsAWSS3 objS3 = new clsAWSS3();
            bool chkUpload = objS3.UploadToS3(st, myBucketName, s3DirectoryName, s3FileName);
            if (chkUpload == true)
            {
                Response.Write("successfully uploaded");

            }
        }
    }
}