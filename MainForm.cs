using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace kingstar2femasfee
{
    public partial class MainForm: Form
    {
        // 日志记录集合，用于倒序显示
        private List<string> logMessages = new List<string>();
        
        // 定义日志级别
        private enum LogLevel
        {
            Info,
            Warning,
            Error
        }
        
        public MainForm()
        {
            InitializeComponent();
            DatabaseHelper.InitializeDatabase();
            LoadConfigFromDatabase();
            
            
            // 不需要这里重复绑定，Designer文件已经绑定了
            // btn_femas_dir.Click += Btn_femas_dir_Click;
            // btn_kingstar_dir.Click += Btn_kingstar_dir_Click;
            
            // 添加一键生成按钮事件
            // button_calc.Click += Button_calc_Click;
        }
        
        private void LoadConfigFromDatabase()
        {
            var (femasDir, kingstarDir) = DatabaseHelper.LoadConfig();
            textBox_femas.Text = femasDir;
            textBox_kingstar.Text = kingstarDir;
        }
        
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
        
        private void SaveConfigToDatabase()
        {
            DatabaseHelper.SaveConfig(textBox_femas.Text, textBox_kingstar.Text);
            LogInfo("配置已保存到数据库");
        }
        
        /// <summary>
        /// 一键生成按钮点击事件
        /// </summary>
        private void Button_calc_Click(object sender, EventArgs e)
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
            ProcessExchangeTradeFeeData(femasDir);
            
            // 处理特殊交易手续费率数据
            ProcessSpecialTradeFeeData(femasDir);
            
            // 处理金士达客户交易手续费率数据
            ProcessKingstarSpecialTradeFeeData(kingstarDir);

        }
        
        /// <summary>
        /// 处理交易所手续费率数据
        /// </summary>
        private void ProcessExchangeTradeFeeData(string femasDir)
        {
            LogInfo("开始处理交易所手续费率数据...");
            
            // 读取Excel文件
            var (success, dataList) = ExcelHelper.ReadExchangeTradeFeeExcel(femasDir, LogInfo);
            
            // 如果读取成功且数据验证通过，则导入数据库
            if (success && dataList.Count > 0)
            {
                LogInfo($"数据校验通过，准备导入 {dataList.Count} 条交易所手续费率数据");
                bool importSuccess = DatabaseHelper.ImportExchangeTradeFeeData(dataList, LogInfo);
                
                if (importSuccess)
                {
                    LogInfo("交易所手续费率数据导入成功");
                }
                else
                {
                    LogInfo("交易所手续费率数据导入失败");
                }
            }
            else
            {
                LogInfo("数据校验未通过，请检查Excel文件");
            }
        }
        
        /// <summary>
        /// 处理特殊交易手续费率数据
        /// </summary>
        private void ProcessSpecialTradeFeeData(string femasDir)
        {
            LogInfo("开始处理特殊交易手续费率数据...");
            
            // 读取Excel文件
            var (success, dataList) = ExcelHelper.ReadSpecialTradeFeeExcel(femasDir, LogInfo);
            
            // 如果读取成功且数据验证通过，则导入数据库
            if (success && dataList.Count > 0)
            {
                LogInfo($"数据校验通过，准备导入 {dataList.Count} 条特殊交易手续费率数据");
                bool importSuccess = DatabaseHelper.ImportSpecialTradeFeeData(dataList, LogInfo);
                
                if (importSuccess)
                {
                    LogInfo("特殊交易手续费率数据导入成功");
                }
                else
                {
                    LogInfo("特殊交易手续费率数据导入失败");
                }
            }
            else
            {
                LogInfo("数据校验未通过，请检查Excel文件");
            }
        }
        
        /// <summary>
        /// 处理金士达客户交易手续费率数据(期货)
        /// </summary>
        private void ProcessKingstarSpecialTradeFeeData(string kingstarDir)
        {
            LogInfo("开始处理金士达客户交易手续费率数据（期货）...");
            
            // 读取Excel文件
            var (success, dataList) = ExcelHelper.ReadKingstarSpecialTradeFeeExcel(kingstarDir, LogInfo);
            
            LogInfo("开始处理金士达客户交易手续费率数据（期权）...");
            var (successOptions, dataListOptions) = ExcelHelper.ReadKingstarSpecialTradeFeeExcelOptions(kingstarDir, LogInfo);

            dataList = dataList.Concat(dataListOptions).ToList();
            
            // 如果读取成功且数据验证通过，则导入数据库
            if (success && dataList.Count > 0)
            {
                LogInfo($"数据校验通过，准备导入 {dataList.Count} 条金士达客户交易手续费率数据");
                bool importSuccess = DatabaseHelper.ImportKingstarSpecialTradeFeeData(dataList, LogInfo);
                
                if (importSuccess)
                {
                    LogInfo("金士达客户交易手续费率数据导入成功");
                }
                else
                {
                    LogInfo("金士达客户交易手续费率数据导入失败");
                }
            }
            else
            {
                LogInfo("数据校验未通过，请检查Excel文件");
            }
        }
        
        /// <summary>
        /// 记录日志信息
        /// </summary>
        private void LogInfo(string message)
        {
            // 检查日志内容是否包含错误关键词
            //LogLevel logLevel;
            //if (message.Contains("失败") || message.Contains("未通过") || message.Contains("错误") || message.Contains("异常") || message.Contains("不完整"))
            //{
            //    logLevel = LogLevel.Error;
            //}
            //else if (message.Contains("警告") || message.Contains("注意"))
            //{
            //    logLevel = LogLevel.Warning;
            //}
            //else
            //{
            //    logLevel = LogLevel.Info;
            //}
            
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
