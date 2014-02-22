using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using IMonitorService.Code;
using System.Collections;
using System.Web.Script.Serialization;
using System.Data;

public partial class Facility_RouterJSON : System.Web.UI.Page
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
                case "UP":
                    {
                        list = GetData(RouterCondition.Up);
                    }
                    break;
                case "DOWN":
                    {
                        list = GetData(RouterCondition.Down);
                    }
                    break;
                case "ALL":
                    {
                        list = GetData(RouterCondition.All);
                    }
                    break;
                case "AMOUNT":
                    {      
                        int total = SqlHelper.GetRouterCount(RouterCondition.All);
                        int up = SqlHelper.GetRouterCount(RouterCondition.Up);
                        int down = SqlHelper.GetRouterCount(RouterCondition.Down);

                        string[] names = { "正常", "异常" };
                        int[] values = { up, down };
                        string[] colors = { "#2ECC71", "#E74C3C" };
                        for (int i = 0; i < 2; i++)
                        {
                            Pie pie = new Pie();
                            pie.Name = names[i];
                            pie.Value = values[i];
                            pie.Percent = Math.Round(100 * (Convert.ToDouble(values[i]) / Convert.ToDouble(total)), 2);
                            pie.Color = colors[i];
                            list.Add(pie);
                        }    
                    }
                    break;
                case "ROUTER":
                    {
                        IMonitorTask.GetTaskData(TaskCondition.Router);
                        Response.Write("路由器信息获取成功");
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

    private static ArrayList GetData(RouterCondition rc)
    {
        ArrayList list = new ArrayList();
        DataSet ds = SqlHelper.GetRouterInformation(rc);
        int count = ds.Tables[0].Rows.Count;
        for (int i = 0; i < count; i++)
        {
            RouterInformation router = new RouterInformation();
            router.StoreNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
            router.StoreRegion = ds.Tables[0].Rows[i]["storeRegion"].ToString();
            router.StoreType = ds.Tables[0].Rows[i]["storeType"].ToString();  
            router.RouterNetwork = ds.Tables[0].Rows[i]["routerNetwork"].ToString();
            router.RouterType = ds.Tables[0].Rows[i]["routerType"].ToString();
            router.Date = ds.Tables[0].Rows[i]["date"].ToString();
            list.Add(router);
        }
        return list;
    }
}