﻿using System;
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
                row["emailAddress"] = null;
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
    }
}
