using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace IMonitorService.Code
{
    public class RequestState
    {
        public HttpWebRequest Request { get; set; }
        public StoreHost Host { get; set; }
        public PrinterInformation Printer { get; set; }
        public PrinterType PrinterType { get; set; }
        public bool IsIndexQuery { get; set; }
    }
}
