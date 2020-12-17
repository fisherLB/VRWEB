using MvcCore.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Configuration;

namespace JN.Services.Tool
{
    public class CacheHelp
    {
        /// <summary>
        /// 根据用户名创建验证表单缓存
        /// </summary>
        /// <param name="UserName">对应用户</param>
        /// <param name="second">验证时长（秒）</param>
        /// <returns></returns>
        public static string CheckFromCommitCache(string UserName, int second)
        {
            string result = "";

            if (!string.IsNullOrEmpty(UserName)) { UserName = "public"; }

            //if (cacheSysParam.SingleAndInit(x => x.ID == 3805).Value == "1")
            //{
            if (MvcCore.Extensions.CacheExtensions.CheckCache(UserName + "repeat"))  //检查
            {
                DateTime time = MvcCore.Extensions.CacheExtensions.GetCache<DateTime>(UserName + "repeat");
                if (DateTime.Now < time)
                {
                    result = "系统正在处理您的相关数据,请在" + second + "秒后再进行操作!";
                }
            }
            else
            {
                MvcCore.Extensions.CacheExtensions.SetCache(UserName + "repeat", DateTime.Now.AddSeconds(second), MvcCore.Extensions.CacheTimeType.ByMinutes, (second / 60));  //创建
            }
            //}
            return result;
        }

        /// <summary>
        /// 根据用户名清除验证表单的缓存
        /// </summary>
        /// <param name="Umodel"></param>
        public static void ClearFromCommitCache(string UserName)
        {
            string result = "";

            if (!string.IsNullOrEmpty(UserName)) { UserName = "public"; }

            //if (cacheSysParam.SingleAndInit(x => x.ID == 3805).Value == "1")
            //{
            if (MvcCore.Extensions.CacheExtensions.CheckCache(UserName + "repeat"))  //检查
            {
                MvcCore.Extensions.CacheExtensions.ClearCache(UserName + "repeat");
            }
            //}
        }
    }
}