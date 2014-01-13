using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMonitorService.Code;

namespace IMonitorService
{
    class Program
    {
        static void Main(string[] args)
        {
            StoreHost host = Common.GetStoreHost("6210");
            Console.WriteLine(host.PrinterIP);
            Console.WriteLine(host.RouterIP);
            Console.WriteLine(host.LaptopIP1);
            Console.WriteLine(host.LaptopIP2);
            Console.WriteLine(host.Urls[0]);
            Console.WriteLine(host.Urls[1]);
            Console.ReadKey();
        }
    }
}
