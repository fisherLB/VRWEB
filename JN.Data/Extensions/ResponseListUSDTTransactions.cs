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
    public class ResponseListUSDTTransactions
    {
        /// <summary>
        /// 交易事务ID(每笔交易都不一样，唯一)
        /// </summary>
        public string txid { get; set; }
        /// <summary>
        /// 手续费
        /// </summary>
        public double fee { get; set; }
        public string sendingaddress { get; set; }
        public string referenceaddress { get; set; }
        public bool ismine { get; set; }
        public double version { get; set; }
        public int type_int { get; set; }
        public string type { get; set; }
        public int propertyid { get; set; }
        public bool divisible { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public double amount { get; set; }
        public bool valid { get; set; }
        /// <summary>
        /// 区块哈希值 
        /// </summary>
        public string blockhash { get; set; }
        /// <summary>
        /// 区块时间
        /// </summary>
        public long blocktime { get; set; }
        public int positioninblock { get; set; }
        public int block { get; set; }
        /// <summary>
        /// 确认数
        /// </summary>
        public int confirmations { get; set; }
    }
}
