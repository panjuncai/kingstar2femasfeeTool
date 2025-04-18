using System;
using System.ComponentModel;

namespace kingstar2femasfee
{
    /// <summary>
    /// 交易所枚举
    /// </summary>
    public enum ExchangeEnum
    {
        [Description("中金所")]
        CFFEX = 'J',
        
        [Description("大商所")]
        DCE = 'D',
        
        [Description("广期所")]
        GFEX = 'G',
        
        [Description("郑商所")]
        CZCE = 'Z',
        
        [Description("上期所")]
        SHFE = 'S',
        
        [Description("能源中心")]
        INE = 'N'
    }

    /// <summary>
    /// 产品类型枚举
    /// </summary>
    public enum ProductTypeEnum
    {
        [Description("*")]
        All = '*',
        
        [Description("期货")]
        Futures = '1',
        
        [Description("期权")]
        Options = '2'
    }

    /// <summary>
    /// 投保标识枚举
    /// </summary>
    public enum HedgeFlagEnum
    {
        [Description("*")]
        All = '*',
        
        [Description("交易/投机")]
        Speculation = '1',
        
        [Description("套利")]
        Arbitrage = '2',
        
        [Description("套保")]
        Hedge = '3'
    }

    /// <summary>
    /// 买卖标识枚举
    /// </summary>
    public enum BuySellEnum
    {
        [Description("*")]
        All = '*',
        
        [Description("买")]
        Buy = '0',
        
        [Description("卖")]
        Sell = '1'
    }

    public enum isFllowEnum
    {
        [Description("*")]
        All = '*',

        [Description("不跟随")]
        No = '0',

        [Description("跟随")]
        Yes = '1'
    }

    /// <summary>
    /// 枚举辅助类
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// 获取枚举项的描述信息
        /// </summary>
        public static string GetDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

        /// <summary>
        /// 根据描述获取对应的枚举项
        /// </summary>
        public static TEnum GetEnumFromDescription<TEnum>(string description) where TEnum : Enum
        {
            foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
            {
                if (GetDescription(value) == description)
                {
                    return value;
                }
            }
            throw new ArgumentException($"在枚举{typeof(TEnum).Name}中找不到描述为'{description}'的枚举项");
        }

        /// <summary>
        /// 根据描述获取对应的字符值
        /// </summary>
        public static char GetCharFromDescription<TEnum>(string description) where TEnum : Enum
        {
            TEnum enumValue = GetEnumFromDescription<TEnum>(description);
            return Convert.ToChar(enumValue);
        }
        
        /// <summary>
        /// 根据枚举Code获取对应的描述信息
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <param name="code">枚举的字符Code值</param>
        /// <returns>对应的描述信息</returns>
        public static string GetDescriptionByCode<TEnum>(char code) where TEnum : Enum
        {
            foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
            {
                if (Convert.ToChar(value) == code)
                {
                    return GetDescription(value);
                }
            }
            return code.ToString();
        }
    }
} 