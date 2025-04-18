using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using static kingstar2femasfee.Utils;
using static kingstar2femasfee.EnumHelper;

namespace kingstar2femasfee
{
    public static class ExcelHelper
    {
        /// <summary>
        /// 提取产品代码
        /// </summary>
        /// <param name="instrumentId">合约代码</param>
        /// <returns>产品代码</returns>
        public static string extractProductId(string instrumentId)
        {
            // ag2504能提取出ag
            if (string.IsNullOrEmpty(instrumentId))
                return instrumentId;

            // 找到第一个数字的位置
            int digitIndex = -1;
            for (int i = 0; i < instrumentId.Length; i++)
            {
                if (char.IsDigit(instrumentId[i]))
                {
                    digitIndex = i;
                    break;
                }
            }

            // 如果找到数字，返回数字前面的部分
            if (digitIndex > 0)
            {
                return instrumentId.Substring(0, digitIndex);
            }

            // 如果没有找到数字，返回原始值
            return instrumentId;
        }

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
                            string exchCode = "";
                            string productType = "";
                            string hedgeFlag = "";
                            string buySell = "";
                            // 读取单元格数据
                            string exchCodeText = worksheet.Cells[row, 1].Text.Trim();
                            string productTypeText = worksheet.Cells[row, 2].Text.Trim();
                            string productId = worksheet.Cells[row, 3].Text.Trim();
                            string productName = worksheet.Cells[row, 4].Text.Trim();
                            string optionSeries = worksheet.Cells[row, 5].Text.Trim() == "" ? "*" : worksheet.Cells[row, 5].Text.Trim();
                            string instrumentId = worksheet.Cells[row, 6].Text.Trim() == "" ? "*" : worksheet.Cells[row, 6].Text.Trim();
                            string hedgeFlagText = worksheet.Cells[row, 7].Text.Trim() == "" ? "*" : worksheet.Cells[row, 7].Text.Trim();
                            string buySellText = worksheet.Cells[row, 8].Text.Trim() == "" ? "*" : worksheet.Cells[row, 8].Text.Trim();

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
                            if (string.IsNullOrEmpty(exchCodeText) || string.IsNullOrEmpty(productTypeText) || string.IsNullOrEmpty(productId) || string.IsNullOrEmpty(optionSeries) || string.IsNullOrEmpty(instrumentId) || string.IsNullOrEmpty(hedgeFlagText) || string.IsNullOrEmpty(buySellText))
                            {
                                LogMessage(logAction, $"第{row}行数据不完整，交易所代码、产品类型、产品代码、期权系列、期权代码、投保标识、买卖标识为必填项");
                                success = false;
                                return (success, resultList);
                            }

                            try
                            {
                                // 交易所代码转换
                                try
                                {
                                    char exchChar = GetCharFromDescription<ExchangeEnum>(exchCodeText);
                                    exchCode = exchChar.ToString();
                                }
                                catch (Exception ex)
                                {
                                    throw new ArgumentException($"交易所代码'{exchCodeText}'转换失败: {ex.Message}");
                                }

                                // 产品类型转换
                                try
                                {
                                    char ptChar = GetCharFromDescription<ProductTypeEnum>(productTypeText);
                                    productType = ptChar.ToString();
                                }
                                catch (Exception ex)
                                {
                                    throw new ArgumentException($"产品类型'{productTypeText}'转换失败: {ex.Message}");
                                }

                                // 投保标识转换
                                try
                                {
                                    char hfChar = GetCharFromDescription<HedgeFlagEnum>(hedgeFlagText);
                                    hedgeFlag = hfChar.ToString();
                                }
                                catch (Exception ex)
                                {
                                    throw new ArgumentException($"投保标识'{hedgeFlagText}'转换失败: {ex.Message}");
                                }

                                // 投保标识如果不为*，则报错
                                try
                                {
                                    char hfChar = GetCharFromDescription<HedgeFlagEnum>(hedgeFlagText);
                                    hedgeFlag = hfChar.ToString();
                                    if (hedgeFlag != "*")
                                    {
                                        throw new ArgumentException($"投保标识'{hedgeFlagText}'为指定值，不符合业务实际情况");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw new ArgumentException($"投保标识'{hedgeFlagText}'转换失败: {ex.Message}");
                                }

                                // 买卖标识转换
                                try
                                {
                                    char bsChar = GetCharFromDescription<BuySellEnum>(buySellText);
                                    buySell = bsChar.ToString();
                                }
                                catch (Exception ex)
                                {
                                    throw new ArgumentException($"买卖标识'{buySellText}'转换失败: {ex.Message}");
                                }

                                // 买卖标识为指定值，则报错
                                try
                                {
                                    char bsChar = GetCharFromDescription<BuySellEnum>(buySellText);
                                    buySell = bsChar.ToString();
                                    if (buySell!="*")
                                    {
                                        throw new ArgumentException($"买卖标识'{buySellText}'为指定值，不符合业务实际情况");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw new ArgumentException($"买卖标识'{buySellText}'转换失败: {ex.Message}");
                                }

                                // 期权系列为指定值，则报错
                                try
                                {
                                    if (optionSeries != "*")
                                    {
                                        throw new ArgumentException($"期权系列'{optionSeries}'为指定值，不符合业务实际情况");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw new ArgumentException($"期权系列'{optionSeries}'转换失败: {ex.Message}");
                                }

                            }
                            catch (ArgumentException ex)
                            {
                                LogMessage(logAction, $"第{row}行数据转换失败: {ex.Message}");
                                success = false;
                                return (success, resultList);
                            }

                            // 检查数据是否重复
                            string key = $"{exchCode}|{productType}|{hedgeFlag}|{optionSeries}|{productId}|{instrumentId}|{buySell}";
                            if (exchangeDataCheck.ContainsKey(key))
                            {
                                LogMessage(logAction, $"交易所手续费率重复，请检查第{exchangeDataCheck[key]}行和第{row}行");
                                success = false;
                                return (success, resultList);
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
                                OperDate = DateTime.Now.ToString("yyyy-MM-dd"),
                                OperTime = DateTime.Now.ToString("HH:mm:ss")
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
        /// 读取飞马特殊交易手续费率Excel文件
        /// </summary>
        /// <param name="directoryPath">文件目录</param>
        /// <param name="logAction">日志记录方法</param>
        /// <returns>处理结果和特殊交易手续费率数据列表</returns>
        public static (bool success, List<SpecialTradeFeeDO> dataList) ReadSpecialTradeFeeExcel(string directoryPath, LogMessageDelegate logAction)
        {
            List<SpecialTradeFeeDO> resultList = new List<SpecialTradeFeeDO>();
            bool success = true;

            try
            {
                // 查找最新的匹配文件
                string[] files = Directory.GetFiles(directoryPath, "*_批量导出_特殊手续费率.xlsx");
                if (files.Length == 0)
                {
                    LogMessage(logAction, "未找到特殊交易手续费率Excel文件");
                    return (false, resultList);
                }

                // 按文件名排序，获取最新的文件
                string latestFile = files.OrderByDescending(f => f).First();
                string fileName = Path.GetFileName(latestFile);
                LogMessage(logAction, $"找到特殊交易手续费率文件: {fileName}");

                // 创建Excel应用程序实例
                using (ExcelPackage package = new ExcelPackage(new FileInfo(latestFile)))
                {
                    // 获取第一个工作表
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    if (worksheet == null)
                    {
                        LogMessage(logAction, "Excel文件中未找到工作表");
                        return (false, resultList);
                    }

                    // 统计数据行数
                    int rowCount = worksheet.Dimension.Rows;
                    if (rowCount <= 1)
                    {
                        LogMessage(logAction, "Excel文件中没有数据行");
                        return (false, resultList);
                    }

                    LogMessage(logAction, $"开始解析特殊交易手续费率数据，共 {rowCount - 1} 行");

                    // 数据重复检查字典
                    Dictionary<string, int> specialDataCheck = new Dictionary<string, int>();

                    // 从第2行开始读取数据（第1行是表头）
                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            // 读取各字段值
                            string investorId = worksheet.Cells[row, 1].Text.Trim();
                            string exchCodeText = worksheet.Cells[row, 2].Text.Trim();
                            string productTypeText = worksheet.Cells[row, 3].Text.Trim();
                            string productId = worksheet.Cells[row, 4].Text.Trim();
                            string optionSeries = worksheet.Cells[row, 6].Text.Trim();
                            string instrumentId = worksheet.Cells[row, 7].Text.Trim();
                            string hedgeFlagText = worksheet.Cells[row, 8].Text.Trim();
                            string buySellText = worksheet.Cells[row, 9].Text.Trim();
                            string followTypeText = worksheet.Cells[row, 10].Text.Trim();
                            
                            // 解析费率和金额
                            decimal multipleRatio = ParseDecimal(worksheet.Cells[row, 11].Text);
                            decimal openFeeRate = ParseDecimal(worksheet.Cells[row, 12].Text);
                            decimal openFeeAmt = ParseDecimal(worksheet.Cells[row, 13].Text);
                            decimal shortOpenFeeRate = ParseDecimal(worksheet.Cells[row, 14].Text);
                            decimal shortOpenFeeAmt = ParseDecimal(worksheet.Cells[row, 15].Text);
                            decimal offsetFeeRate = ParseDecimal(worksheet.Cells[row, 16].Text);
                            decimal offsetFeeAmt = ParseDecimal(worksheet.Cells[row, 17].Text);
                            decimal otFeeRate = ParseDecimal(worksheet.Cells[row, 18].Text);
                            decimal otFeeAmt = ParseDecimal(worksheet.Cells[row, 19].Text);
                            decimal execClearFeeRate = ParseDecimal(worksheet.Cells[row, 20].Text);
                            decimal execClearFeeAmt = ParseDecimal(worksheet.Cells[row, 21].Text);

                            // 检查必填字段
                            if (string.IsNullOrEmpty(investorId) || string.IsNullOrEmpty(exchCodeText) || string.IsNullOrEmpty(productTypeText) || 
                                string.IsNullOrEmpty(productId) || string.IsNullOrEmpty(optionSeries) || string.IsNullOrEmpty(instrumentId) || 
                                string.IsNullOrEmpty(hedgeFlagText) || string.IsNullOrEmpty(buySellText)||string.IsNullOrEmpty(followTypeText))
                            {
                                LogMessage(logAction, $"第{row}行数据不完整，投资者号、交易所代码、产品类型、产品代码、期权系列、期权代码、投保标识、买卖标识、是否跟随为必填项");
                                success = false;
                                return (success, resultList);
                            }

                            // 转换枚举字段
                            string exchCode, productType, hedgeFlag, buySell, followType;
                            try
                            {
                                // 交易所代码转换
                                try
                                {
                                    char exchChar = GetCharFromDescription<ExchangeEnum>(exchCodeText);
                                    exchCode = exchChar.ToString();
                                }
                                catch (Exception ex)
                                {
                                    throw new ArgumentException($"交易所代码'{exchCodeText}'转换失败: {ex.Message}");
                                }

                                // 产品类型转换
                                try
                                {
                                    char ptChar = GetCharFromDescription<ProductTypeEnum>(productTypeText);
                                    productType = ptChar.ToString();
                                }
                                catch (Exception ex)
                                {
                                    throw new ArgumentException($"产品类型'{productTypeText}'转换失败: {ex.Message}");
                                }

                                // 投保标志转换
                                try
                                {
                                    char hfChar = GetCharFromDescription<HedgeFlagEnum>(hedgeFlagText);
                                    hedgeFlag = hfChar.ToString();
                                }
                                catch (Exception ex)
                                {
                                    throw new ArgumentException($"投保标识'{hedgeFlagText}'转换失败: {ex.Message}");
                                }

                                // 买卖标识转换
                                try
                                {
                                    char bsChar = GetCharFromDescription<BuySellEnum>(buySellText);
                                    buySell = bsChar.ToString();
                                }
                                catch (Exception ex)
                                {
                                    throw new ArgumentException($"买卖标识'{buySellText}'转换失败: {ex.Message}");
                                }

                                // 投保标识如果不为*，则报错
                                try
                                {
                                    char hfChar = GetCharFromDescription<HedgeFlagEnum>(hedgeFlagText);
                                    hedgeFlag = hfChar.ToString();
                                    if (hedgeFlag != "*")
                                    {
                                        throw new ArgumentException($"投保标识'{hedgeFlagText}'为指定值，不符合业务实际情况");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw new ArgumentException($"投保标识'{hedgeFlagText}'转换失败: {ex.Message}");
                                }

                                // 买卖标识为指定值，则报错
                                try
                                {
                                    char bsChar = GetCharFromDescription<BuySellEnum>(buySellText);
                                    buySell = bsChar.ToString();
                                    if (buySell != "*")
                                    {
                                        throw new ArgumentException($"买卖标识'{buySellText}'为指定值，不符合业务实际情况");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw new ArgumentException($"买卖标识'{buySellText}'转换失败: {ex.Message}");
                                }

                                // 期权系列为指定值，则报错
                                try
                                {
                                    if (optionSeries != "*")
                                    {
                                        throw new ArgumentException($"期权系列'{optionSeries}'为指定值，不符合业务实际情况");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw new ArgumentException($"期权系列'{optionSeries}'转换失败: {ex.Message}");
                                }

                                // 是否跟随转换
                                try
                                {
                                    char ftChar = GetCharFromDescription<isFllowEnum>(followTypeText);
                                    followType = ftChar.ToString();
                                }
                                catch (Exception ex)
                                {
                                    followType = "0"; // 默认不跟随
                                    LogMessage(logAction, $"第{row}行是否跟随'{followTypeText}'转换失败，设为默认值'否': {ex.Message}");
                                }
                            }
                            catch (ArgumentException ex)
                            {
                                LogMessage(logAction, $"第{row}行数据转换失败: {ex.Message}");
                                success = false;
                                return (success, resultList);
                            }

                            // 检查数据是否重复
                            string key = $"{investorId}|{exchCode}|{productType}|{productId}|{optionSeries}|{instrumentId}|{hedgeFlag}|{buySell}";
                            if (specialDataCheck.ContainsKey(key))
                            {
                                LogMessage(logAction, $"特殊交易手续费率重复，请检查第{specialDataCheck[key]}行和第{row}行");
                                success = false;
                                return (success, resultList);
                            }
                            specialDataCheck.Add(key, row);

                            // 创建数据对象
                            var data = new SpecialTradeFeeDO
                            {
                                InvestorId = investorId,
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
                                FollowType = followType,
                                MultipleRatio = multipleRatio,
                                OperDate = DateTime.Now.ToString("yyyy-MM-dd"),
                                OperTime = DateTime.Now.ToString("HH:mm:ss")
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
                LogMessage(logAction, $"读取特殊交易手续费率Excel文件异常: {ex.Message}");
                return (false, resultList);
            }
        }

        /// <summary>
        /// 读取金士达客户手续费率变更表(期货)Excel文件
        /// </summary>
        /// <param name="directoryPath">文件目录</param>
        /// <param name="logAction">日志记录方法</param>
        /// <returns>处理结果和金士达客户手续费率变更表数据列表</returns>
        public static (bool success, List<KingstarSpecialTradeFeeDO> dataList) ReadKingstarSpecialTradeFeeExcel(string directoryPath, LogMessageDelegate logAction)
        {
            List<KingstarSpecialTradeFeeDO> resultList = new List<KingstarSpecialTradeFeeDO>();
            bool success = true;

            try
            {
                // 查找最新的匹配文件
                string[] files = Directory.GetFiles(directoryPath, "*客户手续费变更表.xlsx");
                if (files.Length == 0)
                {
                    LogMessage(logAction, "未找到金士达客户手续费变更表Excel文件");
                    return (false, resultList);
                }

                // 获取全部文件
                // string latestFile = files.OrderByDescending(f => f).First();
                // string fileName = Path.GetFileName(latestFile);
                foreach(var file in files){
                LogMessage(logAction, $"找到金士达“客户手续费变更表.xlsx”文件: {file}");

                // 创建Excel应用程序实例
                using (ExcelPackage package = new ExcelPackage(new FileInfo(file)))
                {
                    // 获取工作表 - 修改为获取所有工作表并选择第一个非空的工作表
                    ExcelWorksheet worksheet = null;
                    
                    // 先尝试通过索引获取
                    try 
                    {
                        worksheet = package.Workbook.Worksheets[0];
                    }
                    catch
                    {
                        // 如果索引获取失败，尝试通过名称获取或遍历所有工作表
                        if (package.Workbook.Worksheets.Count > 0)
                        {
                            // 尝试遍历所有工作表
                            foreach (var sheet in package.Workbook.Worksheets)
                            {
                                if (sheet != null && sheet.Dimension != null)
                                {
                                    worksheet = sheet;
                                    break;
                                }
                            }
                        }
                    }
                    
                    if (worksheet == null)
                    {
                        LogMessage(logAction, "Excel文件中未找到工作表，请检查文件格式");
                        return (false, resultList);
                    }

                    // 输出工作表信息，帮助调试
                    LogMessage(logAction, $"已找到工作表: {worksheet.Name}，行数: {(worksheet.Dimension != null ? worksheet.Dimension.Rows.ToString() : "未知")}");

                    // 统计数据行数
                    if (worksheet.Dimension == null)
                    {
                        LogMessage(logAction, "工作表结构异常，无法获取行数信息");
                        return (false, resultList);
                    }
                    
                    int rowCount = worksheet.Dimension.Rows;
                    if (rowCount <= 6) // 表头占用前6行
                    {
                        LogMessage(logAction, "Excel文件中没有数据行");
                        return (false, resultList);
                    }

                    LogMessage(logAction, $"开始解析金士达客户手续费变更表数据，共 {rowCount - 6} 行");

                    // 数据重复检查字典
                    Dictionary<string, int> kingstarDataCheck = new Dictionary<string, int>();
                    string investorName = "";
                    string investorId = "";
                    
                    // 尝试获取客户信息
                    try
                    {
                        investorName = worksheet.Cells[4, 2].Text.Trim();
                        investorId = worksheet.Cells[5, 2].Text.Trim();
                        
                        LogMessage(logAction, $"客户信息: {investorName}({investorId})");
                    }
                    catch (Exception ex)
                    {
                        LogMessage(logAction, $"获取客户基本信息失败: {ex.Message}");
                    }

                    // 从表格的数据行开始读取，本例中从第7行开始（交易所、品种等标题行为第7行）
                    // 真正的数据从第8行开始
                    for (int row = 8; row <= rowCount; row++)
                    {
                        try
                        {
                            if(worksheet.Cells[row, 1].Text.Trim().StartsWith("申请于"))
                            {
                                LogMessage(logAction, $"已到达数据末尾，跳过剩余行");
                                break;
                            }
                            // 第三列是交割期
                            string deliveryDate=worksheet.Cells[row, 3].Text.Trim();
                            string productId;
                            string instrumentId;
                            string productType="1";
                            // 当交割期不为空时，说明是指定合约
                            if(!string.IsNullOrEmpty(deliveryDate))
                            {
                                instrumentId = worksheet.Cells[row, 4].Text.Trim();
                                productId = extractProductId(instrumentId);
                            }else
                            {
                                instrumentId = "*";
                                productId =worksheet.Cells[row, 4].Text.Trim();
                            }
                            
                            // 解析费率和金额 - 根据表格的实际列调整
                            decimal openFeeRate = ParseDecimal(worksheet.Cells[row, 5].Text);  // 开仓手续费率
                            decimal openFeeAmt = ParseDecimal(worksheet.Cells[row, 6].Text);   // 开仓手续费额
                            
                            // 短线开仓和平仓与开仓相同
                            decimal shortOpenFeeRate = openFeeRate;
                            decimal shortOpenFeeAmt = openFeeAmt; 
                            decimal offsetFeeRate = openFeeRate;
                            decimal offsetFeeAmt = openFeeAmt;
                            
                            // 平今手续费率和平今手续费额
                            decimal otFeeRate = ParseDecimal(worksheet.Cells[row, 7].Text);
                            decimal otFeeAmt = ParseDecimal(worksheet.Cells[row, 8].Text);
                            
                            // 行权手续费默认为0
                            decimal execClearFeeRate = 0;
                            decimal execClearFeeAmt = 0;

                            // 检查必填字段
                            if (string.IsNullOrEmpty(investorId)  || string.IsNullOrEmpty(productType)||
                                string.IsNullOrEmpty(productId) || string.IsNullOrEmpty(instrumentId))
                            {
                                LogMessage(logAction, $"第{row}行数据不完整，投资者号、产品类型、产品代码、合约代码为必填项");
                                success = false;
                                return (success, resultList);
                            }

                            // 检查数据是否重复
                            string key = $"{investorId}|{productType}|{productId}|{instrumentId}";
                            if (kingstarDataCheck.ContainsKey(key))
                            {
                                LogMessage(logAction, $"金士达客户手续费变更表重复，请检查第{kingstarDataCheck[key]}行和第{row}行");
                                success = false;
                                return (success, resultList);
                            }
                            kingstarDataCheck.Add(key, row);

                            // 创建数据对象
                            var data = new KingstarSpecialTradeFeeDO
                            {
                                InvestorId = investorId,
                                InvestorName = investorName,
                                ExchCode = "",  // 交易所代码暂时留空
                                ProductType = productType,
                                ProductId = productId,
                                InstrumentId = instrumentId,
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
                                OperDate = DateTime.Now.ToString("yyyy-MM-dd"),
                                OperTime = DateTime.Now.ToString("HH:mm:ss")
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
                }
                return (success, resultList);
            }
            catch (Exception ex)
            {
                LogMessage(logAction, $"读取金士达客户手续费变更表Excel文件异常: {ex.Message}");
                return (false, resultList);
            }
        }

        /// <summary>
        /// 读取金士达客户手续费率变更表(期权)Excel文件
        /// </summary>
        /// <param name="directoryPath">文件目录</param>
        /// <param name="logAction">日志记录方法</param>
        /// <returns>处理结果和金士达客户手续费率变更表数据列表</returns>
        public static (bool success, List<KingstarSpecialTradeFeeDO> dataList) ReadKingstarSpecialTradeFeeExcelOptions(string directoryPath, LogMessageDelegate logAction)
        {
            List<KingstarSpecialTradeFeeDO> resultList = new List<KingstarSpecialTradeFeeDO>();
            bool success = true;

            try
            {
                // 查找最新的匹配文件
                string[] files = Directory.GetFiles(directoryPath, "*客户手续费变更表-期权.xlsx");
                if (files.Length == 0)
                {
                    LogMessage(logAction, "未找到金士达“客户手续费变更表-期权.xlsx”Excel文件");
                    return (false, resultList);
                }

                // 获取全部文件
                // string latestFile = files.OrderByDescending(f => f).First();
                // string fileName = Path.GetFileName(latestFile);
                foreach (var file in files)
                {
                    LogMessage(logAction, $"找到金士达客户手续费变更表-期权文件: {file}");

                    // 创建Excel应用程序实例
                    using (ExcelPackage package = new ExcelPackage(new FileInfo(file)))
                    {
                        // 获取工作表 - 修改为获取所有工作表并选择第一个非空的工作表
                        ExcelWorksheet worksheet = null;

                        // 先尝试通过索引获取
                        try
                        {
                            worksheet = package.Workbook.Worksheets[0];
                        }
                        catch
                        {
                            // 如果索引获取失败，尝试通过名称获取或遍历所有工作表
                            if (package.Workbook.Worksheets.Count > 0)
                            {
                                // 尝试遍历所有工作表
                                foreach (var sheet in package.Workbook.Worksheets)
                                {
                                    if (sheet != null && sheet.Dimension != null)
                                    {
                                        worksheet = sheet;
                                        break;
                                    }
                                }
                            }
                        }

                        if (worksheet == null)
                        {
                            LogMessage(logAction, "Excel文件中未找到工作表，请检查文件格式");
                            return (false, resultList);
                        }

                        // 输出工作表信息，帮助调试
                        LogMessage(logAction, $"已找到工作表: {worksheet.Name}，行数: {(worksheet.Dimension != null ? worksheet.Dimension.Rows.ToString() : "未知")}");

                        // 统计数据行数
                        if (worksheet.Dimension == null)
                        {
                            LogMessage(logAction, "工作表结构异常，无法获取行数信息");
                            return (false, resultList);
                        }

                        int rowCount = worksheet.Dimension.Rows;
                        if (rowCount <= 6) // 表头占用前6行
                        {
                            LogMessage(logAction, "Excel文件中没有数据行");
                            return (false, resultList);
                        }

                        LogMessage(logAction, $"开始解析金士达客户手续费变更表（期权）数据，共 {rowCount - 6} 行");

                        // 数据重复检查字典
                        Dictionary<string, int> kingstarDataCheck = new Dictionary<string, int>();
                        string investorName = "";
                        string investorId = "";

                        // 尝试获取客户信息
                        try
                        {
                            investorName = worksheet.Cells[4, 2].Text.Trim();
                            investorId = worksheet.Cells[5, 2].Text.Trim();

                            LogMessage(logAction, $"客户信息: {investorName}({investorId})");
                        }
                        catch (Exception ex)
                        {
                            LogMessage(logAction, $"获取客户基本信息失败: {ex.Message}");
                        }

                        // 从表格的数据行开始读取，本例中从第7行开始（交易所、品种等标题行为第7行）
                        // 真正的数据从第8行开始
                        for (int row = 8; row <= rowCount; row++)
                        {
                            try
                            {
                                if (worksheet.Cells[row, 1].Text.Trim().StartsWith("申请于"))
                                {
                                    LogMessage(logAction, $"已到达数据末尾，跳过剩余行");
                                    break;
                                }
                                string productId = worksheet.Cells[row, 3].Text.Trim();
                                string productType = "2";
                                // 期权时，合约为*
                                string instrumentId="*";
                                // 解析费率和金额 - 根据表格的实际列调整
                                decimal openFeeRate = 0;  // 开仓手续费率,期权按金额的都是0
                                decimal openFeeAmt = ParseDecimal(worksheet.Cells[row, 1].Text);   // 开仓手续费额

                                // 短线开仓和平仓与开仓相同
                                decimal shortOpenFeeRate = openFeeRate;
                                decimal shortOpenFeeAmt = openFeeAmt;
                                decimal offsetFeeRate = openFeeRate;
                                decimal offsetFeeAmt = openFeeAmt;

                                // 平今手续费率和平今手续费额
                                decimal otFeeRate = 0;
                                decimal otFeeAmt = ParseDecimal(worksheet.Cells[row, 5].Text);

                                // 行权手续费默认为0
                                decimal execClearFeeRate = 0;
                                decimal execClearFeeAmt = ParseDecimal(worksheet.Cells[row, 6].Text);

                                if(ParseDecimal(worksheet.Cells[row, 6].Text)!=ParseDecimal(worksheet.Cells[row, 7].Text))
                                {
                                    LogMessage(logAction, $"第{row}行数据异常，行权手续费额与履约手续费额不一致");
                                    success = false;
                                    return (success, resultList);
                                }

                                // 检查必填字段
                                if (string.IsNullOrEmpty(investorId) || string.IsNullOrEmpty(productType) ||
                                    string.IsNullOrEmpty(productId) || string.IsNullOrEmpty(instrumentId))
                                {
                                    LogMessage(logAction, $"第{row}行数据不完整，投资者号、产品类型、产品代码、合约代码为必填项");
                                    success = false;
                                    return (success, resultList);
                                }

                                // 检查数据是否重复
                                string key = $"{investorId}|{productType}|{productId}|{instrumentId}";
                                if (kingstarDataCheck.ContainsKey(key))
                                {
                                    LogMessage(logAction, $"金士达客户手续费变更表（期权）记录重复，请检查第{kingstarDataCheck[key]}行和第{row}行");
                                    success = false;
                                    return (success, resultList);
                                }
                                kingstarDataCheck.Add(key, row);

                                // 创建数据对象
                                var data = new KingstarSpecialTradeFeeDO
                                {
                                    InvestorId = investorId,
                                    InvestorName = investorName,
                                    ExchCode = "",  // 交易所代码暂时留空
                                    ProductType = productType,
                                    ProductId = productId,
                                    InstrumentId = instrumentId,
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
                                    OperDate = DateTime.Now.ToString("yyyy-MM-dd"),
                                    OperTime = DateTime.Now.ToString("HH:mm:ss")
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
                }
                return (success, resultList);
            }
            catch (Exception ex)
            {
                LogMessage(logAction, $"读取金士达客户手续费变更表（期权）Excel文件异常: {ex.Message}");
                return (false, resultList);
            }
        }
    }

}