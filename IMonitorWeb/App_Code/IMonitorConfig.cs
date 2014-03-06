using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

/// <summary>
/// IMonitorConfig 的摘要说明
/// </summary>
public class IMonitorConfig
{
    public static string GetSetting(string name)
    {
        return WebConfigurationManager.AppSettings[name];
    }

    public static void WriteSetting(string name, string value)
    {
        WebConfigurationManager.AppSettings.Set(name, value);
    }
}