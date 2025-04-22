using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kingstar2femasfee
{
    /// <summary>
    /// 产品数据对象
    /// </summary>
    public class ProductDO
    {
        public string ExchCode { get; set; }
        public string ProductType { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string UnderlyingId { get; set; }
        public decimal? UnderlyingMultiple { get; set; }
        public string OfferCurrency { get; set; }
        public string SettleCurrency { get; set; }
        public string IsSpecial { get; set; }
        public decimal? VolumeMultiple { get; set; }
        public string MarketId { get; set; }
        public string IsTradingRightSpecial { get; set; }
        public string UnderlyingType { get; set; }
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

    /// <summary>
    /// 特殊交易手续费率数据对象
    /// </summary>
    public class SpecialTradeFeeDO
    {
        public string InvestorId { get; set; }
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
        public string FollowType { get; set; }
        public decimal MultipleRatio { get; set; }
        public string OperDate { get; set; }
        public string OperTime { get; set; }
    }

    /// <summary>
    /// 金士达特殊交易手续费数据对象
    /// </summary>
    public class KingstarSpecialTradeFeeDO
    {
        public string InvestorId { get; set; }
        public string InvestorName { get; set; }
        public string ExchCode { get; set; }
        public string ProductType { get; set; }
        public string ProductId { get; set; }
        public string InstrumentId { get; set; }
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

    /// <summary>
    /// 金士达特殊交易手续费浮动数据对象
    /// </summary>
    public class KingstarSpecialTradeFeeFloatDO
    {
        public string CheckResult { get; set; }
        public string CheckCode { get; set; }
        public string InvestorId { get; set; }
        public string InvestorName { get; set; }
        public string ExchCode { get; set; }
        public string ProductType { get; set; }
        public string ProductId { get; set; }
        public string InstrumentId { get; set; }
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
        public string FollowType { get; set; }
        public string OperDate { get; set; }
        public string OperTime { get; set; }
    }

}
