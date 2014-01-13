using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace IMonitorService.Code
{
    public class SqlHelper
    {
        #region 连接字符串

        private static string connRemote = @"Data Source=10.15.130.78,51433;Initial Catalog=LUXERP;User ID=sa;Password=portal123;Max Pool Size = 512;Connection Timeout=15;";
        //private static string connLocal = @"Data Source=10.15.140.110;Initial Catalog=IMonitor;User ID=iwooo;Password=iwooo2013;Max Pool Size = 512;Connection Timeout=15;";
        private static string connLocal = @"Data Source=.;Initial Catalog=IMonitor;User ID=sa;Password=Sikong1986;Max Pool Size = 512;Connection Timeout=15;";
        
        #endregion

        public static DataSet GetOpeningStores()
        {
            DataSet ds = new DataSet();
            using (SqlConnection con = new SqlConnection(connRemote))
            {
                string sql = "select StoreNo, Region, StoreType from dbo.tb_Stores where StoreState='900';";
                SqlDataAdapter da = new SqlDataAdapter();               
                da.SelectCommand = new SqlCommand(sql, con);

                con.Open();
                da.Fill(ds);
                con.Close();
            }
            return ds;
        }

        public static void CommonBulkInsert(DataTable dt, string tableName)
        {
            int count = dt.Rows.Count;
            if (count == 0)
            {
                return;
            }
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                using (SqlBulkCopy copy = new SqlBulkCopy(conn))
                {
                    copy.BatchSize = count;
                    copy.DestinationTableName = tableName;
                    for (int i = 0, l = dt.Columns.Count; i < l; i++)
                    {
                        copy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                    }
                    conn.Open();
                    copy.WriteToServer(dt);
                    conn.Close();
                }
            }
        }

        #region StoreInformation



        #endregion
    }
}
