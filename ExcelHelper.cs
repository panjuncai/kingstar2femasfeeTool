using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using System.Globalization;

namespace kingstar2femasfee
{
    public class ExcelHelper
    {
        /// <summary>
        /// 日志记录委托
        /// </summary>
        /// <param name="message">日志消息</param>
        public delegate void LogMessageDelegate(string message);

        /// <summary>
        /// 读取交易所手续费率Excel文件
        /// </summary>
        /// <param name="directoryPath">文件目录</param>
        /// <param name="logAction">日志记录方法</param>
        /// <returns>处理结果和交易所手续费率数据列表</returns>
        public static (bool success, List<ExchangeTradeFeeDO> dataList) ReadExchangeTradeFeeExcel(string directoryPath, LogMessageDelegate logAction)
        {
            List<ExchangeTradeFeeDO> resultList = new List<ExchangeTradeFeeDO>();
            bool success = true;
            
            try
            {
                // 查找最新的匹配文件
                string[] files = Directory.GetFiles(directoryPath, "*_批量导出_交易所手续费率.xlsx");
                if (files.Length == 0)
                {
                    LogMessage(logAction, "未找到交易所手续费率Excel文件");
                    return (false, resultList);
                }

                // 按文件名排序，获取最新的文件
                string latestFile = files.OrderByDescending(f => f).First();
                string fileName = Path.GetFileName(latestFile);
                LogMessage(logAction, $"找到交易所手续费率文件: {fileName}");
                
                // 使用EPPlus读取Excel
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(new FileInfo(latestFile)))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        LogMessage(logAction, "Excel文件不包含工作表");
                        return (false, resultList);
                    }

                    // 获取行列范围
                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;
                    
                    // 检查数据重复
                    var exchangeDataCheck = new Dictionary<string, int>();
                    
                    // 跳过标题行，从第二行开始读取
                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            // 读取单元格数据
                            string exchCode = worksheet.Cells[row, 1].Text.Trim();
                            string productType = worksheet.Cells[row, 2].Text.Trim();
                            string productId = worksheet.Cells[row, 3].Text.Trim();
                            string productName = worksheet.Cells[row, 4].Text.Trim();
                            string optionSeries = worksheet.Cells[row, 5].Text.Trim() == "" ? "*" : worksheet.Cells[row, 5].Text.Trim();
                            string instrumentId = worksheet.Cells[row, 6].Text.Trim() == "" ? "*" : worksheet.Cells[row, 6].Text.Trim();
                            string hedgeFlag = worksheet.Cells[row, 7].Text.Trim() == "" ? "*" : worksheet.Cells[row, 7].Text.Trim();
                            string buySell = worksheet.Cells[row, 8].Text.Trim() == "" ? "*" : worksheet.Cells[row, 8].Text.Trim();
                            
                            // 解析费率和金额
                            decimal openFeeRate = ParseDecimal(worksheet.Cells[row, 9].Text);
                            decimal openFeeAmt = ParseDecimal(worksheet.Cells[row, 10].Text);
                            decimal shortOpenFeeRate = ParseDecimal(worksheet.Cells[row, 11].Text);
                            decimal shortOpenFeeAmt = ParseDecimal(worksheet.Cells[row, 12].Text);
                            decimal offsetFeeRate = ParseDecimal(worksheet.Cells[row, 13].Text);
                            decimal offsetFeeAmt = ParseDecimal(worksheet.Cells[row, 14].Text);
                            decimal otFeeRate = ParseDecimal(worksheet.Cells[row, 15].Text);
                            decimal otFeeAmt = ParseDecimal(worksheet.Cells[row, 16].Text);
                            decimal execClearFeeRate = ParseDecimal(worksheet.Cells[row, 17].Text);
                            decimal execClearFeeAmt = ParseDecimal(worksheet.Cells[row, 18].Text);
                            
                            // 检查必填字段
                            if (string.IsNullOrEmpty(exchCode) || string.IsNullOrEmpty(productType) || string.IsNullOrEmpty(productId))
                            {
                                LogMessage(logAction, $"第{row}行数据不完整，交易所代码、产品类型和产品代码为必填项");
                                success = false;
                                continue;
                            }
                            
                            // 验证枚举值
                            try
                            {
                                // 交易所代码验证
                                if (exchCode.Length != 1)
                                {
                                    LogMessage(logAction, $"第{row}行数据有误，交易所代码应为单个字符");
                                    success = false;
                                    continue;
                                }
                                var exchange = EnumHelper.GetEnumFromChar<ExchangeEnum>(exchCode[0]);
                                
                                // 产品类型验证
                                if (productType.Length != 1)
                                {
                                    LogMessage(logAction, $"第{row}行数据有误，产品类型应为单个字符");
                                    success = false;
                                    continue;
                                }
                                var prodType = EnumHelper.GetEnumFromChar<ProductTypeEnum>(productType[0]);
                                
                                // 投保标识验证
                                if (hedgeFlag.Length != 1)
                                {
                                    LogMessage(logAction, $"第{row}行数据有误，投保标识应为单个字符");
                                    success = false;
                                    continue;
                                }
                                var hedge = EnumHelper.GetEnumFromChar<HedgeFlagEnum>(hedgeFlag[0]);
                                
                                // 买卖标识验证
                                if (buySell.Length != 1)
                                {
                                    LogMessage(logAction, $"第{row}行数据有误，买卖标识应为单个字符");
                                    success = false;
                                    continue;
                                }
                                var bs = EnumHelper.GetEnumFromChar<BuySellEnum>(buySell[0]);
                            }
                            catch (ArgumentException ex)
                            {
                                LogMessage(logAction, $"第{row}行数据有误，{ex.Message}");
                                success = false;
                                continue;
                            }
                            
                            // 检查数据是否重复
                            string key = $"{exchCode}|{productType}|{hedgeFlag}|{optionSeries}|{productId}|{instrumentId}|{buySell}";
                            if (exchangeDataCheck.ContainsKey(key))
                            {
                                LogMessage(logAction, $"交易所手续费率重复，请检查第{exchangeDataCheck[key]}行和第{row}行");
                                success = false;
                                continue;
                            }
                            exchangeDataCheck.Add(key, row);
                            
                            // 创建数据对象
                            var data = new ExchangeTradeFeeDO
                            {
                                ExchCode = exchCode,
                                ProductType = productType,
                                ProductId = productId,
                                OptionSeriesId = optionSeries,
                                InstrumentId = instrumentId,
                                HedgeFlag = hedgeFlag,
                                BuySell = buySell,
                                OpenFeeRate = openFeeRate,
                                OpenFeeAmt = openFeeAmt,
                                ShortOpenFeeRate = shortOpenFeeRate,
                                ShortOpenFeeAmt = shortOpenFeeAmt,
                                OffsetFeeRate = offsetFeeRate,
                                OffsetFeeAmt = offsetFeeAmt,
                                OtFeeRate = otFeeRate,
                                OtFeeAmt = otFeeAmt,
                                ExecClearFeeRate = execClearFeeRate,
                                ExecClearFeeAmt = execClearFeeAmt,
                                OperDate = DateTime.Now.ToString("yyyyMMdd"),
                                OperTime = DateTime.Now.ToString("HHmmss")
                            };
                            
                            resultList.Add(data);
                        }
                        catch (Exception ex)
                        {
                            LogMessage(logAction, $"第{row}行数据处理异常：{ex.Message}");
                            success = false;
                        }
                    }
                }
                
                return (success, resultList);
            }
            catch (Exception ex)
            {
                LogMessage(logAction, $"读取Excel文件异常：{ex.Message}");
                return (false, resultList);
            }
        }
        
        /// <summary>
        /// 解析小数值
        /// </summary>
        private static decimal ParseDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;
            
            if (decimal.TryParse(value, out decimal result))
                return result;
            
            return 0;
        }
        
        /// <summary>
        /// 记录日志
        /// </summary>
        private static void LogMessage(LogMessageDelegate logAction, string message)
        {
            if (logAction != null)
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}";
                logAction(logEntry);
            }
        }
    }
    
    /// <summary>
    /// 交易所手续费率数据对象
    /// </summary>
    public class ExchangeTradeFeeDO
    {
        public string ExchCode { get; set; }
        public string ProductType { get; set; }
        public string ProductId { get; set; }
        public string OptionSeriesId { get; set; }
        public string InstrumentId { get; set; }
        public string HedgeFlag { get; set; }
        public string BuySell { get; set; }
        public decimal OpenFeeRate { get; set; }
        public decimal OpenFeeAmt { get; set; }
        public decimal ShortOpenFeeRate { get; set; }
        public decimal ShortOpenFeeAmt { get; set; }
        public decimal OffsetFeeRate { get; set; }
        public decimal OffsetFeeAmt { get; set; }
        public decimal OtFeeRate { get; set; }
        public decimal OtFeeAmt { get; set; }
        public decimal ExecClearFeeRate { get; set; }
        public decimal ExecClearFeeAmt { get; set; }
        public string OperDate { get; set; }
        public string OperTime { get; set; }
    }
} 