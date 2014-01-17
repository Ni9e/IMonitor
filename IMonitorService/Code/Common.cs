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

namespace IMonitorService.Code
{
    public class Common
    {
        public static int count = 0; // 打印机抓取完成计数
        public static List<PrinterInformation> PrinterList { get; set; }
        const int defaultTimeout = 10 * 1000; // 打印机抓取超时，10秒
        public static int storeCount = 0; // 店铺数量
        
        public static StoreHost GetStoreHost(string storeNo)
        {
            StoreHost host = new StoreHost();            
            string url = "http://10.1" + storeNo.Substring(0, 2) + "." + Convert.ToInt32(storeNo.Substring(2, 2)).ToString();
            host.Url = url + ".100";
            host.PrinterIP = host.Url.Substring(7);
            host.RouterIP = (url + ".1").Substring(7);
            host.LaptopIP1 = (url + ".40").Substring(7);
            host.LaptopIP2 = (url + ".50").Substring(7);           
            
            return host;
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
                state.Printer.PrinterType = html;

                if (html.IndexOf("HP LASERJET 400 M401N") != -1)
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
            state.Printer.Date = DateTime.Now;
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
                state.Printer.Date = DateTime.Now;
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
                        state.Printer.Date = DateTime.Now;
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
                    state.Printer.Date = DateTime.Now;
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
    }
}
