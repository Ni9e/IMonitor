using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;

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
            string url = "http://10.1" + storeNo.Substring(0, 2) + "." + storeNo.Substring(2, 2);
            host.PrinterIP = (url + ".100").Substring(7);
            host.RouterIP = (url + ".1").Substring(7);
            host.LaptopIP1 = (url + ".40").Substring(7);
            host.LaptopIP2 = (url + ".50").Substring(7);
            host.Urls.Add(url + ".100");
            host.Urls.Add(url + ".100" + "/hp/device/info_deviceStatus.html");
            return host;
        }

        #region 打印机信息抓取

        

        #endregion
    }
}
