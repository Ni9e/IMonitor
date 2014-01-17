using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMonitorService.Code
{
    public class StoreHost : StoreIP
    {
        public string Url { get; set; }         // 打印机Url地址        
        public string Html { get; set; }        // 打印机页面
        public string Status { get; set; }      // 状态  
    }
}
