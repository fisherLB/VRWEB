using APICore;
using JN.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Web;

namespace JN.Services.Tool
{
    /// <summary>
    /// 访问钱包服务器api帮助类
    /// 要求在web.config 中设置 appSettings 会员
    /// 
    /// 
    /// </summary>
    public class BitcionHelper
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
            string url = apihost + "/api/Transfer/send";
            WebApiClientRequest client = new WebApiClientRequest();
            Dictionary<string,string> dic=new Dictionary<string,string> ();
            dic.Add("address",address);
            dic.Add("amount", amount.ToString());
            var result= client.Post(url, dic).FromJson<ReturnResult<string>>();

            return result;
        }

        /// <summary>
        /// 转账，返回200状态码说明钱包已经转账出来了，可以查看扣会员数据库的金额了
        /// </summary>
        /// <param name="address"></param>
        /// <param name="amount"></param>
        /// <returns>
        /// 如果返回是200的成功状态码，Data 属性就是交易的ID,建议保存到数据库里方便日后审查用
        /// </returns>
        public static ReturnResult<string> TransferGetSqlServer(string apihost, string address, double amount)
        {
            string url =apihost+ "/api/Transfer/send";
            WebApiClientRequest client = new WebApiClientRequest();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("address", address);
            dic.Add("amount", amount.ToString());
            var result = client.Post(url, dic).FromJson<ReturnResult<string>>();

            return result;
        }


        /// <summary>
        /// 获取充值地址，建议把地址保存到充值订单数据库，方便日后查询转账金额以确认对方有没有转账
        /// </summary>
        /// <param name="name">钱包地址对应的tag名,建议传入会员的ID，这样每个会员都对应有一个地址了(不支持中文，传入中文返回时中文是乱码)</param>
        /// <param name="callbackUrl">该地址金额发生改变时回调的url，目前未起作用</param>
        /// <returns></returns>
        public static ReturnResult<string> CreateNewAddressGetConfig(string apihost, string name, string callbackUrl = "")
        {
            string url = apihost + "/api/Recharge/GetNewAddress";
            WebApiClientRequest client = new WebApiClientRequest();
            Dictionary<string, string> dic = new Dictionary<string, string>();

            //对方转账后，钱包服务器会回调该地址（目前服务器还没有写该功能，先设计成这样，日后改进就不需要改这边的代码了）
            dic.Add("callUrl",url);
            dic.Add("name",name);
            var result = client.Post(url, dic).FromJson<ReturnResult<string>>(); 
            return result;
        }


        /// <summary>
        /// 获取充值地址，建议把地址保存到充值订单数据库，方便日后查询转账金额以确认对方有没有转账
        /// </summary>
        /// <param name="name">钱包地址对应的tag名,建议传入会员的ID，这样每个会员都对应有一个地址了(不支持中文，传入中文返回时中文是乱码)</param>
        /// <param name="callbackUrl">该地址金额发生改变时回调的url，目前未起作用</param>
        /// <returns></returns>
        public static ReturnResult<string> CreateNewAddressGetSqlServer(string apihost, string name, string callbackUrl = "")
        {
            string url = apihost+ "/api/Recharge/GetNewAddress";
            WebApiClientRequest client = new WebApiClientRequest();
            Dictionary<string, string> dic = new Dictionary<string, string>();

            //对方转账后，钱包服务器会回调该地址（目前服务器还没有写该功能，先设计成这样，日后改进就不需要改这边的代码了）
            dic.Add("callUrl", url);
            dic.Add("name", name);
            var result = client.Post(url, dic).FromJson<ReturnResult<string>>();

            return result;
        }

       /// <summary>
        /// 获取指定地址接收到的金额(默认获取已经确认的交易金额)
       /// </summary>
       /// <param name="address">收款地址</param>
       /// <param name="minconf">获取到的金额是否是已经确认的金额，0：否，1：是</param>
       /// <returns></returns>
        public static ReturnResult<double> GetAddressReceivedByAddress(string apihost, string address,byte minconf=1)
        {
            string url = apihost + "/api/Recharge/GetAddressReceivedByAddress";
            WebApiClientRequest client = new WebApiClientRequest();
           
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("address",address);
            dic.Add("minconf", minconf.ToString());
            var result = client.Post(url, dic).FromJson<ReturnResult<double>>();

            return result;
        }

        /// <summary>
        /// 获取数据库指定地址接收到的金额(默认获取已经确认的交易金额)
        /// </summary>
        /// <param name="address">收款地址</param>
        /// <param name="minconf">获取到的金额是否是已经确认的金额，0：否，1：是</param>
        /// <returns></returns>
        public static ReturnResult<double> GetSqlServerAddressReceivedByAddress(string apihost, string address, byte minconf = 1)
        {
            string url =apihost+ "/api/Recharge/GetAddressReceivedByAddress";
            WebApiClientRequest client = new WebApiClientRequest();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("address", address);
            dic.Add("minconf", minconf.ToString());
            var result = client.Post(url, dic).FromJson<ReturnResult<double>>();

            return result;
        }


        public static ReturnResult<bool> ValidateAddress(string apihost, string address)
        {
            string url = apihost + "/api/Transfer/ValidateAddress";
            WebApiClientRequest client = new WebApiClientRequest();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("address", address);
            var result = client.Post(url, dic).FromJson<ReturnResult<bool>>();

            return result;
        }

        public static ReturnResult<bool> ValidateAddressGetSqlServer(string apihost, string address)
        {
            string url = apihost + "/api/Transfer/ValidateAddress";
            WebApiClientRequest client = new WebApiClientRequest();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("address", address);
            var result = client.Post(url, dic).FromJson<ReturnResult<bool>>();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account">
        /// 建议新建地址时传入会员的尖，这里就可以通过会员的id获取该会员下的所以交易记录了，默认返回100条交易记录
        /// </param>
        /// <returns></returns>
        public static ReturnResult<List<ResponseListTransactions>> GetListTransactions(string apihost, string account)
        {
            string url = apihost + "/api/Recharge/ListTransactions";
            WebApiClientRequest client = new WebApiClientRequest();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(account))
            {
                dic.Add("account", account);    
            }
            
            
            var result = client.Post(url, dic).FromJson<ReturnResult<List<ResponseListTransactions>>>();

            return result;
        }



        /// <summary>
        /// 获取地址
        /// </summary>
        /// <param name="account">
        /// 建议新建地址时传入会员的尖，这里就可以通过会员的id获取该会员下的所以交易记录了，默认返回100条交易记录
        /// </param>
        /// <returns></returns>
        public static ReturnResult<List<ResponseListTransactions>> GetListTransactionsGetSqlServer(string apihost, string account)
        {
            string url = apihost + "/api/Recharge/ListTransactions";
            WebApiClientRequest client = new WebApiClientRequest();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(account))
            {
                dic.Add("account", account);
            }

            var result = client.Post(url, dic).FromJson<ReturnResult<List<ResponseListTransactions>>>();

            return result;
        }

    }
}