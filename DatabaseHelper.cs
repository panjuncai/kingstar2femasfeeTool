using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace kingstar2femasfee
{
    public class DatabaseHelper
    {
        private static string dbName = "kingstar2femasfee.db";
        private static string connectionString = $"Data Source={dbName};Version=3;";

        
        /// <summary>
        /// 产品数据对象
        /// </summary>
        public class ProductDO
        {
            public string ExchCode { get; set; }
            public string ProductType { get; set; }
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string UnderlyingId { get; set; }
            public decimal? UnderlyingMultiple { get; set; }
            public string OfferCurrency { get; set; }
            public string SettleCurrency { get; set; }
            public string IsSpecial { get; set; }
            public decimal? VolumeMultiple { get; set; }
            public string MarketId { get; set; }
            public string IsTradingRightSpecial { get; set; }
            public string UnderlyingType { get; set; }
        }

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

                    // 创建特殊交易手续费表
                    string createSpecialTradeFeeTable = @"
                    CREATE TABLE IF NOT EXISTS T_SPECIAL_TRADE_FEE (
                      id INTEGER PRIMARY KEY AUTOINCREMENT,
                      investor_id VARCHAR(18) NOT NULL,
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
                      follow_type VARCHAR(1),
                      multiple_ratio NUMERIC(17,8),
                      oper_date VARCHAR(8),
                      oper_time VARCHAR(8)
                    )";
                    using (SQLiteCommand command = new SQLiteCommand(createSpecialTradeFeeTable, connection))
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

                    // 创建特殊交易手续费唯一索引
                    string createSpecialIndex = @"
                    CREATE UNIQUE INDEX IF NOT EXISTS idx_SPECIAL_TRADE_FEE 
                    ON T_SPECIAL_TRADE_FEE (INVESTOR_ID, EXCH_CODE, PRODUCT_TYPE, PRODUCT_ID, OPTION_SERIES_ID, INSTRUMENT_ID, HEDGE_FLAG, BUY_SELL)";
                    using (SQLiteCommand command = new SQLiteCommand(createSpecialIndex, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // 创建产品表
                    string createProductTable = @"
                    CREATE TABLE IF NOT EXISTS T_PRODUCT (
                      exch_code VARCHAR(1) NOT NULL,
                      product_type VARCHAR(1) NOT NULL,
                      product_id VARCHAR(10) NOT NULL,
                      product_name VARCHAR(20),
                      underlying_id VARCHAR(20),
                      underlying_multiple NUMERIC(10),
                      offer_currency VARCHAR(3),
                      settle_currency VARCHAR(3),
                      is_special VARCHAR(1),
                      volume_multiple NUMERIC(10),
                      market_id VARCHAR(1),
                      is_traderight_special VARCHAR(1) DEFAULT '0',
                      underlying_type VARCHAR(1),
                      PRIMARY KEY (exch_code, product_type, product_id)
                    )";
                    using (SQLiteCommand command = new SQLiteCommand(createProductTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // 创建产品表唯一索引
                    string createProductIndex = @"
                    CREATE UNIQUE INDEX IF NOT EXISTS idx_PRODUCT 
                    ON T_PRODUCT (EXCH_CODE, PRODUCT_TYPE, PRODUCT_ID)";
                    using (SQLiteCommand command = new SQLiteCommand(createProductIndex, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // 初始化配置表插入一条默认数据
                    string insertInitData = "INSERT OR IGNORE INTO T_CONFIG (id, femas_dir, kingstar_dir) VALUES (1, '', '')";
                    using (SQLiteCommand command = new SQLiteCommand(insertInitData, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    string productDataFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "product_data.sql");
                    if (File.Exists(productDataFile))
                    {
                        DatabaseHelper.ImportProductData(productDataFile);
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
                logAction(message);
            }
        }

        /// <summary>
        /// 导入特殊交易手续费率数据
        /// </summary>
        /// <param name="dataList">特殊交易手续费率数据列表</param>
        /// <param name="logAction">日志记录方法</param>
        /// <returns>是否导入成功</returns>
        public static bool ImportSpecialTradeFeeData(List<SpecialTradeFeeDO> dataList, ExcelHelper.LogMessageDelegate logAction)
        {
            if (dataList == null || dataList.Count == 0)
            {
                LogMessage(logAction, "没有特殊交易手续费率数据需要导入");
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
                            string deleteSql = "DELETE FROM T_SPECIAL_TRADE_FEE";
                            using (SQLiteCommand command = new SQLiteCommand(deleteSql, connection, transaction))
                            {
                                int rows = command.ExecuteNonQuery();
                                LogMessage(logAction, $"已清除原有特殊交易手续费率数据 {rows} 条");
                            }
                            
                            // 批量插入新数据
                            string insertSql = @"
                            INSERT INTO T_SPECIAL_TRADE_FEE 
                            (investor_id, exch_code, product_type, product_id, option_series_id, instrument_id, hedge_flag, buy_sell, 
                             open_fee_rate, open_fee_amt, short_open_fee_rate, short_open_fee_amt, 
                             offset_fee_rate, offset_fee_amt, ot_fee_rate, ot_fee_amt, 
                             exec_clear_fee_rate, exec_clear_fee_amt, follow_type, multiple_ratio, oper_date, oper_time)
                            VALUES 
                            (@InvestorId, @ExchCode, @ProductType, @ProductId, @OptionSeriesId, @InstrumentId, @HedgeFlag, @BuySell,
                             @OpenFeeRate, @OpenFeeAmt, @ShortOpenFeeRate, @ShortOpenFeeAmt,
                             @OffsetFeeRate, @OffsetFeeAmt, @OtFeeRate, @OtFeeAmt,
                             @ExecClearFeeRate, @ExecClearFeeAmt, @FollowType, @MultipleRatio, @OperDate, @OperTime)";
                            
                            using (SQLiteCommand command = new SQLiteCommand(insertSql, connection, transaction))
                            {
                                // 创建参数
                                command.Parameters.Add(new SQLiteParameter("@InvestorId", System.Data.DbType.String));
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
                                command.Parameters.Add(new SQLiteParameter("@FollowType", System.Data.DbType.String));
                                command.Parameters.Add(new SQLiteParameter("@MultipleRatio", System.Data.DbType.Decimal));
                                command.Parameters.Add(new SQLiteParameter("@OperDate", System.Data.DbType.String));
                                command.Parameters.Add(new SQLiteParameter("@OperTime", System.Data.DbType.String));
                                
                                // 逐条插入数据
                                foreach (var data in dataList)
                                {
                                    command.Parameters["@InvestorId"].Value = data.InvestorId;
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
                                    command.Parameters["@FollowType"].Value = data.FollowType;
                                    command.Parameters["@MultipleRatio"].Value = data.MultipleRatio;
                                    command.Parameters["@OperDate"].Value = data.OperDate;
                                    command.Parameters["@OperTime"].Value = data.OperTime;
                                    
                                    command.ExecuteNonQuery();
                                }
                            }
                            
                            transaction.Commit();
                            LogMessage(logAction, $"成功导入特殊交易手续费率数据 {dataList.Count} 条");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            LogMessage(logAction, $"导入特殊交易手续费率数据失败: {ex.Message}");
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

        /// <summary>
        /// 导入产品数据
        /// </summary>
        /// <param name="sqlFilePath">SQL文件路径</param>
        /// <returns>是否导入成功</returns>
        public static bool ImportProductData(string sqlFilePath)
        {
            if (!File.Exists(sqlFilePath))
            {
                MessageBox.Show($"产品数据SQL文件不存在: {sqlFilePath}");
                return false;
            }
            
            try
            {
                // 读取SQL文件内容
                string sqlContent = File.ReadAllText(sqlFilePath, Encoding.UTF8);
                
                // 导入数据库
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    
                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // 清空旧数据
                            string deleteSql = "DELETE FROM T_PRODUCT";
                            using (SQLiteCommand command = new SQLiteCommand(deleteSql, connection, transaction))
                            {
                                int rows = command.ExecuteNonQuery();
                                // 使用MessageBox输出日志
                                //Console.WriteLine($"已清除原有产品数据 {rows} 条");
                            }
                            
                            // 按分号分割SQL语句
                            string[] sqlStatements = sqlContent.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            int successCount = 0;
                            
                            foreach (string sql in sqlStatements)
                            {
                                string trimmedSql = sql.Trim();
                                if (string.IsNullOrWhiteSpace(trimmedSql))
                                    continue;
                                
                                // 执行SQL语句
                                using (SQLiteCommand command = new SQLiteCommand(trimmedSql, connection, transaction))
                                {
                                    try
                                    {
                                        command.ExecuteNonQuery();
                                        successCount++;
                                    }
                                    catch (Exception ex)
                                    {
                                        // 使用MessageBox输出日志
                                        MessageBox.Show($"执行SQL语句失败: {ex.Message}\nSQL: {trimmedSql}");
                                    }
                                }
                            }
                            
                            transaction.Commit();
                            MessageBox.Show($"成功导入产品数据 {successCount} 条");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"导入产品数据失败: {ex.Message}");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取SQL文件异常: {ex.Message}");
                return false;
            }
        }
    }
} 