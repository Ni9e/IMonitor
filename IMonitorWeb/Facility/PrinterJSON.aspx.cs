using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using IMonitorService.Code;

public partial class Facility_PrinterJSON : System.Web.UI.Page
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
                        list = GetData(PrinterCondition.Up);
                    }
                    break;
                case "DOWN":
                    {
                        list = GetData(PrinterCondition.Down);
                    }
                    break;
                case "ALL":
                    {
                        list = GetData(PrinterCondition.All);
                    }
                    break;
                case "AMOUNT":
                    {
                        int total = SqlHelper.GetPrinterCount(PrinterCondition.All);
                        int low = SqlHelper.GetPrinterCount(PrinterCondition.Up);
                        int exception = SqlHelper.GetPrinterCount(PrinterCondition.Down);
                        int ok = total - exception - low;
                        string[] names = { "正常", "缺墨", "异常" };
                        int[] values = { ok, low, exception };
                        string[] colors = { "#2ECC71", "#e9e400", "#E74C3C" };  // #E67E22          
                        for (int i = 0; i < 3; i++)
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
                case "PRINT":
                    {                        
                        IMonitorTask.GetTaskData(TaskCondition.Print);
                        Response.Write("打印机信息获取成功");
                        Response.End();
                        return; 
                    }
                case "SENDEMAIL":
                    {
                        IMonitorTask.GetTaskData(TaskCondition.SendEmail);

                        DataSet ds = SqlHelper.GetEmailSendResult();
                        string msg = string.Empty;
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            string storeNo = ds.Tables[0].Rows[i][0].ToString();
                            string isSend = ds.Tables[0].Rows[i][1].ToString();
                            if (isSend == "False")
                            {
                                msg += storeNo + " 门店邮件发送失败！ ";
                            }
                        }

                        if (string.IsNullOrEmpty(msg))
                        {
                            msg = " 所有缺墨门店已经发送邮件通知！";
                        }
                        else
                        {
                            msg += " 其他缺墨门店已经发送邮件通知！";
                        }
                        Response.Write(msg);
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

    private static ArrayList GetData(PrinterCondition pc)
    {
        ArrayList list = new ArrayList();        
        DataSet ds = SqlHelper.GetPrinterInformation(pc);        
        int count = ds.Tables[0].Rows.Count;
        for (int i = 0; i < count; i++)
        {
            PrinterInformation printer = new PrinterInformation();
            printer.StoreNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
            printer.StoreRegion = ds.Tables[0].Rows[i]["storeRegion"].ToString();
            printer.StoreType = ds.Tables[0].Rows[i]["storeType"].ToString();
            printer.PrinterStatus = ds.Tables[0].Rows[i]["printerStatus"].ToString();
            printer.TonerStatus = ds.Tables[0].Rows[i]["tonerStatus"].ToString();
            printer.PrinterType = ds.Tables[0].Rows[i]["printerType"].ToString();
            printer.TonerType = ds.Tables[0].Rows[i]["tonerType"].ToString();
            printer.Date = ds.Tables[0].Rows[i]["date"].ToString();
            printer.PrinterNetwork = ds.Tables[0].Rows[i]["printerNetwork"].ToString();
            list.Add(printer);
        }
        return list;
    }
}