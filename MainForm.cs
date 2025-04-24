using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using static kingstar2femasfee.EnumHelper;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Text.RegularExpressions;

namespace kingstar2femasfee
{
    public partial class MainForm: Form
    {
        // 日志记录集合，用于倒序显示
        private List<string> logMessages = new List<string>();
        
        public MainForm()
        {
            InitializeComponent();
            DatabaseHelper.InitializeDatabase();
            LoadConfigFromDatabase();
            
            // 加载交易所手续费率数据
            RefreshExchangeFeeDataGridView();
            
            // 加载特殊交易手续费率数据
            RefreshSpecialFeeDataGridView();
            
            // 加载金士达特殊交易手续费率数据
            RefreshKingstarSpecialFeeDataGridView();

            // 加载金士达特殊交易手续费率浮动数据
            RefreshKingstarSpecialFeeFloatDataGridView();
            
            // 加载飞马特殊交易手续费导出数据
            RefreshSpecialTradeFeeExportDataGridView();
        }
        
        /// <summary>
        /// 从数据库加载配置
        /// </summary>
        private void LoadConfigFromDatabase()
        {
            var (femasDir, kingstarDir) = DatabaseHelper.LoadConfig();
            textBox_femas.Text = femasDir;
            textBox_kingstar.Text = kingstarDir;
        }
        
        /// <summary>
        /// 选择飞马费率目录按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_femas_dir_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "选择飞马费率目录";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox_femas.Text = folderDialog.SelectedPath;
                    SaveConfigToDatabase();
                }
            }
        }

        /// <summary>
        /// 选择金士达费率目录按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_kingstar_dir_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "选择金士达费率目录";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox_kingstar.Text = folderDialog.SelectedPath;
                    SaveConfigToDatabase();
                }
            }
        }
             
        /// <summary>
        /// 一键生成按钮点击事件
        /// </summary>
        private void Button_calc_Click(object sender, EventArgs e)
        {
            try
            {
                // 清空日志
                ClearLog();
                
                // 检查目录配置
                string femasDir = textBox_femas.Text.Trim();
                string kingstarDir = textBox_kingstar.Text.Trim();
                
                if (string.IsNullOrEmpty(femasDir))
                {
                    LogInfo("请选择飞马费率目录");
                    return;
                }
                
                if (string.IsNullOrEmpty(kingstarDir))
                {
                    LogInfo("请选择金士达费率目录");
                    return;
                }
                
                // 处理交易所手续费率数据
                if (!ProcessExchangeTradeFeeData(femasDir))
                {
                    return;
                }
                
                // 处理特殊交易手续费率数据
                if (!ProcessSpecialTradeFeeData(femasDir))
                {
                    return;
                }
                
                // 处理金士达客户交易手续费率数据
                if (!ProcessKingstarSpecialTradeFeeData(kingstarDir))
                {
                    return;
                }

                // 处理金士达客户交易手续费率浮动数据
                if (!ProcessKingstarSpecialTradeFeeFloatData())
                {
                    return;
                }

                // 处理飞马导出数据
                if(!ProcessSpecialTradeFeeExportData())
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                LogInfo($"处理过程中发生异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理交易所手续费率数据
        /// </summary>
        private bool ProcessExchangeTradeFeeData(string femasDir)
        {
            LogInfo("开始处理交易所手续费率数据...");
            
            // 读取Excel文件
            var (success, dataList) = ExcelHelper.ReadExchangeTradeFeeExcel(femasDir, LogInfo);
            
            // 如果读取不成功，立即返回，不执行后续代码
            if (!success)
            {
                LogInfo("交易所手续费率数据处理失败，请检查Excel文件");
                return false;
            }

            // 如果数据列表为空，立即返回
            if (dataList.Count == 0)
            {
                LogInfo("交易所手续费率数据为空，请检查Excel文件");
                return false;
            }
            
            LogInfo($"数据校验通过，准备导入 {dataList.Count} 条交易所手续费率数据");
            bool importSuccess = DatabaseHelper.ImportExchangeTradeFeeData(dataList, LogInfo);
            
            if (importSuccess)
            {
                LogInfo("交易所手续费率数据导入成功");
                // 刷新DataGridView显示
                RefreshExchangeFeeDataGridView();
            }
            else
            {
                LogInfo("交易所手续费率数据导入失败");
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// 刷新交易所手续费率DataGridView控件
        /// </summary>
        private void RefreshExchangeFeeDataGridView()
        {
            try
            {
                // 获取交易所手续费率数据
                List<ExchangeTradeFeeDO> dataList = DatabaseHelper.GetExchangeTradeFeeData();
                
                // 使用更友好的显示方式
                var displayData = dataList.Select(data => new {
                    交易所 = GetDescriptionByCode<ExchangeEnum>(data.ExchCode[0]),
                    产品类型 = GetDescriptionByCode<ProductTypeEnum>(data.ProductType[0]),
                    产品 = data.ProductId,
                    合约 = data.InstrumentId,
                    // 投保 = GetDescriptionByCode<HedgeFlagEnum>(data.HedgeFlag[0]),
                    // 买卖 = GetDescriptionByCode<BuySellEnum>(data.BuySell[0]),
                    开仓按金额 = data.OpenFeeRate,
                    开仓按手数 = data.OpenFeeAmt,
                    短开按金额 = data.ShortOpenFeeRate,
                    短开按手数 = data.ShortOpenFeeAmt,
                    平仓按金额 = data.OffsetFeeRate,
                    平仓按手数 = data.OffsetFeeAmt,
                    平今按金额 = data.OtFeeRate,
                    平今按手数 = data.OtFeeAmt,
                    行权按金额 = data.ExecClearFeeRate,
                    行权按手数 = data.ExecClearFeeAmt,
                    更新日期 = data.OperDate,
                    更新时间 = data.OperTime
                }).ToList();
                
                // 绑定数据源
                dataGridView_exchange_fee.DataSource = displayData;
                
                // 设置表格样式
                dataGridView_exchange_fee.BorderStyle = BorderStyle.None;
                // 去掉斑马纹，使用统一背景色
                dataGridView_exchange_fee.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
                dataGridView_exchange_fee.DefaultCellStyle.BackColor = Color.White;
                dataGridView_exchange_fee.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
                dataGridView_exchange_fee.DefaultCellStyle.SelectionForeColor = Color.Black;
                dataGridView_exchange_fee.EnableHeadersVisualStyles = false;
                dataGridView_exchange_fee.ColumnHeadersDefaultCellStyle.BackColor = Color.LightSteelBlue;
                dataGridView_exchange_fee.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                dataGridView_exchange_fee.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView_exchange_fee.Font, FontStyle.Bold);
                
                // 隐藏行号列（第一列）
                dataGridView_exchange_fee.RowHeadersVisible = false;
                
                // 设置小数列的格式，让数据更紧凑
                foreach (DataGridViewColumn column in dataGridView_exchange_fee.Columns)
                {
                    // 使所有列宽度紧凑但不隐藏数据
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
                
                // 最终调整，确保所有数据可见
                dataGridView_exchange_fee.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

                // 更新记录条数
                label_exchange_fee_count.Text = $"记录条数：{displayData.Count()}条";
                
                LogInfo($"已刷新交易所手续费率数据，共 {displayData.Count()} 条");
            }
            catch (Exception ex)
            {
                LogInfo($"刷新交易所手续费率数据异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 处理特殊交易手续费率数据
        /// </summary>
        private bool ProcessSpecialTradeFeeData(string femasDir)
        {
            LogInfo("开始处理特殊交易手续费率数据...");
            
            // 读取Excel文件
            var (success, dataList) = ExcelHelper.ReadSpecialTradeFeeExcel(femasDir, LogInfo);
            
            // 如果读取不成功，立即返回，不执行后续代码
            if (!success)
            {
                LogInfo("特殊交易手续费率数据处理失败，请检查Excel文件");
                return false;
            }
            
            // 如果数据列表为空，立即返回
            if (dataList.Count == 0)
            {
                LogInfo("特殊交易手续费率数据为空，请检查Excel文件");
                return false;
            }
            
            LogInfo($"数据校验通过，准备导入 {dataList.Count} 条特殊交易手续费率数据");
            bool importSuccess = DatabaseHelper.ImportSpecialTradeFeeData(dataList, LogInfo);
            
            if (importSuccess)
            {
                LogInfo("特殊交易手续费率数据导入成功");
                // 刷新特殊交易手续费率DataGridView显示
                RefreshSpecialFeeDataGridView();
                return true;
            }
            else
            {
                LogInfo("特殊交易手续费率数据导入失败");
                return false;
            }
        }
        
        /// <summary>
        /// 刷新特殊交易手续费率DataGridView控件
        /// </summary>
        private void RefreshSpecialFeeDataGridView()
        {
            try
            {
                // 获取特殊交易手续费率数据
                List<SpecialTradeFeeDO> dataList = DatabaseHelper.GetSpecialTradeFeeData();
                
                // 使用更友好的显示方式
                var displayData = dataList.Select(data => new {
                    客户号 = data.InvestorId,
                    交易所 = GetDescriptionByCode<ExchangeEnum>(data.ExchCode[0]),
                    产品类型 = GetDescriptionByCode<ProductTypeEnum>(data.ProductType[0]),
                    产品 = data.ProductId,
                    合约 = data.InstrumentId,
                    // 投保 = GetDescriptionByCode<HedgeFlagEnum>(data.HedgeFlag[0]),
                    // 买卖 = GetDescriptionByCode<BuySellEnum>(data.BuySell[0]),
                    开仓按金额 = data.OpenFeeRate,
                    开仓按手数 = data.OpenFeeAmt,
                    短开按金额 = data.ShortOpenFeeRate,
                    短开按手数 = data.ShortOpenFeeAmt,
                    平仓按金额 = data.OffsetFeeRate,
                    平仓按手数 = data.OffsetFeeAmt,
                    平今按金额 = data.OtFeeRate,
                    平今按手数 = data.OtFeeAmt,
                    行权按金额 = data.ExecClearFeeRate,
                    行权按手数 = data.ExecClearFeeAmt,
                    是否跟随 = GetDescriptionByCode<isFllowEnum>(data.FollowType[0]),
                    倍率 = data.MultipleRatio,
                    更新日期 = data.OperDate,
                    更新时间 = data.OperTime
                }).ToList();
                
                // 绑定数据源
                dataGridView_femas_special_fee.DataSource = displayData;
                
                // 设置表格样式
                dataGridView_femas_special_fee.BorderStyle = BorderStyle.None;
                // 去掉斑马纹，使用统一背景色
                dataGridView_femas_special_fee.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
                dataGridView_femas_special_fee.DefaultCellStyle.BackColor = Color.White;
                dataGridView_femas_special_fee.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
                dataGridView_femas_special_fee.DefaultCellStyle.SelectionForeColor = Color.Black;
                dataGridView_femas_special_fee.EnableHeadersVisualStyles = false;
                dataGridView_femas_special_fee.ColumnHeadersDefaultCellStyle.BackColor = Color.LightSteelBlue;
                dataGridView_femas_special_fee.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                dataGridView_femas_special_fee.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView_femas_special_fee.Font, FontStyle.Bold);
                
                // 隐藏行号列（第一列）
                dataGridView_femas_special_fee.RowHeadersVisible = false;
                
                // 设置小数列的格式，让数据更紧凑
                foreach (DataGridViewColumn column in dataGridView_femas_special_fee.Columns)
                {
                    // 使所有列宽度紧凑但不隐藏数据
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
                
                // 最终调整，确保所有数据可见
                dataGridView_femas_special_fee.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

                // 更新记录条数
                label_femas_special_fee_count.Text = $"记录条数：{displayData.Count()}条";
                
                LogInfo($"已刷新飞马特殊交易手续费率数据，共 {displayData.Count()} 条");
            }
            catch (Exception ex)
            {
                LogInfo($"刷新飞马特殊交易手续费率数据异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 处理金士达客户交易手续费率数据
        /// </summary>
        private bool ProcessKingstarSpecialTradeFeeData(string kingstarDir)
        {
            LogInfo("开始处理金士达客户交易手续费率数据（期货）...");
            
            // 读取Excel文件
            var (success, dataList) = ExcelHelper.ReadKingstarSpecialTradeFeeExcel(kingstarDir, LogInfo);
            
            // 如果读取不成功，立即返回，不执行后续代码
            if (!success)
            {
                LogInfo("金士达客户交易手续费率数据(期货)处理失败，请检查Excel文件");
                return false;
            }
            
            LogInfo("开始处理金士达客户交易手续费率数据（期权）...");
            var (successOptions, dataListOptions) = ExcelHelper.ReadKingstarSpecialTradeFeeExcelOptions(kingstarDir, LogInfo);
            
            // 如果读取不成功，立即返回，不执行后续代码
            if (!successOptions)
            {
                LogInfo("金士达客户交易手续费率数据(期权)处理失败，请检查Excel文件");
                return false;
            }

            // 合并数据
            dataList = dataList.Concat(dataListOptions).ToList();
            
            // 如果合并后的数据列表为空，立即返回
            if (dataList.Count == 0)
            {
                LogInfo("金士达客户交易手续费率数据为空，请检查Excel文件");
                return false;
            }
            
            LogInfo($"数据校验通过，准备导入 {dataList.Count} 条金士达客户交易手续费率数据");
            bool importSuccess = DatabaseHelper.ImportKingstarSpecialTradeFeeData(dataList, LogInfo);
            
            if (importSuccess)
            {
                LogInfo("金士达客户交易手续费率数据导入成功");
                // 刷新金士达特殊手续费率DataGridView控件
                RefreshKingstarSpecialFeeDataGridView();
            }
            else
            {
                LogInfo("金士达客户交易手续费率数据导入失败");
                return false;
            }

            // 填充交易所代码
            bool processDataSuccess=DatabaseHelper.ProcessKingstarDbData(LogInfo);
            if (!processDataSuccess)
            {
                LogInfo("金士达客户交易手续费率数据填充交易所代码失败");
                return false;
            }
            else
            {
                LogInfo("金士达客户交易手续费率数据填充交易所代码成功");
            }
            
            return true;
        }

        /// <summary>
        /// 处理金士达客户交易手续费率浮动数据
        /// </summary>
        private bool ProcessKingstarSpecialTradeFeeFloatData()
        {
            LogInfo("开始处理金士达客户交易手续费率浮动数据...");
            bool success = DatabaseHelper.ConvertKingstarSpecial2FloatData(LogInfo);
            if (success)
            {
                LogInfo("金士达客户交易手续费率浮动数据处理成功");
                // 刷新金士达浮动DataGridView显示
                RefreshKingstarSpecialFeeFloatDataGridView();
                
                return true;
            }
            else
            {
                LogInfo("金士达客户交易手续费率浮动数据处理失败");
                return false;
            }
        }
        

        /// <summary>
        /// 处理导出手续费率
        /// </summary>
        private bool ProcessSpecialTradeFeeExportData()
        {
            LogInfo("开始处理导出数据...");
            bool success = DatabaseHelper.ConvertSpecial2ExportData(LogInfo);
            if (success)
            {
                LogInfo("导出数据处理成功");
                // 刷新飞马特殊交易手续费导出DataGridView显示
                RefreshSpecialTradeFeeExportDataGridView();
                
                return true;
            }
            else
            {
                LogInfo("导出数据处理失败");
                return false;
            }
        }

        /// <summary>
        /// 加载飞马特殊交易手续费导出数据
        /// </summary>
        private void RefreshSpecialTradeFeeExportDataGridView()
        {
            try
            {
                // 获取飞马特殊交易手续费导出数据
                List<SpecialTradeFeeExportDO> dataList = DatabaseHelper.GetSpecialTradeFeeExportData();
                
                // 使用更友好的显示方式
                var displayData = dataList.Select(data =>
                new {
                    检查结果 = data.CheckResult,
                    客户号 = data.InvestorId,
                    客户名称 = data.InvestorName,
                    交易所 = GetDescriptionByCode<ExchangeEnum>(data.ExchCode[0]),
                    产品类型 = GetDescriptionByCode<ProductTypeEnum>(data.ProductType[0]),
                    产品 = data.ProductId,
                    合约 = data.InstrumentId,
                    原开仓按金额 = data.OpenFeeRate,
                    新开仓按金额=data.OpenFeeRateNew,
                    原开仓按手数 = data.OpenFeeAmt,
                    新开仓按手数=data.OpenFeeAmtNew,
                    原短开按金额 = data.ShortOpenFeeRate,
                    新短开按金额 = data.ShortOpenFeeRateNew,
                    原短开按手数 = data.ShortOpenFeeAmt,
                    新短开按手数 = data.ShortOpenFeeAmtNew,
                    原平仓按金额 = data.OffsetFeeRate,
                    新平仓按金额 = data.OffsetFeeRateNew,
                    原平仓按手数 = data.OffsetFeeAmt,
                    新平仓按手数 = data.OffsetFeeAmtNew,
                    原平今按金额 = data.OtFeeRate,
                    新平今按金额 = data.OtFeeRateNew,
                    原平今按手数 = data.OtFeeAmt,
                    新平今按手数 = data.OtFeeAmtNew,
                    原行权按金额 = data.ExecClearFeeRate,
                    新行权按金额 = data.ExecClearFeeRateNew,
                    原行权按手数 = data.ExecClearFeeAmt,
                    新行权按手数 = data.ExecClearFeeAmtNew,
                    原是否跟随 =!string.IsNullOrEmpty(data.FollowType) ? GetDescriptionByCode<isFllowEnum>(data.FollowType[0]) : "", 
                    新是否跟随 =!string.IsNullOrEmpty(data.FollowTypeNew) ? GetDescriptionByCode<isFllowEnum>(data.FollowTypeNew[0]) : "",
                    更新日期 = data.OperDate,
                    更新时间 = data.OperTime,
                    结果代码 = data.CheckCode
                }).ToList();
                
                // 绑定数据源
                dataGridView_femas_special_fee_export.DataSource = displayData;
                
                // 设置表格样式
                dataGridView_femas_special_fee_export.BorderStyle = BorderStyle.None;
                // 去掉斑马纹，使用统一背景色
                dataGridView_femas_special_fee_export.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
                dataGridView_femas_special_fee_export.DefaultCellStyle.BackColor = Color.White;
                dataGridView_femas_special_fee_export.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
                dataGridView_femas_special_fee_export.DefaultCellStyle.SelectionForeColor = Color.Black;
                dataGridView_femas_special_fee_export.EnableHeadersVisualStyles = false;
                dataGridView_femas_special_fee_export.ColumnHeadersDefaultCellStyle.BackColor = Color.LightSteelBlue;
                dataGridView_femas_special_fee_export.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                dataGridView_femas_special_fee_export.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView_femas_special_fee_export.Font, FontStyle.Bold);
                
                // 隐藏行号列（第一列）
                dataGridView_femas_special_fee_export.RowHeadersVisible = false;
                
                // 设置小数列的格式，让数据更紧凑
                foreach (DataGridViewColumn column in dataGridView_femas_special_fee_export.Columns)
                {
                    // 使所有列宽度紧凑但不隐藏数据
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
                
                // 最终调整，确保所有数据可见
                dataGridView_femas_special_fee_export.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
                
                // 更新记录条数
                label_export_count.Text = $"记录条数：{displayData.Count()}条";
                
                // 设置检查结果单元格样式
                if (dataGridView_femas_special_fee_export.Columns["检查结果"] != null)
                {
                    int checkResultColumnIndex = dataGridView_femas_special_fee_export.Columns["检查结果"].Index;
                    
                    // 遍历所有行设置检查结果单元格样式
                    foreach (DataGridViewRow row in dataGridView_femas_special_fee_export.Rows)
                    {
                        if (row.Cells[checkResultColumnIndex].Value != null && 
                            row.Cells[checkResultColumnIndex].Value.ToString() != "正确" &&
                            row.Cells[checkResultColumnIndex].Value.ToString() != "匹配")
                        {
                            // 只设置检查结果单元格的样式
                            row.Cells[checkResultColumnIndex].Style.ForeColor = Color.Red;
                            row.Cells[checkResultColumnIndex].Style.Font = new Font(dataGridView_femas_special_fee_export.Font, FontStyle.Bold);
                        }
                    }
                }
                
                LogInfo($"已刷新飞马特殊交易手续费导出数据，共 {displayData.Count()} 条");
            }
            catch (Exception ex)
            {
                LogInfo($"刷新飞马特殊交易手续费导出数据异常: {ex.Message}");
            }
        }

        private void SaveConfigToDatabase()
        {
            DatabaseHelper.SaveConfig(textBox_femas.Text, textBox_kingstar.Text);
            LogInfo("配置已保存到数据库");
        }

        /// <summary>
        /// 记录日志信息
        /// </summary>
        private void LogInfo(string message)
        {
            
            // 添加时间戳
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}";
            
            // 添加到日志集合，同时记录日志级别
            logMessages.Insert(0, logEntry);
            
            // 更新日志文本框
            UpdateLogTextBox();
        }
        
        /// <summary>
        /// 清空日志
        /// </summary>
        private void ClearLog()
        {
            logMessages.Clear();
            textBox_log.Clear();
        }
        
        /// <summary>
        /// 更新日志文本框
        /// </summary>
        private void UpdateLogTextBox()
        {
            // 确保textBox_log是RichTextBox类型
            if (!(textBox_log is RichTextBox richTextBox))
            {
                // 如果不是RichTextBox，则使用普通方式更新
                StringBuilder sb = new StringBuilder();
                foreach (string log in logMessages)
                {
                    sb.AppendLine(log);
                }
                textBox_log.Text = sb.ToString();
                return;
            }
            
            // 记住最初的滚动位置
            int initialScrollValue = richTextBox.GetPositionFromCharIndex(0).Y;
            
            // 清空当前内容
            richTextBox.Clear();
            
            // 设置默认格式
            richTextBox.SelectionFont = new System.Drawing.Font(richTextBox.Font, System.Drawing.FontStyle.Regular);
            richTextBox.SelectionColor = System.Drawing.Color.Black;
            
            // 逐行添加日志，并根据日志内容设置格式
            foreach (string log in logMessages)
            {
                int startIndex = richTextBox.TextLength;
                richTextBox.AppendText(log + Environment.NewLine);
                
                // 检查是否包含错误关键词，如果包含则设置为红色加粗
                if (log.Contains("失败") || log.Contains("未通过") || log.Contains("错误") || log.Contains("异常") || log.Contains("不完整"))
                {
                    richTextBox.SelectionStart = startIndex;
                    richTextBox.SelectionLength = log.Length;
                    richTextBox.SelectionFont = new System.Drawing.Font(richTextBox.Font, System.Drawing.FontStyle.Bold);
                    richTextBox.SelectionColor = System.Drawing.Color.Red;
                }
                // 警告信息设置为橙色
                else if (log.Contains("警告") || log.Contains("注意"))
                {
                    richTextBox.SelectionStart = startIndex;
                    richTextBox.SelectionLength = log.Length;
                    richTextBox.SelectionFont = new System.Drawing.Font(richTextBox.Font, System.Drawing.FontStyle.Bold);
                    richTextBox.SelectionColor = System.Drawing.Color.Orange;
                }
            }
            
            // 滚动到最新的日志（顶部）
            richTextBox.SelectionStart = 0;
            richTextBox.SelectionLength = 0;
            richTextBox.ScrollToCaret();
        }

        /// <summary>
        /// 刷新金士达特殊手续费率DataGridView控件
        /// </summary>
        private void RefreshKingstarSpecialFeeDataGridView()
        {
            try
            {
                // 获取金士达特殊手续费率数据
                List<KingstarSpecialTradeFeeDO> dataList = DatabaseHelper.GetKingstarSpecialTradeFeeData();
                
                // 使用更友好的显示方式
                var displayData = dataList.Select(data => new {
                    客户号 = data.InvestorId,
                    客户名称 = data.InvestorName,
                    交易所 = GetDescriptionByCode<ExchangeEnum>(data.ExchCode[0]),
                    产品类型 = GetDescriptionByCode<ProductTypeEnum>(data.ProductType[0]),
                    产品 = data.ProductId,
                    合约 = data.InstrumentId,
                    开仓按金额 = data.OpenFeeRate,
                    开仓按手数 = data.OpenFeeAmt,
                    短开按金额 = data.ShortOpenFeeRate,
                    短开按手数 = data.ShortOpenFeeAmt,
                    平仓按金额 = data.OffsetFeeRate,
                    平仓按手数 = data.OffsetFeeAmt,
                    平今按金额 = data.OtFeeRate,
                    平今按手数 = data.OtFeeAmt,
                    行权按金额 = data.ExecClearFeeRate,
                    行权按手数 = data.ExecClearFeeAmt,
                    更新日期 = data.OperDate,
                    更新时间 = data.OperTime
                }).ToList();
                
                // 绑定数据源
                dataGridView_kingstar_special_fee.DataSource = displayData;
                
                // 设置表格样式
                dataGridView_kingstar_special_fee.BorderStyle = BorderStyle.None;
                // 去掉斑马纹，使用统一背景色
                dataGridView_kingstar_special_fee.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
                dataGridView_kingstar_special_fee.DefaultCellStyle.BackColor = Color.White;
                dataGridView_kingstar_special_fee.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
                dataGridView_kingstar_special_fee.DefaultCellStyle.SelectionForeColor = Color.Black;
                dataGridView_kingstar_special_fee.EnableHeadersVisualStyles = false;
                dataGridView_kingstar_special_fee.ColumnHeadersDefaultCellStyle.BackColor = Color.LightSteelBlue;
                dataGridView_kingstar_special_fee.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                dataGridView_kingstar_special_fee.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView_kingstar_special_fee.Font, FontStyle.Bold);
                
                // 隐藏行号列（第一列）
                dataGridView_kingstar_special_fee.RowHeadersVisible = false;
                
                // 设置小数列的格式，让数据更紧凑
                foreach (DataGridViewColumn column in dataGridView_kingstar_special_fee.Columns)
                {
                    // 使所有列宽度紧凑但不隐藏数据
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
                
                // 最终调整，确保所有数据可见
                dataGridView_kingstar_special_fee.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
                
                // 更新记录条数
                label_kingstar_special_fee_count.Text = $"记录条数：{displayData.Count()}条";
                LogInfo($"已刷新金士达客户特殊手续费率数据，共 {displayData.Count()} 条");
            }
            catch (Exception ex)
            {
                LogInfo($"刷新金士达客户特殊手续费率数据异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 刷新金士达特殊手续费率浮动DataGridView控件
        /// </summary>
        private void RefreshKingstarSpecialFeeFloatDataGridView()
        {
            try
            {
                // 获取金士达特殊手续费率浮动数据
                List<KingstarSpecialTradeFeeFloatDO> dataList = DatabaseHelper.GetKingstarSpecialTradeFeeFloatData();
                
                // 创建DataTable以支持RowPrePaint事件
                System.Data.DataTable dt = new System.Data.DataTable();
                
                // 添加列
                dt.Columns.Add("检查结果", typeof(string));
                dt.Columns.Add("客户号", typeof(string));
                dt.Columns.Add("客户名称", typeof(string));
                dt.Columns.Add("交易所", typeof(string));
                dt.Columns.Add("产品类型", typeof(string));
                dt.Columns.Add("产品", typeof(string));
                dt.Columns.Add("合约", typeof(string));
                dt.Columns.Add("开仓按金额", typeof(decimal));
                dt.Columns.Add("开仓按手数", typeof(decimal));
                dt.Columns.Add("短开按金额", typeof(decimal));
                dt.Columns.Add("短开按手数", typeof(decimal));
                dt.Columns.Add("平仓按金额", typeof(decimal));
                dt.Columns.Add("平仓按手数", typeof(decimal));
                dt.Columns.Add("平今按金额", typeof(decimal));
                dt.Columns.Add("平今按手数", typeof(decimal));
                dt.Columns.Add("行权按金额", typeof(decimal));
                dt.Columns.Add("行权按手数", typeof(decimal));
                dt.Columns.Add("是否跟随", typeof(string));
                dt.Columns.Add("更新日期", typeof(string));
                dt.Columns.Add("更新时间", typeof(string));
                dt.Columns.Add("结果代码", typeof(int));
                
                // 添加数据行
                foreach (var data in dataList)
                {
                    var row = dt.NewRow();
                    row["检查结果"] = data.CheckResult;
                    row["客户号"] = data.InvestorId;
                    row["客户名称"] = data.InvestorName;
                    row["交易所"] = GetDescriptionByCode<ExchangeEnum>(data.ExchCode[0]);
                    row["产品类型"] = GetDescriptionByCode<ProductTypeEnum>(data.ProductType[0]);
                    row["产品"] = data.ProductId;
                    row["合约"] = data.InstrumentId;
                    row["开仓按金额"] = data.OpenFeeRate;
                    row["开仓按手数"] = data.OpenFeeAmt;
                    row["短开按金额"] = data.ShortOpenFeeRate;
                    row["短开按手数"] = data.ShortOpenFeeAmt;
                    row["平仓按金额"] = data.OffsetFeeRate;
                    row["平仓按手数"] = data.OffsetFeeAmt;
                    row["平今按金额"] = data.OtFeeRate;
                    row["平今按手数"] = data.OtFeeAmt;
                    row["行权按金额"] = data.ExecClearFeeRate;
                    row["行权按手数"] = data.ExecClearFeeAmt;
                    row["是否跟随"] = GetDescriptionByCode<isFllowEnum>(data.FollowType[0]);
                    row["更新日期"] = data.OperDate;
                    row["更新时间"] = data.OperTime;
                    row["结果代码"] = data.CheckCode;
                    
                    dt.Rows.Add(row);
                }
                
                // 绑定数据源
                dataGridView_kingstar_special_fee_float.DataSource = dt;
                
                // 设置表格样式
                dataGridView_kingstar_special_fee_float.BorderStyle = BorderStyle.None;
                // 去掉斑马纹，使用统一背景色
                dataGridView_kingstar_special_fee_float.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
                dataGridView_kingstar_special_fee_float.DefaultCellStyle.BackColor = Color.White;
                dataGridView_kingstar_special_fee_float.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
                dataGridView_kingstar_special_fee_float.DefaultCellStyle.SelectionForeColor = Color.Black;
                dataGridView_kingstar_special_fee_float.EnableHeadersVisualStyles = false;
                dataGridView_kingstar_special_fee_float.ColumnHeadersDefaultCellStyle.BackColor = Color.LightSteelBlue;
                dataGridView_kingstar_special_fee_float.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                dataGridView_kingstar_special_fee_float.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView_kingstar_special_fee_float.Font, FontStyle.Bold);
                
                // 隐藏行号列（第一列）
                dataGridView_kingstar_special_fee_float.RowHeadersVisible = false;
                
                // 设置小数列的格式，让数据更紧凑
                foreach (DataGridViewColumn column in dataGridView_kingstar_special_fee_float.Columns)
                {
                    // 使所有列宽度紧凑但不隐藏数据
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
                
                // 最终调整，确保所有数据可见
                dataGridView_kingstar_special_fee_float.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
                
                // 更新记录条数
                label_kingstar_special_fee_float_count.Text = $"记录条数：{dt.Rows.Count}条";
                LogInfo($"已刷新金士达客户特殊手续费率浮动数据，共 {dt.Rows.Count} 条");
                
                
                // dataGridView_kingstar_special_fee_float.DataBindingComplete += DataGridView_kingstar_special_fee_float_DataBindingComplete;
                // 强制刷新
                dataGridView_kingstar_special_fee_float.Refresh();
            }
            catch (Exception ex)
            {
                LogInfo($"刷新金士达客户特殊手续费率浮动数据异常: {ex.Message}");
            }
        }

        private void DataGridView_kingstar_special_fee_float_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            int checkResultColumnIndex = dataGridView_kingstar_special_fee_float.Columns["检查结果"].Index;
            
            foreach (DataGridViewRow row in dataGridView_kingstar_special_fee_float.Rows)
            {
                if (row.IsNewRow) continue;
                
                var cell = row.Cells[checkResultColumnIndex];
                if (cell.Value != null && !Convert.IsDBNull(cell.Value))
                {
                    string value = cell.Value.ToString().Trim();
                    if (value != "正确")
                    {
                        cell.Style.ForeColor = Color.Red;
                        cell.Style.Font = new Font(dataGridView_kingstar_special_fee_float.Font, FontStyle.Bold);
                    }
                }
            }
            
            // dataGridView_kingstar_special_fee_float.Refresh();
        }

        private void btn_export_Click(object sender, EventArgs e)
        {
            ExportDataToExcel();
        }

        ///// <summary>
        ///// 创建导出按钮
        ///// </summary>
        //private void CreateExportButton()
        //{
        //    // 创建导出按钮
        //    Button button_export_excel = new Button();
        //    button_export_excel.Text = "导出Excel";
        //    button_export_excel.Size = new Size(100, 30);
        //    button_export_excel.Location = new Point(10, 10);
        //    button_export_excel.BackColor = Color.LightSteelBlue;
        //    button_export_excel.ForeColor = Color.Black;
        //    button_export_excel.Font = new Font(this.Font.FontFamily, 9, FontStyle.Bold);
        //    button_export_excel.FlatStyle = FlatStyle.Flat;
        //    button_export_excel.Click += (sender, e) => ExportDataToExcel();
            
        //    // 添加到第5个标签页（飞马导出）
        //    if (tabControl1.TabPages.Count >= 5)
        //    {
        //        tabControl1.TabPages[4].Controls.Add(button_export_excel);
        //    }
        //}
        
        /// <summary>
        /// 导出数据到Excel
        /// </summary>
        private void ExportDataToExcel()
        {
            try
            {
                // 检查是否有数据
                if (dataGridView_femas_special_fee_export.RowCount == 0)
                {
                    LogInfo("没有数据可以导出，请先生成数据");
                    return;
                }
                
                // 创建导出文件名: YYYYMMDD_批量导出_特殊手续费率.xlsx
                string fileName = $"{DateTime.Now:yyyyMMdd}_批量导出_特殊手续费率.xlsx";
                
                // 确保femas目录存在
                string femasDir = textBox_femas.Text.Trim();
                if (string.IsNullOrEmpty(femasDir))
                {
                    LogInfo("飞马费率目录未设置，请先选择飞马费率目录");
                    return;
                }
                
                // 检查目录是否存在，不存在则创建
                if (!Directory.Exists(femasDir))
                {
                    LogInfo($"飞马费率目录不存在: {femasDir}，尝试创建");
                    try
                    {
                        Directory.CreateDirectory(femasDir);
                    }
                    catch (Exception ex)
                    {
                        LogInfo($"创建目录失败: {ex.Message}");
                        return;
                    }
                }
                
                // 完整的文件路径
                string filePath = Path.Combine(femasDir, fileName);
                
                // 检查文件是否已存在，如果存在则重命名为_old后缀
                if (File.Exists(filePath))
                {
                    string oldFilePath = Path.Combine(femasDir, Path.GetFileNameWithoutExtension(fileName) + "_old" + Path.GetExtension(fileName));
                    
                    // 如果旧文件也存在，先删除
                    if (File.Exists(oldFilePath))
                    {
                        try
                        {
                            File.Delete(oldFilePath);
                            LogInfo($"删除旧备份文件: {oldFilePath}");
                        }
                        catch (Exception ex)
                        {
                            LogInfo($"删除旧备份文件失败: {ex.Message}");
                            // 继续执行，不中断流程
                        }
                    }
                    
                    // 重命名当前文件为_old
                    try
                    {
                        File.Move(filePath, oldFilePath);
                        LogInfo($"已将现有文件重命名为: {Path.GetFileName(oldFilePath)}");
                    }
                    catch (Exception ex)
                    {
                        LogInfo($"重命名现有文件失败: {ex.Message}");
                        // 如果无法重命名，询问用户是否覆盖
                        if (MessageBox.Show($"文件 {fileName} 已存在且无法重命名。是否覆盖？", "文件已存在", 
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                        {
                            return;
                        }
                    }
                }
                
                // 提示用户确认
                if (MessageBox.Show($"确定导出数据到:\n{filePath}?", "确认导出", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
                
                // 显示正在导出的消息
                LogInfo("正在导出数据到Excel，请稍候...");
                
                // 创建导出数据
                var exportData = PrepareExportData();
                
                // 导出到Excel
                if (ExportToExcelWithEPPlus(exportData, filePath))
                {
                    LogInfo($"数据已成功导出到: {filePath}");
                    
                    // 询问是否打开文件夹
                    if (MessageBox.Show("数据导出成功，是否打开所在文件夹？", "导出完成", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        // 打开文件所在的文件夹
                        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                    }
                }
                else
                {
                    LogInfo("导出数据失败");
                }
            }
            catch (Exception ex)
            {
                LogInfo($"导出过程中发生异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 使用EPPlus导出Excel文件
        /// </summary>
        private bool ExportToExcelWithEPPlus(List<Dictionary<string, string>> data, string filePath)
        {
            try
            {
                if (data.Count == 0)
                {
                    LogInfo("没有数据可导出");
                    return false;
                }
                
                // 使用 EPPlus 创建 Excel 文件
                using (var package = new ExcelPackage())
                {
                    // 添加工作表
                    var worksheet = package.Workbook.Worksheets.Add("特殊手续费率");
                    
                    // 获取所有列名
                    var columns = data[0].Keys.ToList();
                    
                    // 写入表头
                    for (int i = 0; i < columns.Count; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = columns[i];
                        // 设置表头样式
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                        worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        worksheet.Cells[1, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    
                    // 写入数据行
                    for (int row = 0; row < data.Count; row++)
                    {
                        for (int col = 0; col < columns.Count; col++)
                        {
                            string value = data[row][columns[col]];
                            worksheet.Cells[row + 2, col + 1].Value = value;
                            worksheet.Cells[row + 2, col + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            
                            // 设置数值型列的格式
                            if (IsNumeric(value))
                            {
                                worksheet.Cells[row + 2, col + 1].Style.Numberformat.Format = "#,##0.000000";
                            }
                        }
                    }
                    
                    // 自动调整列宽以适应内容
                    for (int i = 1; i <= columns.Count; i++)
                    {
                        worksheet.Column(i).AutoFit();
                    }
                    
                    // 保存文件
                    var fileInfo = new FileInfo(filePath);
                    package.SaveAs(fileInfo);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                LogInfo($"Excel文件创建失败: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 判断字符串是否为数值
        /// </summary>
        private bool IsNumeric(string value)
        {
            return decimal.TryParse(value, out _);
        }
        
        /// <summary>
        /// 准备导出的数据
        /// </summary>
        private List<Dictionary<string, string>> PrepareExportData()
        {
            var exportData = new List<Dictionary<string, string>>();
            
            // 遍历DataGridView的每一行，将数据映射到目标结构
            foreach (DataGridViewRow row in dataGridView_femas_special_fee_export.Rows)
            {
                if (row.IsNewRow) continue;
                
                var rowData = new Dictionary<string, string>();
                
                // 按照映射关系填充数据
                rowData["投资者号"] = GetCellValue(row, "客户号");
                rowData["交易所名称"] = GetCellValue(row, "交易所");
                rowData["产品类型"] = GetCellValue(row, "产品类型");
                rowData["产品代码"] = GetCellValue(row, "产品");
                rowData["产品名称"] = ""; // 空值
                rowData["期权系列"] = "*"; // 默认值 *
                rowData["合约代码"] = GetCellValue(row, "合约");
                rowData["投保标识"] = "*"; // 默认值 *
                rowData["买卖标识"] = "*"; // 默认值 *
                rowData["跟随交易所标识"] = GetCellValue(row, "新是否跟随");
                rowData["跟随交易所倍数"] = "0"; // 默认值 0
                rowData["开仓手续费率（按金额）"] = GetCellValue(row, "新开仓按金额");
                rowData["开仓手续费额（按手数）"] = GetCellValue(row, "新开仓按手数");
                rowData["短线开仓手续费率（按金额）"] = GetCellValue(row, "新短开按金额");
                rowData["短线开仓手续费额（按手数）"] = GetCellValue(row, "新短开按手数");
                rowData["平仓手续费率（按金额）"] = GetCellValue(row, "新平仓按金额");
                rowData["平仓手续费额（按手数）"] = GetCellValue(row, "新平仓按手数");
                rowData["平今手续费率（按金额）"] = GetCellValue(row, "新平今按金额");
                rowData["平今手续费额（按手数）"] = GetCellValue(row, "新平今按手数");
                rowData["行权手续费率（按金额）"] = GetCellValue(row, "新行权按金额");
                rowData["行权手续费额（按手数）"] = GetCellValue(row, "新行权按手数");
                
                exportData.Add(rowData);
            }
            
            return exportData;
        }
        
        /// <summary>
        /// 获取DataGridView单元格的值
        /// </summary>
        private string GetCellValue(DataGridViewRow row, string columnName)
        {
            try
            {
                if (row.Cells[columnName].Value == null)
                    return "";
                    
                return row.Cells[columnName].Value.ToString();
            }
            catch
            {
                return "";
            }
        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            try
            {
                // 获取金士达和飞马文件夹路径
                string femasDir = textBox_femas.Text.Trim();
                string kingstarDir = textBox_kingstar.Text.Trim();
                
                // 检查路径是否配置
                if (string.IsNullOrEmpty(femasDir) && string.IsNullOrEmpty(kingstarDir))
                {
                    MessageBox.Show("飞马和金士达文件夹路径均未配置，请先设置路径。", "路径错误", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // 确认是否清空文件夹
                string message = "确定要删除以下文件夹中的所有文件吗？此操作不可恢复！\n\n";
                if (!string.IsNullOrEmpty(femasDir))
                    message += $"飞马文件夹: {femasDir}\n";
                if (!string.IsNullOrEmpty(kingstarDir))
                    message += $"金士达文件夹: {kingstarDir}";
                
                if (MessageBox.Show(message, "确认清空文件夹", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                {
                    return;
                }
                
                // 统计删除的文件数量
                int deletedCount = 0;
                int errorCount = 0;
                
                // 清空飞马文件夹
                if (!string.IsNullOrEmpty(femasDir) && Directory.Exists(femasDir))
                {
                    deletedCount += ClearDirectory(femasDir, out int errors);
                    errorCount += errors;
                }
                
                // 清空金士达文件夹
                if (!string.IsNullOrEmpty(kingstarDir) && Directory.Exists(kingstarDir))
                {
                    deletedCount += ClearDirectory(kingstarDir, out int errors);
                    errorCount += errors;
                }
                
                // 显示结果
                if (errorCount == 0)
                {
                    LogInfo($"文件清理完成，共删除 {deletedCount} 个文件。");
                    MessageBox.Show($"文件清理完成，共删除 {deletedCount} 个文件。", "清理成功", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    LogInfo($"文件清理部分完成，成功删除 {deletedCount} 个文件，有 {errorCount} 个文件删除失败。");
                    MessageBox.Show($"文件清理部分完成，成功删除 {deletedCount} 个文件，有 {errorCount} 个文件删除失败。\n\n请检查日志了解详情。", 
                        "清理部分完成", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                LogInfo($"清空文件夹时发生错误: {ex.Message}");
                MessageBox.Show($"清空文件夹时发生错误:\n{ex.Message}", "操作失败", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// 清空指定目录中的所有文件
        /// </summary>
        /// <param name="directoryPath">要清空的目录路径</param>
        /// <param name="errorCount">删除失败的文件数量</param>
        /// <returns>成功删除的文件数量</returns>
        private int ClearDirectory(string directoryPath, out int errorCount)
        {
            int deletedCount = 0;
            errorCount = 0;
            
            try
            {
                // 获取目录中的所有文件
                string[] files = Directory.GetFiles(directoryPath);
                
                // 删除每个文件
                foreach (string file in files)
                {
                    try
                    {
                        // 获取文件名以便日志记录
                        string fileName = Path.GetFileName(file);
                        
                        // 删除文件
                        File.Delete(file);
                        deletedCount++;
                        
                        // 记录日志
                        LogInfo($"已删除文件: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        // 记录错误
                        errorCount++;
                        LogInfo($"删除文件 {Path.GetFileName(file)} 失败: {ex.Message}");
                    }
                }
                
                // 清空子目录中的文件(不删除子目录本身)
                string[] subdirectories = Directory.GetDirectories(directoryPath);
                foreach (string subdir in subdirectories)
                {
                    int subDeletedCount = ClearDirectory(subdir, out int subErrorCount);
                    deletedCount += subDeletedCount;
                    errorCount += subErrorCount;
                }
                
                return deletedCount;
            }
            catch (Exception ex)
            {
                LogInfo($"清空目录 {directoryPath} 时发生错误: {ex.Message}");
                errorCount++;
                return deletedCount;
            }
        }
    }
}
