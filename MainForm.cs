using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        /// 记录日志信息
        /// </summary>
        private void LogInfo(string message)
        {
            // 添加时间戳
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}";
            
            // 添加到日志集合
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
            StringBuilder sb = new StringBuilder();
            foreach (string log in logMessages)
            {
                sb.AppendLine(log);
            }
            textBox_log.Text = sb.ToString();
        }
    }
}
