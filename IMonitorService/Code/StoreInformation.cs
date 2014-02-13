using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMonitorService.Code
{
    public class StoreInformation : StoreIP
    {
        public string StoreNo { get; set; }
        public string StoreRegion { get; set; }
        public string StoreType { get; set; }
        public string EmailAddress { get; set; }
        public string PrinterType { get; set; }
        public string TonerType { get; set; }
        public string RouterType { get; set; }

        public StoreInformation()
        {

        }

        public StoreInformation(StoreHost host)
        {
            this.PrinterIP = host.PrinterIP;
            this.RouterIP = host.RouterIP;
            this.LaptopIP1 = host.LaptopIP1;
            this.LaptopIP2 = host.LaptopIP2;
            this.FingerIP = host.FingerIP;
            this.FlowIP = host.FlowIP;
        }
    }
}
