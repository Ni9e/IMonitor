using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using IMonitorService.Code;
using System.Diagnostics;


namespace IMonitorAssist
{
    class Program
    {           
        static void Main(string[] args)
        {
            switch (args[0].ToUpper())
            {
                case "PRINT":
                    {
                        Common.DoGetPrinterInfomationTask();
                    }
                    break;
            } 
        }        
    }
}
