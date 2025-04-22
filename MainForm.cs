using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using static kingstar2femasfee.EnumHelper;

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
        /// <returns></returns>
        private bool ProcessKingstarSpecialTradeFeeFloatData()
        {
            LogInfo("开始处理金士达客户交易手续费率浮动数据...");

            // 转换金士达终值手续费为浮动手续费
            bool convertSuccess = DatabaseHelper.ConvertKingstarSpecial2FloatData(LogInfo);
            if (!convertSuccess)
            {
                LogInfo("转换金士达终值手续费为浮动手续费失败");
                return false;
            }
            else
            {
                LogInfo("转换金士达终值手续费为浮动手续费成功");
                // 刷新金士达特殊手续费率浮动DataGridView控件
                RefreshKingstarSpecialFeeFloatDataGridView();
            }

            return true;
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
                
                // 移除先前的事件（如果有）
                // dataGridView_kingstar_special_fee_float.RowPrePaint -= DataGridView_kingstar_special_fee_float_RowPrePaint;
                
                // 添加行预绘制事件
                dataGridView_kingstar_special_fee_float.RowPrePaint += DataGridView_kingstar_special_fee_float_RowPrePaint;
                
                // 强制刷新
                dataGridView_kingstar_special_fee_float.Refresh();
            }
            catch (Exception ex)
            {
                LogInfo($"刷新金士达客户特殊手续费率浮动数据异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 行预绘制事件 - 为结果代码不等于0的行应用样式
        /// </summary>
        private void DataGridView_kingstar_special_fee_float_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView_kingstar_special_fee_float.Rows.Count)
            {
                DataGridViewRow row = dataGridView_kingstar_special_fee_float.Rows[e.RowIndex];
                
                // 找到"结果代码"列
                DataGridViewColumn resultCodeColumn = null;
                foreach (DataGridViewColumn column in dataGridView_kingstar_special_fee_float.Columns)
                {
                    if (column.HeaderText == "结果代码")
                    {
                        resultCodeColumn = column;
                        break;
                    }
                }
                
                if (resultCodeColumn != null && row.Cells[resultCodeColumn.Index].Value != null)
                {
                    object value = row.Cells[resultCodeColumn.Index].Value;
                    if (value != null)
                    {
                        // 尝试解析结果代码
                        int checkCode = 0;
                        if (value is int)
                        {
                            checkCode = (int)value;
                        }
                        else if (int.TryParse(value.ToString(), out int parsedCode))
                        {
                            checkCode = parsedCode;
                        }
                        
                        // 如果结果代码不为0，设置行样式
                        if (checkCode != 0)
                        {
                            // 记录发现的问题行
                            // LogInfo($"发现异常行 #{e.RowIndex + 1}，结果代码: {checkCode}");
                            
                            // 设置样式
                            row.DefaultCellStyle.BackColor = Color.Red;
                            row.DefaultCellStyle.ForeColor = Color.White;
                            row.DefaultCellStyle.Font = new Font(dataGridView_kingstar_special_fee_float.Font, FontStyle.Bold);
                            
                            // 确保视觉更新
                            row.Selected = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 为结果代码不等于0的行应用样式 (不再使用此方法)
        /// </summary>
        private void ApplyRowStylesBasedOnResultCode()
        {
            // 找到"结果代码"列的索引
            int resultCodeIndex = -1;
            for (int i = 0; i < dataGridView_kingstar_special_fee_float.Columns.Count; i++)
            {
                if (dataGridView_kingstar_special_fee_float.Columns[i].HeaderText == "结果代码")
                {
                    resultCodeIndex = i;
                    break;
                }
            }
            
            if (resultCodeIndex == -1)
            {
                LogInfo("找不到'结果代码'列，无法设置行样式");
                return;
            }
            
            LogInfo($"找到结果代码列索引: {resultCodeIndex}，开始设置样式...");
            int modifiedRows = 0;
                
            // 循环处理每一行
            foreach (DataGridViewRow row in dataGridView_kingstar_special_fee_float.Rows)
            {
                if (row.Cells[resultCodeIndex].Value != null)
                {
                    // 记录当前值用于调试
                    string checkCodeValue = row.Cells[resultCodeIndex].Value.ToString();
                    LogInfo($"检查行 {row.Index}，结果代码值: '{checkCodeValue}'");
                    
                    // 尝试转换为整数并比较
                    int checkCodeInt;
                    bool isInt = int.TryParse(checkCodeValue, out checkCodeInt);
                    
                    // 使用字符串比较和整数比较两种方式
                    if ((isInt && checkCodeInt != 0) || (!isInt && checkCodeValue != "0"))
                    {
                        // 设置整行的样式为红底白字加粗
                        row.DefaultCellStyle = new DataGridViewCellStyle
                        {
                            BackColor = Color.Red,
                            ForeColor = Color.White,
                            Font = new Font(dataGridView_kingstar_special_fee_float.Font, FontStyle.Bold)
                        };
                        modifiedRows++;
                        
                        // 强制更新行
                        dataGridView_kingstar_special_fee_float.UpdateCellValue(resultCodeIndex, row.Index);
                        
                        // 添加详细日志
                        LogInfo($"已设置行 {row.Index} 样式为红底白字加粗，值: '{checkCodeValue}'");
                    }
                }
            }
            
            LogInfo($"已设置 {modifiedRows} 行样式为红底白字加粗");
            
            // 刷新DataGridView强制重绘
            dataGridView_kingstar_special_fee_float.Refresh();
        }
    }
}
