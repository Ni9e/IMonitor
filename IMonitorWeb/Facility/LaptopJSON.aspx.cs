using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using IMonitorService.Code;
using System.Data;

public partial class Facility_LaptopJSON : System.Web.UI.Page
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
                case "ALL":
                    {                       
                        DataSet ds = SqlHelper.GetLaptopInformation();
                        int count = ds.Tables[0].Rows.Count;
                        for (int i = 0; i < count; i++)
                        {
                            LaptopInformation laptop = new LaptopInformation();
                            laptop.StoreNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
                            laptop.StoreRegion = ds.Tables[0].Rows[i]["storeRegion"].ToString();
                            laptop.StoreType = ds.Tables[0].Rows[i]["storeType"].ToString();
                            laptop.LaptopNetwork = ds.Tables[0].Rows[i]["laptopNetwork"].ToString();
                            laptop.IP = ds.Tables[0].Rows[i]["ip"].ToString();
                            laptop.PrinterService = ds.Tables[0].Rows[i]["printerService"].ToString();
                            laptop.Date = ds.Tables[0].Rows[i]["date"].ToString();
                            list.Add(laptop);
                        }
                    }
                    break;               
                case "LAPTOP":
                    {
                        IMonitorTask.GetTaskData(TaskCondition.Laptop);
                        Response.Write("笔记本信息获取成功");
                        Response.End();
                        return;
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