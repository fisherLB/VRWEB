using JN.Data.Service;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using JN.Services.Filters;
using JN.Services.Manager;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class MarketController : BaseController
    {
        private static List<Data.SysParam> cacheSysParam = null;

        private readonly IUserService UserService;
        private readonly ISupplyHelpService SupplyHelpService;
        private readonly IAcceptHelpService AcceptHelpService;
        private readonly IMatchingService MatchingService;
        private readonly ISysSettingService SysSettingService;
        private readonly IBonusDetailService BonusDetailService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;

        public MarketController(ISysDBTool SysDBTool,
            IUserService UserService,
            ISupplyHelpService SupplyHelpService,
            IAcceptHelpService AcceptHelpService,
            IMatchingService MatchingService,
            ISysSettingService SysSettingService,
            IBonusDetailService BonusDetailService,
            IActLogService ActLogService)
        {
            this.UserService = UserService;
            this.SupplyHelpService = SupplyHelpService;
            this.AcceptHelpService = AcceptHelpService;
            this.MatchingService = MatchingService;
            this.SysSettingService = SysSettingService;
            this.BonusDetailService = BonusDetailService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparams", x => x.ID < 8000).ToList();
        }
        /// <summary>
        /// 已匹配数据
        /// </summary>
        public ActionResult MatchdList(int? page)
        {
            ActMessage = "已匹配数据";
            string strstatus = Request["ac"];
            int status = 0;
            if (strstatus == "overduepayment")
            {
                status = 1;
            }
            if (strstatus == "overdueverified")
            {
                status = -3;
            }
            if (strstatus == "falseorders")
            {
                status = -2;
            }
            if (strstatus == "cancel")
            {
                status = -1;
            }

            ActMessage = "已匹配数据";
            var list = MatchingService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query))).OrderByDescending(x => x.ID).ToList();
            //if (!String.IsNullOrEmpty(strstatus))
            if (status == 0)
            {
                list = list.Where(x => x.Status > 0).ToList();
            }
            else if (status == 1)
            {

                int SPayendhour = cacheSysParam.SingleAndInit(x => x.ID == 3206).Value.ToInt(); //付款时限参数 大单
                list = list.Where(x => x.Status == status && ((DateTime.Now - x.CreateTime).Minutes > SPayendhour)).ToList();
            }
            else if (status == 3)
            {
                list = list.Where(x => x.Status == status && x.VerifiedEndTime < DateTime.Now).ToList();
            }
            else if (status == -2)
            {
                list = list.Where(x => x.Status == status).ToList();
            }
            else if (status == -1)
            {
                list = list.Where(x => x.Status == status).ToList();
            }
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.ToPagedList(page ?? 1, 20));
        }


        /// <summary>
        /// 匹配管理
        /// </summary>
        public ActionResult PPList()
        {
            ActMessage = "匹配管理";
            ActMessage = "小单匹配管理";
            return View();
        }

        public ActionResult PPList2()
        {
            ActMessage = "大单匹配管理";
            return View();
        }

        /// <summary>
        /// 提供帮助列表
        /// </summary>
        public ActionResult SupplyHelp(int? page)
        {
            ActMessage = "提供帮助列表";
            var list = SupplyHelpService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }


        /// <summary>
        /// 接受帮助列表
        /// </summary>
        public ActionResult AcceptHelp(int? page)
        {
            ActMessage = "接受帮助列表";
            var list = AcceptHelpService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            if (Request["IsExport"] == "1")
            {
                string FileName = string.Format("{0}_{1}_{2}_{3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                MvcCore.Extensions.ExcelHelperV2.ToExcel(list.ToList()).SaveToExcel(Server.MapPath("/upfile/" + FileName + ".xls"));
                return File(Server.MapPath("/upfile/" + FileName + ".xls"), "application/ms-excel", FileName + ".xls");
            }
            return View(list.OrderByDescending(x => x.ID).ToPagedList(page ?? 1, 20));
        }

        /// <summary>
        /// 点击匹配按钮时的事件
        /// </summary>
        public ActionResult doMatching(string ids, string ida)
        {
            ActMessage = "点击匹配按钮时的事件";
            if (string.IsNullOrEmpty(ids) || string.IsNullOrEmpty(ida))
                return Json(new { result = "erro", refMsg = "请选择匹配供单及受单！" }, JsonRequestBehavior.AllowGet);

            #region 事务操作
            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                string outMsg = "";
                MMM.Matching(ids, ida, ref outMsg); //匹配处理
                ts.Complete();
                return Json(new { result = "ok", refMsg = outMsg }, JsonRequestBehavior.AllowGet);
            }
            #endregion
        }


        /// <summary>
        /// 点击惩罚按钮时的事件
        /// </summary>
        public ActionResult Punish(int matchid)
        {
            ActMessage = "点击惩罚按钮时的事件";
            var mModel = MatchingService.Single(matchid);
            if (mModel == null)
            {
                ViewBag.ErrorMsg = "记录不存在。";
                return View("Error");
            }

            #region 事务操作
            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                MMM.Punish(mModel.ID); //虚假信息处理

                ts.Complete();
                ViewBag.SuccessMsg = "操作成功！";
                return View("Success");
            }
            #endregion
        }

        /// <summary>
        /// 点击取消按钮时的事件
        /// </summary>
        public ActionResult Cancel(int matchid)
        {
            ActMessage = "点击取消按钮时的事件";
            var mModel = MatchingService.Single(matchid);
            if (mModel == null)
            {
                ViewBag.ErrorMsg = "记录不存在。";
                return View("Error");
            }

            if (mModel.Status == (int)Data.Enum.MatchingStatus.UnPaid)
            {
                #region 事务操作
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    MMM.CancelMatching(mModel, "管理员取消", false);
                    ts.Complete();
                    ViewBag.SuccessMsg = "操作成功！";
                    return View("Success");
                }
                #endregion
            }
            else
            {
                ViewBag.ErrorMsg = "订单当前状态不可取消。";
                return View("Error");
            }
        }


        /// <summary>
        /// 供单置顶
        /// </summary>
        /// <returns></returns>
        public ActionResult SupplyHelpCommand(int id, string commandtype)
        {
            var model = SupplyHelpService.Single(id);
            if (commandtype.ToLower() == "ontop")
                model.IsTop = true;
            else if (commandtype.ToLower() == "untop")
                model.IsTop = false;
            SupplyHelpService.Update(model);
            SysDBTool.Commit();
            ViewBag.SuccessMsg = "操作成功！";
            return View("Success");
        }

        /// <summary>
        /// 受单置顶
        /// </summary>
        /// <returns></returns>
        public ActionResult AcceptHelpCommand(int id, string commandtype)
        {
            var model = AcceptHelpService.Single(id);
            if (commandtype.ToLower() == "ontop")
                model.IsTop = true;
            else if (commandtype.ToLower() == "untop")
                model.IsTop = false;
            AcceptHelpService.Update(model);
            SysDBTool.Commit();
            ViewBag.SuccessMsg = "操作成功！";
            return View("Success");
        }

        /// <summary>
        /// 追加付款截止时间
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DelayPay(int matchid, int hour)
        {
            var mModel = MatchingService.Single(matchid);
            if (mModel != null)
            {
                mModel.PayEndTime = (mModel.PayEndTime ?? DateTime.Now).AddHours(hour);
                MatchingService.Update(mModel);
                SysDBTool.Commit();
                return Content("ok");
            }
            return Content("Error");
        }

        #region 确认收款
        /// <summary>
        /// 确认收款
        /// </summary>
        /// <returns></returns>
        public ActionResult FinshPay(int matchid)
        {
            ActMessage = "确认收款";
            var mModel = MatchingService.Single(matchid);
            if (mModel == null)
            {
                ViewBag.ErrorMsg = "记录不存在。";
                return View("Error");
            }

            if (mModel.Status == (int)Data.Enum.MatchingStatus.Paid || mModel.Status == (int)Data.Enum.MatchingStatus.Falsehood)
            {
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                {
                    //结算提供单利息，奖金并更新成交状态
                    Bonus.Settlement(mModel);
                    //Bonus.FreezeMoney(mModel, SupplyHelpService,UserService);   //冻结本金和利息

                    mModel.Status = (int)Data.Enum.MatchingStatus.Verified;
                    mModel.VerifiedEndTime = DateTime.Now;

                    MatchingService.Update(mModel);
                    SysDBTool.Commit();
                    ts.Complete();
                    ViewBag.SuccessMsg = "操作成功！";
                    return View("Success");
                }
            }
            else
            {
                ViewBag.ErrorMsg = "订单状态不可确认。";
                return View("Error");
            }
        }
        #endregion

        #region 取消退出队列

        /// <summary>
        /// 退出队列（供单)
        /// </summary>
        /// <returns></returns>
        public ActionResult CancelSupplyQueuing(int id)
        {
            var sModel = SupplyHelpService.Single(id);
            if (sModel != null)
            {
                if (sModel.Status == (int)Data.Enum.HelpStatus.NoMatching)
                {
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        //删除利息,奖金
                        BonusDetailService.Delete(x => x.SupplyNo == sModel.SupplyNo && x.IsBalance == false);

                        sModel.Status = (int)Data.Enum.HelpStatus.Cancel;
                        sModel.IsAccruaCount = false;
                        sModel.CancelTime = DateTime.Now;
                        SupplyHelpService.Update(sModel);
                        SysDBTool.Commit();
                        ts.Complete();
                    }
                    ViewBag.SuccessMsg = "操作成功！";
                    return View("Success");
                }
                else
                {
                    ViewBag.ErrorMsg = "当前提供订单状态不可退出。";
                    return View("Error");
                }
            }
            else
            {
                ViewBag.ErrorMsg = "记录不存在。";
                return View("Error");
            }
        }

        /// <summary>
        /// 退出队列（供单)
        /// </summary>
        /// <returns></returns>
        public ActionResult CancelAcceptQueuing(int id)
        {
            var aModel = AcceptHelpService.Single(id);
            if (aModel != null)
            {
                if (aModel.Status == (int)Data.Enum.HelpStatus.NoMatching)
                {
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        var c = MvcCore.Unity.Get<JN.Data.Service.ICurrencyService>().Single(x => x.ID == aModel.CoinID);

                        Wallets.changeWallet(aModel.UID, aModel.ExchangeAmount + (aModel.fee ?? 0), c.WalletCurID ?? 0, "取消接受帮助“" + aModel.AcceptNo + "”订单返还", c);
                        aModel.Status = (int)Data.Enum.HelpStatus.Cancel;
                        aModel.CancelTime = DateTime.Now;
                        AcceptHelpService.Update(aModel);
                        SysDBTool.Commit();
                        ts.Complete();
                    }
                    ViewBag.SuccessMsg = "操作成功！";
                    return View("Success");
                }
                else
                {
                    ViewBag.ErrorMsg = "当前提供订单状态不可退出。";
                    return View("Error");
                }
            }
            else
            {
                ViewBag.ErrorMsg = "记录不存在。";
                return View("Error");
            }
        }
        #endregion

        #region 恢复排队（只有在被系统自动取消或虚假信息取消时才可恢复）

        /// <summary>
        /// 退出队列（供单)
        /// </summary>
        /// <returns></returns>
        public ActionResult RecoverySupplyQueuing(int id)
        {
            var sModel = SupplyHelpService.Single(id);
            if (sModel != null)
            {
                if (sModel.Status == (int)Data.Enum.HelpStatus.Cancel && sModel.HaveMatchingAmount < sModel.ExchangeAmount)
                {
                    using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
                    {
                        //重新修正状态
                        if (sModel.HaveMatchingAmount == 0)
                            sModel.Status = (int)Data.Enum.HelpStatus.NoMatching;
                        else if (sModel.HaveMatchingAmount > 0 && sModel.HaveMatchingAmount < sModel.ExchangeAmount)
                            sModel.Status = (int)Data.Enum.HelpStatus.PartOfMatching;
                        SupplyHelpService.Update(sModel);
                        SysDBTool.Commit();
                        ts.Complete();
                    }
                    ViewBag.SuccessMsg = "操作成功！";
                    return View("Success");
                }
                else
                {
                    ViewBag.ErrorMsg = "当前提供订单状态不可恢复。";
                    return View("Error");
                }
            }
            else
            {
                ViewBag.ErrorMsg = "记录不存在。";
                return View("Error");
            }
        }
        #endregion

        public ActionResult ChangeMathingMode()
        {
            var sysEntity = SysSettingService.Single(1);
            sysEntity.MatchingMode = sysEntity.MatchingMode == 0 ? 1 : 0;
            SysSettingService.Update(sysEntity);
            SysDBTool.Commit();
            ViewBag.SuccessMsg = "操作成功！";
            return View("Success");
        }

        /// <summary>
        /// 移至抢单区
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangeAccepMode()
        {
            string aid = Request["id"];
            if (!string.IsNullOrEmpty(aid))
            {
                var accmodel = AcceptHelpService.Single(aid.ToInt());

                if ((accmodel.ReserveInt1 ?? 0) == 0)
                    accmodel.ReserveInt1 = 1;
                else
                    accmodel.ReserveInt1 = 0;

                AcceptHelpService.Update(accmodel);
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "操作成功！";
                return View("Success");
            }
            else
            {
                ViewBag.SuccessMsg = "参数错误！";
                return View("Error");
            }
        }
    }
}
