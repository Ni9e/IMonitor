using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IMonitorService.Code
{
    public class IndexQuery
    {
        public string StoreNo { get; set; }
        public string StoreRegion { get; set; }
        public string StoreType { get; set; }
        public string RouterIP { get; set; }
        public string RouterNetwork { get; set; }

        public string PrinterIP { get; set; }
        public string PrinterNetwork { get; set; }
        public string PrinterType { get; set; }
        public string TonerType { get; set; }
        public string PrinterStatus { get; set; }
        public string TonerStatus { get; set; }

        public List<string> IPs { get; set; }
        public string LaptopNetwork { get; set; }
        public string LaptopIP { get; set; }
        public string PrinterService { get; set; }

        public int Count { get; set; }

        public IndexQuery()
        {
            IPs = new List<string>();
        }
    }
}
