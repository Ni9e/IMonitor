using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Data;
using IMonitorService.Code;
using System.Web.Script.Serialization;


public partial class Store_StoreJSON : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string url = Request.Url.ToString();
        ArrayList list = new ArrayList();
        string str = string.Empty;

        if (Request.HttpMethod == "GET")
        {
            if (url.IndexOf("status") != -1)
            {
                string query = Request.QueryString["status"].ToString();
                if (query.ToUpper() == "ALL")
                {                    
                    DataSet ds = SqlHelper.GetStoreInformation();
                    int count = ds.Tables[0].Rows.Count;
                    for (int i = 0; i < count; i++)
                    {
                        StoreInformation store = new StoreInformation();
                        store.StoreNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
                        store.StoreRegion = ds.Tables[0].Rows[i]["storeRegion"].ToString();
                        store.StoreType = ds.Tables[0].Rows[i]["storeType"].ToString();
                        store.PrinterIP = ds.Tables[0].Rows[i]["printerIP"].ToString();
                        store.RouterIP = ds.Tables[0].Rows[i]["routerIP"].ToString();
                        store.LaptopIP1 = ds.Tables[0].Rows[i]["laptopIP1"].ToString();
                        store.LaptopIP2 = ds.Tables[0].Rows[i]["laptopIP2"].ToString();
                        store.FingerIP = ds.Tables[0].Rows[i]["fingerIP"].ToString();
                        store.FlowIP = ds.Tables[0].Rows[i]["flowIP"].ToString();
                        store.EmailAddress = ds.Tables[0].Rows[i]["emailAddress"].ToString();
                        store.PrinterType = ds.Tables[0].Rows[i]["printerType"].ToString();
                        store.TonerType = ds.Tables[0].Rows[i]["tonerType"].ToString();
                        store.RouterType = ds.Tables[0].Rows[i]["routerType"].ToString();

                        list.Add(store);
                    }
                }
                else if (query.ToUpper() == "SYNC")
                {
                    SqlHelper.SyncStoreInformation();
                    Response.Write("同步成功！");
                    Response.End();
                    return;
                }
            }
            else
            {
                Response.Write("This is Iwooo Monitor System");
                Response.End();
                return;
            }
        }        
        else if(Request.HttpMethod == "POST")
        {
            StoreInformation store = new StoreInformation();
            string oper = Request.Form["oper"].ToString();

            if (oper == "edit")
            {
                store.StoreNo = Request.Form["StoreNo"].ToString();                
                store.PrinterIP = Request.Form["PrinterIP"].ToString();
                store.RouterIP = Request.Form["RouterIP"].ToString();
                store.LaptopIP1 = Request.Form["LaptopIP1"].ToString();
                store.LaptopIP2 = Request.Form["LaptopIP2"].ToString();
                store.FingerIP = Request.Form["FingerIP"].ToString();
                store.FlowIP = Request.Form["FlowIP"].ToString();
                store.EmailAddress = Request.Form["EmailAddress"].ToString();
                store.PrinterType = Request.Form["PrinterType"].ToString();
                store.TonerType = Request.Form["TonerType"].ToString();
                store.RouterType = Request.Form["RouterType"].ToString();
                SqlHelper.UpdateStoreInformation(store);
            }
            else if (oper == "del")
            {
                string storeNo = Request.Form["name"].ToString();
                SqlHelper.DeleteStoreInformation(storeNo);
            }
        }        

        JavaScriptSerializer json = new JavaScriptSerializer();
        str = json.Serialize(list);

        Response.Write(str);
        Response.End();
        return;
    }
}