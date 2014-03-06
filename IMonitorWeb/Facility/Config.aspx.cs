using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Facility_Config : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        lblprintMorning.Text = "打印机信息获取时间1: " + IMonitorConfig.GetSetting("printMorning");
        lblprintAfternoon.Text = "打印机信息获取时间2: " + IMonitorConfig.GetSetting("printAfternoon");
        lblrouter.Text = "路由器信息获取周期: " + IMonitorConfig.GetSetting("router") + "分钟";
        lbllaptop.Text = "笔记本信息获取周期: " + IMonitorConfig.GetSetting("laptop") + "分钟";
    }
}