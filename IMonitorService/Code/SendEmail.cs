using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IMonitorService.Code
{
    public class SendEmail
    {
        public string StoreNo { get; set; }
        public bool IsSend { get; set; }
        public string Date { get; set; }
        public int TonerCount { get; set; }
        public string StoreStatus { get; set; }
    }
}
