using IMonitorService.Code;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Store_PrintReportJSON : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string url = Request.Url.ToString();
        ArrayList list = new ArrayList();
        string str = string.Empty;

        if (url.IndexOf("status") != -1)
        {
            string query = Request.QueryString["status"].ToString();
            if (query.ToUpper() == "QUERY")
            {
                string year = Request.QueryString["year"].ToString();
                string month = Request.QueryString["month"].ToString();
                bool isyear = Request.QueryString["isyear"].ToString() == "0" ? false : true;
                DataSet ds = SqlHelper.TonerSumReport(month, year, isyear);

                int count = ds.Tables[0].Rows.Count;
                for (int i = 0; i < count; i++)
                {
                    TonerReport toner = new TonerReport();
                    toner.StoreNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
                    toner.StoreRegion = ds.Tables[0].Rows[i]["storeRegion"].ToString();
                    toner.StoreType = ds.Tables[0].Rows[i]["storeType"].ToString();
                    toner.TonerCount = ds.Tables[0].Rows[i]["tonerCount"].ToString();
                    toner.StoreStatus = ds.Tables[0].Rows[i]["storeStatus"].ToString();
                    toner.Date = isyear == false ? year + "-" + month : year;                    

                    list.Add(toner);
                }
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