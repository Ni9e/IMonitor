using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMonitorService.Code;
using System.Data;

namespace IMonitorService
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlHelper.SyncStoreInformation();
            DataSet ds = SqlHelper.GetStoreInformation();
            
            
            //Console.ReadKey();
        }
    }
}
