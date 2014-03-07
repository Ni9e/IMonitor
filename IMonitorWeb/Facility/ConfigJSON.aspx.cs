using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using IMonitorService.Code;
using System.Web.Script.Serialization;
using System.IO;

public partial class Facility_ConfigJSON : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string url = Request.Url.ToString();
        ArrayList list = new ArrayList();
        string str = string.Empty;
        DirectoryInfo dir = new DirectoryInfo(Server.MapPath(""));
        string filepath = dir.ToString().Replace("Facility", "app_offline.htm");

        if (url.IndexOf("status") != -1)
        {
            string query = Request.QueryString["status"].ToString();
            switch (query.ToUpper())
            {
                case "GET":
                    {
                        Config config = new Config();
                        config.PrintConfig1 = IMonitorConfig.GetSetting("print1");
                        config.PrintConfig2 = IMonitorConfig.GetSetting("print2");
                        config.RouterConfig = IMonitorConfig.GetSetting("router");
                        config.LaptopConfig = IMonitorConfig.GetSetting("laptop");
                        list.Add(config);                                            
                    }
                    break;
                case "SET":
                    {
                        if (!File.Exists(filepath))
                        {
                            using (FileStream fs = File.Create(filepath))
                            {
                                fs.WriteByte(0);
                            }
                        }
                        File.Delete(filepath);                          
                        string print1 = Request.QueryString["print1"].ToString();
                        string print2 = Request.QueryString["print2"].ToString();
                        string router = Request.QueryString["router"].ToString();
                        string laptop = Request.QueryString["laptop"].ToString();
                        string[] name = { "print1", "print2", "router", "laptop" };
                        string[] value = { print1, print2, router, laptop };
                        IMonitorConfig.WriteSetting(name, value);                        
                        Response.Write("更新成功！");
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