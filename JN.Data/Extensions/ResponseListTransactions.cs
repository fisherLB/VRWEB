using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JN.Data.Extensions
{
    /// <summary>
    /// 接收 listtransactions 接口返回的数据实体
    /// </summary>
    public class ResponseListTransactions
    {
        /// <summary>
        /// 账户名
        /// </summary>
        public string account { get; set; }
        /// <summary>
        /// 账户地址
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// 类型 接收：receive ,发送：send
        /// </summary>
        public string category { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public double amount { get; set; }
        /// <summary>
        /// 手续费
        /// </summary>
        public double fee { get; set; }
        /// <summary>
        /// 确认数
        /// </summary>
        public int confirmations { get; set; }
        /// <summary>
        /// 区块哈希值 
        /// </summary>
        public string blockhash { get; set; }
        /// <summary>
        /// 区块索引
        /// </summary>
        public int blockindex { get; set; }
        /// <summary>
        /// 区块时间
        /// </summary>
        public string blocktime { get; set; }
        /// <summary>
        /// 交易事务ID(每笔交易都不一样，唯一)
        /// </summary>
        public string txid { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public long time { get; set; }
        /// <summary>
        /// 接收时间
        /// </summary>
        public long timereceived { get; set; }
    }
}
