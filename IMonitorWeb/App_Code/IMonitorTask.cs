using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using IMonitorService.Code;

public class IMonitorTask
{
    public static string GetPath()
    {
        string pa = AppDomain.CurrentDomain.BaseDirectory;
        int idx = pa.IndexOf("IMonitorWeb");
        string path = pa.Substring(0, idx) + @"IMonitorAssist\bin\Debug\IMonitorAssist.exe";
        return path;
    }

    public static void GetTaskData(TaskCondition tc)
    {       
        Process p = new Process();
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.Arguments = tc.ToString();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.FileName = GetPath();
        p.Start();
        p.WaitForExit();
    }

    public static void GetPrinterTask()
    {       
        Process p = new Process();
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.Arguments = "Print";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.FileName = GetPath();
        p.Start();
        p.WaitForExit(270 * 1000);
        GetSendEmailTask();
    }

    public static void GetRouterTask()
    {  
        Process p = new Process();
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.Arguments = "Router";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.FileName = GetPath();
        p.Start();
    }

    public static void GetLaptopTask()
    {
        Process p = new Process();
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.Arguments = "Laptop";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.FileName = GetPath();
        p.Start();
    }

    public static void GetSendEmailTask()
    {        
        Process p = new Process();
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.Arguments = "SENDEMAIL";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.FileName = GetPath();
        p.Start();
    }
}