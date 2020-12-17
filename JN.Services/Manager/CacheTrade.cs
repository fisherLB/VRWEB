using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvcCore.Extensions;
using JN.Data.Service;
using System.Data.Entity.SqlServer;
using JN.Data.Enum;
using JN.Data;

namespace JN.Services.Manager
{
    /// <summary>
    /// time:2017年8月29日 18:30:04     name:lin  alt:交易所缓存
    /// </summary>
    public class CacheTransactionDataHelper
    {

        private static string prefixKey = "Trade_";

        #region 获取写入产品列表

        /// <summary>
        /// 获取产品列表（买卖币）
        /// </summary>
        /// <returns></returns>
        public static List<Data.Currency> GetCodeList()
        {
            string key = prefixKey + "CurrencyList";


            if (CacheExtensions.CheckCache(key))
            {
                return CacheExtensions.GetCache<List<Data.Currency>>(key);
            }

            return new List<Data.Currency>();
        }


        /// <summary>
        /// 写入产品列表（买卖币）
        /// </summary>
        /// <param name="list"></param>
        public static void SetCodeList(List<Data.Currency> list)
        {
            string key = prefixKey + "CurrencyList";

            CacheExtensions.SetCache(key, list);
        } 


        #endregion

        #region 获取写入产品Model

        /// <summary>
        /// 获取产品（买卖币）
        /// </summary>
        /// <returns></returns>
        public static Data.Currency GetCurrencyModel(int code)
        {
            string key = prefixKey + "CurrencyModel"+code;


            if (CacheExtensions.CheckCache(key))
            {
                return CacheExtensions.GetCache<Currency>(key);
            }
            else {
                var data = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.ID == code);
                if (data != null)
                {
                    SetCurrencyModel(data.ID, data);
                    return data;
                }
                else {
                    return null;
                }
            }

        }


        /// <summary>
        /// 写入产品（买卖币）
        /// </summary>
        /// <param name="list"></param>
        public static void SetCurrencyModel(int code, Data.Currency c)
        {
            string key = prefixKey + "CurrencyModel"+code;
            CacheExtensions.SetCache(key, c);
        }


        #endregion

        #region 获取和写入当天已成交的记录
        /// <summary>
        /// 获取当天的成交数据
        /// </summary>
        /// <returns></returns>
        public static List<AdvertiseOrder> GetTradeListDay()
        {
            string key = prefixKey + "_StockTradeDayList";

            if (CacheExtensions.CheckCache(key))
            {
                return CacheExtensions.GetCache<List<AdvertiseOrder>>(key);
            }
            else
            {
                var list = MvcCore.Unity.Get<JN.Data.Service.IAdvertiseOrderService>().List(x => x.Status >= (int)JN.Data.Enum.AdvertiseOrderStatus.GoodsReceived && SqlFunctions.DateDiff("DAY", (x.ConfirmPayTime ?? DateTime.Now), DateTime.Now) == 0).ToList();
                CacheExtensions.SetCache(key, list);
                return list;
            }
        }

        /// <summary>
        /// 写入当天的成交数据
        /// </summary>
        public static void SetTradeListDay()
        {
            string key = prefixKey + "_StockTradeDayList";
            var list = MvcCore.Unity.Get<JN.Data.Service.IAdvertiseOrderService>().List(x => x.Status >= (int)JN.Data.Enum.AdvertiseOrderStatus.GoodsReceived && SqlFunctions.DateDiff("DAY", (x.ConfirmPayTime ?? DateTime.Now), DateTime.Now) == 0).ToList();
            CacheExtensions.SetCache(key, list);
        } 
        #endregion

        #region 获取和写入买单委托记录【未成交】
        /// <summary>
        /// 获取买单委托记录（未成交）[50条]
        /// </summary>
        /// <param name="code">产品ID号</param>
        /// <returns></returns>
        public static List<StockEntrustsTrade> GetBuyStockEntrusts(int code)
        {
            string key = prefixKey + "_NotBuyStockEntrustsList"+code;

            if (CacheExtensions.CheckCache(key))
            {
                return CacheExtensions.GetCache<List<StockEntrustsTrade>>(key);
            }
            else
            {
                var list = MvcCore.Unity.Get<JN.Data.Service.IStockEntrustsTradeService>().List(x => x.Direction == 0 && x.Status <= (int)TTCStatus.PartOfTheDeal && x.Status >= (int)TTCStatus.Entrusts && x.CurID == code).OrderByDescending(x => x.ID).Take(50).ToList();//获取列表
                CacheExtensions.SetCache(key, list);
                return list;
            }
        }

        /// <summary>
        /// 写入买单委托记录（未成交）[50条]
        /// </summary>
        /// <param name="code">产品ID号</param>
        /// <returns></returns>
        public static void SetBuyStockEntrusts(int code)
        {
            string key = prefixKey + "_NotBuyStockEntrustsList" + code;
            var list = MvcCore.Unity.Get<JN.Data.Service.IStockEntrustsTradeService>().List(x => x.Direction == 0 && x.Status <= (int)TTCStatus.PartOfTheDeal && x.Status >= (int)TTCStatus.Entrusts && x.CurID == code).OrderByDescending(x => x.ID).Take(50).ToList();//获取列表
            CacheExtensions.SetCache(key, list);
        } 
        #endregion

        #region 获取和写入卖单委托记录【未成交】
        /// <summary>
        /// 获取买单委托记录（未成交）[50条]
        /// </summary>
        /// <param name="code">产品ID号</param>
        /// <returns></returns>
        public static List<StockEntrustsTrade> GetSellStockEntrusts(int code)
        {
            string key = prefixKey + "_NotSellStockEntrustsList" + code;

            if (CacheExtensions.CheckCache(key))
            {
                return CacheExtensions.GetCache<List<StockEntrustsTrade>>(key);
            }
            else
            {
                var list = MvcCore.Unity.Get<JN.Data.Service.IStockEntrustsTradeService>().List(x => x.Direction == 1 && x.Status <= (int)TTCStatus.PartOfTheDeal && x.Status >= (int)TTCStatus.Entrusts && x.CurID == code).OrderByDescending(x => x.ID).Take(50).ToList();//获取列表
                CacheExtensions.SetCache(key, list);
                return list;
            }
        }

        /// <summary>
        /// 写入买单委托记录（未成交）[50条]
        /// </summary>
        /// <param name="code">产品ID号</param>
        /// <returns></returns>
        public static void SetSellStockEntrusts(int code)
        {
            string key = prefixKey + "_NotSellStockEntrustsList" + code;
            var list = MvcCore.Unity.Get<JN.Data.Service.IStockEntrustsTradeService>().List(x => x.Direction == 1 && x.Status <= (int)TTCStatus.PartOfTheDeal && x.Status >= (int)TTCStatus.Entrusts && x.CurID == code).OrderByDescending(x => x.ID).Take(50).ToList();//获取列表
            CacheExtensions.SetCache(key, list);
        }
        #endregion

        #region 获取和写入已成交记录【已成交】
        /// <summary>
        /// 获取已成交记录（已成交）[50条]
        /// </summary>
        /// <param name="code">产品ID号</param>
        /// <returns></returns>
        public static List<StockTrade> GetStockTrade(int code)
        {
            string key = prefixKey + "_StockTradeList" + code;

            if (CacheExtensions.CheckCache(key))
            {
                return CacheExtensions.GetCache<List<StockTrade>>(key);
            }
            else
            {
                var list = MvcCore.Unity.Get<JN.Data.Service.IStockTradeService>().List(x =>  x.CurID == code).OrderByDescending(x => x.ID).Take(50).ToList();//获取列表
                CacheExtensions.SetCache(key, list);
                return list;
            }
        }

        /// <summary>
        /// 写入已成交记录（已成交）[50条]
        /// </summary>
        /// <param name="code">产品ID号</param>
        /// <returns></returns>
        public static void SetStockTrade(int code)
        {
            string key = prefixKey + "_StockTradeList" + code;
            var list = MvcCore.Unity.Get<JN.Data.Service.IStockTradeService>().List(x=> x.CurID == code).OrderByDescending(x => x.ID).Take(50).ToList();//获取列表
            CacheExtensions.SetCache(key, list);
        }
        #endregion

        #region 获取和写入实时价格行情数据
        /// <summary>
        /// 获取实时价格行情数据
        /// </summary>
        /// <param name="code">产品ID号</param>
        /// <returns></returns>
        public static StockChartDay GetPriceModel(int code)
        {
            //缓存加入日期 By Annie: 2017.08.08
            string key = prefixKey + "_PriceModel" + code + DateTime.Now.ToString("yyyy-MM-dd");

            if (CacheExtensions.CheckCache(key))
            {
                return CacheExtensions.GetCache<StockChartDay>(key);
            }
            else
            {
                //查找今天的数据 By Annie:2017.08.08
                var model = MvcCore.Unity.Get<IStockChartDayService>().List(x => x.CurID == code && SqlFunctions.DateDiff("DAY", x.StockDate, DateTime.Now) == 0).OrderByDescending(x => x.StockDate).ToList().FirstOrDefault();
                if (model != null)
                {
                    //写进缓存
                    SetPriceModel(code, model);
                }
                else
                {
                    var currency = MvcCore.Unity.Get<ICurrencyService>().Single(x=>x.ID==code);
                    model = new Data.StockChartDay
                    {
                        ClosePrice = currency.OriginalPrice,
                        OpenPrice = currency.OriginalPrice,
                        CurID = code,
                        LowPrice = currency.OriginalPrice,
                        HightPrice = currency.OriginalPrice,
                        Turnover = 0,
                        TotalStock = 0,
                        UpsAndDownsPrice = 0,
                        UpsAndDownsScale = 0,
                        YesterdayClosePrice = 0,
                        Volume = 0,
                        CreateTime = DateTime.Now
                    };
                    SetPriceModel(code, model);
                }
                return model;
            }
        }

        /// <summary>
        /// 写入实时价格行情数据
        /// </summary>
        /// <param name="code">产品ID号</param>
        /// <returns></returns>
        public static void SetPriceModel(int code, StockChartDay StockChartDayModel)
        {
            string key = prefixKey + "_PriceModel" + code + DateTime.Now.ToString("yyyy-MM-dd");//根据日期设置缓存 By Annie:2017.08.08
            CacheExtensions.SetCache(key, StockChartDayModel);
        }
        #endregion

        #region 获取和写入实时价格行情数据全部
        /// <summary>
        /// 获取实时价格行情数据全部
        /// </summary>
        /// <returns></returns>
        public static List<StockChartDay> GetPriceModelAll()
        {
            //缓存加入日期 By Annie: 2017.08.08
            string key = prefixKey + "_PriceModel"+ DateTime.Now.ToString("yyyy-MM-dd");

            if (CacheExtensions.CheckCache(key))
            {
                return CacheExtensions.GetCache<List<StockChartDay>>(key);
            }
            else
            {
                
                List<JN.Data.StockChartDay> modellist = new List<StockChartDay>();
              
                    var currencylist = MvcCore.Unity.Get<ICurrencyService>().List(x => x.TranSwitch && !x.IsICO).ToList();
                    foreach (var item in currencylist)
                    {
                       var stockmodel= MvcCore.Unity.Get<IStockChartDayService>().Single(x => SqlFunctions.DateDiff("DAY", x.StockDate, DateTime.Now) == 0 && x.CurID == item.ID);
                       if (stockmodel == null)
                       {

                            stockmodel = new Data.StockChartDay
                           {
                               ClosePrice = item.OriginalPrice,
                               OpenPrice = item.OriginalPrice,
                               CurID = item.ID,
                               LowPrice = item.OriginalPrice,
                               HightPrice = item.OriginalPrice,
                               Turnover = 0,
                               UpOrDown = "",
                               CurImages = item.CurrencyLogo,
                               RegionID = 0,
                               TotalStock = 0,
                               CurEnglish = item.EnSigns,
                               CurName = item.CurrencyName,
                               TotalValue = item.TotalIssued * item.OriginalPrice,
                               UpsAndDownsPrice = 0,
                               UpsAndDownsScale = 0,
                               YesterdayClosePrice = 0,
                               Volume = 0,
                               CreateTime = DateTime.Now
                           };
                          
                       }
                       modellist.Add(stockmodel);
                       
                    }

                    //查找今天的数据 By Annie:2017.08.08
                    //var newmodellist = MvcCore.Unity.Get<IStockChartDayService>().List(x =>  SqlFunctions.DateDiff("DAY", x.StockDate, DateTime.Now) == 0).OrderByDescending(x => x.StockDate).ToList();
                    SetPriceModelAll(modellist);
                    return modellist;
                }
                
        }

        /// <summary>
        /// 写入实时价格行情数据全部币种
        /// </summary>
        /// <param name="StockChartDayModel">产品列表</param>
        /// <returns></returns>
        public static void SetPriceModelAll(List<StockChartDay> StockChartDayModel)
        {
            string key = prefixKey + "_PriceModel"+DateTime.Now.ToString("yyyy-MM-dd");//根据日期设置缓存 By Annie:2017.08.08
            CacheExtensions.SetCache(key, StockChartDayModel);
        }
        #endregion


    }
}
