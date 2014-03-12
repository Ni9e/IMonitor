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
        public static int indexCount = 0;
        public static int indexlapCount = 0;
        public static List<PrinterInformation> PrinterList { get; set; }
        public static List<RouterInformation> RouterList { get; set; }
        public static List<LaptopInformation> LaptopList { get; set; }
        const int defaultTimeout = 10 * 1000; // 打印机抓取超时，10秒
        private static int storeCount = 0; // 店铺数量
        private static int[] laptopComplete; // 笔记本完成Ping的数量
        private static int[] routerComplete; // 路由器完成Ping的数量
        private static AutoResetEvent flag = new AutoResetEvent(false);
        
        public static StoreHost GetStoreHost(string storeNo)
        {
            StoreHost host = new StoreHost();            
            string url = "http://10.1" + storeNo.Substring(0, 2) + "." + Convert.ToInt32(storeNo.Substring(2, 2)).ToString();
            host.Url = url + ".100";
            host.PrinterIP = host.Url.Substring(7);
            host.RouterIP = (url + ".1").Substring(7);
            host.LaptopIP1 = (url + ".40").Substring(7) + ";" + (url + ".41").Substring(7);
            host.LaptopIP2 = (url + ".50").Substring(7) + ";" + (url + ".51").Substring(7);
            host.FingerIP = (url + ".140").Substring(7);
            host.FlowIP = (url + ".120").Substring(7);
            
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

        public static IndexQuery GetIndexData(string storeNo)
        {
            IndexQuery iq = new IndexQuery();
            DataSet ds = SqlHelper.GetStoreInformation(storeNo);
            iq.StoreNo = ds.Tables[0].Rows[0]["storeNo"].ToString();
            iq.StoreRegion = ds.Tables[0].Rows[0]["storeRegion"].ToString();
            iq.StoreType = ds.Tables[0].Rows[0]["storeType"].ToString();
            iq.PrinterType = ds.Tables[0].Rows[0]["printerType"].ToString();
            iq.TonerType = ds.Tables[0].Rows[0]["tonerType"].ToString();
            iq.RouterIP = ds.Tables[0].Rows[0]["routerIP"].ToString();
            iq.PrinterIP = ds.Tables[0].Rows[0]["printerIP"].ToString();
            string[] ip1 = ds.Tables[0].Rows[0]["laptopIP1"].ToString().Split(';');
            for (int j = 0; j < ip1.Length; j++)
            {
                iq.IPs.Add(ip1[j]);
            }

            string[] ip2 = ds.Tables[0].Rows[0]["laptopIP2"].ToString().Split(';');
            for (int k = 0; k < ip2.Length; k++)
            {
                iq.IPs.Add(ip2[k]);
            }
            iq.Count = 0;

            try
            {
                if (new Ping().Send(iq.RouterIP).Status == IPStatus.Success)
                {
                    iq.RouterNetwork = "Up";
                    SetPrinterInformation(iq);
                    SetLaptopInformation(iq);                    
                }
                else
                {
                    iq.RouterNetwork = "Down";
                    iq.PrinterNetwork = "Down";
                    iq.PrinterStatus = "";
                    iq.TonerStatus = "";
                    iq.LaptopIP = "";
                    iq.LaptopNetwork = "Down";
                    iq.PrinterService = "";
                }
            }
            catch (System.Exception ex)
            {
                iq.RouterNetwork = string.IsNullOrEmpty(iq.RouterNetwork) ? ex.Message : iq.RouterNetwork;
                iq.PrinterNetwork = string.IsNullOrEmpty(iq.PrinterNetwork) ? ex.Message : iq.PrinterNetwork;
                iq.PrinterStatus = string.IsNullOrEmpty(iq.PrinterStatus) ? ex.Message : iq.PrinterStatus;
                iq.TonerStatus = string.IsNullOrEmpty(iq.TonerStatus) ? ex.Message : iq.TonerStatus;
                iq.LaptopIP = string.IsNullOrEmpty(iq.LaptopIP) ? ex.Message : iq.LaptopIP;
                iq.LaptopNetwork = string.IsNullOrEmpty(iq.LaptopNetwork) ? ex.Message : iq.LaptopNetwork;
                iq.PrinterService = "";
            }
            return iq;
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
                        if (state.IsIndexQuery)
                        {
                            indexCount++;
                        }
                        else
                        {
                            count++;
                            PrinterList.Add(state.Printer);
                            Console.WriteLine(count.ToString() + "/" + storeCount.ToString() + " " + state.Printer.StoreNo + ": " + state.Printer.PrinterStatus + " " + state.Printer.TonerStatus);
                        }
                    }
                }
                response.Close();
            }
            catch (System.Exception ex)
            {
                state.Printer.PrinterStatus = ex.Message;
                state.Printer.TonerStatus = ex.Message;
                state.Printer.PrinterNetwork = "Down";
                state.Printer.Date = DateTime.Now.ToString();
                if (state.IsIndexQuery)
                {
                    indexCount++;
                }
                else
                {
                    count++;                    
                    PrinterList.Add(state.Printer);
                    Console.WriteLine(count.ToString() + "/" + storeCount.ToString() + " " + state.Printer.StoreNo + ": " + ex.Message.ToString());
                }                  
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
                state.IsIndexQuery = false;
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

        public static void SetPrinterInformation(IndexQuery iq)
        {
            RequestState state = new RequestState();
            PrinterInformation printer = new PrinterInformation();
            printer.StoreNo = iq.StoreNo;
            printer.StoreRegion = iq.StoreRegion;
            printer.StoreType = iq.StoreType;
            printer.PrinterType = iq.PrinterType;
            printer.TonerType = iq.TonerType;

            state.Host = Common.GetStoreHost(iq.StoreNo);
            state.Host.PrinterIP = iq.PrinterIP;
            state.Printer = printer;
            state.IsIndexQuery = true;
            try
            {
                if (new Ping().Send(state.Host.PrinterIP).Status == IPStatus.Success)
                {
                    BeginResponse(state);
                }
                else
                {
                    indexCount++;
                    state.Printer.PrinterStatus = "打印机无法连接";
                    state.Printer.TonerStatus = "打印机无法连接";
                    state.Printer.PrinterNetwork = "Down";
                    state.Printer.Date = DateTime.Now.ToString();                    
                    Console.WriteLine(count.ToString() + "/" + storeCount.ToString() + " " + state.Printer.StoreNo + ": 打印机无法连接");
                }
            }
            catch (System.Exception ex)
            {
                indexCount++;
                state.Printer.PrinterStatus = ex.Message;
                state.Printer.TonerStatus = ex.Message;
                state.Printer.PrinterNetwork = "Down";
                state.Printer.Date = DateTime.Now.ToString();                
                Console.WriteLine(count.ToString() + "/" + storeCount.ToString() + " " + state.Printer.StoreNo + ": " + ex.Message.ToString());
            }

            while (true)
            {
                if (indexCount == 1)
                {
                    iq.PrinterNetwork = state.Printer.PrinterNetwork;
                    iq.PrinterStatus = state.Printer.PrinterStatus;
                    iq.TonerStatus = state.Printer.TonerStatus;
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
            p.SendAsync(router.IP, 500, router);
            flag.WaitOne();
        }

        private static void RouterCallback(object sender, PingCompletedEventArgs e)
        {
            RouterInformation router = (RouterInformation)e.UserState;
            router.RouterNetwork = (e.Reply.Status == IPStatus.Success) ? "Up" : "Down";
            router.Date = DateTime.Now.ToString();
            RouterList.Add(router);
            routerComplete[router.I] = 1; // 相应位置1表示已经Ping完            
            Console.WriteLine((router.I + 1).ToString() + "/" + router.Total.ToString() + " " + router.StoreNo + ": " + router.RouterNetwork);
            flag.Set();
        }

        public static void GetRouterTask()
        {
            SqlHelper.DeleteRouterInformationTemp();
            Common.DoGetRouterInformationTask();
            Console.WriteLine("Thread sleep 30s...");
            Thread.Sleep(30 * 1000);
            Common.DoGetRouterInformationTask();
            SqlHelper.InsertRouterInformation();
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
                laptop.IsIndexQuery = false;

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
                LaptopAssist(laptop, laptop.Count); 
            }
            while (true)
            {
                if( count == GetArraySum(laptopComplete))
                {
                    string[] clist = { "storeNo", "storeRegion", "storeType", "laptopNetwork", "ip", "printerService", "date" };
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
                        row["ip"] = LaptopList[i].IP;
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

        private static void LaptopAssist(LaptopInformation laptop, int i)
        {
            Ping p = new Ping();            
            p.PingCompleted += LaptopCallback;
            p.SendAsync(laptop.IPs[i], 1000, laptop);
            flag.WaitOne();
        }

        private static void LaptopCallback(object sender, PingCompletedEventArgs e)
        {            
            LaptopInformation laptop = (LaptopInformation)e.UserState;
            laptop.LaptopNetwork = (e.Reply.Status == IPStatus.Success) ? "Up" : "Down";
            laptop.Count++;
            if(laptop.LaptopNetwork == "Down" && laptop.Count != laptop.IPs.Count)
            {
                flag.Set();
                LaptopAssist(laptop, laptop.Count);
            }
            else
            {
                // 获取打印机服务 待添加                
                laptop.Date = DateTime.Now.ToString();
                laptop.IP = laptop.IPs[laptop.Count - 1];                
                if (!laptop.IsIndexQuery)
                {
                    LaptopList.Add(laptop);
                    laptopComplete[laptop.I] = 1; // 置1表示该打印机已经Ping完
                    Console.WriteLine((laptop.I + 1).ToString() + "/" + laptop.Total.ToString() + " " + laptop.StoreNo + ": " + laptop.LaptopNetwork);                 
                }
                else
                {
                    indexlapCount++;
                }                
            }
            flag.Set();     
        }

        public static void GetPrinterService(string laptopIP)
        {            
            //连接远程计算机  
            ConnectionOptions co = new ConnectionOptions();
            co.Username = ".\\store";
            co.Password = "20122012";

            ManagementScope ms = new ManagementScope("\\\\" + laptopIP + "\\root\\cimv2", co);
            //查询远程计算机  
            ObjectQuery oq = new ObjectQuery("SELECT * FROM Win32_Service");
            ManagementObjectSearcher query1 = new ManagementObjectSearcher(ms, oq);
            ManagementObjectCollection queryCollection1 = query1.Get();
            foreach (ManagementObject mo in queryCollection1)
            {
                if (Convert.ToString(mo["Name"]) == "Spooler")
                {
                    Console.WriteLine((mo["Started"].ToString() == "True") ? "打印机服务已启动" : "打印机服务未启动");
                }
            }
        }

        public static string GetPrinterService(LaptopInformation laptop)
        {
            string result = string.Empty;
            if (laptop.StoreType == "IFocus")
            {
                //连接远程计算机  
                ConnectionOptions co = new ConnectionOptions();
                co.Username = ".\\store";
                co.Password = "20122012";
                ManagementScope ms = new ManagementScope("\\\\" + laptop.IPs[laptop.Count - 1] + "\\root\\cimv2", co);
                //查询远程计算机  
                ObjectQuery oq = new ObjectQuery("SELECT * FROM Win32_Service");
                ManagementObjectSearcher query1 = new ManagementObjectSearcher(ms, oq);
                ManagementObjectCollection queryCollection1 = query1.Get();
                foreach (ManagementObject mo in queryCollection1)
                {
                    if (Convert.ToString(mo["Name"]) == "Spooler")
                    {
                        result = (mo["Started"].ToString() == "True") ? "打印机服务已启动" : "打印机服务未启动";
                    }
                }
            }            
            return result;
        }

        public static void SetLaptopInformation(IndexQuery iq)
        {
            LaptopInformation laptop = new LaptopInformation();
            laptop.StoreNo = iq.StoreNo;
            laptop.StoreRegion = iq.StoreRegion;
            laptop.StoreType = iq.StoreType;
            laptop.I = 0;
            laptop.Total = 1;
            laptop.Count = 0;
            laptop.IPs = iq.IPs;
            laptop.IsIndexQuery = true;
            LaptopAssist(laptop, laptop.Count);
            while (true)
            {
                if (indexlapCount == 1)
                {
                    iq.LaptopNetwork = laptop.LaptopNetwork;
                    iq.LaptopIP = laptop.IP;
                    break;
                }
            }
        }
        #endregion

        #region 发送邮件

        public static void SendLowinkEmail()
        {
            // 同步门店信息，当日发送邮件状态同步
            SqlHelper.SyncSendEmail();
            SqlHelper.UpdateIsSend();

            // 获取缺墨的门店
            List<string> storeNos = new List<string>();
            DataSet lowink = SqlHelper.GetLowInkPrinter();
            for (int i = 0; i < lowink.Tables[0].Rows.Count; i++)
            {
                storeNos.Add(lowink.Tables[0].Rows[i][0].ToString());
            }

            // 获取缺墨门店的发送邮件状态
            DataSet ds = SqlHelper.GetEmailIsSend(storeNos);
            int count = ds.Tables[0].Rows.Count;
            bool[] status = new bool[count]; // 记录发送邮件的状态

            // EmailFrom emailFrom = new EmailFrom("IwoooMonitor@163.com", "iwooo2014", "smtp.163.com", 25);   
            EmailFrom emailFrom = new EmailFrom("zhanggb@iwooo.com", "finkle1986819", "59.60.9.101", 25);

            List<string> cc = new List<string>();            
            //cc.Add("liull@iwooo.com");
            cc.Add("zhanggb@iwooo.com");

            string subject = DateTime.Now.ToString("yyyy-MM-dd") + " 门店打印机缺墨提醒";
            string mailBody = "以下门店墨盒不足10%，请尽快更换！<br>";
            int flag = 0;
            for (int i = 0; i < count; i++)
            {
                string storeNo = ds.Tables[0].Rows[i][0].ToString();
                string isSend = ds.Tables[0].Rows[i][1].ToString();
                if (isSend == "False")
                {
                    mailBody += " " + storeNo + ",";
                }
                else
                {
                    flag++;  // 记录已发送过邮件的数量，如果和缺墨的门店的数量一样说明都发送过了
                }
                status[i] = true;
            }
            mailBody = mailBody.TrimEnd(',') + "<br>共 " + (count - flag).ToString() + " 家门店";
            EmailHelper email = new EmailHelper(emailFrom, "yangyj@iwooo.com", cc);

            if (flag == count)
            {
                Console.WriteLine("所有门店已经通知了");
            }
            else
            {
                if (email.SendMail(subject, mailBody) == true)
                {
                    Console.WriteLine("邮件发送成功！");
                }
                else
                {
                    // 发送失败，需要回滚isSend的状态
                    for (int i = 0; i < count; i++)
                    {
                        string isSend = ds.Tables[0].Rows[i][1].ToString();
                        if (isSend == "False")
                        {
                            status[i] = false;
                        }
                    }
                    Console.WriteLine("邮件发送失败！");
                }
            }
            // 更新发送邮件状态到数据库
            List<SendEmail> sendEmail = new List<SendEmail>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                SendEmail sendemail = new SendEmail();
                sendemail.StoreNo = ds.Tables[0].Rows[i][0].ToString();
                sendemail.IsSend = status[i];
                sendEmail.Add(sendemail);
            }
            SqlHelper.UpdateIsSend(sendEmail);
        }

        public static void SendLowinkEmailPerStore()
        {
            // 同步门店信息，当日发送邮件状态同步
            SqlHelper.SyncSendEmail();
            SqlHelper.UpdateIsSend();

            // 获取缺墨的门店
            List<string> storeNos = new List<string>();
            DataSet lowink = SqlHelper.GetLowInkPrinter();
            for (int i = 0; i < lowink.Tables[0].Rows.Count; i++)
            {
                storeNos.Add(lowink.Tables[0].Rows[i][0].ToString());
            }

            // 获取缺墨门店的发送邮件状态
            DataSet ds = SqlHelper.GetEmailIsSend(storeNos);
            int count = ds.Tables[0].Rows.Count;
            bool[] status = new bool[count]; // 记录发送邮件的状态

            EmailFrom emailFrom = new EmailFrom("iMonitor@iwooo.com ", "1q2w3e4r", "59.60.9.101", 25);

            List<string> cc = new List<string>();             
            cc.Add("zhanggb@iwooo.com");
            //cc.Add("HelpDesk.IT@lrgc.com.cn");


            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                string storeNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
                string isSend = ds.Tables[0].Rows[i]["isSend"].ToString();
                string emailAddress = ds.Tables[0].Rows[i]["emailAddress"].ToString();
                string region = ds.Tables[0].Rows[i]["storeRegion"].ToString();

                string subject = storeNo + " 门店缺墨";
                string mailBody = storeNo + " 门店墨盒不足10%，请注意更换！";
                EmailHelper email = new EmailHelper(emailFrom, "iwooomonitor@163.com", cc);
                if (isSend == "False")
                {
                    if (email.SendMail(subject, mailBody) == true)
                    {
                        status[i] = true;
                        Console.WriteLine("{0} 发送成功！", storeNo);
                    }
                    else
                    {
                        status[i] = false;
                        Console.WriteLine("{0} 发送失败！", storeNo);
                    }
                }
                else
                {
                    status[i] = true;
                    Console.WriteLine("{0} 已经发送过了！", storeNo);
                }
            }

            // 更新发送邮件状态到数据库
            List<SendEmail> sendEmail = new List<SendEmail>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                SendEmail sendemail = new SendEmail();
                sendemail.StoreNo = ds.Tables[0].Rows[i][0].ToString();
                sendemail.IsSend = status[i];
                sendEmail.Add(sendemail);
            }
            SqlHelper.UpdateIsSend(sendEmail);
        }

        #endregion
    }
}
