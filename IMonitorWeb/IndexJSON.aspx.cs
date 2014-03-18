using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using IMonitorService.Code;
using System.Data;

public partial class IndexJSON : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string url = Request.Url.ToString();
        ArrayList list = new ArrayList();
        string str = string.Empty;

        if (url.IndexOf("status") != -1)
        {
            string query = Request.QueryString["status"].ToString();
            switch (query.ToUpper())
            {                               
                case "QUERY":
                    {
                        if (url.IndexOf("storeNo") != -1)
                        {
                            string storeNo = Request.QueryString["storeNo"].ToString();

                            string path = AppDomain.CurrentDomain.BaseDirectory;
                            int idx = path.IndexOf("IMonitorWeb");
                            path = path.Substring(0, idx) + @"IMonitorAssist\bin\Debug\IMonitorAssist.exe";

                            Process p = new Process();
                            p.StartInfo.CreateNoWindow = true;
                            p.StartInfo.Arguments = "INDEXQUERY " + storeNo;
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.FileName = path;
                            p.Start();
                            p.WaitForExit();

                            DataSet ds = SqlHelper.GetIndexQuery();
                            IndexQuery iq = new IndexQuery();
                            iq.RouterNetwork = ds.Tables[0].Rows[0]["routerNetwork"].ToString();
                            iq.PrinterNetwork = ds.Tables[0].Rows[0]["printerNetwork"].ToString();
                            iq.PrinterStatus = ds.Tables[0].Rows[0]["printerStatus"].ToString();
                            iq.TonerStatus = ds.Tables[0].Rows[0]["tonerStatus"].ToString();
                            iq.LaptopNetwork = ds.Tables[0].Rows[0]["laptopNetwork"].ToString();
                            iq.PrinterType = ds.Tables[0].Rows[0]["printerType"].ToString();
                            iq.TonerType = ds.Tables[0].Rows[0]["tonerType"].ToString();
                            list.Add(iq);
                        }                        
                    }
                    break;
            }
        }
        else
        {
            Response.Write("This is Iwooo Monitor System");
            Response.End();
            return;
        }

        JavaScriptSerializer json = new JavaScriptSerializer();
        str = json.Serialize(list);

        Response.Write(str);
        Response.End();
        return;
    }
}