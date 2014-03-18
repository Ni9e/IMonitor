using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;

/// <summary>
/// IMonitorConfig 的摘要说明
/// </summary>
public class IMonitorConfig
{
    private static string p = AppDomain.CurrentDomain.BaseDirectory;
    //private static string p = @"C:\Users\helpdesk.it\Documents";

    public static string GetSetting(string name)
    {
        string result = string.Empty;
        string filepath = p + "\\conf.txt";
        string line;
        using (StreamReader sr = new StreamReader(filepath, Encoding.Default))
        {
            while ((line = sr.ReadLine()) != null)
            {
                int n = line.IndexOf(name + "$");
                if (n != -1)
                {
                    int k = line.IndexOf("$");
                    result = line.Substring(k + 1);
                }
            }
        }

        return result;
    }

    public static void WriteSetting(string[] name, string[] value)
    {
        string filepath = p + "\\conf.txt";
        using (FileStream fs = new FileStream(filepath, FileMode.Open))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                for (int i = 0; i < name.Length; i++ )
                {
                    sw.WriteLine(name[i] + "$" + value[i]);
                }
            }
        }
    }
}