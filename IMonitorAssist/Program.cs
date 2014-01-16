using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using IMonitorService.Code;
using System.Diagnostics;
using OLEPRNLib;

namespace IMonitorAssist
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            GetPrinterInfo("10.169.28.100");
        }

        public static void GetPrinterInfo()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int Retries = 1;
            int TimeoutInMS = 2000;
            string CommunityString = "public";
            DataSet ds = SqlHelper.GetNonHKStoreInformation();
            int count = ds.Tables[0].Rows.Count;
            for (int i = 0; i < count; i++)
            {
                string storeNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
                try
                {
                    OLEPRNLib.SNMP snmp = new OLEPRNLib.SNMP();
                    snmp.Open(ds.Tables[0].Rows[i]["printerIP"].ToString(), CommunityString, Retries, TimeoutInMS);
                    var printerStatus = snmp.Get(".1.3.6.1.2.1.43.16.5.1.2.1.1");
                    var tonerCur = snmp.Get(".1.3.6.1.2.1.43.11.1.1.9.1.1");
                    var tonerMax = snmp.Get(".1.3.6.1.2.1.43.11.1.1.8.1.1");
                    var str6 = snmp.Get(".1.3.6.1.2.1.43.11.1.1.6.1.1");
                    var model = snmp.Get(".1.3.6.1.2.1.25.3.2.1.3.1");
                    double tonerStatus = Math.Round(Convert.ToDouble(tonerCur) / Convert.ToDouble(tonerMax), 2) * 100;
                    Console.Write(storeNo + ": ");
                    Console.Write(printerStatus + ", ");
                    Console.Write(str6 + " " + tonerStatus + "%, ");
                    Console.WriteLine(model);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(storeNo + ": " + ex.Message);
                }
            }
            sw.Stop();
            double s = sw.ElapsedMilliseconds / 1000;
            Console.WriteLine("耗时: {0} 秒", s);
        }

        public static void GetPrinterInfo(string ip)
        {
            int Retries = 1;
            int TimeoutInMS = 2000;
            string CommunityString = "public";

            OLEPRNLib.SNMP snmp = new OLEPRNLib.SNMP();
            snmp.Open(ip, CommunityString, Retries, TimeoutInMS);
            //var printerStatus = snmp.Get(".1.3.6.1.2.1.43.16.5.1.2.1.1");
            //Console.WriteLine(printerStatus);
            var tonerCur = snmp.Get(".1.3.6.1.2.1.43.11.1.1.9.1.1");
            var tonerMax = snmp.Get(".1.3.6.1.2.1.43.11.1.1.8.1.1");
            var str6     = snmp.Get(".1.3.6.1.2.1.43.11.1.1.6.1.1");
            var model    = snmp.Get(".1.3.6.1.2.1.25.3.2.1.3.1");
            double tonerStatus = Math.Round(Convert.ToDouble(tonerCur) / Convert.ToDouble(tonerMax), 2) * 100;
            //Console.Write(printerStatus + ", ");
            Console.Write(str6 + " " + tonerStatus + "%, ");
            Console.WriteLine(model);
        }
    }
}
