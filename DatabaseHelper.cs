using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static kingstar2femasfee.Utils;

namespace kingstar2femasfee
{
    public class DatabaseHelper
    {
        private static string dbName = "kingstar2femasfee.db";
        private static string connectionString = $"Data Source={dbName};Version=3;";

        /// <summary>
        /// 初始化数据库
        /// </summary>
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

                    // 创建金士达特殊交易手续费表
                    string createKingstarSpecialTradeFeeTable = @"
                    CREATE TABLE IF NOT EXISTS T_SPECIAL_TRADE_FEE_KINGSTAR (
                      id INTEGER PRIMARY KEY AUTOINCREMENT,
                      investor_id VARCHAR(18) NOT NULL,
                      investor_name VARCHAR(100),
                      exch_code VARCHAR(1),
                      product_type VARCHAR(1) NOT NULL,
                      product_id VARCHAR(10) NOT NULL,
                      instrument_id VARCHAR(30) NOT NULL,
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
                    using (SQLiteCommand command = new SQLiteCommand(createKingstarSpecialTradeFeeTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // 创建金士达特殊交易手续费表唯一索引
                    string createKingstarSpecialIndex = @"
                    CREATE UNIQUE INDEX IF NOT EXISTS idx_SPECIAL_TRADE_FEE_KINGSTAR 
                    ON T_SPECIAL_TRADE_FEE_KINGSTAR (INVESTOR_ID, PRODUCT_TYPE, PRODUCT_ID, INSTRUMENT_ID)";
                    using (SQLiteCommand command = new SQLiteCommand(createKingstarSpecialIndex, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // 创建金士达特殊交易手续费浮动表
                    string createKingstarSpecialTradeFeeFloatTable = @"
                    CREATE TABLE IF NOT EXISTS T_SPECIAL_TRADE_FEE_KINGSTAR_FLOAT (
                      id INTEGER PRIMARY KEY AUTOINCREMENT,
                      investor_id VARCHAR(18) NOT NULL,
                      investor_name VARCHAR(100),
                      exch_code VARCHAR(1),
                      product_type VARCHAR(1) NOT NULL,
                      product_id VARCHAR(10) NOT NULL,
                      instrument_id VARCHAR(30) NOT NULL,
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
                      oper_date VARCHAR(8),
                      oper_time VARCHAR(8),
                      check_result VARCHAR(100),
                      check_code NUMBER(1)
                    )";
                    using (SQLiteCommand command = new SQLiteCommand(createKingstarSpecialTradeFeeFloatTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // 创建飞马特殊交易手续费导出表
                    string createFemasSpecialTradeFeeExportTable = @"
                    CREATE TABLE IF NOT EXISTS T_SPECIAL_TRADE_FEE_EXPORT (
                      id INTEGER PRIMARY KEY AUTOINCREMENT,
                      investor_id VARCHAR(18) NOT NULL,
                      investor_name VARCHAR(100),
                      exch_code VARCHAR(1),
                      product_type VARCHAR(1) NOT NULL,
                      product_id VARCHAR(10) NOT NULL,
                      instrument_id VARCHAR(30) NOT NULL,
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
                      open_fee_rate_new NUMERIC(17,8),
                      open_fee_amt_new NUMERIC(17,8),
                      short_open_fee_rate_new NUMERIC(17,8),
                      short_open_fee_amt_new NUMERIC(17,8),
                      offset_fee_rate_new NUMERIC(17,8),
                      offset_fee_amt_new NUMERIC(17,8),
                      ot_fee_rate_new NUMERIC(17,8),
                      ot_fee_amt_new NUMERIC(17,8),
                      exec_clear_fee_rate_new NUMERIC(17,8),
                      exec_clear_fee_amt_new NUMERIC(17,8),
                      follow_type_new VARCHAR(1),
                      oper_date VARCHAR(8),
                      oper_time VARCHAR(8),
                      check_result VARCHAR(100),
                      check_code NUMERIC(1)
                    )";
                    using (SQLiteCommand command = new SQLiteCommand(createFemasSpecialTradeFeeExportTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // 创建飞马特殊交易手续费导出表唯一索引
                    string createFemasSpecialTradeFeeExportIndex = @"
                    CREATE UNIQUE INDEX IF NOT EXISTS idx_SPECIAL_TRADE_FEE_EXPORT 
                    ON T_SPECIAL_TRADE_FEE_EXPORT (INVESTOR_ID, PRODUCT_TYPE, PRODUCT_ID, INSTRUMENT_ID)";
                    using (SQLiteCommand command = new SQLiteCommand(createFemasSpecialTradeFeeExportIndex, connection))
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
        /// 保存配置
        /// </summary>
        /// <param name="femasDir">femas目录</param>
        /// <param name="kingstarDir">金士达目录</param>
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

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <returns>femas目录, 金士达目录</returns>
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

        /// <summary>
        /// 导入交易所手续费率数据
        /// </summary>
        /// <param name="dataList">交易所手续费率数据列表</param>
        /// <param name="logAction">日志记录方法</param>
        /// <returns>是否导入成功</returns>
        public static bool ImportExchangeTradeFeeData(List<ExchangeTradeFeeDO> dataList, LogMessageDelegate logAction)
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

        /// <summary>
        /// 导入特殊交易手续费率数据
        /// </summary>
        /// <param name="dataList">特殊交易手续费率数据列表</param>
        /// <param name="logAction">日志记录方法</param>
        /// <returns>是否导入成功</returns>
        public static bool ImportSpecialTradeFeeData(List<SpecialTradeFeeDO> dataList, LogMessageDelegate logAction)
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
        /// 导入金士达客户手续费变更表数据
        /// </summary>
        /// <param name="dataList">金士达客户手续费变更表数据列表</param>
        /// <param name="logAction">日志记录方法</param>
        /// <returns>是否导入成功</returns>
        public static bool ImportKingstarSpecialTradeFeeData(List<KingstarSpecialTradeFeeDO> dataList, LogMessageDelegate logAction)
        {
            if (dataList == null || dataList.Count == 0)
            {
                LogMessage(logAction, "没有金士达客户手续费变更表数据需要导入");
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
                            string deleteSql = "DELETE FROM T_SPECIAL_TRADE_FEE_KINGSTAR";
                            using (SQLiteCommand command = new SQLiteCommand(deleteSql, connection, transaction))
                            {
                                int rows = command.ExecuteNonQuery();
                                LogMessage(logAction, $"已清除原有金士达客户手续费变更表数据 {rows} 条");
                            }

                            // 批量插入新数据
                            string insertSql = @"
                            INSERT INTO T_SPECIAL_TRADE_FEE_KINGSTAR 
                            (investor_id, investor_name, exch_code, product_type, product_id, instrument_id, 
                             open_fee_rate, open_fee_amt, short_open_fee_rate, short_open_fee_amt, 
                             offset_fee_rate, offset_fee_amt, ot_fee_rate, ot_fee_amt, 
                             exec_clear_fee_rate, exec_clear_fee_amt, oper_date, oper_time)
                            VALUES 
                            (@InvestorId, @InvestorName, @ExchCode, @ProductType, @ProductId, @InstrumentId,
                             @OpenFeeRate, @OpenFeeAmt, @ShortOpenFeeRate, @ShortOpenFeeAmt,
                             @OffsetFeeRate, @OffsetFeeAmt, @OtFeeRate, @OtFeeAmt,
                             @ExecClearFeeRate, @ExecClearFeeAmt, @OperDate, @OperTime)";

                            using (SQLiteCommand command = new SQLiteCommand(insertSql, connection, transaction))
                            {
                                // 创建参数
                                command.Parameters.Add(new SQLiteParameter("@InvestorId", System.Data.DbType.String));
                                command.Parameters.Add(new SQLiteParameter("@InvestorName", System.Data.DbType.String));
                                command.Parameters.Add(new SQLiteParameter("@ExchCode", System.Data.DbType.String));
                                command.Parameters.Add(new SQLiteParameter("@ProductType", System.Data.DbType.String));
                                command.Parameters.Add(new SQLiteParameter("@ProductId", System.Data.DbType.String));
                                command.Parameters.Add(new SQLiteParameter("@InstrumentId", System.Data.DbType.String));
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
                                    command.Parameters["@InvestorId"].Value = data.InvestorId;
                                    command.Parameters["@InvestorName"].Value = (object)data.InvestorName ?? DBNull.Value;
                                    command.Parameters["@ExchCode"].Value = (object)data.ExchCode ?? DBNull.Value;
                                    command.Parameters["@ProductType"].Value = data.ProductType;
                                    command.Parameters["@ProductId"].Value = data.ProductId;
                                    command.Parameters["@InstrumentId"].Value = data.InstrumentId;
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
                            LogMessage(logAction, $"成功导入金士达客户手续费变更表数据 {dataList.Count} 条");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            LogMessage(logAction, $"导入金士达客户手续费变更表数据失败: {ex.Message}");
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
        /// 转换金士达终值手续费为浮动手续费
        /// </summary>
        /// <param name="dataList">金士达客户手续费变更表数据列表</param>
        /// <param name="logAction">日志记录方法</param>
        /// <returns>是否导入成功</returns>
        public static bool ConvertKingstarSpecial2FloatData(LogMessageDelegate logAction)
        {

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
                            string deleteSql = "DELETE FROM T_SPECIAL_TRADE_FEE_KINGSTAR_FLOAT";
                            using (SQLiteCommand command = new SQLiteCommand(deleteSql, connection, transaction))
                            {
                                int rows = command.ExecuteNonQuery();
                                LogMessage(logAction, $"已清除原有金士达浮动手续费数据 {rows} 条");
                            }

                            // 转换浮动手续费率
                            string convertSql = @"insert into t_special_trade_fee_kingstar_float
                                                (investor_id   
                                                ,investor_name 
                                                ,exch_code     
                                                ,product_type  
                                                ,product_id    
                                                ,instrument_id 
                                                ,open_fee_rate      
                                                ,open_fee_amt       
                                                ,short_open_fee_rate
                                                ,short_open_fee_amt 
                                                ,offset_fee_rate    
                                                ,offset_fee_amt     
                                                ,ot_fee_rate        
                                                ,ot_fee_amt         
                                                ,exec_clear_fee_rate
                                                ,exec_clear_fee_amt 
                                                ,follow_type
                                                ,oper_date
                                                ,oper_time
                                                ,check_result
                                                ,check_code)
                                                SELECT
                                                a.investor_id   
                                                ,a.investor_name 
                                                ,a.exch_code     
                                                ,a.product_type  
                                                ,a.product_id    
                                                ,a.instrument_id 
                                                ,ROUND(a.open_fee_rate          -b.open_fee_rate,8) as  open_fee_rate      
                                                ,ROUND(a.open_fee_amt - b.open_fee_amt, 8) as open_fee_amt       
                                                ,ROUND(a.short_open_fee_rate   -b.short_open_fee_rate,8) as  short_open_fee_rate
                                                ,ROUND(a.short_open_fee_amt - b.short_open_fee_amt, 8) as short_open_fee_amt 
                                                ,ROUND(a.offset_fee_rate       -b.offset_fee_rate,8) as  offset_fee_rate    
                                                ,ROUND(a.offset_fee_amt - b.offset_fee_amt, 8) as offset_fee_amt     
                                                ,ROUND(a.ot_fee_rate - b.ot_fee_rate, 8) as  ot_fee_rate        
                                                ,ROUND(a.ot_fee_amt - b.ot_fee_amt, 8) as ot_fee_amt
                                                ,ROUND(a.exec_clear_fee_rate - b.exec_clear_fee_rate, 8) as exec_clear_fee_rate
                                                ,ROUND(a.exec_clear_fee_amt - b.exec_clear_fee_amt, 8) as exec_clear_fee_amt
                                                ,'1' as follow_type
                                                ,strftime('%Y%m%d', 'now') AS oper_date
                                                ,strftime('%H:%M:%S', 'now') AS oper_time
                                                ,'正确' as check_result
                                                ,0 as check_code
                                                FROM
                                                    t_special_trade_fee_kingstar a,
                                                    t_exchange_trade_fee b 
                                                WHERE
                                                    a.product_type = b.product_type 
                                                    AND a.product_id = b.product_id 
                                                    AND a.instrument_id = b.instrument_id";

                            using (SQLiteCommand command = new SQLiteCommand(convertSql, connection, transaction))
                            {
                                command.ExecuteNonQuery();
                            }

                            string checkOtherRecords = @"insert into t_special_trade_fee_kingstar_float
                                                        (investor_id   
                                                        ,investor_name 
                                                        ,exch_code     
                                                        ,product_type  
                                                        ,product_id    
                                                        ,instrument_id 
                                                        ,open_fee_rate      
                                                        ,open_fee_amt       
                                                        ,short_open_fee_rate
                                                        ,short_open_fee_amt 
                                                        ,offset_fee_rate    
                                                        ,offset_fee_amt     
                                                        ,ot_fee_rate        
                                                        ,ot_fee_amt         
                                                        ,exec_clear_fee_rate
                                                        ,exec_clear_fee_amt 
                                                        ,follow_type
                                                        ,oper_date
                                                        ,oper_time
                                                        ,check_result
                                                        ,check_code)
                                                        SELECT
                                                            a.investor_id,
                                                            a.investor_name,
                                                            a.exch_code,
                                                            a.product_type,
                                                            a.product_id,
                                                            a.instrument_id,
                                                            a.open_fee_rate,
                                                            a.open_fee_amt,
                                                            a.short_open_fee_rate,
                                                            a.short_open_fee_amt,
                                                            a.offset_fee_rate,
                                                            a.offset_fee_amt,
                                                            a.ot_fee_rate,
                                                            a.ot_fee_amt,
                                                            a.exec_clear_fee_rate,
                                                            a.exec_clear_fee_amt,
                                                            '0' AS follow_type,
                                                            strftime( '%Y%m%d', 'now' ) AS oper_date,
                                                            strftime( '%H:%M:%S', 'now' ) AS oper_time,
                                                            '未找到交易所跟随记录' AS check_result,
                                                            1 AS check_code 
                                                        FROM
                                                            T_SPECIAL_TRADE_FEE_KINGSTAR a 
                                                        WHERE
                                                            NOT EXISTS (
                                                            SELECT
                                                                1 
                                                            FROM
                                                                t_special_trade_fee_kingstar_float b 
                                                            WHERE
                                                                a.investor_id = b.investor_id 
                                                                AND a.product_type = b.product_type 
                                                                AND a.product_id = b.product_id 
                                                            AND a.instrument_id = b.instrument_id)";

                            using (SQLiteCommand command = new SQLiteCommand(checkOtherRecords, connection, transaction))
                            {
                                command.ExecuteNonQuery();
                            }

                            string checkNegativeRecords = @"UPDATE T_SPECIAL_TRADE_FEE_KINGSTAR_FLOAT 
                                                        SET check_result = '客户费率低于交易所',
                                                        check_code = 1 
                                                        WHERE
                                                            (
                                                                open_fee_rate < 0 
                                                                OR open_fee_amt < 0 
                                                                OR short_open_fee_rate < 0 
                                                                OR short_open_fee_amt < 0 
                                                                OR offset_fee_rate < 0 
                                                                OR offset_fee_amt < 0 
                                                                OR ot_fee_rate < 0 
                                                                OR ot_fee_amt < 0 
                                                                OR exec_clear_fee_rate < 0 
                                                            OR exec_clear_fee_amt < 0 
                                                            )";
                            using (SQLiteCommand command = new SQLiteCommand(checkNegativeRecords, connection, transaction))
                            {
                                command.ExecuteNonQuery();
                            }


                            transaction.Commit();
                            LogMessage(logAction, $"成功转换金士达终值手续费为浮动手续费");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            LogMessage(logAction, $"转换金士达终值手续费为浮动手续费失败: {ex.Message}");
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
        /// 转换导出数据
        /// </summary>
        /// <param name="dataList">飞马导出列表</param>
        /// <param name="logAction">日志记录方法</param>
        /// <returns>是否导入成功</returns>
        public static bool ConvertSpecial2ExportData(LogMessageDelegate logAction)
        {

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
                            string deleteSql = "DELETE FROM T_SPECIAL_TRADE_FEE_EXPORT";
                            using (SQLiteCommand command = new SQLiteCommand(deleteSql, connection, transaction))
                            {
                                int rows = command.ExecuteNonQuery();
                                LogMessage(logAction, $"已清除飞马导出数据 {rows} 条");
                            }

                            // 找出金士达多
                            string kingstarMoreSql = @"INSERT INTO T_SPECIAL_TRADE_FEE_EXPORT (
                                                investor_id,
                                                investor_name,
                                                exch_code,
                                                product_type,
                                                product_id,
                                                instrument_id,
                                                open_fee_rate_new,
                                                open_fee_amt_new,
                                                short_open_fee_rate_new,
                                                short_open_fee_amt_new,
                                                offset_fee_rate_new,
                                                offset_fee_amt_new,
                                                ot_fee_rate_new,
                                                ot_fee_amt_new,
                                                exec_clear_fee_rate_new,
                                                exec_clear_fee_amt_new,
                                                follow_type_new,
                                                oper_date,
                                                oper_time,
                                                check_result,
                                                check_code 
                                            ) SELECT
                                            a.investor_id,
                                            a.investor_name,
                                            a.exch_code,
                                            a.product_type,
                                            a.product_id,
                                            a.instrument_id,
                                            a.open_fee_rate,
                                            a.open_fee_amt,
                                            a.short_open_fee_rate,
                                            a.short_open_fee_amt,
                                            a.offset_fee_rate,
                                            a.offset_fee_amt,
                                            a.ot_fee_rate,
                                            a.ot_fee_amt,
                                            a.exec_clear_fee_rate,
                                            a.exec_clear_fee_amt,
                                            a.follow_type,
                                            strftime( '%Y%m%d', 'now' ) AS oper_date,
                                            strftime( '%H:%M:%S', 'now' ) AS oper_time,
                                            '金士达多' AS check_result,
                                            1 AS check_code 
                                            FROM
                                                T_SPECIAL_TRADE_FEE_KINGSTAR_FLOAT a 
                                            WHERE
                                                NOT EXISTS (
                                                SELECT
                                                    1 from T_SPECIAL_TRADE_FEE b 
                                                WHERE
                                                    a.investor_id = b.investor_id 
                                                    AND a.product_id = b.product_id 
                                                    AND a.product_type = b.product_type 
                                                AND a.instrument_id = b.instrument_id 
                                                )";

                            using (SQLiteCommand command = new SQLiteCommand(kingstarMoreSql, connection, transaction))
                            {
                                command.ExecuteNonQuery();
                            }

                            string femasMoreSql = @"INSERT INTO T_SPECIAL_TRADE_FEE_EXPORT (
                                                    investor_id,
                                                    exch_code,
                                                    product_type,
                                                    product_id,
                                                    instrument_id,
                                                    open_fee_rate_new,
                                                    open_fee_amt_new,
                                                    short_open_fee_rate_new,
                                                    short_open_fee_amt_new,
                                                    offset_fee_rate_new,
                                                    offset_fee_amt_new,
                                                    ot_fee_rate_new,
                                                    ot_fee_amt_new,
                                                    exec_clear_fee_rate_new,
                                                    exec_clear_fee_amt_new,
                                                    follow_type_new,
                                                    oper_date,
                                                    oper_time,
                                                    check_result,
                                                    check_code 
                                                ) SELECT
                                                a.investor_id,
                                                a.exch_code,
                                                a.product_type,
                                                a.product_id,
                                                a.instrument_id,
                                                a.open_fee_rate,
                                                a.open_fee_amt,
                                                a.short_open_fee_rate,
                                                a.short_open_fee_amt,
                                                a.offset_fee_rate,
                                                a.offset_fee_amt,
                                                a.ot_fee_rate,
                                                a.ot_fee_amt,
                                                a.exec_clear_fee_rate,
                                                a.exec_clear_fee_amt,
                                                a.follow_type,
                                                strftime( '%Y%m%d', 'now' ) AS oper_date,
                                                strftime( '%H:%M:%S', 'now' ) AS oper_time,
                                                '飞马多' AS check_result,
                                                1 AS check_code 
                                                FROM
                                                    T_SPECIAL_TRADE_FEE a 
                                                WHERE
                                                    NOT EXISTS (
                                                    SELECT
                                                        1 
                                                    FROM
                                                        T_SPECIAL_TRADE_FEE_KINGSTAR_FLOAT b 
                                                    WHERE
                                                        a.investor_id = b.investor_id 
                                                        AND a.product_id = b.product_id 
                                                        AND a.product_type = b.product_type 
                                                    AND a.instrument_id = b.instrument_id 
                                                    )";

                            using (SQLiteCommand command = new SQLiteCommand(femasMoreSql, connection, transaction))
                            {
                                command.ExecuteNonQuery();
                            }

                            string matchSql = @"INSERT INTO T_SPECIAL_TRADE_FEE_EXPORT (
                                                investor_id,
                                                investor_name,
                                                exch_code,
                                                product_type,
                                                product_id,
                                                instrument_id,
                                                open_fee_rate,
                                                open_fee_amt,
                                                short_open_fee_rate,
                                                short_open_fee_amt,
                                                offset_fee_rate,
                                                offset_fee_amt,
                                                ot_fee_rate,
                                                ot_fee_amt,
                                                exec_clear_fee_rate,
                                                exec_clear_fee_amt,
                                                follow_type,
                                                open_fee_rate_new,
                                                open_fee_amt_new,
                                                short_open_fee_rate_new,
                                                short_open_fee_amt_new,
                                                offset_fee_rate_new,
                                                offset_fee_amt_new,
                                                ot_fee_rate_new,
                                                ot_fee_amt_new,
                                                exec_clear_fee_rate_new,
                                                exec_clear_fee_amt_new,
                                                follow_type_new,
                                                oper_date,
                                                oper_time,
                                                check_result,
                                                check_code 
                                            ) SELECT
                                            a.investor_id,
                                            a.investor_name,
                                            a.exch_code,
                                            a.product_type,
                                            a.product_id,
                                            a.instrument_id,
                                            a.open_fee_rate,
                                            a.open_fee_amt,
                                            a.short_open_fee_rate,
                                            a.short_open_fee_amt,
                                            a.offset_fee_rate,
                                            a.offset_fee_amt,
                                            a.ot_fee_rate,
                                            a.ot_fee_amt,
                                            a.exec_clear_fee_rate,
                                            a.exec_clear_fee_amt,
                                            a.follow_type,
                                            b.open_fee_rate as open_fee_rate_new,
                                            b.open_fee_amt as open_fee_amt_new,
                                            b.short_open_fee_rate as short_open_fee_rate_new,
                                            b.short_open_fee_amt as short_open_fee_amt_new,
                                            b.offset_fee_rate as offset_fee_rate_new,
                                            b.offset_fee_amt as offset_fee_amt_new,
                                            b.ot_fee_rate as ot_fee_rate_new,
                                            b.ot_fee_amt as ot_fee_amt_new,
                                            b.exec_clear_fee_rate as exec_clear_fee_rate_new,
                                            b.exec_clear_fee_amt as exec_clear_fee_amt_new,
                                            b.follow_type as follow_type_new,
                                            strftime( '%Y%m%d', 'now' ) AS oper_date,
                                            strftime( '%H:%M:%S', 'now' ) AS oper_time,
                                            '匹配' AS check_result,
                                            0 AS check_code 
                                            FROM
                                                T_SPECIAL_TRADE_FEE_KINGSTAR_FLOAT a,
                                                T_SPECIAL_TRADE_FEE b 
                                            WHERE
                                                a.investor_id = b.investor_id 
                                                AND a.product_id = b.product_id 
                                                AND a.product_type = b.product_type 
                                                AND a.instrument_id = b.instrument_id";
                            using (SQLiteCommand command = new SQLiteCommand(matchSql, connection, transaction))
                            {
                                command.ExecuteNonQuery();
                            }

                            string updateNegtiveSql=@"UPDATE T_SPECIAL_TRADE_FEE_EXPORT 
                            SET 
                                open_fee_amt=CASE WHEN open_fee_amt<0 THEN 0 ELSE open_fee_amt END,
                                short_open_fee_amt=CASE WHEN short_open_fee_amt<0 THEN 0 ELSE short_open_fee_amt END,
                                offset_fee_amt=CASE WHEN offset_fee_amt<0 THEN 0 ELSE offset_fee_amt END,
                                ot_fee_amt=CASE WHEN ot_fee_amt<0 THEN 0 ELSE ot_fee_amt END,
                                exec_clear_fee_amt=CASE WHEN exec_clear_fee_amt<0 THEN 0 ELSE exec_clear_fee_amt END,
                                open_fee_rate=CASE WHEN open_fee_rate<0 THEN 0 ELSE open_fee_rate END,
                                short_open_fee_rate=CASE WHEN short_open_fee_rate<0 THEN 0 ELSE short_open_fee_rate END,
                                offset_fee_rate=CASE WHEN offset_fee_rate<0 THEN 0 ELSE offset_fee_rate END,
                                ot_fee_rate=CASE WHEN ot_fee_rate<0 THEN 0 ELSE ot_fee_rate END,
                                exec_clear_fee_rate=CASE WHEN exec_clear_fee_rate<0 THEN 0 ELSE exec_clear_fee_rate END,
                                open_fee_amt_new=CASE WHEN open_fee_amt_new<0 THEN 0 ELSE open_fee_amt_new END,
                                short_open_fee_amt_new=CASE WHEN short_open_fee_amt_new<0 THEN 0 ELSE short_open_fee_amt_new END,
                                offset_fee_amt_new=CASE WHEN offset_fee_amt_new<0 THEN 0 ELSE offset_fee_amt_new END,
                                ot_fee_amt_new=CASE WHEN ot_fee_amt_new<0 THEN 0 ELSE ot_fee_amt_new END,
                                exec_clear_fee_amt_new=CASE WHEN exec_clear_fee_amt_new<0 THEN 0 ELSE exec_clear_fee_amt_new END,
                                open_fee_rate_new=CASE WHEN open_fee_rate_new<0 THEN 0 ELSE open_fee_rate_new END,
                                short_open_fee_rate_new=CASE WHEN short_open_fee_rate_new<0 THEN 0 ELSE short_open_fee_rate_new END,
                                offset_fee_rate_new=CASE WHEN offset_fee_rate_new<0 THEN 0 ELSE offset_fee_rate_new END,
                                ot_fee_rate_new=CASE WHEN ot_fee_rate_new<0 THEN 0 ELSE ot_fee_rate_new END,
                                exec_clear_fee_rate_new=CASE WHEN exec_clear_fee_rate_new<0 THEN 0 ELSE exec_clear_fee_rate_new END";
                            using (SQLiteCommand command = new SQLiteCommand(updateNegtiveSql, connection, transaction))
                            {
                                command.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            LogMessage(logAction, $"成功转换飞马导出数据");
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            LogMessage(logAction, $"转换飞马导出数据失败: {ex.Message}");
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


        public static bool ProcessKingstarDbData(LogMessageDelegate logAction)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // 填充交易所代码
                            string updateSql = @"UPDATE T_SPECIAL_TRADE_FEE_KINGSTAR
                            SET exch_code = b.exch_code
                            FROM t_product b
                            WHERE T_SPECIAL_TRADE_FEE_KINGSTAR.product_type = b.product_type 
                            AND T_SPECIAL_TRADE_FEE_KINGSTAR.product_id = b.product_id";
                            using (SQLiteCommand command = new SQLiteCommand(updateSql, connection, transaction))
                            {
                                int rows = command.ExecuteNonQuery();
                                LogMessage(logAction, $"已填充交易所代码 {rows} 条");
                            }

                            // 大商所短开=平今
                            string updateDCESql = @"UPDATE T_SPECIAL_TRADE_FEE_KINGSTAR
                            SET short_open_fee_rate = ot_fee_rate,
                            short_open_fee_amt = ot_fee_amt
                            WHERE exch_code = 'D'";
                            using (SQLiteCommand command = new SQLiteCommand(updateDCESql, connection, transaction))
                            {
                                int rows = command.ExecuteNonQuery();
                                LogMessage(logAction, $"已填充大商所短开=平今 {rows} 条");
                            }

                            // 郑商所合约特殊处理
                            string updateZSESql = @"UPDATE T_SPECIAL_TRADE_FEE_KINGSTAR
                            SET instrument_id = CASE 
                                WHEN substr(instrument_id, 3, 1) = '2' 
                                THEN substr(instrument_id, 1, 2) || substr(instrument_id, 4)
                                ELSE instrument_id 
                            END
                            WHERE exch_code = 'Z'";
                            using (SQLiteCommand command = new SQLiteCommand(updateZSESql, connection, transaction))
                            {
                                int rows = command.ExecuteNonQuery();
                                LogMessage(logAction, $"已填充郑商所合约特殊处理 {rows} 条");
                            }

                            

                            transaction.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            LogMessage(logAction, $"填充交易所代码失败: {ex.Message}");
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
        /// 获取交易所手续费率数据
        /// </summary>
        /// <returns>交易所手续费率数据列表</returns>
        public static List<ExchangeTradeFeeDO> GetExchangeTradeFeeData()
        {
            List<ExchangeTradeFeeDO> resultList = new List<ExchangeTradeFeeDO>();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string selectSql = @"
                    SELECT 
                        exch_code, product_type, product_id, option_series_id, instrument_id, 
                        hedge_flag, buy_sell, open_fee_rate, open_fee_amt, 
                        short_open_fee_rate, short_open_fee_amt, offset_fee_rate, offset_fee_amt, 
                        ot_fee_rate, ot_fee_amt, exec_clear_fee_rate, exec_clear_fee_amt, 
                        oper_date, oper_time
                    FROM T_EXCHANGE_TRADE_FEE
                    ORDER BY exch_code, product_type, product_id, option_series_id, instrument_id, hedge_flag, buy_sell";

                    using (SQLiteCommand command = new SQLiteCommand(selectSql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = new ExchangeTradeFeeDO
                                {
                                    ExchCode = reader["exch_code"].ToString(),
                                    ProductType = reader["product_type"].ToString(),
                                    ProductId = reader["product_id"].ToString(),
                                    OptionSeriesId = reader["option_series_id"].ToString(),
                                    InstrumentId = reader["instrument_id"].ToString(),
                                    HedgeFlag = reader["hedge_flag"].ToString(),
                                    BuySell = reader["buy_sell"].ToString(),
                                    OpenFeeRate = Convert.ToDecimal(reader["open_fee_rate"]),
                                    OpenFeeAmt = Convert.ToDecimal(reader["open_fee_amt"]),
                                    ShortOpenFeeRate = Convert.ToDecimal(reader["short_open_fee_rate"]),
                                    ShortOpenFeeAmt = Convert.ToDecimal(reader["short_open_fee_amt"]),
                                    OffsetFeeRate = Convert.ToDecimal(reader["offset_fee_rate"]),
                                    OffsetFeeAmt = Convert.ToDecimal(reader["offset_fee_amt"]),
                                    OtFeeRate = Convert.ToDecimal(reader["ot_fee_rate"]),
                                    OtFeeAmt = Convert.ToDecimal(reader["ot_fee_amt"]),
                                    ExecClearFeeRate = Convert.ToDecimal(reader["exec_clear_fee_rate"]),
                                    ExecClearFeeAmt = Convert.ToDecimal(reader["exec_clear_fee_amt"]),
                                    OperDate = reader["oper_date"].ToString(),
                                    OperTime = reader["oper_time"].ToString()
                                };

                                resultList.Add(data);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取交易所手续费率数据异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return resultList;
        }

        /// <summary>
        /// 获取飞马特殊交易手续费率数据
        /// </summary>
        /// <returns>飞马特殊交易手续费率数据列表</returns>
        public static List<SpecialTradeFeeDO> GetSpecialTradeFeeData()
        {
            List<SpecialTradeFeeDO> resultList = new List<SpecialTradeFeeDO>();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string selectSql = @"
                    SELECT 
                        investor_id, exch_code, product_type, product_id, option_series_id, instrument_id, 
                        hedge_flag, buy_sell, open_fee_rate, open_fee_amt, 
                        short_open_fee_rate, short_open_fee_amt, offset_fee_rate, offset_fee_amt, 
                        ot_fee_rate, ot_fee_amt, exec_clear_fee_rate, exec_clear_fee_amt, 
                        follow_type, multiple_ratio, oper_date, oper_time
                    FROM T_SPECIAL_TRADE_FEE
                    ORDER BY investor_id, exch_code, product_type, product_id, option_series_id, instrument_id, hedge_flag, buy_sell";

                    using (SQLiteCommand command = new SQLiteCommand(selectSql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = new SpecialTradeFeeDO
                                {
                                    InvestorId = reader["investor_id"].ToString(),
                                    ExchCode = reader["exch_code"].ToString(),
                                    ProductType = reader["product_type"].ToString(),
                                    ProductId = reader["product_id"].ToString(),
                                    OptionSeriesId = reader["option_series_id"].ToString(),
                                    InstrumentId = reader["instrument_id"].ToString(),
                                    HedgeFlag = reader["hedge_flag"].ToString(),
                                    BuySell = reader["buy_sell"].ToString(),
                                    OpenFeeRate = Convert.ToDecimal(reader["open_fee_rate"]),
                                    OpenFeeAmt = Convert.ToDecimal(reader["open_fee_amt"]),
                                    ShortOpenFeeRate = Convert.ToDecimal(reader["short_open_fee_rate"]),
                                    ShortOpenFeeAmt = Convert.ToDecimal(reader["short_open_fee_amt"]),
                                    OffsetFeeRate = Convert.ToDecimal(reader["offset_fee_rate"]),
                                    OffsetFeeAmt = Convert.ToDecimal(reader["offset_fee_amt"]),
                                    OtFeeRate = Convert.ToDecimal(reader["ot_fee_rate"]),
                                    OtFeeAmt = Convert.ToDecimal(reader["ot_fee_amt"]),
                                    ExecClearFeeRate = Convert.ToDecimal(reader["exec_clear_fee_rate"]),
                                    ExecClearFeeAmt = Convert.ToDecimal(reader["exec_clear_fee_amt"]),
                                    FollowType = reader["follow_type"].ToString(),
                                    MultipleRatio = Convert.ToDecimal(reader["multiple_ratio"]),
                                    OperDate = reader["oper_date"].ToString(),
                                    OperTime = reader["oper_time"].ToString()
                                };

                                resultList.Add(data);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取飞马特殊交易手续费率数据异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return resultList;
        }

        /// <summary>
        /// 获取金士达客户特殊手续费率数据
        /// </summary>
        /// <returns>金士达客户特殊手续费率数据列表</returns>
        public static List<KingstarSpecialTradeFeeDO> GetKingstarSpecialTradeFeeData()
        {
            List<KingstarSpecialTradeFeeDO> resultList = new List<KingstarSpecialTradeFeeDO>();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string selectSql = @"
                    SELECT 
                        investor_id, investor_name, exch_code, product_type, product_id, instrument_id, 
                        open_fee_rate, open_fee_amt, short_open_fee_rate, short_open_fee_amt, 
                        offset_fee_rate, offset_fee_amt, ot_fee_rate, ot_fee_amt, 
                        exec_clear_fee_rate, exec_clear_fee_amt, oper_date, oper_time
                    FROM T_SPECIAL_TRADE_FEE_KINGSTAR
                    ORDER BY investor_id, product_type, product_id, instrument_id";

                    using (SQLiteCommand command = new SQLiteCommand(selectSql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = new KingstarSpecialTradeFeeDO
                                {
                                    InvestorId = reader["investor_id"].ToString(),
                                    InvestorName = reader["investor_name"].ToString(),
                                    ExchCode = reader["exch_code"].ToString(),
                                    ProductType = reader["product_type"].ToString(),
                                    ProductId = reader["product_id"].ToString(),
                                    InstrumentId = reader["instrument_id"].ToString(),
                                    OpenFeeRate = Convert.ToDecimal(reader["open_fee_rate"]),
                                    OpenFeeAmt = Convert.ToDecimal(reader["open_fee_amt"]),
                                    ShortOpenFeeRate = Convert.ToDecimal(reader["short_open_fee_rate"]),
                                    ShortOpenFeeAmt = Convert.ToDecimal(reader["short_open_fee_amt"]),
                                    OffsetFeeRate = Convert.ToDecimal(reader["offset_fee_rate"]),
                                    OffsetFeeAmt = Convert.ToDecimal(reader["offset_fee_amt"]),
                                    OtFeeRate = Convert.ToDecimal(reader["ot_fee_rate"]),
                                    OtFeeAmt = Convert.ToDecimal(reader["ot_fee_amt"]),
                                    ExecClearFeeRate = Convert.ToDecimal(reader["exec_clear_fee_rate"]),
                                    ExecClearFeeAmt = Convert.ToDecimal(reader["exec_clear_fee_amt"]),
                                    OperDate = reader["oper_date"].ToString(),
                                    OperTime = reader["oper_time"].ToString()
                                };

                                resultList.Add(data);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取金士达特殊手续费率数据异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return resultList;
        }

        /// <summary>
        /// 获取金士达客户特殊手续费率浮动数据
        /// </summary>
        /// <returns>金士达客户特殊手续费率浮动数据列表</returns>
        public static List<KingstarSpecialTradeFeeFloatDO> GetKingstarSpecialTradeFeeFloatData()
        {
            List<KingstarSpecialTradeFeeFloatDO> resultList = new List<KingstarSpecialTradeFeeFloatDO>();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string selectSql = @"
                    SELECT 
                        check_result,check_code,investor_id, investor_name, exch_code, product_type, product_id, instrument_id, 
                        open_fee_rate, open_fee_amt, short_open_fee_rate, short_open_fee_amt, 
                        offset_fee_rate, offset_fee_amt, ot_fee_rate, ot_fee_amt, 
                        exec_clear_fee_rate, exec_clear_fee_amt, follow_type, oper_date, oper_time
                    FROM T_SPECIAL_TRADE_FEE_KINGSTAR_FLOAT
                    ORDER BY check_code desc,check_result,investor_id, product_type, product_id, instrument_id";

                    using (SQLiteCommand command = new SQLiteCommand(selectSql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = new KingstarSpecialTradeFeeFloatDO
                                {
                                    CheckResult = reader["check_result"].ToString(),
                                    CheckCode = reader["check_code"].ToString(),
                                    InvestorId = reader["investor_id"].ToString(),
                                    InvestorName = reader["investor_name"].ToString(),
                                    ExchCode = reader["exch_code"].ToString(),
                                    ProductType = reader["product_type"].ToString(),
                                    ProductId = reader["product_id"].ToString(),
                                    InstrumentId = reader["instrument_id"].ToString(),
                                    OpenFeeRate = Convert.ToDecimal(reader["open_fee_rate"]),
                                    OpenFeeAmt = Convert.ToDecimal(reader["open_fee_amt"]),
                                    ShortOpenFeeRate = Convert.ToDecimal(reader["short_open_fee_rate"]),
                                    ShortOpenFeeAmt = Convert.ToDecimal(reader["short_open_fee_amt"]),
                                    OffsetFeeRate = Convert.ToDecimal(reader["offset_fee_rate"]),
                                    OffsetFeeAmt = Convert.ToDecimal(reader["offset_fee_amt"]),
                                    OtFeeRate = Convert.ToDecimal(reader["ot_fee_rate"]),
                                    OtFeeAmt = Convert.ToDecimal(reader["ot_fee_amt"]),
                                    ExecClearFeeRate = Convert.ToDecimal(reader["exec_clear_fee_rate"]),
                                    ExecClearFeeAmt = Convert.ToDecimal(reader["exec_clear_fee_amt"]),
                                    FollowType = reader["follow_type"].ToString(),
                                    OperDate = reader["oper_date"].ToString(),
                                    OperTime = reader["oper_time"].ToString()
                                };

                                resultList.Add(data);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取金士达特殊手续费率数据异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return resultList;
        }

        /// <summary>
        /// 获取飞马特殊交易手续费导出数据
        /// </summary>
        /// <returns>飞马特殊交易手续费导出数据列表</returns>
        public static List<SpecialTradeFeeExportDO> GetSpecialTradeFeeExportData()
        {
            List<SpecialTradeFeeExportDO> resultList = new List<SpecialTradeFeeExportDO>();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string selectSql = @"
                    SELECT 
                        check_result, check_code, investor_id, investor_name, exch_code, product_type, product_id, instrument_id, 
                        open_fee_rate, open_fee_amt, short_open_fee_rate, short_open_fee_amt, 
                        offset_fee_rate, offset_fee_amt, ot_fee_rate, ot_fee_amt, 
                        exec_clear_fee_rate, exec_clear_fee_amt, follow_type,
                        open_fee_rate_new, open_fee_amt_new, short_open_fee_rate_new, short_open_fee_amt_new,
                        offset_fee_rate_new, offset_fee_amt_new, ot_fee_rate_new, ot_fee_amt_new,
                        exec_clear_fee_rate_new, exec_clear_fee_amt_new, follow_type_new,
                        oper_date, oper_time
                    FROM T_SPECIAL_TRADE_FEE_EXPORT
                    ORDER BY check_code desc, check_result, investor_id, product_type, product_id, instrument_id";

                    using (SQLiteCommand command = new SQLiteCommand(selectSql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var data = new SpecialTradeFeeExportDO
                                {
                                    CheckResult = reader["check_result"].ToString(),
                                    CheckCode = reader["check_code"].ToString(),
                                    InvestorId = reader["investor_id"].ToString(),
                                    InvestorName = reader["investor_name"].ToString(),
                                    ExchCode = reader["exch_code"].ToString(),
                                    ProductType = reader["product_type"].ToString(),
                                    ProductId = reader["product_id"].ToString(),
                                    InstrumentId = reader["instrument_id"].ToString(),
                                    OpenFeeRate = reader["open_fee_rate"] == DBNull.Value ? null : (Nullable<decimal>)Convert.ToDecimal(reader["open_fee_rate"]),
                                    OpenFeeAmt = reader["open_fee_amt"] == DBNull.Value ? null : (Nullable<decimal>)Convert.ToDecimal(reader["open_fee_amt"]),
                                    ShortOpenFeeRate = reader["short_open_fee_rate"] == DBNull.Value ? null : (Nullable<decimal>)Convert.ToDecimal(reader["short_open_fee_rate"]),
                                    ShortOpenFeeAmt = reader["short_open_fee_amt"] == DBNull.Value ? null : (Nullable<decimal>)Convert.ToDecimal(reader["short_open_fee_amt"]),
                                    OffsetFeeRate = reader["offset_fee_rate"] == DBNull.Value ? null : (Nullable<decimal>)Convert.ToDecimal(reader["offset_fee_rate"]),
                                    OffsetFeeAmt = reader["offset_fee_amt"] == DBNull.Value ? null : (Nullable<decimal>)Convert.ToDecimal(reader["offset_fee_amt"]),
                                    OtFeeRate = reader["ot_fee_rate"] == DBNull.Value ? null : (Nullable<decimal>)Convert.ToDecimal(reader["ot_fee_rate"]),
                                    OtFeeAmt = reader["ot_fee_amt"] == DBNull.Value ? null : (Nullable<decimal>)Convert.ToDecimal(reader["ot_fee_amt"]),
                                    ExecClearFeeRate = reader["exec_clear_fee_rate"] == DBNull.Value ? null : (Nullable<decimal>)Convert.ToDecimal(reader["exec_clear_fee_rate"]),
                                    ExecClearFeeAmt = reader["exec_clear_fee_amt"] == DBNull.Value ? null : (Nullable<decimal>)Convert.ToDecimal(reader["exec_clear_fee_amt"]),
                                    FollowType = reader["follow_type"] == DBNull.Value ? "" : reader["follow_type"].ToString(),
                                    OpenFeeRateNew = reader["open_fee_rate_new"] == DBNull.Value ? null : (Nullable<decimal>)Convert.ToDecimal(reader["open_fee_rate_new"]),
                                    OpenFeeAmtNew = reader["open_fee_amt_new"] == DBNull.Value ? null : (Nullable<decimal>)Convert.ToDecimal(reader["open_fee_amt_new"]),
                                    ShortOpenFeeRateNew = reader["short_open_fee_rate_new"] == DBNull.Value ? null : (Nullable<decimal>)Convert.ToDecimal(reader["short_open_fee_rate_new"]),
                                    ShortOpenFeeAmtNew = reader["short_open_fee_amt_new"] == DBNull.Value ? null : (Nullable<decimal>)Convert.ToDecimal(reader["short_open_fee_amt_new"]),
                                    OffsetFeeRateNew = reader["offset_fee_rate_new"] == DBNull.Value ? null : (Nullable<decimal>)Convert.ToDecimal(reader["offset_fee_rate_new"]),
                                    OffsetFeeAmtNew = reader["offset_fee_amt_new"] == DBNull.Value ? null : (Nullable<decimal>)Convert.ToDecimal(reader["offset_fee_amt_new"]),
                                    OtFeeRateNew = reader["ot_fee_rate_new"] == DBNull.Value ? null : (Nullable<decimal>)Convert.ToDecimal(reader["ot_fee_rate_new"]),
                                    OtFeeAmtNew = reader["ot_fee_amt_new"] == DBNull.Value ? null : (Nullable<decimal>)Convert.ToDecimal(reader["ot_fee_amt_new"]),
                                    ExecClearFeeRateNew = reader["exec_clear_fee_rate_new"] == DBNull.Value ? null : (Nullable<decimal>)Convert.ToDecimal(reader["exec_clear_fee_rate_new"]),
                                    ExecClearFeeAmtNew = reader["exec_clear_fee_amt_new"] == DBNull.Value ? null : (Nullable<decimal>)Convert.ToDecimal(reader["exec_clear_fee_amt_new"]),
                                    FollowTypeNew = reader["follow_type_new"] == DBNull.Value ? "" : reader["follow_type_new"].ToString(),
                                    OperDate = reader["oper_date"].ToString(),
                                    OperTime = reader["oper_time"].ToString()
                                };

                                resultList.Add(data);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取飞马特殊交易手续费导出数据异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return resultList;
        }

        /// <summary>
        /// 清空所有数据表
        /// </summary>
        /// <param name="logAction">日志记录方法</param>
        /// <returns>是否成功清空</returns>
        public static bool ClearAllTables(LogMessageDelegate logAction)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    
                    // 清空交易所交易手续费表
                    using (SQLiteCommand command = new SQLiteCommand("DELETE FROM T_EXCHANGE_TRADE_FEE", connection))
                    {
                        int count = command.ExecuteNonQuery();
                        logAction?.Invoke($"已清空交易所交易手续费表，删除{count}条记录");
                    }
                    
                    // 清空特殊交易手续费表
                    using (SQLiteCommand command = new SQLiteCommand("DELETE FROM T_SPECIAL_TRADE_FEE", connection))
                    {
                        int count = command.ExecuteNonQuery();
                        logAction?.Invoke($"已清空特殊交易手续费表，删除{count}条记录");
                    }
                    
                    // 清空金士达特殊交易手续费表
                    using (SQLiteCommand command = new SQLiteCommand("DELETE FROM T_SPECIAL_TRADE_FEE_KINGSTAR", connection))
                    {
                        int count = command.ExecuteNonQuery();
                        logAction?.Invoke($"已清空金士达特殊交易手续费表，删除{count}条记录");
                    }
                    
                    // 清空金士达特殊交易手续费浮动表
                    using (SQLiteCommand command = new SQLiteCommand("DELETE FROM T_SPECIAL_TRADE_FEE_KINGSTAR_FLOAT", connection))
                    {
                        int count = command.ExecuteNonQuery();
                        logAction?.Invoke($"已清空金士达特殊交易手续费浮动表，删除{count}条记录");
                    }
                    
                    // 清空飞马特殊交易手续费导出表
                    using (SQLiteCommand command = new SQLiteCommand("DELETE FROM T_SPECIAL_TRADE_FEE_EXPORT", connection))
                    {
                        int count = command.ExecuteNonQuery();
                        logAction?.Invoke($"已清空飞马特殊交易手续费导出表，删除{count}条记录");
                    }
                    
                    // 不清空配置表和产品表，这些是基础数据
                    return true;
                }
            }
            catch (Exception ex)
            {
                logAction?.Invoke($"清空数据表时发生错误: {ex.Message}");
                return false;
            }
        }
    }
}