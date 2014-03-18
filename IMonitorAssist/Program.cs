﻿using System;
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
                            Common.GetRouterTask();
                        }
                        break;
                    case "LAPTOP":
                        {
                            Common.DoGetLaptopInformationTask();
                        }
                        break;
                    case "INDEXQUERY":
                        {
                            string storeNo = args[1].ToString();
                            IndexQuery iq = Common.GetIndexData(storeNo);
                            SqlHelper.DeleteIndexQuery();
                            SqlHelper.InsertIndexQuery(iq);                            
                        }
                        break;
                    case "SENDEMAIL":
                        {
                            Common.SendLowinkEmailPerStore();
                        }
                        break;
                }
            }
            else
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Common.DoGetLaptopInformationTask();
                //Common.GetPrinterService("10.160.14.50");

                //IndexQuery iq = Common.GetIndexData("6608");
                //SqlHelper.DeleteIndexQuery();
                //SqlHelper.InsertIndexQuery(iq);
                //Console.WriteLine("StoreNo: " + iq.StoreNo);
                //Console.WriteLine("Router: " + iq.RouterNetwork);
                //Console.WriteLine("Printer: " + iq.PrinterNetwork);
                //Console.WriteLine("PStatus: " + iq.PrinterStatus);
                //Console.WriteLine("TStatus: " + iq.TonerStatus);
                //Console.WriteLine("Laptop: " + iq.LaptopNetwork);
                //Console.WriteLine("LapIP: " + iq.LaptopIP);

                //Common.DoGetPrinterInfomationTask();
                //Common.DoGetRouterInformationTask();
                //Common.DoGetLaptopInformationTask();
                

                //Common.SendLowinkEmailPerStore();

                //DataSet ds = SqlHelper.TonerSumReport("03", "2014", false);
                //for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                //{
                //    string storeNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
                //    string storeRegion = ds.Tables[0].Rows[i]["storeRegion"].ToString();
                //    string storeType = ds.Tables[0].Rows[i]["storeType"].ToString();
                //    string tonerCount = ds.Tables[0].Rows[i]["tonerCount"].ToString();
                //    string storeStatus = ds.Tables[0].Rows[i]["storeStatus"].ToString();
                //}

                sw.Stop();
                double s = sw.ElapsedMilliseconds / 1000.0;
                Console.WriteLine(s.ToString());
            }
             
        }

       
    }
}
