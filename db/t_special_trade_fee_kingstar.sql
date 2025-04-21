-- Create table
create table T_SPECIAL_TRADE_FEE_KINGSTAR
(
  id primary key,
  investor_id                  VARCHAR2(18) not null,
  investor_name                VARCHAR2(100),
  exch_code                    VARCHAR2(1),
  product_type                 VARCHAR2(1) not null,
  product_id                   VARCHAR2(10) not null,
  instrument_id                VARCHAR2(30) not null,
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
comment on table T_SPECIAL_TRADE_FEE
  is '金士达特殊交易手续费';
-- Add comments to the columns 
comment on column T_SPECIAL_TRADE_FEE.investor_id
  is '投资者号';
comment on column T_SPECIAL_TRADE_FEE.investor_name
  is '投资者名称';
comment on column T_SPECIAL_TRADE_FEE.exch_code
  is '交易所代码';
comment on column T_SPECIAL_TRADE_FEE.product_type
  is '产品类型';
comment on column T_SPECIAL_TRADE_FEE.product_id
  is '产品代码';
comment on column T_SPECIAL_TRADE_FEE.instrument_id
  is '合约代码';
comment on column T_SPECIAL_TRADE_FEE.open_fee_rate
  is '开仓手续费率（按金额）';
comment on column T_SPECIAL_TRADE_FEE.open_fee_amt
  is '开仓手续费额（按手数）';
comment on column T_SPECIAL_TRADE_FEE.short_open_fee_rate
  is '短线开仓手续费率（按金额）';
comment on column T_SPECIAL_TRADE_FEE.short_open_fee_amt
  is '短线开仓手续费额（按手数）';
comment on column T_SPECIAL_TRADE_FEE.offset_fee_rate
  is '平仓手续费率（按金额）';
comment on column T_SPECIAL_TRADE_FEE.offset_fee_amt
  is '平仓手续费额（按手数）';
comment on column T_SPECIAL_TRADE_FEE.ot_fee_rate
  is '平今手续费率（按金额）';
comment on column T_SPECIAL_TRADE_FEE.ot_fee_amt
  is '平今手续费额（按手数）';
comment on column T_SPECIAL_TRADE_FEE.exec_clear_fee_rate
  is '行权手续费率（按金额）';
comment on column T_SPECIAL_TRADE_FEE.exec_clear_fee_amt
  is '行权手续费额（按手数）';
comment on column T_SPECIAL_TRADE_FEE.oper_date
  is '操作日期';
comment on column T_SPECIAL_TRADE_FEE.oper_time
  is '操作时间';
-- Create/Recreate primary, unique and foreign key constraints 
alter table T_SPECIAL_TRADE_FEE
  add unique index idx_SPECIAL_TRADE_FEE (INVESTOR_ID, PRODUCT_TYPE, PRODUCT_ID, INSTRUMENT_ID);
