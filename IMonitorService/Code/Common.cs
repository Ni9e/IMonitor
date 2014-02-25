using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Management;

namespace IMonitorService.Code
{
    public class Common
    {
        public static int count = 0; // 打印机抓取完成计数
        public static List<PrinterInformation> PrinterList { get; set; }
        public static List<RouterInformation> RouterList { get; set; }
        public static List<LaptopInformation> LaptopList { get; set; }
        const int defaultTimeout = 10 * 1000; // 打印机抓取超时，10秒
        private static int storeCount = 0; // 店铺数量
        private static int[] laptopComplete; // 笔记本完成Ping的数量
        private static int[] routerComplete; // 路由器完成Ping的数量
        
        public static StoreHost GetStoreHost(string storeNo)
        {
            StoreHost host = new StoreHost();            
            string url = "http://10.1" + storeNo.Substring(0, 2) + "." + Convert.ToInt32(storeNo.Substring(2, 2)).ToString();
            host.Url = url + ".100";
            host.PrinterIP = host.Url.Substring(7);
            host.RouterIP = (url + ".1").Substring(7);
            host.LaptopIP1 = (url + ".40").Substring(7) + ";" + (url + ".41").Substring(7);
            host.LaptopIP2 = (url + ".50").Substring(7) + ";" + (url + ".51").Substring(7);
            host.FingerIP = "";
            host.FlowIP = "";
            
            return host;
        }

        public static int GetArraySum(int[] arr)
        {
            int sum = 0;
            int count = arr.Length;
            for (int i = 0; i < count; i++)
            {
                sum += arr[i];
            }
            return sum;
        }
        
        #region 打印机信息抓取

        private static void TimeoutCallback(object state, bool timeOut)
        {
            if (timeOut)
            {
                HttpWebRequest request = (state as RequestState).Request;
                if (request != null)
                {
                    request.Abort();
                }
            }
        }

        private static void GetPrinterType(RequestState state)
        {
            string html = state.Host.Html;
            if (!String.IsNullOrEmpty(html))
            {
                int ps, pe;
                ps = html.IndexOf("<title>");
                pe = html.IndexOf("</title>");
                html = html.Substring(ps + 7, pe - ps - 7).Replace("&nbsp;", " ").ToUpper();
                if (html != "DEVICE STATUS") // HP1300第二次抓取忽略打印机类型
                {
                    state.Printer.PrinterType = html.Substring(0, html.IndexOf("10.1")).Trim();
                }                

                if (html.IndexOf("HP LASERJET 400") != -1)
                {
                    state.PrinterType = PrinterType.HPM401;                    
                }
                else if (html.IndexOf("HP LASERJET 13") != -1)
                {
                    state.PrinterType = PrinterType.HP1300;
                    state.Host.Url = "http://" + state.Host.PrinterIP + "/hp/device/info_deviceStatus.html";
                    BeginResponse(state);
                }
                else if (html.IndexOf("HP LASERJET PROFESSIONAL P1606DN") != -1)
                {
                    state.PrinterType = PrinterType.HP1606;
                }
                else if (html.IndexOf("HP LASERJET P20") != -1)
                {
                    state.PrinterType = PrinterType.HP2000;
                }
            }
            else
            {
                state.PrinterType = PrinterType.NONE;
            }   
        }

        private static void GetPrinterStatus(RequestState state)
        {
            string html = state.Host.Html;
            Pattern pattern;
            switch (state.PrinterType)
            {
                case PrinterType.HP1606:
                    {
                        pattern = new Pattern("itemLargeFont", "mainContentArea", string.Empty, string.Empty, ">", "</td>", @"2"">", "</td>", string.Empty, string.Empty, 0, "Cartridge");
                        GetPrinterStatus(state, pattern);
                    }
                    break;
                case PrinterType.HP1300:
                    {
                        pattern = new Pattern(@"<font class=""if"">", @"<font class=""if"">", string.Empty, string.Empty, ">", "</font>", ">", "</font>", string.Empty, string.Empty, 50, "Cartridge");
                        GetPrinterStatus(state, pattern);
                    }
                    break;
                case PrinterType.HPM401:
                    {
                        pattern = new Pattern("itemLargeFont", "mainContentArea", @"<td class=""alignRight valignTop"">", string.Empty, ">", "<br>", "<td>", "<br>", ">", "</td>", 0, "Cartridge");
                        GetPrinterStatus(state, pattern);
                    }
                    break;
                case PrinterType.HP2000:
                    {
                        pattern = new Pattern("itemLargeFont", @"<td class=""tableDataCellStand width30"">", "<td>", @"<td class=""tableDataCellStand width25"" style=""vertical-align: bottom"">", ">", "</td>", ">", "</td>", ">", "</td>", 0, "Cartridge");
                        GetPrinterStatus(state, pattern);
                    }
                    break;
                default:
                    {
                        state.Printer.PrinterStatus = "不支持该打印机信息抓取";
                        state.Printer.TonerStatus = "不支持该打印机信息抓取";
                        state.Printer.PrinterNetwork = "Down";
                    }
                    break;
            }
            state.Printer.Date = DateTime.Now.ToString();
        }

        private static void GetPrinterStatus(RequestState state, Pattern pat)
        {
            string printerStatus, tonerStatus, percent = string.Empty;
            int ps, pe, ts, te;
            string html = state.Host.Html;
            PrinterInformation printer = state.Printer;
            try
            {
                printerStatus = html.Substring(html.IndexOf(pat.SearchString1)).Replace("&nbsp;", " ");
                tonerStatus = printerStatus.Substring(printerStatus.IndexOf(pat.SearchString2, pat.SearchStartIndex));
                if (!String.IsNullOrEmpty(pat.SearchString3))
                {
                    int idx = tonerStatus.IndexOf(pat.SearchString3) == -1 ? tonerStatus.IndexOf(pat.SearchString3N) : tonerStatus.IndexOf(pat.SearchString3);
                    percent = tonerStatus.Substring(idx);
                }

                ps = printerStatus.IndexOf(pat.Anchor1);
                pe = printerStatus.IndexOf(pat.Anchor2);
                printer.PrinterStatus = printerStatus.Substring(ps + pat.Anchor1.Length, pe - ps - pat.Anchor1.Length).Trim().Replace("<br>", "");

                ts = tonerStatus.IndexOf(pat.Anchor3);
                te = tonerStatus.IndexOf(pat.Anchor4);
                printer.TonerStatus = tonerStatus.Substring(ts + pat.Anchor3.Length, te - ts - pat.Anchor3.Length).Trim().Replace(pat.ReplaceString, pat.ReplaceString + " ");

                if (!string.IsNullOrEmpty(pat.SearchString3) && !string.IsNullOrEmpty(pat.Anchor5) && !string.IsNullOrEmpty(pat.Anchor6))
                {
                    int idxs, idxe;
                    idxs = percent.IndexOf(pat.Anchor5);
                    idxe = percent.IndexOf(pat.Anchor6);
                    percent = " " + percent.Substring(idxs + pat.Anchor5.Length, idxe - idxs - pat.Anchor5.Length).Trim();
                    printer.TonerStatus += percent;
                }
                printer.PrinterNetwork = "Up";
            }
            catch (System.Exception ex)
            {
                printer.PrinterStatus = ex.Message.ToString();
                printer.TonerStatus = ex.Message.ToString();
                printer.PrinterNetwork = "Down";
            }
        }

        private static void BeginResponse(RequestState state)
        {
            HttpWebRequest request = WebRequest.Create(state.Host.Url) as HttpWebRequest;
            state.Request = request;
            IAsyncResult result = request.BeginGetResponse(new AsyncCallback(OnResponse), state);
            ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), state, defaultTimeout, true);
        }

        private static void OnResponse(IAsyncResult ar)
        {
            RequestState state = ar.AsyncState as RequestState;
            try
            {
                HttpWebRequest request = state.Request;
                HttpWebResponse response = request.EndGetResponse(ar) as HttpWebResponse;
                using (StreamReader read = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    string html = read.ReadToEnd();
                    state.Host.Html = html;
                    GetPrinterType(state);
                    GetPrinterStatus(state);

                    Regex replaceSpace = new Regex(@"[\s]+", RegexOptions.IgnoreCase); // 去掉连续空格
                    string pstatus, tstatus;
                    pstatus = state.Printer.PrinterStatus.Replace("<br>", "").Replace("\n", "").Replace("</pre>", "").Trim();
                    tstatus = state.Printer.TonerStatus.Replace("<br>", "").Replace("\n", "").Replace("*", "").Trim();
                    pstatus = replaceSpace.Replace(pstatus, " ");
                    tstatus = replaceSpace.Replace(tstatus, " ");

                    state.Printer.PrinterStatus = pstatus;
                    state.Printer.TonerStatus = tstatus;
                    if (pstatus.IndexOf("StartIndex") == -1)
                    {
                        count++;
                        PrinterList.Add(state.Printer);
                        Console.WriteLine(count.ToString() + "/" + storeCount.ToString() + " " + state.Printer.StoreNo + ": " + state.Printer.PrinterStatus + " " + state.Printer.TonerStatus);
                    }
                }
                response.Close();
            }
            catch (System.Exception ex)
            {
                count++;
                state.Printer.PrinterStatus = ex.Message;
                state.Printer.TonerStatus = ex.Message;
                state.Printer.PrinterNetwork = "Down";
                state.Printer.Date = DateTime.Now.ToString();
                PrinterList.Add(state.Printer);
                Console.WriteLine(count.ToString() + "/" + storeCount.ToString() + " " + state.Printer.StoreNo + ": " + ex.Message.ToString());
            }
        }

        public static void DoGetPrinterInfomationTask()
        {
            PrinterList = new List<PrinterInformation>();
            DataSet ds = SqlHelper.GetNonHKStoreInformation();
            storeCount = ds.Tables[0].Rows.Count;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < storeCount; i++)
            {
                RequestState state = new RequestState();
                PrinterInformation printer = new PrinterInformation();
                printer.StoreNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
                printer.StoreRegion = ds.Tables[0].Rows[i]["storeRegion"].ToString();
                printer.StoreType = ds.Tables[0].Rows[i]["storeType"].ToString();
                printer.PrinterType = ds.Tables[0].Rows[i]["printerType"].ToString();
                printer.TonerType = ds.Tables[0].Rows[i]["tonerType"].ToString();

                state.Host = Common.GetStoreHost(printer.StoreNo);
                state.Host.PrinterIP = ds.Tables[0].Rows[i]["printerIP"].ToString(); // IP设置为门店维护的IP                

                state.Printer = printer;
                try
                {
                    if (new Ping().Send(state.Host.PrinterIP).Status == IPStatus.Success)
                    {
                        BeginResponse(state);
                    }
                    else
                    {
                        count++;
                        state.Printer.PrinterStatus = "打印机无法连接";
                        state.Printer.TonerStatus = "打印机无法连接";
                        state.Printer.PrinterNetwork = "Down";
                        state.Printer.Date = DateTime.Now.ToString();
                        PrinterList.Add(state.Printer);
                        Console.WriteLine(count.ToString() + "/" + storeCount.ToString() + " " + state.Printer.StoreNo + ": 打印机无法连接");
                    }
                }
                catch (System.Exception ex)
                {
                    count++;
                    state.Printer.PrinterStatus = ex.Message;
                    state.Printer.TonerStatus = ex.Message;
                    state.Printer.PrinterNetwork = "Down";
                    state.Printer.Date = DateTime.Now.ToString();
                    PrinterList.Add(state.Printer);
                    Console.WriteLine(count.ToString() + "/" + storeCount.ToString() + " " + state.Printer.StoreNo + ": " + ex.Message.ToString());
                    continue;
                }
            }
            while (true)
            {
                if (count == storeCount)
                {
                    string[] clist = { "storeNo", "storeRegion", "storeType", "printerNetwork", "printerStatus", "tonerStatus", "printerType", "tonerType", "date" };
                    DataTable dt = new DataTable();
                    foreach (string colname in clist)
                    {
                        dt.Columns.Add(colname);
                    }
                    int rowsCount = PrinterList.Count;
                    for (int i = 0; i < rowsCount; i++)
                    {
                        DataRow row = dt.NewRow();
                        row["storeNo"] = PrinterList[i].StoreNo;
                        row["storeRegion"] = PrinterList[i].StoreRegion;
                        row["storeType"] = PrinterList[i].StoreType;
                        row["printerNetwork"] = PrinterList[i].PrinterNetwork;
                        row["printerStatus"] = PrinterList[i].PrinterStatus;
                        row["tonerStatus"] = PrinterList[i].TonerStatus;
                        row["printerType"] = PrinterList[i].PrinterType;
                        row["tonerType"] = PrinterList[i].TonerType;
                        row["date"] = PrinterList[i].Date;
                        dt.Rows.Add(row);
                    }
                    SqlHelper.DelCurDatePrinterInformation();
                    SqlHelper.CommonBulkInsert(dt, "PrinterInformation");
                    sw.Stop();
                    double ms = sw.ElapsedMilliseconds / 1000.0;
                    Console.WriteLine("耗时: " + ms.ToString() + " 秒");
                    break;
                }
            }
        }
        
        #endregion

        #region 路由器信息抓取

        public static void DoGetRouterInformationTask()
        {
            RouterList = new List<RouterInformation>();
            DataSet ds = SqlHelper.GetStoreInformation();
            int count = ds.Tables[0].Rows.Count;
            routerComplete = new int[count];
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < count; i++)
            {
                routerComplete[i] = 0;
            }
            for (int i = 0; i < count; i++)
            {
                RouterInformation router = new RouterInformation();
                router.StoreNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
                router.StoreRegion = ds.Tables[0].Rows[i]["storeRegion"].ToString();
                router.StoreType = ds.Tables[0].Rows[i]["storeType"].ToString();
                router.IP = ds.Tables[0].Rows[i]["routerIP"].ToString();
                router.I = i;
                router.Total = count;
                RouterAssist(router);                
            }
            while (true)
            {
                if(count == GetArraySum(routerComplete))
                {
                    string[] clist = { "storeNo", "storeRegion", "storeType", "routerNetwork", "routerType", "date" };
                    DataTable dt = new DataTable();
                    foreach (string colName in clist)
                    {
                        dt.Columns.Add(colName);
                    }
                    int rowCount = RouterList.Count;
                    for (int i = 0; i < rowCount; i++)
                    {
                        DataRow row = dt.NewRow();
                        row["storeNo"] = RouterList[i].StoreNo;
                        row["storeRegion"] = RouterList[i].StoreRegion;
                        row["storeType"] = RouterList[i].StoreType;
                        row["routerNetwork"] = RouterList[i].RouterNetwork;
                        row["routerType"] = RouterList[i].RouterType;
                        row["date"] = RouterList[i].Date;
                        dt.Rows.Add(row);
                    }
                    SqlHelper.CommonBulkInsert(dt, "RouterInformationTemp"); // 插入到Temp表中用做对比
                    sw.Stop();
                    double ms = sw.ElapsedMilliseconds / 1000.0;
                    Console.WriteLine("耗时: " + ms.ToString());
                    break;
                }
            }            
        }

        private static void RouterAssist(RouterInformation router)
        {
            Ping p = new Ping();
            p.PingCompleted += RouterCallback;
            p.SendAsync(router.IP, 5 * 1000, router);
        }

        private static void RouterCallback(object sender, PingCompletedEventArgs e)
        {
            RouterInformation router = (RouterInformation)e.UserState;
            router.RouterNetwork = (e.Reply.Status == IPStatus.Success) ? "Up" : "Down";
            router.Date = DateTime.Now.ToString();
            RouterList.Add(router);
            routerComplete[router.I] = 1; // 相应位置1表示已经Ping完            
            Console.WriteLine((router.I + 1).ToString() + "/" + router.Total.ToString() + " " + router.StoreNo + ": " + router.RouterNetwork);
            
        }

        #endregion

        #region 笔记本信息抓取

        public static void DoGetLaptopInformationTask()
        {
            LaptopList = new List<LaptopInformation>();
            DataSet ds = SqlHelper.GetStoreInformation();
            int count = ds.Tables[0].Rows.Count;
            laptopComplete = new int[count];
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < count; i++)
            {
                laptopComplete[i] = 0;
            }
            for (int i = 0; i < count; i++)
            {
                LaptopInformation laptop = new LaptopInformation();
                laptop.StoreNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
                laptop.StoreRegion = ds.Tables[0].Rows[i]["storeRegion"].ToString();
                laptop.StoreType = ds.Tables[0].Rows[i]["storeType"].ToString();
                laptop.I = i;
                laptop.Total = count;
                laptop.Count = 0;

                string[] ip1 = ds.Tables[0].Rows[i]["laptopIP1"].ToString().Split(';');
                for (int j = 0; j < ip1.Length; j++ )
                {
                    laptop.IPs.Add(ip1[j]);
                }

                string[] ip2 = ds.Tables[0].Rows[i]["laptopIP2"].ToString().Split(';');
                for (int k = 0; k < ip2.Length; k++ )
                {
                    laptop.IPs.Add(ip2[k]);
                }

                laptop.LaptopNetwork = string.Empty;
                PingAssist(laptop, laptop.Count); 
            }
            while (true)
            {
                if( count == GetArraySum(laptopComplete))
                {
                    string[] clist = { "storeNo", "storeRegion", "storeType", "laptopNetwork", "printerService", "date" };
                    DataTable dt = new DataTable();
                    foreach (string colName in clist)
                    {
                        dt.Columns.Add(colName);
                    }
                    int rowCount = LaptopList.Count;
                    for (int i = 0; i < rowCount; i++)
                    {
                        DataRow row = dt.NewRow();
                        row["storeNo"] = LaptopList[i].StoreNo;
                        row["storeRegion"] = LaptopList[i].StoreRegion;
                        row["storeType"] = LaptopList[i].StoreType;
                        row["laptopNetwork"] = LaptopList[i].LaptopNetwork;
                        row["printerService"] = LaptopList[i].PrinterService;
                        row["date"] = LaptopList[i].Date;
                        dt.Rows.Add(row);
                    }
                    SqlHelper.DeleteLaptopInformation();
                    SqlHelper.CommonBulkInsert(dt, "LaptopInformation");
                    sw.Stop();
                    double ms = sw.ElapsedMilliseconds / 1000.0;
                    Console.WriteLine("耗时: " + ms.ToString());
                    break;
                }
            }            
        }

        private static void PingAssist(LaptopInformation laptop, int i)
        {
            Ping p = new Ping();            
            p.PingCompleted += PingCallback;
            p.SendAsync(laptop.IPs[i], 5 * 1000, laptop);          
        }

        private static void PingCallback(object sender, PingCompletedEventArgs e)
        {            
            LaptopInformation laptop = (LaptopInformation)e.UserState;
            laptop.LaptopNetwork = (e.Reply.Status == IPStatus.Success) ? "Up" : "Down";
            laptop.Count++;
            if(laptop.LaptopNetwork == "Down" && laptop.Count != laptop.IPs.Count)
            {
                PingAssist(laptop, laptop.Count);
            }
            else
            {
                // 获取打印机服务 待添加
                laptop.Date = DateTime.Now.ToString();                
                LaptopList.Add(laptop);
                laptopComplete[laptop.I] = 1; // 置1表示该打印机已经Ping完
                Console.WriteLine((laptop.I + 1).ToString() + "/" + laptop.Total.ToString() + " " + laptop.StoreNo + ": " + laptop.LaptopNetwork);                 
            }                      
        }

        public static void GetPrinterService()
        {
            // WMI获取服务状态
            ConnectionOptions con = new ConnectionOptions();
            con.Username = "";
            con.Password = "123456";
            ManagementScope ms = new ManagementScope(@"\\.\root\cimv2", null);
            ObjectQuery oq = new ObjectQuery("SELECT * FROM Win32_Service");
            ManagementObjectSearcher query1 = new ManagementObjectSearcher(ms, oq);
            ManagementObjectCollection queryCollection1 = query1.Get();
            foreach (ManagementObject mo in queryCollection1)
            {
                Console.WriteLine("{0} started is {1}, mode is {2}", mo["Name"].ToString(), mo["Started"].ToString(), mo["StartMode"].ToString());
            }

            ////连接远程计算机  
            //ConnectionOptions co = new ConnectionOptions();
            //co.Username = "john";
            //co.Password = "john";
            //ManagementScope ms = new ManagementScope("\\\\192.168.1.2\\root\\cimv2", co);
            ////查询远程计算机  
            //ObjectQuery oq = new System.Management.ObjectQuery("SELECT * FROM Win32_Service");

            //ManagementObjectSearcher query1 = new ManagementObjectSearcher(ms, oq);
            //ManagementObjectCollection queryCollection1 = query1.Get();
            //foreach (ManagementObject mo in queryCollection1)
            //{
            //    string[] ss = { "" };
            //    mo.InvokeMethod("Reboot", ss);
            //    Console.WriteLine(mo.ToString());
            //}  
        }
        #endregion
    }
}
