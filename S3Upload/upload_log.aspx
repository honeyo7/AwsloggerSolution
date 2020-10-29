<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="upload_log.aspx.cs" Inherits="S3Upload.upload_log" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>

            <asp:FileUpload ID="FileUpload1" runat="server" />

            <asp:Button ID="btnUpload" runat="server" Text="Upload" onclick="btnUpload_Click"/>
        </div>
    </form>
</body>
</html>
