using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMonitorService.Code
{
    public class Common
    {
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
    }
}
