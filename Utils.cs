using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static kingstar2femasfee.ExcelHelper;

namespace kingstar2femasfee
{
    public static class Utils
    {
        /// <summary>
        /// 日志记录委托
        /// </summary>
        /// <param name="message">日志消息</param>
        public delegate void LogMessageDelegate(string message);

        /// <summary>
        /// 解析小数值
        /// </summary>
        public static decimal ParseDecimal(string value)
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
        public static void LogMessage(LogMessageDelegate logAction, string message)
        {
            if (logAction != null)
            {
                logAction(message);
            }
        }
    }
}
