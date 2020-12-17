using APICore.Util;
using JN.Data.Extensions;
using JN.Services.Manager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Web;

namespace JN.Services.Tool
{
    /// <summary>
    /// 访问钱包服务器api帮助类
    /// 官方文档https://developers.ripple.com/data-api.html
    /// 要求在web.config 中设置 appSettings 会员
    /// 李2018/08/16
    /// </summary>
    public class EosHelper
    {

        /// <summary>
        /// 获取充值地址,瑞波币只有一个地址，可分配给不同的会员使用不同的destination_tag来区分
        /// 同火币网方案
        /// </summary>
        /// <returns></returns>
        public static string CreateNewAddress(int uid)
        {
            //订单种子
            int XrpSeed = ConfigHelper.GetConfigInt("EosSeed");
            //return (100000 + uid).ToString();
            return (XrpSeed + uid).ToString();
        }

        /// <summary>
        /// 获取充值结果
        /// https://developer.eospark.com/api-doc/account/#get-account-transactions
        /// </summary>
        public static ReturnResult<ResponseEOS_get_transactions> GetListTransactions(string account, int page)
        {
            string url = string.Format("https://api.eospark.com/api?module=account&action=get_account_related_trx_info&apikey=a9564ebc3289b7a14551baf8ad5ec60a&account={0}&page={1}&size=20&symbol=EOS&code=eosio.token", account, page);
            WebUtils web = new WebUtils();
            ReturnResult<ResponseEOS_get_transactions> result = new ReturnResult<ResponseEOS_get_transactions>();
            string jsonstr = web.DoGet(url, null, "UTF-8");
            result.Data = jsonstr.FromJson<ResponseEOS_get_transactions>();
            result.Status = 200;
            return result;
        }

        public static ResponseEOS_get_info get_info(string apihost)
        {
            var parameters = new JObject();
            var rpc = new RPCHelper(apihost);
            return rpc.CallMethod<ResponseEOS_get_info>("get_info", parameters);
        }
        public static ResponseEOS_get_info get_currency_balance(string apihost, string code, string account)
        {
            var parameters = new JObject();
            parameters.Add(new JProperty("code", code));
            parameters.Add(new JProperty("account", account));
            var rpc = new RPCHelper(apihost);
            return rpc.CallMethod<ResponseEOS_get_info>("get_currency_balance", parameters);
        }


        /// <summary>
        /// 获取账户信息 (官方Ripple Data API RPC方案)
        /// //http://s1.ripple.com:51234/ 
        /// </summary>
        public static ResponseBaseXRP<ResponseXRP_account_info> GetAccountInfo(string apihost, string address)
        {
            var parameters = new JObject();
            parameters.Add(new JProperty("account", address));
            parameters.Add(new JProperty("strict", true));
            parameters.Add(new JProperty("ledger_index", "current"));
            parameters.Add(new JProperty("queue", true));

            var rpc = new RPCHelper(apihost);
            return rpc.CallMethod<ResponseBaseXRP<ResponseXRP_account_info>>("account_info", parameters);
        }

        ///提币采用WebStocket方案
        ///详见https://developers.ripple.com/sign.html  （签名)  https://developers.ripple.com/submit.html  (提交到区块链)
        ///js实现签名然后发送，暂时写在后台提币审核页面  /AdminCenter/WalletCoin/TakeCashRa.cshtml

        /// <summary>
        /// 转账操作 (官方Ripple Data API RPC方案)
        /// //http://s1.ripple.com:51234/ 
        /// </summary>
        public static ReturnResult<string> Transfer(string apihost, string formaddress, string toaddress, decimal amount, string tag)
        {
            ReturnResult<string> result = new ReturnResult<string>();
            try
            {
                string XrpSecretKey = ConfigHelper.GetConfigString("XrpSecretKey"); //私钥需要在Web.config定义

                #region 签名信息
                var rpc = new RPCHelper(apihost);

                var parameters = new JObject();
                var tx_json = new JObject();
                tx_json.Add(new JProperty("TransactionType", "Payment"));
                tx_json.Add(new JProperty("Account", formaddress));
                tx_json.Add(new JProperty("Destination", toaddress));
                tx_json.Add(new JProperty("DestinationTag", tag));
                tx_json.Add(new JProperty("Amount", (amount * 1000000).ToDouble() + ""));
                parameters.Add(new JProperty("tx_json", tx_json));
                parameters.Add(new JProperty("secret", XrpSecretKey));
                parameters.Add(new JProperty("offline", false));
                parameters.Add(new JProperty("fee_mult_max", 1000));
                var sign = rpc.CallMethod<ResponseBaseXRP<ResponseXRP_sign>>("sign", parameters);
                if (!sign.result.status.ToLower().Equals("success")) throw new Exception("交易签名失败，错误消息：" + sign.result.error);
                #endregion

                #region 提交交易
                parameters = new JObject();
                parameters.Add(new JProperty("tx_blob", sign.result.tx_blob));
                var response = rpc.CallMethod<ResponseBaseXRP<ResponseXRP_submit>>("submit", parameters);
                if (!response.result.status.ToLower().Equals("success")) throw new Exception("交易提交失败，错误消息：" + response.result.error);
                if (response.result.engine_result_code != 0) throw new Exception("交易提交失败，错误消息：" + response.result.engine_result_message);
                #endregion
                result.Data = response.result.tx_json.hash;
                result.Status = ReturnResultStatus.Succeed.GetShortValue();
            }
            catch (Exception e)
            {
                result.Message = e.Message;
                logs.WriteErrorLog(HttpContext.Current.Request.Url.ToString(), e);
            }
            return result;
        }

        /// <summary>
        /// 获取交易信息
        /// </summary>
        public static ReturnResult<ResponseXRPTransactionsForTX> GetTransactionsForTX(string tx)
        {
            string url = string.Format("https://data.ripple.com/v2/transactions/{0}?binary=false", tx);
            WebUtils web = new WebUtils();
            ReturnResult<ResponseXRPTransactionsForTX> result = new ReturnResult<ResponseXRPTransactionsForTX>();
            string jsonstr = web.DoGet(url, null, "UTF-8");
            result.Data = jsonstr.FromJson<ResponseXRPTransactionsForTX>();
            result.Status = 200;
            return result;
        }

    }
}