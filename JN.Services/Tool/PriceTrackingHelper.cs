using System;
using System.Linq;
using JN.Data.Service;
using System.Data.Entity.Validation;
using System.Text;
using Newtonsoft.Json;
using JN.Data;

namespace JN.Services.Tool
{
    public class PriceTrackingHelper
    {    
        /// <summary>
        /// 检测队列中的数据并更新到数据库
        /// </summary>
        public static void SubmitDataFromQueue()
        {
            var priceTrackingService = new PriceTrackingService(new SysDbFactory());

            SysDbFactory dbFactory = new SysDbFactory();
            var priceTracking5Service = new PriceTracking5MinService(new SysDbFactory());
            var priceTracking480Service = new PriceTracking300MinService(new SysDbFactory());

            try
            {
                //获取成交列表(当天)
                var transactionDataList = Manager.CacheTransactionDataHelper.GetTradeListDay(); 

                foreach (var item in transactionDataList.GroupBy(x => x.CurID))//循环分组
                {
                    //查找这个分组里面的币种ID号
                    var c = Manager.CacheTransactionDataHelper.GetCurrencyModel(item.Key);
                    if (c == null) return;

                    DateTime servertime = DateTime.Now;
                    var starttime = DateTime.Now.AddMinutes(-1);               
                    if (servertime.Minute % 5 == 0)
                    {
                        starttime = servertime.AddMinutes(-5);
                        var v = item.Where(d => (d.ConfirmPayTime ?? DateTime.Now) >= starttime).ToList(); //取5分钟数据
                        if (v.Count > 0)
                        {
                            var currpricetrack = item.Where(d => (d.ConfirmPayTime ?? DateTime.Now) >= starttime).FirstOrDefault();
                            PriceTracking5Min priceTracking5 = new PriceTracking5Min();
                            priceTracking5.CurrPrice = currpricetrack.Price.ToDouble();
                            priceTracking5.MaxPrice = v.Max(x => x.Price).ToDouble();
                            priceTracking5.MinPrice = v.Min(x => x.Price).ToDouble();
                            priceTracking5.OpenPrice = v.LastOrDefault().Price.ToDouble();
                            priceTracking5.ClosePrice = currpricetrack.Price.ToDouble();
                            priceTracking5.ProductID = c.ID;
                            priceTracking5.SourceStr = "";
                            priceTracking5.Volume = v.Sum(x => x.Quantity);
                            priceTracking5.Time = servertime;
                            priceTracking5Service.Add(priceTracking5);
                            priceTracking5Service.Commit();
                            //写进缓存
                            Manager.CachePriceTrackMinKlin.Set5Min(priceTracking5);
                        }
                    }
                 
                    if (servertime.Hour  % 5 == 0)
                    {
                        starttime = servertime.AddHours(-8);
                        var v = item.Where(d => (d.ConfirmPayTime ?? DateTime.Now) >= starttime).ToList(); //取480分钟数据(8小时)
                        if (v.Count > 0)
                        {
                            var currpricetrack = item.Where(d => (d.ConfirmPayTime ?? DateTime.Now) >= starttime).FirstOrDefault();
                            PriceTracking300Min priceTracking300 = new PriceTracking300Min();
                            priceTracking300.CurrPrice = currpricetrack.Price.ToDouble();
                            priceTracking300.MaxPrice = v.Max(x => x.Price).ToDouble();
                            priceTracking300.MinPrice = v.Min(x => x.Price).ToDouble();
                            priceTracking300.OpenPrice = v.LastOrDefault().Price.ToDouble();
                            priceTracking300.ClosePrice = currpricetrack.Price.ToDouble();
                            priceTracking300.ProductID = c.ID;
                            priceTracking300.SourceStr = "";
                            priceTracking300.Volume = v.Sum(x => x.Quantity);
                            priceTracking300.Time = servertime;
                            priceTracking480Service.Add(priceTracking300);
                            priceTracking480Service.Commit();
                            //写进缓存
                            Manager.CachePriceTrackMinKlin.Set480Min(priceTracking300);
                        }                       
                    }
                }
               // }
            }
            catch (DbEntityValidationException ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("*** DbEntityValidationException.EntityValidationErrors ***");
                sb.AppendLine(JsonConvert.SerializeObject(ex.EntityValidationErrors
                            .SelectMany(o => o.ValidationErrors)
                            .Select(o => o.ErrorMessage).ToArray()));
                //此状态下Log无法使用
            }
            catch (Exception ex)
            {
                //此状态下Log无法使用
            }
            finally
            {
            }
        }

    }
}
