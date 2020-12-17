using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JN.Data.Extensions
{
    public class ResponseNEOTransactions
    {
        /// <summary>
        /// 哈希值 
        /// </summary>
        public string Hash { get; set; }
        /// <summary>
        /// 交易事务ID(每笔交易都不一样，唯一)
        /// </summary>
        public string Txid { get; set; }
        /// <summary>
        /// 资产
        /// </summary>
        public string Asset { get; set; }
        /// <summary>
        /// 账户地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double Value { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public long? Time { get; set; }
        /// <summary>
        /// 区块索引
        /// </summary>
        public long? BlockIndex { get; set; }
        /// <summary>
        /// 确认数
        /// </summary>
        public int? Confirmations { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
