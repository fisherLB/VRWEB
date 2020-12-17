using APICore;
using JN.Data.Extensions;
using System;
using System.Collections.Generic;

namespace JN.Services.Tool
{
    /// <summary>
    /// 访问钱包服务器api帮助类
    /// 要求在web.config 中设置 appSettings 会员
    /// </summary>
    public class NEOHelper
    {
        /// <summary>
        /// 转账，返回200状态码说明钱包已经转账出来了，可以查看扣会员数据库的金额了
        /// </summary>
        /// <param name="address"></param>
        /// <param name="amount"></param>
        /// <returns>
        /// 如果返回是200的成功状态码，Data 属性就是交易的ID,建议保存到数据库里方便日后审查用
        /// </returns>
        public static ReturnResult<string> Transfer(string apihost, string address, double amount)
        {
            string url = apihost + "/api/NEO/SendToAddress";
            WebApiClientRequest client = new WebApiClientRequest();
            Dictionary<string,string> dic=new Dictionary<string,string> ();
            dic.Add("address",address);
            dic.Add("amount", amount.ToString());
            var result= client.Post(url, dic).FromJson<ReturnResult<string>>();
            return result;
        }


        /// <summary>
        /// 获取充值地址，建议把地址保存到充值订单数据库，方便日后查询转账金额以确认对方有没有转账
        /// </summary>
        /// <returns></returns>
        public static ReturnResult<string> CreateNewAddress(string apihost, string username)
        {
            string url = apihost + "/api/NEO/GetNewAddress";
            WebApiClientRequest client = new WebApiClientRequest();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("username", username);
            var result = client.Post(url, dic).FromJson<ReturnResult<string>>();
            return result;
        }



        public static ReturnResult<bool> ValidateAddress(string apihost, string address)
        {
            string url = apihost + "/api/NEO/ValidateAddress";
            WebApiClientRequest client = new WebApiClientRequest();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("address", address);
            var result = client.Post(url, dic).FromJson<ReturnResult<bool>>();

            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static ReturnResult<List<ResponseNEOTransactions>> GetListTransactions(string apihost, string address)
        {
            string url = apihost + "/api/NEO/GetListTransactions";
            WebApiClientRequest client = new WebApiClientRequest();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(address))
            {
                dic.Add("address", address);
            }
            var result = client.Post(url, dic).FromJson<ReturnResult<List<ResponseNEOTransactions>>>();
            return result;
        }
    }
}