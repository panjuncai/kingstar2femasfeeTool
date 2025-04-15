using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace kingstar2femasfee
{
    public class DatabaseHelper
    {
        private static string dbName = "kingstar2femasfee.db";
        private static string connectionString = $"Data Source={dbName};Version=3;";

        public static void InitializeDatabase()
        {
            if (!File.Exists(dbName))
            {
                SQLiteConnection.CreateFile(dbName);
                
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    
                    // 创建配置表
                    string createConfigTable = "CREATE TABLE IF NOT EXISTS T_CONFIG (id INTEGER PRIMARY KEY, femas_dir VARCHAR(100), kingstar_dir VARCHAR(100))";
                    using (SQLiteCommand command = new SQLiteCommand(createConfigTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    
                    // 创建交易所交易手续费表
                    string createExchangeTradeFeeTable = @"
                    CREATE TABLE IF NOT EXISTS T_EXCHANGE_TRADE_FEE (
                      id INTEGER PRIMARY KEY AUTOINCREMENT,
                      exch_code VARCHAR(1) NOT NULL,
                      product_type VARCHAR(1) NOT NULL,
                      product_id VARCHAR(10) NOT NULL,
                      option_series_id VARCHAR(30) NOT NULL,
                      instrument_id VARCHAR(30) NOT NULL,
                      hedge_flag VARCHAR(1) NOT NULL,
                      buy_sell VARCHAR(1) DEFAULT '*' NOT NULL,
                      open_fee_rate NUMERIC(17,8),
                      open_fee_amt NUMERIC(17,8),
                      short_open_fee_rate NUMERIC(17,8),
                      short_open_fee_amt NUMERIC(17,8),
                      offset_fee_rate NUMERIC(17,8),
                      offset_fee_amt NUMERIC(17,8),
                      ot_fee_rate NUMERIC(17,8),
                      ot_fee_amt NUMERIC(17,8),
                      exec_clear_fee_rate NUMERIC(17,8),
                      exec_clear_fee_amt NUMERIC(17,8),
                      oper_date VARCHAR(8),
                      oper_time VARCHAR(8)
                    )";
                    using (SQLiteCommand command = new SQLiteCommand(createExchangeTradeFeeTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    
                    // 创建唯一索引
                    string createIndex = @"
                    CREATE UNIQUE INDEX IF NOT EXISTS idx_EXCHANGE_TRADE_FEE 
                    ON T_EXCHANGE_TRADE_FEE (EXCH_CODE, PRODUCT_TYPE, HEDGE_FLAG, OPTION_SERIES_ID, PRODUCT_ID, INSTRUMENT_ID, BUY_SELL)";
                    using (SQLiteCommand command = new SQLiteCommand(createIndex, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    
                    // 初始化配置表插入一条默认数据
                    string insertInitData = "INSERT OR IGNORE INTO T_CONFIG (id, femas_dir, kingstar_dir) VALUES (1, '', '')";
                    using (SQLiteCommand command = new SQLiteCommand(insertInitData, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// 导入交易所手续费率数据
        /// </summary>
        /// <param name="dataList">交易所手续费率数据列表</param>
        /// <param name="logAction">日志记录方法</param>
        /// <returns>是否导入成功</returns>
        public static bool ImportExchangeTradeFeeData(List<ExchangeTradeFeeDO> dataList, ExcelHelper.LogMessageDelegate logAction)
        {
            if (dataList == null || dataList.Count == 0)
            {
                LogMessage(logAction, "没有数据需要导入");
                return false;
            }
            
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    
                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // 清空旧数据
                            string deleteSql = "DELETE FROM T_EXCHANGE_TRADE_FEE";
                            using (SQLiteCommand command = new SQLiteCommand(deleteSql, connection, transaction))
                            {
                                int rows = command.ExecuteNonQuery();
                                LogMessage(logAction, $"已清除原有交易所手续费率数据 {rows} 条");
                            }
                            
                            // 批量插入新数据
                            string insertSql = @"
                            INSERT INTO T_EXCHANGE_TRADE_FEE 
                            (exch_code, product_type, product_id, option_series_id, instrument_id, hedge_flag, buy_sell, 
                             open_fee_rate, open_fee_amt, short_open_fee_rate, short_open_fee_amt, 
                             offset_fee_rate, offset_fee_amt, ot_fee_rate, ot_fee_amt, 
                             exec_clear_fee_rate, exec_clear_fee_amt, oper_date, oper_time)
                            VALUES 
                            (@ExchCode, @ProductType, @ProductId, @OptionSeriesId, @InstrumentId, @HedgeFlag, @BuySell,
                             @OpenFeeRate, @OpenFeeAmt, @ShortOpenFeeRate, @ShortOpenFeeAmt,
                             @OffsetFeeRate, @OffsetFeeAmt, @OtFeeRate, @OtFeeAmt,
                             @ExecClearFeeRate, @ExecClearFeeAmt, @OperDate, @OperTime)";
                            
                            using (SQLiteCommand command = new SQLiteCommand(insertSql, connection, transaction))
                            {
                                // 创建参数
                                command.Parameters.Add(new SQLiteParameter("@ExchCode", System.Data.DbType.String));
                                command.Parameters.Add(new SQLiteParameter("@ProductType", System.Data.DbType.String));
                                command.Parameters.Add(new SQLiteParameter("@ProductId", System.Data.DbType.String));
                                command.Parameters.Add(new SQLiteParameter("@OptionSeriesId", System.Data.DbType.String));
                                command.Parameters.Add(new SQLiteParameter("@InstrumentId", System.Data.DbType.String));
                                command.Parameters.Add(new SQLiteParameter("@HedgeFlag", System.Data.DbType.String));
                                command.Parameters.Add(new SQLiteParameter("@BuySell", System.Data.DbType.String));
                                command.Parameters.Add(new SQLiteParameter("@OpenFeeRate", System.Data.DbType.Decimal));
                                command.Parameters.Add(new SQLiteParameter("@OpenFeeAmt", System.Data.DbType.Decimal));
                                command.Parameters.Add(new SQLiteParameter("@ShortOpenFeeRate", System.Data.DbType.Decimal));
                                command.Parameters.Add(new SQLiteParameter("@ShortOpenFeeAmt", System.Data.DbType.Decimal));
                                command.Parameters.Add(new SQLiteParameter("@OffsetFeeRate", System.Data.DbType.Decimal));
                                command.Parameters.Add(new SQLiteParameter("@OffsetFeeAmt", System.Data.DbType.Decimal));
                                command.Parameters.Add(new SQLiteParameter("@OtFeeRate", System.Data.DbType.Decimal));
                                command.Parameters.Add(new SQLiteParameter("@OtFeeAmt", System.Data.DbType.Decimal));
                                command.Parameters.Add(new SQLiteParameter("@ExecClearFeeRate", System.Data.DbType.Decimal));
                                command.Parameters.Add(new SQLiteParameter("@ExecClearFeeAmt", System.Data.DbType.Decimal));
                                command.Parameters.Add(new SQLiteParameter("@OperDate", System.Data.DbType.String));
                                command.Parameters.Add(new SQLiteParameter("@OperTime", System.Data.DbType.String));
                                
                                // 逐条插入数据
                                foreach (var data in dataList)
                                {
                                    command.Parameters["@ExchCode"].Value = data.ExchCode;
                                    command.Parameters["@ProductType"].Value = data.ProductType;
                                    command.Parameters["@ProductId"].Value = data.ProductId;
                                    command.Parameters["@OptionSeriesId"].Value = data.OptionSeriesId;
                                    command.Parameters["@InstrumentId"].Value = data.InstrumentId;
                                    command.Parameters["@HedgeFlag"].Value = data.HedgeFlag;
                                    command.Parameters["@BuySell"].Value = data.BuySell;
                                    command.Parameters["@OpenFeeRate"].Value = data.OpenFeeRate;
                                    command.Parameters["@OpenFeeAmt"].Value = data.OpenFeeAmt;
                                    command.Parameters["@ShortOpenFeeRate"].Value = data.ShortOpenFeeRate;
                                    command.Parameters["@ShortOpenFeeAmt"].Value = data.ShortOpenFeeAmt;
                                    command.Parameters["@OffsetFeeRate"].Value = data.OffsetFeeRate;
                                    command.Parameters["@OffsetFeeAmt"].Value = data.OffsetFeeAmt;
                                    command.Parameters["@OtFeeRate"].Value = data.OtFeeRate;
                                    command.Parameters["@OtFeeAmt"].Value = data.OtFeeAmt;
                                    command.Parameters["@ExecClearFeeRate"].Value = data.ExecClearFeeRate;
                                    command.Parameters["@ExecClearFeeAmt"].Value = data.ExecClearFeeAmt;
                                    command.Parameters["@OperDate"].Value = data.OperDate;
                                    command.Parameters["@OperTime"].Value = data.OperTime;
                                    
                                    command.ExecuteNonQuery();
                                }
                            }
                            
                            transaction.Commit();
                            LogMessage(logAction, $"成功导入交易所手续费率数据 {dataList.Count} 条");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            LogMessage(logAction, $"导入交易所手续费率数据失败: {ex.Message}");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage(logAction, $"操作数据库异常: {ex.Message}");
                return false;
            }
        }

        public static void SaveConfig(string femasDir, string kingstarDir)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string updateSql = "UPDATE T_CONFIG SET femas_dir = @FemasDir, kingstar_dir = @KingstarDir WHERE id = 1";
                    using (SQLiteCommand command = new SQLiteCommand(updateSql, connection))
                    {
                        command.Parameters.AddWithValue("@FemasDir", femasDir);
                        command.Parameters.AddWithValue("@KingstarDir", kingstarDir);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"保存配置失败: {ex.Message}", "错误", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        public static (string femasDir, string kingstarDir) LoadConfig()
        {
            string femasDir = string.Empty;
            string kingstarDir = string.Empty;
            
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string selectSql = "SELECT femas_dir, kingstar_dir FROM T_CONFIG WHERE id = 1";
                    using (SQLiteCommand command = new SQLiteCommand(selectSql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                femasDir = reader["femas_dir"].ToString();
                                kingstarDir = reader["kingstar_dir"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"加载配置失败: {ex.Message}", "错误", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            
            return (femasDir, kingstarDir);
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        private static void LogMessage(ExcelHelper.LogMessageDelegate logAction, string message)
        {
            if (logAction != null)
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}";
                logAction(logEntry);
            }
        }
    }
} 