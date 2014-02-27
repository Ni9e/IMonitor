using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace IMonitorService.Code
{
    public class LaptopInformation
    {
        public string StoreNo { get; set; }
        public string StoreRegion { get; set; }
        public string StoreType { get; set; }
        public string LaptopNetwork { get; set; }
        public string IP { get; set; }
        public string PrinterService { get; set; }
        public string Date { get; set; }

        // 辅助属性        
        public int I { get; set; }
        public int Total { get; set; }
        public int Count { get; set; }
        public List<string> IPs { get; set; }
        public bool IsIndexQuery { get; set; }

        public LaptopInformation()
        {
            IPs = new List<string>();
        }
    }
}
