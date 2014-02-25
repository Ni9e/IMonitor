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
                //SqlHelper.DeleteRouterInformationTemp();
                //Common.DoGetRouterInformationTask();
                //Console.WriteLine("Thread sleep 60s...");
                //Thread.Sleep(60 * 1000);
                //Common.DoGetRouterInformationTask();
                //SqlHelper.InsertRouterInformation();

                Common.DoGetLaptopInformationTask();
            }
             
        }        
    }
}
