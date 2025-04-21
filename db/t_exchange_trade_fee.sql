-- Create table
create table T_EXCHANGE_TRADE_FEE
(
  id primary key,
  exch_code                    VARCHAR2(1) not null,
  product_type                 VARCHAR2(1) not null,
  product_id                   VARCHAR2(10) not null,
  option_series_id             VARCHAR2(30) not null,
  instrument_id                VARCHAR2(30) not null,
  hedge_flag                   VARCHAR2(1) not null,
  buy_sell                     VARCHAR2(1) default '*' not null,
  open_fee_rate                NUMBER(17,8),
  open_fee_amt                 NUMBER(17,8),
  short_open_fee_rate          NUMBER(17,8),
  short_open_fee_amt           NUMBER(17,8),
  offset_fee_rate              NUMBER(17,8),
  offset_fee_amt               NUMBER(17,8),
  ot_fee_rate                  NUMBER(17,8),
  ot_fee_amt                   NUMBER(17,8),
  exec_clear_fee_rate          NUMBER(17,8),
  exec_clear_fee_amt           NUMBER(17,8),
  oper_date                    VARCHAR2(8),
  oper_time                    VARCHAR2(8)
);
-- Add comments to the table 
comment on table T_EXCHANGE_TRADE_FEE
  is '交易所交易手续费';
-- Add comments to the columns 
comment on column T_EXCHANGE_TRADE_FEE.exch_code
  is '交易所代码';
comment on column T_EXCHANGE_TRADE_FEE.product_type
  is '产品类型';
comment on column T_EXCHANGE_TRADE_FEE.product_id
  is '产品代码';
comment on column T_EXCHANGE_TRADE_FEE.option_series_id
  is '期权系列';
comment on column T_EXCHANGE_TRADE_FEE.instrument_id
  is '合约代码';
comment on column T_EXCHANGE_TRADE_FEE.hedge_flag
  is '投保标识';
comment on column T_EXCHANGE_TRADE_FEE.buy_sell
  is '买卖标识';
comment on column T_EXCHANGE_TRADE_FEE.open_fee_rate
  is '开仓手续费率（按金额）';
comment on column T_EXCHANGE_TRADE_FEE.open_fee_amt
  is '开仓手续费额（按手数）';
comment on column T_EXCHANGE_TRADE_FEE.short_open_fee_rate
  is '短线开仓手续费率（按金额）';
comment on column T_EXCHANGE_TRADE_FEE.short_open_fee_amt
  is '短线开仓手续费额（按手数）';
comment on column T_EXCHANGE_TRADE_FEE.offset_fee_rate
  is '平仓手续费率（按金额）';
comment on column T_EXCHANGE_TRADE_FEE.offset_fee_amt
  is '平仓手续费额（按手数）';
comment on column T_EXCHANGE_TRADE_FEE.ot_fee_rate
  is '平今手续费率（按金额）';
comment on column T_EXCHANGE_TRADE_FEE.ot_fee_amt
  is '平今手续费额（按手数）';
comment on column T_EXCHANGE_TRADE_FEE.exec_clear_fee_rate
  is '行权结算手续费率';
comment on column T_EXCHANGE_TRADE_FEE.exec_clear_fee_amt
  is '行权结算每手费额';
comment on column T_EXCHANGE_TRADE_FEE.oper_date
  is '操作日期';
comment on column T_EXCHANGE_TRADE_FEE.oper_time
  is '操作时间';
-- Create/Recreate primary, unique and foreign key constraints 
alter table T_EXCHANGE_TRADE_FEE
  add unique index idx_EXCHANGE_TRADE_FEE (EXCH_CODE, PRODUCT_TYPE, HEDGE_FLAG, OPTION_SERIES_ID, PRODUCT_ID, INSTRUMENT_ID, BUY_SELL);
