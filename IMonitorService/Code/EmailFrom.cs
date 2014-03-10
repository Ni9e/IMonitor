using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IMonitorService.Code
{
    public class EmailFrom
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }

        public EmailFrom(string name, string password, string host, int port)
        {
            Name = name;
            Password = password;
            Host = host;
            Port = port;
        }
    }
}
