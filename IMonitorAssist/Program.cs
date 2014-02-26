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
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Common.GetPrinterService();
                sw.Stop();
                double s = sw.ElapsedMilliseconds / 1000;
                Console.WriteLine(s.ToString());

            }
             
        }        
    }
}
