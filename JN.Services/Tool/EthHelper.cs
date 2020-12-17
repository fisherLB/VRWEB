using APICore;
using APICore.Util;
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
    /// https://api.etherscan.io/api?module=account&action=tokenbalance&contractaddress=0x53c929A52f0b588E982546EC4d497D9752Dd1F86&address=0x3685d70e13cc5b610047f66a10fab64c28dbbb19&tag=latest&apikey=V2CWMKS72U634CSECS71B3KY344TT55U4W
    /// </summary>
    public class EthHelper
    {
        /// <summary>
        /// 以太坊提币
        /// </summary>
        /// <param name="apihost">API接口地址</param>
        /// <param name="address">收币地址</param>
        /// <param name="amount">提币数量</param>
        /// <returns></returns>
        public static ReturnResult<string> Transfer(string apihost, string ordernumber, string toaddress, string tousername, double quantity)
        {
            string url = apihost + "/api/Eth/SendEth";
            WebApiClientRequest client = new WebApiClientRequest();
            Dictionary<string,string> dic=new Dictionary<string,string> ();
            dic.Add("ordernumber", ordernumber);
            dic.Add("toaddress", toaddress);
            dic.Add("tousername", tousername);
            dic.Add("quantity", quantity.ToString());
            var result= client.Post(url, dic).FromJson<ReturnResult<string>>();
            return result;
        }

        /// <summary>
        /// 代币提币
        /// </summary>
        /// <param name="apihost">API接口地址</param>
        /// <param name="ordernumber">订单号</param>
        /// <param name="toaddress">收币地址</param>
        /// <param name="tousername">提币会员</param>
        /// <param name="quantity">提币数量</param>
        /// <returns></returns>
        public static ReturnResult<string> TransferContract(string apihost, string ordernumber, string toaddress, string tousername, double quantity)
        {
            string url = apihost + "/api/Eth/SendContract";
            WebApiClientRequest client = new WebApiClientRequest();
            Dictionary<string, string> dic = new Dictionary<string, string>();

            //对方转账后，钱包服务器会回调该地址（目前服务器还没有写该功能，先设计成这样，日后改进就不需要改这边的代码了）
            dic.Add("ordernumber", ordernumber);
            dic.Add("toaddress", toaddress);
            dic.Add("tousername", tousername);
            dic.Add("quantity", quantity.ToString());
            var result = client.Post(url, dic).FromJson<ReturnResult<string>>();
            return result;
        }

        /// <summary>
        /// 提币结果查询
        /// </summary>
        /// <param name="ordernumber">订单号</param>
        /// <returns></returns>
        public static ReturnResult<ResponseContractTransfer> QueryTransferContract(string apihost, string ordernumber)
        {
            string url = apihost + "/api/Eth/QueryContractTransfer";
            WebApiClientRequest client = new WebApiClientRequest();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(ordernumber))
            {
                dic.Add("ordernumber", ordernumber);
            }
            var result = client.Post(url, dic).FromJson<ReturnResult<ResponseContractTransfer>>();
            return result;
        }

        /// <summary>
        /// 提币结果查询（批量)
        /// </summary>
        /// <param name="ordernumbers">多个订单号用,隔开</param>
        /// <returns></returns>
        public static ReturnResult<List<ResponseContractTransfer>> QueryMultiContractTransfer(string apihost, string ordernumbers)
        {
            string url = apihost + "/api/Eth/QueryMultiContractTransfer";
            WebApiClientRequest client = new WebApiClientRequest();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(ordernumbers))
            {
                dic.Add("ordernumbers", ordernumbers);
            }
            var result = client.Post(url, dic).FromJson<ReturnResult<List<ResponseContractTransfer>>>();
            return result;
        }


        /// <summary>
        /// 获取充值地址
        /// </summary>
        /// <returns></returns>
        public static ReturnResult<string> CreateNewAddress(string apihost, string username)
        {
            string url = apihost + "/api/Eth/GetNewAddress";
            WebApiClientRequest client = new WebApiClientRequest();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("username", username);
            var result = client.Post(url, dic).FromJson<ReturnResult<string>>();
            return result;
        }

        public static ReturnResult<string> UnLockAccount(string apihost, string account, string passowrd)
        {
            string url = apihost + "/api/Eth/unlockAccount";
            WebApiClientRequest client = new WebApiClientRequest();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("ethAccount", account);
            dic.Add("password", passowrd);
            var result = client.Post(url, dic).FromJson<ReturnResult<string>>();
            return result;
        }

        public static ReturnResult<string> LockAccount(string apihost, string account)
        {
            string url = apihost + "/api/Eth/lockAccount";
            WebApiClientRequest client = new WebApiClientRequest();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("ethAccount", account);
            var result = client.Post(url, dic).FromJson<ReturnResult<string>>();
            return result;
        }

        //public static ReturnResult<bool> ValidateAddress(string apihost, string address)
        //{
        //    string url = apihost + "/api/Eth/ValidateAddress";
        //    WebApiClientRequest client = new WebApiClientRequest();

        //    Dictionary<string, string> dic = new Dictionary<string, string>();
        //    dic.Add("address", address);
        //    var result = client.Post(url, dic).FromJson<ReturnResult<bool>>();

        //    return result;
        //}


        /// <summary>
        /// 受限弃用(以太坊第三方)
        /// </summary>
        /// <param name="account">
        /// 建议新建地址时传入会员的尖，这里就可以通过会员的id获取该会员下的所以交易记录了，默认返回100条交易记录
        /// </param>
        /// <returns></returns>
        //public static ReturnResult<ResponseTransactions> GetListTransactions(string account)
        //{
        //    //查询地址，采用第三方https://etherchain.org/api/account/0x3685d70e13cc5b610047f66a10fab64c28dbbb19/tx/0
        //    string url = string.Format("https://etherchain.org/api/account/{0}/tx/0", account);
        //    WebUtils web = new WebUtils();
        //    var result = web.DoGet(url, null, "UTF-8").FromJson<ReturnResult<ResponseTransactions>>();
        //    return result;
        //}


        /// <summary>
        /// 获取充值结果(以太坊专用)
        /// </summary>
        public static ReturnResult<ResponseTransactions> GetEthTransactions(string account)
        {
            //https://etherscan.io/apis#accounts
            //查询地址，采用第三方http://api.etherscan.io/api?module=account&action=txlist&address=0xddbd2b932c763ba5b1b7ae3b362eac3e8d40121a&startblock=0&endblock=99999999&sort=asc&apikey=YourApiKeyToken
            string url = string.Format("http://api.etherscan.io/api?module=account&action=txlist&address={0}&startblock=0&endblock=99999999&sort=asc&apikey=V2CWMKS72U634CSECS71B3KY344TT55U4W", account);
            WebUtils web = new WebUtils();
            ReturnResult<ResponseTransactions> result = new ReturnResult<ResponseTransactions>();
            result.Data = web.DoGet(url, null, "UTF-8").FromJson<ResponseTransactions>();
            return result;
        }

        /// <summary>
        /// 受限弃用(代币第三方)
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        //public static ReturnResult<ResponseTransactions> GetListContractTransactions(string account)
        //{
        //    //查询地址，采用第三方https://etherchain.org/api/account/0x3685d70e13cc5b610047f66a10fab64c28dbbb19/tx/0
        //    string url = string.Format("https://etherscan.io/token/generic-tokentxns2?contractAddress=0x53c929A52f0b588E982546EC4d497D9752Dd1F86&a={0}&mode=", account);

        //    //WebBrowserCrawler webBrowserCrawler = new WebBrowserCrawler();
        //    //string html = webBrowserCrawler.GetReult(url);

        //    WebClient web = new WebClient();
        //      string html = web.DownloadString(url);


        //    //WebUtils web = new WebUtils();
        //    //string html = web.DoGet(url, null, "UTF-8");
        //    var list = new List<ContractTransaction>();
        //    HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
        //    htmlDocument.LoadHtml(html);
        //    var res = htmlDocument.DocumentNode.SelectSingleNode(@"/html[1]/body[1]/div[2]/table[1]");//跟Xpath一样，轻松的定位到相应节点下
        //    if (res != null)
        //    {
        //        var trlist = res.SelectNodes(@"tr");
        //        if (trlist.Count > 1)
        //        {
        //            trlist.Remove(0);
        //            foreach (var item in trlist)
        //            {
        //                var line = item.SelectNodes(@"td");
        //                list.Add(new ContractTransaction() { hash = line[0].InnerText, age = line[1].InnerText, from = line[2].InnerText, input = line[3].InnerText.Replace("&nbsp;", "").Trim(), to = line[4].InnerText, value = line[5].InnerText });
        //            }
        //        }
        //    }
        //    ReturnResult<Models.ResponseTransactions> result = new ReturnResult<Models.ResponseTransactions>();
        //    result.Status = 200;
        //    result.Data.contractdata = list;
        //    return result;
        //}

        /// <summary>
        /// 获取充值结果,此为以太合约（代币）使用，采用第三方案(不受api影响) By :Ann；time：2018/06/22
        /// <param name="contractAddress">以太合约地址</param>
        /// <param name="account">账户地址</param>
        /// </summary>
        public static ReturnResult<ResponseContractTransactionsNew> GetContractTransactionsNew(string contractAddress, string account)
        {
            //参考网址：https://etherscan.io/apis#accounts

            string url = string.Format("https://api.etherscan.io/api?module=account&action=tokentx&contractaddress={0}&address={1}&page=1&offset=100&sort=asc&apikey=V2CWMKS72U634CSECS71B3KY344TT55U4W", contractAddress, account);

            WebUtils web = new WebUtils();
            ReturnResult<ResponseContractTransactionsNew> result = new ReturnResult<ResponseContractTransactionsNew>();
            result.Data = web.DoGet(url, null, "UTF-8").FromJson<ResponseContractTransactionsNew>();
            return result;
        }



        /// <summary>
        /// 获取充值结果,此为以太合约（代币）使用，第三方监听方案
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static ReturnResult<List<ResponseContractTransactionsFromEtherscan>> GetContractTransactionFromEtherscan(string apihost, string account)
        {
            string url = apihost + "/api/Eth/GetContractTransactionFromEtherscan";
            WebApiClientRequest client = new WebApiClientRequest();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(account))
            {
                dic.Add("account", account);
            }
            var result = client.Post(url, dic).FromJson<ReturnResult<List<ResponseContractTransactionsFromEtherscan>>>();
            return result;
        }

        /// <summary>
        ///　获取充值结果, 此为以太合约（代币）使用，本地区块监听方案
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static ReturnResult<List<ResponseContractTransactionsFromLocal>> GetContractTransactionFromLocal(string apihost, string account)
        {
            string url = apihost + "/api/Eth/GetContractTransactionFromLocal";
            WebApiClientRequest client = new WebApiClientRequest();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(account))
            {
                dic.Add("account", account);
            }
            var result = client.Post(url, dic).FromJson<ReturnResult<List<ResponseContractTransactionsFromLocal>>>();
            return result;
        }


    }
}