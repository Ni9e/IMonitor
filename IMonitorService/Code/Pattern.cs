using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IMonitorService.Code
{
    public class Pattern
    {
        public string SearchString1 { get; set; }
        public string SearchString2 { get; set; }
        public string SearchString3 { get; set; }
        public string SearchString3N { get; set; }
        public string Anchor1 { get; set; }
        public string Anchor2 { get; set; }
        public string Anchor3 { get; set; }
        public string Anchor4 { get; set; }
        public string Anchor5 { get; set; }
        public string Anchor6 { get; set; }

        public int SearchStartIndex { get; set; }
        public string ReplaceString { get; set; }

        public Pattern(string s1, string s2, string s3, string s3n, string a1, string a2, string a3, string a4, string a5, string a6, int idx, string rs)
        {
            this.SearchString1 = s1;
            this.SearchString2 = s2;
            this.SearchString3 = s3;
            this.SearchString3N = s3n;
            this.Anchor1 = a1;
            this.Anchor2 = a2;
            this.Anchor3 = a3;
            this.Anchor4 = a4;
            this.Anchor5 = a5;
            this.Anchor6 = a6;
            this.SearchStartIndex = idx;
            this.ReplaceString = rs;

        }
    }
}
