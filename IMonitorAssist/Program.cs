using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using IMonitorService.Code;
using System.Diagnostics;
using System.Threading;


namespace IMonitorAssist
{
    class Program
    {           
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                switch (args[0].ToUpper())
                {
                    case "PRINT":
                        {
                            Common.DoGetPrinterInfomationTask();
                        }
                        break;
                    case "ROUTER":
                        {
                            SqlHelper.DeleteRouterInformationTemp();
                            Common.DoGetRouterInformationTask();
                            Console.WriteLine("Thread sleep 30s...");
                            Thread.Sleep(30 * 1000);
                            Common.DoGetRouterInformationTask();
                            SqlHelper.InsertRouterInformation();
                        }
                        break;
                    case "LAPTOP":
                        {
                            Common.DoGetLaptopInformationTask();
                        }
                        break;
                }
            }
            else
            {
                //Stopwatch sw = new Stopwatch();
                //sw.Start();
                //Common.GetPrinterService("10.160.14.50");
                //sw.Stop();
                //double s = sw.ElapsedMilliseconds / 1000.0;
                //Console.WriteLine(s.ToString());

                IndexQuery iq = Common.GetIndexData("6028");
                Console.WriteLine("StoreNo: " + iq.StoreNo);
                Console.WriteLine("Router: " + iq.RouterNetwork);
                Console.WriteLine("Printer: " + iq.PrinterNetwork);
                Console.WriteLine("PStatus: " + iq.PrinterStatus);
                Console.WriteLine("TStatus: " + iq.TonerStatus);
                Console.WriteLine("Laptop: " + iq.LaptopNetwork);
                Console.WriteLine("LapIP: " + iq.LaptopIP);

                //IndexQuery iq = new IndexQuery();
                //iq.StoreNo = "6014";
                //iq.StoreRegion = "BJ";
                //iq.StoreType = "IFocus";
                //iq.IPs.Add("10.160.14.40");
                //iq.IPs.Add("10.160.14.41");
                //iq.IPs.Add("10.160.14.50");
                //iq.IPs.Add("10.160.14.51");
                //Common.SetLaptopInformation(iq);
                //Console.WriteLine(iq.LaptopNetwork + " " + iq.LaptopIP);
            }
             
        }        
    }
}
