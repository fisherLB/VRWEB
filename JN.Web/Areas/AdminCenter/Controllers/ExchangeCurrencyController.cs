using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using JN.Services.Tool;
using System.IO;
using JN.Services.Manager;
using System.Data.Entity.Validation;
using Newtonsoft.Json.Linq;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    [ValidateInput(false)]
    public class ExchangeCurrencyController : BaseController
    {
        private readonly ISysDBTool SysDBTool;
        private readonly IExchangeCurrencyService ExchangeCurrencyService;
        private readonly ICurrencyService CurrencyService;

        public ExchangeCurrencyController(ISysDBTool SysDBTool,
            IExchangeCurrencyService ExchangeCurrencyService,
            ICurrencyService CurrencyService)
        {
            this.SysDBTool = SysDBTool;
            this.ExchangeCurrencyService = ExchangeCurrencyService;
            this.CurrencyService = CurrencyService;
        }

     
        public ActionResult OffSales(int? page)
        {
            ActMessage = "下架规则管理";
            var list = ExchangeCurrencyService.List(x => x.IsUse == false).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            return View("RulerList", list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }

        public ActionResult ExchangeCommand(int id, string commandtype)
        {
            Data.ExchangeCurrency model = ExchangeCurrencyService.Single(id);
            
            if (commandtype.ToLower() == "onsales")
                model.IsUse = true;
            else if (commandtype.ToLower() == "offsales")
                model.IsUse = false;
            ExchangeCurrencyService.Update(model);
            SysDBTool.Commit();
            Users.ClearCacheAll();//清空缓存
            return RedirectToAction("RulerList", "ExchangeCurrency");
        }

        public ActionResult Delete(int id)
        {
            var model = ExchangeCurrencyService.Single(id);
            if (model != null)
            {
                ActPacket = model;
                ExchangeCurrencyService.Delete(id);
                SysDBTool.Commit();
                Users.ClearCacheAll();//清空缓存
                ViewBag.SuccessMsg = "“" + model.ExchangeName + "”已被删除！";
                ActMessage = ViewBag.SuccessMsg;
                return View("Success");
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }

        public ActionResult Modify(int? id)
        {
            ActMessage = "编辑规则";
            if (id.HasValue)
                return View(ExchangeCurrencyService.Single(id));
            else
            {
                ActMessage = "添加规则";
                return View(new Data.ExchangeCurrency());
            }
        }

        [HttpPost]
        public ActionResult Modify(FormCollection fc)
        {
            try
            {
                var entity = ExchangeCurrencyService.SingleAndInit(fc["ID"].ToInt());
                int FromCoinID=(fc["FromCoinID"] ?? "0").ToInt();
                int ToCoinID = (fc["ToCoinID"] ?? "0").ToInt();
                TryUpdateModel(entity, fc.AllKeys);
              
                if(entity.ToCoinID==entity.FromCoinID)
                {
                    ViewBag.ErrorMsg = "目标币种不能和兑换币种相同";
                    return View("Error");
                }

                if(entity.Rate<=0&&(entity.ISEchangeRate??false)==false)
                {
                    ViewBag.ErrorMsg = "请输入正确的兑换比例";
                    return View("Error");
                }
                // entity.ClassId = 0;
                if (entity.ID > 0)
                {
                    var list = ExchangeCurrencyService.List(x => x.FromCoinID == entity.FromCoinID && x.ToCoinID == entity.ToCoinID && x.ID!=entity.ID);
                    if (list.Any())
                    {
                        ViewBag.ErrorMsg = "已存在相同规则";
                        return View("Error");
                    }
                    entity.FromCoinName = CurrencyService.Single(x => x.ID == entity.FromCoinID).CurrencyName;
                    entity.ToCoinName = CurrencyService.Single(x => x.ID == entity.ToCoinID).CurrencyName;

                    ExchangeCurrencyService.Update(entity);
                }
                else
                {
                    var list = ExchangeCurrencyService.List(x => x.FromCoinID == entity.FromCoinID && x.ToCoinID == entity.ToCoinID);
                    if(list.Any())
                    {
                        ViewBag.ErrorMsg = "已存在相同规则";
                        return View("Error");
                    }
                    entity.ToCoinName = CurrencyService.Single(x => x.ID == entity.ToCoinID).CurrencyName;
                    entity.FromCoinName = CurrencyService.Single(x => x.ID == entity.FromCoinID).CurrencyName;

                    entity.IsUse = true;
                    entity.CreateTime = DateTime.Now;
                    ExchangeCurrencyService.Add(entity);
                }
                SysDBTool.Commit();
                Users.ClearCacheAll();//清空缓存
                ViewBag.SuccessMsg = "规则修改/添加成功！";
                return View("Success");
            }
            catch (DbEntityValidationException ex)
            {
                logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
                ViewBag.ErrorMsg = "系统异常！请查看系统日志。";
                return View("Error");
            }
        }

        public ActionResult RulerList(int? page)
        {
            ActMessage = "规则管理";
            ViewBag.Title = ActMessage;
            var list = ExchangeCurrencyService.List(x => x.IsUse??true).WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));           
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }

        [HttpPost]
        public ActionResult GetEchangeRate()
        {
            ReturnResult result = new ReturnResult();
            int id = Request["id"].ToInt();
            try
            {
               
                int fromCoinID = Request["fromCoinID"].ToInt();
                int toCoinID = Request["toCoinID"].ToInt();
                //var coinList = MD_CacheEntrustsData.GetCurrencyList();
                decimal Rate = 0;// Stocks.GetEchangeRate(coinList.Single(x => x.ID == fromCoinID), coinList.Single(x => x.ID == toCoinID));
                //var fromCoinUSDPrice = JN.Services.Manager.Stocks.getcurrentprice(coinList.Single(x => x.ID == fromCoinID));
                //var toCoinUSDPrice = JN.Services.Manager.Stocks.getcurrentprice(coinList.Single(x => x.ID == toCoinID));
                //if (toCoinUSDPrice > 0 && fromCoinUSDPrice > 0)
                if (Rate>0)
                {
                    //string Rate = (fromCoinUSDPrice / toCoinUSDPrice).ToString("F5");
                    result.Status = 200;
                    //result.Message = "{\"id\":\"" + id + "\",\"Rate\":\"" + Rate + "\"}";
                    result.Message = "{\"id\":\"" + id + "\",\"Rate\":\"" + Rate.ToString("F5") + "\"}";
                }
                else
                {
                    result.Message = "{\"id\":\"" + id + "\"}";
                }
            }
            catch
            {
                result.Message = "{\"id\":\"" + id + "\"}";
            }
            return Json(result);
        }

    }
}
