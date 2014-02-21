using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using IMonitorService.Code;

public class IMonitorTask
{
    public static void GetTaskData(TaskCondition tc)
    {
        string path = AppDomain.CurrentDomain.BaseDirectory;
        int idx = path.IndexOf("IMonitorWeb");
        path = path.Substring(0, idx) + @"IMonitorAssist\bin\Debug\IMonitorAssist.exe";

        Process p = new Process();
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.Arguments = tc.ToString();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.FileName = path;
        p.Start();
        p.WaitForExit();
    }
}