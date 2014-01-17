using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OLEPRNLib;
using System.Diagnostics;
using System.Data;

namespace IMonitorService.Code
{
    /// <summary>
    /// 未使用
    /// </summary>
    public class SNMPStatus
    {
        public static string[] DeviceStatus = { "", "unkonwn", "running", "warning", "testing", "down" };
        public static string[] PrinterStatus = { "", "other", "unkonwn", "idle", "printing", "warmup" };
        public static string[] PrinterErrorState = { "lowPaper", "noPaper", "lowToner", "doorOpen", "jammed", "offline", "serviceRequested", "inputTrayMissing", "outputTrayMissing", "markerSupplyMissing", "outputNearFull", "outputFull", "inputTrayEmpty", "overduePreventMaint" };
        
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

        public static void GetPrinterInfo(string ip, string storeNo)
        {
            // ".1.3.6.1.2.1.43" 为打印机的MIB，这个Tree下面有所有打印机相关信息
            int Retries = 1;
            int TimeoutInMS = 2000;
            string CommunityString = "public";

            OLEPRNLib.SNMP snmp = new OLEPRNLib.SNMP();
            snmp.Open(ip, CommunityString, Retries, TimeoutInMS);

            //var printerArray = snmp.GetTree(".1.3.6.1.2.1.25"); // ".1.3.6.1.2.1.43.16.5.1.2.1.1"
            var deviceName = snmp.Get("host.hrDevice.hrDeviceTable.hrDeviceEntry.hrDeviceDescr.1");
            var deviceStatus = snmp.Get("host.hrDevice.hrDeviceTable.hrDeviceEntry.hrDeviceStatus.1");
            var printerStatus = snmp.Get("host.hrDevice.hrPrinterTable.hrPrinterEntry.hrPrinterStatus.1");
            var printerErrorState = snmp.Get("host.hrDevice.hrPrinterTable.hrPrinterEntry.hrPrinterDetectedErrorState.1");

            string s1 = SNMPStatus.DeviceStatus[deviceStatus];
            string s2 = SNMPStatus.PrinterStatus[printerStatus];
            if (printerErrorState.Trim() != "")
            {
                string s3 = SNMPStatus.PrinterErrorState[printerErrorState];
            }


            var tonerDesc = snmp.Get("printmib.prtMarkerSupplies.prtMarkerSuppliesTable.prtMarkerSuppliesEntry.prtMarkerSuppliesDescription.1.1");
            var tonerMax = snmp.Get("printmib.prtMarkerSupplies.prtMarkerSuppliesTable.prtMarkerSuppliesEntry.prtMarkerSuppliesMaxCapacity.1.1");
            var tonerCur = snmp.Get("printmib.prtMarkerSupplies.prtMarkerSuppliesTable.prtMarkerSuppliesEntry.prtMarkerSuppliesLevel.1.1");
            double tonerStatus = Math.Round(Convert.ToDouble(tonerCur) / Convert.ToDouble(tonerMax), 2) * 100;

            Console.WriteLine(storeNo + ": ");
            Console.WriteLine("Device name: {0}", deviceName);
            Console.WriteLine("Printer status: {0} {1}", s1, s2);
            Console.WriteLine("Toner status: {0} {1}", tonerDesc, tonerStatus);

        }

        public static void GetPrinterName()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int Retries = 1;
            int TimeoutInMS = 2000;
            string CommunityString = "public";
            DataSet ds = SqlHelper.GetNonHKStoreInformation();
            int count = ds.Tables[0].Rows.Count;
            List<string> names = new List<string>();

            for (int i = 0; i < count; i++)
            {
                string storeNo = ds.Tables[0].Rows[i]["storeNo"].ToString();
                try
                {
                    OLEPRNLib.SNMP snmp = new OLEPRNLib.SNMP();
                    snmp.Open(ds.Tables[0].Rows[i]["printerIP"].ToString(), CommunityString, Retries, TimeoutInMS);
                    var printerName = snmp.Get("host.hrDevice.hrDeviceTable.hrDeviceEntry.hrDeviceDescr.1");
                    if (!names.Contains(printerName))
                    {
                        names.Add(printerName);
                    }
                    Console.WriteLine(storeNo + ": " + printerName);
                   
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(storeNo + ": " + ex.Message);
                }
            }
            sw.Stop();
            double s = sw.ElapsedMilliseconds / 1000;
            Console.WriteLine("耗时: {0} 秒", s);
            foreach (string item in names)
            {
                Console.WriteLine(item);
            }
        }
    }
}
