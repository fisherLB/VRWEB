using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace JN.Data.Enum
{
    //币种API网络类型
    public enum CurrencyNetworkType
    {
        /// <summary>
        /// 默认（未配置）
        /// </summary>
        [Description("默认（未配置）")]
        Default = 0,

        /// <summary>
        /// 比特币网络
        /// </summary>
        [Description("比特币网络")]
        Bitcoin = 1,

        /// <summary>
        /// 以太坊
        /// </summary>
        [Description("以太坊")]
        Ethereum = 2,

        /// <summary>
        /// 以太坊合约（代币）
        /// </summary>
        [Description("以太坊合约（代币）")]
        EthereumContract = 3,

        /// <summary>
        /// 小蚁（NEO）
        /// </summary>
        [Description("小蚁（NEO）")]
        NEO = 4,

        /// <summary>
        /// 瑞波币（XRP）
        /// </summary>
        [Description("瑞波币（XRP）")]
        Ripple = 5,
        /// <summary>
        /// USDT
        /// </summary>
        [Description("USDT")]
        USDT = 6,

        /// <summary>
        /// EOS
        /// </summary>
        [Description("EOS")]
        EOS = 7,

        /// <summary>
        /// EOS合约
        /// </summary>
        [Description("EOS合约（代币）")]
        EOSContract = 8,
    }
}