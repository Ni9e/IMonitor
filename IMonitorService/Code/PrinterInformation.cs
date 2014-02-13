using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IMonitorService.Code
{
    public class PrinterInformation
    {
        public string StoreNo { get; set; }
        public string StoreRegion { get; set; }
        public string StoreType { get; set; }
        public string PrinterNetwork { get; set; }
        public string PrinterStatus { get; set; }
        public string TonerStatus { get; set; }
        public string PrinterType { get; set; }
        public string TonerType { get; set; }
        public string Date { get; set; }

        public PrinterInformation()
        {

        }

        public PrinterInformation(StoreInformation store)
        {
            this.StoreNo = store.StoreNo;
            this.StoreRegion = store.StoreRegion;
            this.StoreType = store.StoreType;
            this.PrinterType = store.StoreType;
            this.TonerType = store.TonerType;
        }
    }
}
