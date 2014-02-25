using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IMonitorService.Code
{
    public class RouterInformation
    {
        public string StoreNo { get; set; }
        public string StoreRegion { get; set; }
        public string StoreType { get; set; }
        public string RouterNetwork { get; set; }
        public string RouterType { get; set; }
        public string Date { get; set; }

        // 辅助属性
        public string IP { get; set; }
        public int I { get; set; }
        public int Total { get; set; }
    }
}
