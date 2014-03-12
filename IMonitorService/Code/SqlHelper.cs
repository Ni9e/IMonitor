using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Net.Sockets;

namespace IMonitorService.Code
{
    public class SqlHelper
    {
        #region 连接字符串

        private static string connRemote = @"Data Source=10.15.130.78,51433;Initial Catalog=LUXERP;User ID=sa;Password=portal123;Max Pool Size = 512;Connection Timeout=15;";
        //private static string connLocal = @"Data Source=10.15.140.110;Initial Catalog=IMonitor;User ID=iwooo;Password=iwooo2013;Max Pool Size = 512;Connection Timeout=15;";
        private static string connLocal = @"Data Source=.;Initial Catalog=IMonitor;User ID=sa;Password=Sikong1986;Max Pool Size = 512;Connection Timeout=15;";
        //private static string connLocal = @"Data Source=FINKLE-WIN8\SQL2008R2;Initial Catalog=IMonitor;User ID=sa;Password=Sikong1986;Max Pool Size = 512;Connection Timeout=15;";

        #endregion

        #region 通用        

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
               
        public static int ExecuteNonQuery(string spName, SqlParameter[] paras)
        {
            int rows;
            using (SqlConnection con = new SqlConnection(connLocal))
            {
                using (SqlCommand com = new SqlCommand())
                {
                    com.Connection = con;
                    com.CommandText = spName;
                    com.CommandType = CommandType.StoredProcedure;
                    com.CommandTimeout = 300;
                    if (paras != null)
                    {
                        com.Parameters.AddRange(paras);
                    }
                    con.Open();
                    rows = com.ExecuteNonQuery();                    
                    con.Close();
                }
            }
            return rows;
        }

        public static DataSet ExecuteDataSet(string spName, SqlParameter[] paras)
        {
            DataSet ds = new DataSet();            
            using (SqlConnection con = new SqlConnection(connLocal))
            {
                using (SqlCommand com = new SqlCommand())
                {
                    SqlDataAdapter ad = new SqlDataAdapter();
                    com.Connection = con;
                    ad.SelectCommand = com;
                    
                    com.CommandText = spName;
                    com.CommandType = CommandType.StoredProcedure;
                    com.CommandTimeout = 300;                    
                    if (paras != null)
                    {
                        com.Parameters.AddRange(paras);
                    }                    
                    con.Open();
                    ad.Fill(ds);
                    con.Close();
                }
            }
            return ds;
        }

        #endregion

        #region StoreInformation

        public static void TruncateStoreInformationTemp()
        {
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "truncate table dbo.StoreInformationTemp;";
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static void InsertStoreInformationTemp()
        {
            DataSet ds = SqlHelper.GetOpeningStores();
            List<StoreInformation> list = new List<StoreInformation>();
            int count = ds.Tables[0].Rows.Count;
            for (int i = 0; i < count; i++ )
            {
                string storeNo = ds.Tables[0].Rows[i][0].ToString();
                StoreHost host = Common.GetStoreHost(storeNo);
                StoreInformation store = new StoreInformation(host);
                store.StoreNo = storeNo;
                store.StoreRegion = ds.Tables[0].Rows[i][1].ToString();
                store.StoreType = ds.Tables[0].Rows[i][2].ToString();
                list.Add(store);
            }

            string[] clist = { "storeNo", "storeRegion", "storeType", "printerIP", "routerIP", "laptopIP1", "laptopIP2", "fingerIP", "flowIP", "emailAddress", "printerType", "tonerType", "routerType" };
            DataTable dt = new DataTable();
            foreach (string colname in clist)
            {
                dt.Columns.Add(colname);
            }
            int rowcount = list.Count;
            for (int i = 0; i < rowcount; i++ )
            {
                DataRow row = dt.NewRow();
                row["storeNo"] = list[i].StoreNo;
                row["storeRegion"] = list[i].StoreRegion;
                row["storeType"] = list[i].StoreType;
                row["printerIP"] = list[i].PrinterIP;
                row["routerIP"] = list[i].RouterIP;
                row["laptopIP1"] = list[i].LaptopIP1;
                row["laptopIP2"] = list[i].LaptopIP2;
                row["fingerIP"] = list[i].FingerIP;
                row["flowIP"] = list[i].FlowIP;
                row["emailAddress"] = list[i].StoreNo + "Store@luxottica.com.hk"; // 1525Store@luxottica.com.hk
                row["printerType"] = null;
                row["tonerType"] = null;
                row["routerType"] = null;
                dt.Rows.Add(row);
            }
            SqlHelper.TruncateStoreInformationTemp();
            SqlHelper.CommonBulkInsert(dt, "StoreInformationTemp");
        }

        public static void SyncStoreInformation()
        {
            SqlHelper.InsertStoreInformationTemp();
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "insert dbo.StoreInformation select * from dbo.StoreInformationTemp where storeNo in(select storeNo from dbo.StoreInformationTemp except select storeNo from dbo.StoreInformation); delete dbo.StoreInformation where storeNo in(select storeNo from dbo.StoreInformation except select storeNo from dbo.StoreInformationTemp);";
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static DataSet GetStoreInformation()
        {
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "select * from dbo.StoreInformation order by storeNo;";
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = new SqlCommand(sql, conn);
                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        public static DataSet GetStoreInformation(string storeNo)
        {            
            SqlParameter[] paras = { new SqlParameter("@storeNo", storeNo) };
            return SqlHelper.ExecuteDataSet("GetStoreInformation", paras);
        }

        public static DataSet GetNonHKStoreInformation()
        {
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "select * from dbo.StoreInformation where storeRegion<>'HK' order by storeNo;";
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = new SqlCommand(sql, conn);
                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        public static void UpdateStoreInformation(StoreInformation store)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@storeNo",store.StoreNo),
                                        new SqlParameter("@printerIP",store.PrinterIP),
                                        new SqlParameter("@routerIP",store.RouterIP),
                                        new SqlParameter("@laptopIP1",store.LaptopIP1),
                                        new SqlParameter("@laptopIP2",store.LaptopIP2),
                                        new SqlParameter("@fingerIP",store.FingerIP),
                                        new SqlParameter("@flowIP",store.FlowIP),
                                        new SqlParameter("@emailAddress",store.EmailAddress),
                                        new SqlParameter("@printerType",store.PrinterType),
                                        new SqlParameter("@tonerType",store.TonerType),
                                        new SqlParameter("@routerType",store.RouterType)
                                   };
            SqlHelper.ExecuteNonQuery("UpdateStoreInformation", paras);
        }

        public static void DeleteStoreInformation(string storeNo)
        {
            SqlParameter[] paras = { 
                                       new SqlParameter("@storeNo", storeNo) 
                                   };
            SqlHelper.ExecuteNonQuery("DeleteStoreInformation", paras);
        }

        public static void UpdatePrinterTypeFromPrinterInformation()
        {
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "update S set printerType = P.printerType	from dbo.StoreInformation S inner join dbo.PrinterInformation P on S.storeNo=P.storeNo	where P.printerType<>'' ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        #endregion

        #region PrinterInformation

        public static void DelCurDatePrinterInformation()
        {
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "delete dbo.PrinterInformation where convert(nvarchar(10),date,127) = convert(nvarchar(10),GETDATE(),127)";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public static DataSet GetPrinterInformation(PrinterCondition pc)
        {
            DataSet ds = new DataSet();
            string sql = string.Empty;
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                switch (pc)
                {
                    case PrinterCondition.All:
                        sql = "select * from dbo.PrinterInformation where convert(nvarchar(10),date,127) = convert(nvarchar(10),GETDATE(),127) order by storeNo";
                        break;
                    case PrinterCondition.Up:
                        {
                            sql = "select * from dbo.PrinterInformation where convert(nvarchar(10),date,127) = convert(nvarchar(10),GETDATE(),127) and printerNetwork='Up' ";
                            sql += "order by (case when substring(tonerStatus,PATINDEX('%[0-9]%',tonerStatus),CHARINDEX('%',tonerStatus,1)-PATINDEX('%[0-9]%',tonerStatus))<>'' then CAST(substring(tonerStatus,PATINDEX('%[0-9]%',tonerStatus),CHARINDEX('%',tonerStatus,1)-PATINDEX('%[0-9]%',tonerStatus)) as int) else 999 end), storeNo";
                        }
                        break;
                    case PrinterCondition.Down:
                        sql = "select * from dbo.PrinterInformation where convert(nvarchar(10),date,127) = convert(nvarchar(10),GETDATE(),127) and printerNetwork='Down' order by storeNo";
                        break;
                }
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(sql, conn);
                da.SelectCommand = cmd;
                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        public static int GetPrinterCount(PrinterCondition pc)
        {
            DataSet ds = new DataSet();
            string sql = string.Empty;
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                switch (pc)
                {
                    case PrinterCondition.All:
                        sql = "select count(*) total from dbo.PrinterInformation where convert(nvarchar(10),date,127) = convert(nvarchar(10),GETDATE(),127)";
                        break;
                    case PrinterCondition.Up: // 墨盒低于10%的数量
                        {
                            sql = "select count(*) total from dbo.PrinterInformation where convert(nvarchar(10),date,127) = convert(nvarchar(10),GETDATE(),127) and printerNetwork='Up' ";
                            sql += "and (case when substring(tonerStatus,PATINDEX('%[0-9]%',tonerStatus),CHARINDEX('%',tonerStatus,1)-PATINDEX('%[0-9]%',tonerStatus))<>'' then CAST(substring(tonerStatus,PATINDEX('%[0-9]%',tonerStatus),CHARINDEX('%',tonerStatus,1)-PATINDEX('%[0-9]%',tonerStatus)) as int) else 999 end) <= 10";
                        }
                        break;
                    case PrinterCondition.Down:
                        sql = "select count(*) total from dbo.PrinterInformation where convert(nvarchar(10),date,127) = convert(nvarchar(10),GETDATE(),127) and printerNetwork='Down'";
                        break;
                }
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(sql, conn);
                da.SelectCommand = cmd;
                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
        }

        public static DataSet GetLowInkPrinter()
        {
            DataSet ds = new DataSet();
            string sql = string.Empty;
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                sql = "select storeNo from dbo.PrinterInformation where convert(nvarchar(10),date,127) = convert(nvarchar(10),GETDATE(),127) and printerNetwork='Up' and (case when substring(tonerStatus,PATINDEX('%[0-9]%',tonerStatus),CHARINDEX('%',tonerStatus,1)-PATINDEX('%[0-9]%',tonerStatus))<>'' then CAST(substring(tonerStatus,PATINDEX('%[0-9]%',tonerStatus),CHARINDEX('%',tonerStatus,1)-PATINDEX('%[0-9]%',tonerStatus)) as int) else 999 end) <= 10 order by storeNo";
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(sql, conn);
                da.SelectCommand = cmd;
                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        #endregion

        #region RouterInformation

        public static DataSet GetRouterInformation(RouterCondition rc)
        {
            DataSet ds = new DataSet();
            string sql = string.Empty;
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                switch (rc)
                {
                    case RouterCondition.All:
                        sql = "select * from dbo.RouterInformation where convert(nvarchar(10),date,127)=convert(nvarchar(10),GETDATE(),127) order by routerNetwork, storeNo;";
                        break;
                    case RouterCondition.Up:
                        sql = "select * from dbo.RouterInformation where routerNetwork='Up' and convert(nvarchar(10),date,127)=convert(nvarchar(10),GETDATE(),127) order by storeNo;";
                        break;
                    case RouterCondition.Down:
                        sql = "select * from dbo.RouterInformation where routerNetwork='Down' and convert(nvarchar(10),date,127)=convert(nvarchar(10),GETDATE(),127) order by storeNo;";
                        break;
                }                
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(sql, conn);
                da.SelectCommand = cmd;
                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        public static int GetRouterCount(RouterCondition rc)
        {
            DataSet ds = new DataSet();
            string sql = string.Empty;
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                switch (rc)
                {
                    case RouterCondition.All:
                        sql = "select COUNT(*) total from dbo.RouterInformation where convert(nvarchar(10),date,127)=convert(nvarchar(10),GETDATE(),127)";
                        break;
                    case RouterCondition.Up:
                        sql = "select COUNT(*) total from dbo.RouterInformation where routerNetwork='Up' and convert(nvarchar(10),date,127)=convert(nvarchar(10),GETDATE(),127)";
                        break;
                    case RouterCondition.Down:
                        sql = "select COUNT(*) total from dbo.RouterInformation where routerNetwork='Down' and convert(nvarchar(10),date,127)=convert(nvarchar(10),GETDATE(),127)";
                        break;
                }
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(sql, conn);
                da.SelectCommand = cmd;
                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
        }

        public static void DeleteRouterInformationTemp()
        {
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "truncate table dbo.RouterInformationTemp";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public static void InsertRouterInformation()
        {
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "truncate table dbo.RouterInformation;insert dbo.RouterInformation select a.storeNo, a.storeRegion, a.storeType, case when a.routerNetwork='Down' and b.routerNetwork='Down' then 'Down' else 'Up' end as routerNetwork, '', b.date from (select *, ROW_NUMBER() over(partition by storeNo,CONVERT(nvarchar(10),date,127) order by storeNo,date) n from dbo.RouterInformationTemp) a, (select *, ROW_NUMBER() over(partition by storeNo,CONVERT(nvarchar(10),date,127) order by storeNo,date) n from dbo.RouterInformationTemp) b where a.n=1 and b.n=2 and a.storeNo=b.storeNo and CONVERT(nvarchar(10),a.date,127)=CONVERT(nvarchar(10),b.date,127) and CONVERT(nvarchar(10),a.date,127) = CONVERT(nvarchar(10),GETDATE(),127);";
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        #endregion

        #region LaptopInformation

        public static DataSet GetLaptopInformation()
        {
            DataSet ds = new DataSet();
            string sql = string.Empty;
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                sql = "select * from dbo.LaptopInformation where convert(nvarchar(10),date,127) = convert(nvarchar(10),GETDATE(),127) order by laptopNetwork, storeNo;";
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(sql, conn);
                da.SelectCommand = cmd;
                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        public static void DeleteLaptopInformation()
        {
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "truncate table dbo.LaptopInformation";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        #endregion

        #region IndexQueryInfo

        public static void DeleteIndexQuery()
        {
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                string sql = "truncate table dbo.IndexQuery";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }
        
        public static void InsertIndexQuery(IndexQuery iq)
        {
            SqlParameter[] paras = {
                                        new SqlParameter("@storeNo",iq.StoreNo),
                                        new SqlParameter("@storeRegion",iq.StoreRegion),
                                        new SqlParameter("@storeType",iq.StoreType),
                                        new SqlParameter("@routerIP",iq.RouterIP),
                                        new SqlParameter("@routerNetwork",iq.RouterNetwork),
                                        new SqlParameter("@printerIP",iq.PrinterIP),
                                        new SqlParameter("@printerNetwork",iq.PrinterNetwork),
                                        new SqlParameter("@printerType",iq.PrinterType),
                                        new SqlParameter("@tonerType",iq.TonerType),
                                        new SqlParameter("@printerStatus",iq.PrinterStatus),
                                        new SqlParameter("@tonerStatus",iq.TonerStatus),
                                        new SqlParameter("@laptopNetwork",iq.LaptopNetwork),
                                        new SqlParameter("@laptopIP",iq.LaptopIP)
                                   };
            SqlHelper.ExecuteNonQuery("InsertIndexQuery", paras);
        }

        public static DataSet GetIndexQuery()
        {
            DataSet ds = new DataSet();
            string sql = string.Empty;
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                sql = "select * from dbo.IndexQuery";
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(sql, conn);
                da.SelectCommand = cmd;
                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        #endregion

        #region EmailSend

        public static void SyncSendEmail()
        {
            SqlHelper.ExecuteNonQuery("SyncSendEmail", null);
        }

        // 更新所有邮件发送状态, 打印机墨水大于10%则isSend = 0
        public static void UpdateIsSend()
        {
            SqlHelper.ExecuteNonQuery("UpdateSendEmail", null);
        }

        // 邮件发送成功，isSend = 1 否则 isSend = 0
        public static void UpdateIsSend(SendEmail email)
        {
            string sql = string.Empty;
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                if (email.IsSend)
                {
                    sql = "update dbo.SendEmail set isSend=1, date=GETDATE() where storeStatus='900' and storeNo=@storeNo;";
                }
                else
                {
                    sql = "update dbo.SendEmail set isSend=0, date=GETDATE() where storeStatus='900' and storeNo=@storeNo;";
                }
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@storeNo", email.StoreNo);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static void UpdateIsSend(List<SendEmail> emailList)
        {
            string sql0 = "update dbo.SendEmail set isSend=0, date=GETDATE() where storeStatus='900' and storeNo in('',";
            string sql1 = "update dbo.SendEmail set isSend=1, date=GETDATE() where storeStatus='900' and storeNo in('',";

            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                foreach (SendEmail email in emailList)
                {
                    if (email.IsSend)
                    {
                        sql1 += "'" + email.StoreNo + "',";
                    }
                    else
                    {
                        sql0 += "'" + email.StoreNo + "',";
                    }
                }
                sql0 = sql0.TrimEnd(',') + ");";
                sql1 = sql1.TrimEnd(',') + ");";
                string sql = sql0 + sql1;
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }
        
        // 获取邮件发送状态
        public static bool GetEmailIsSend(string storeNo)
        {
            string sql = string.Empty;
            bool isSend = false;
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                sql = "select isSend from dbo.SendEmail where storeNo=@storeNo;";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@storeNo", storeNo);
                conn.Open();
                isSend = (bool)cmd.ExecuteScalar();
                conn.Close();
            }
            return isSend;
        }

        public static DataSet GetEmailIsSend(List<string> storesNo)
        {
            DataSet ds = new DataSet();
            string sql = string.Empty;
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                sql = "select e.storeNo, isSend, emailAddress, storeRegion from dbo.SendEmail e left join dbo.StoreInformation s on e.storeNo=s.storeNo  where e.storeNo in('',";
                foreach (string storeNo in storesNo)
                {
                    sql += "'" + storeNo + "',";
                }
                sql = sql.TrimEnd(',') + ") order by e.storeNo;";
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(sql, conn);
                da.SelectCommand = cmd;
                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        public static DataSet GetEmailAddress(List<string> storesNo)
        {
            DataSet ds = new DataSet();
            string sql = string.Empty;
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                sql = "select storeNo, storeRegion, emailAddress from dbo.StoreInformation where storeNo in('',";
                foreach (string storeNo in storesNo)
                {
                    sql += "'" + storeNo + "',";
                }
                sql = sql.TrimEnd(',') + ") order by storeNo;";
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(sql, conn);
                da.SelectCommand = cmd;
                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        public static DataSet GetEmailSendResult()
        {
            DataSet ds = new DataSet();
            string sql = string.Empty;
            using (SqlConnection conn = new SqlConnection(connLocal))
            {
                sql = "select p.storeNo, s.isSend from dbo.PrinterInformation p left join dbo.SendEmail s on p.storeNo=s.storeNo where convert(nvarchar(10),p.date,127) = convert(nvarchar(10),GETDATE(),127) and printerNetwork='Up' and (case when substring(tonerStatus,PATINDEX('%[0-9]%',tonerStatus),CHARINDEX('%',tonerStatus,1)-PATINDEX('%[0-9]%',tonerStatus))<>'' then CAST(substring(tonerStatus,PATINDEX('%[0-9]%',tonerStatus),CHARINDEX('%',tonerStatus,1)-PATINDEX('%[0-9]%',tonerStatus)) as int) else 999 end) <= 10 order by p.storeNo";
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(sql, conn);
                da.SelectCommand = cmd;
                conn.Open();
                da.Fill(ds);
                conn.Close();
            }
            return ds;
        }

        #endregion

        #region 报表

        public static DataSet TonerSumReport(string month, string year, bool isCurrent)
        {
            SqlParameter[] paras = { 
                                       new SqlParameter("@month", month),
                                       new SqlParameter("@year", year),
                                       new SqlParameter("@currentyear", isCurrent)                                   
                                   };
            return SqlHelper.ExecuteDataSet("TonerSumReport", paras);
        }

        #endregion
    }
}
