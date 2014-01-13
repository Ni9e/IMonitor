using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMonitorService.Code
{
    public class StoreHost
    {
        public List<string> Urls { get; set; }  // 打印机Url地址
        public string PrinterIP { get; set; }   // 打印机IP
        public string RouterIP { get; set; }    // 路由器IP
        public string LaptopIP1 { get; set; }   // 笔记本IP1
        public string LaptopIP2 { get; set; }   // 笔记本IP2
        public string Html { get; set; }        // 打印机页面
        public string Status { get; set; }      // 状态
        
    }
}
