using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JN.Data.Extensions
{
    public class APIKlineModel
    {
        /// <summary>
        /// 时间刻度
        /// </summary>
        public long XTime { get; set; }
        /// <summary>
        /// 时间刻度(字符串类型)
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// 最高价
        /// </summary>
        public decimal Hight { get; set; }
        /// <summary>
        /// 最低价
        /// </summary>
        public decimal Lowest { get; set; }
        /// <summary>
        /// 开盘价
        /// </summary>
        public decimal Open { get; set; }
        /// <summary>
        /// 收盘价
        /// </summary>
        public decimal Close { get; set; }
        /// <summary>
        /// 成交量
        /// </summary>
        public decimal Volumns { get; set; }
    }

    
}