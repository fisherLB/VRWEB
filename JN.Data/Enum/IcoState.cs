using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //ico状态
    public enum IcoState
    {
        /// <summary>
        /// 正在进行
        /// </summary>
        [Description("正在进行")]
        Underway =1,
        /// <summary>
        /// 即将开始
        /// </summary>
        [Description("即将开始")]
        Ahead = 2,
        /// <summary>
        /// 敬请期待
        /// </summary>
        [Description("敬请期待")]
         Expect = 3,
        /// <summary>
        /// 已经完成
        /// </summary>
        [Description("已经完成")]
        Completed = 4,

        /// <summary>
        /// 已删除
        /// </summary>
        [Description("已删除")]
        Del = -1,
      

    }
}