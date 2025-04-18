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
                
                // 显示行号
                // dataGridView_exchange_fee.RowHeadersVisible = true;
                // dataGridView_exchange_fee.RowHeadersWidth = 60; // 调整行头宽度
                
                // 为每一行添加行号
                // for (int i = 0; i < dataGridView_exchange_fee.Rows.Count; i++)
                // {
                //     dataGridView_exchange_fee.Rows[i].HeaderCell.Value = (i + 1).ToString();
                // }
                
                // 设置小数列的格式，让数据更紧凑
                foreach (DataGridViewColumn column in dataGridView_exchange_fee.Columns)
                {
                    //if (column.Name.Contains("费率") || column.Name.Contains("费额"))
                    //{
                    //    column.DefaultCellStyle.Format = "F8"; // 8位小数，不使用千分位分隔符
                    //    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    //}
                    // 使所有列宽度紧凑但不隐藏数据
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
                
                // 最终调整，确保所有数据可见
                dataGridView_exchange_fee.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
                
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
            }
            else
            {
                LogInfo("特殊交易手续费率数据导入失败");
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// 处理金士达客户交易手续费率数据(期货)
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
            }
            else
            {
                LogInfo("金士达客户交易手续费率数据导入失败");
                return false;
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
    }
}
