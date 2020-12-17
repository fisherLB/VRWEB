using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APICore
{
    /// <summary>
    /// 任务帮助类
    /// </summary>
    public class TaskHelper
    {

        /// <summary>
        /// 添加执行一次的类型,回调url会以post形式回调，把paramJson数据 以json 键post回回调url
        /// </summary>
        /// <param name="name"></param>
        /// <param name="url"></param>
        /// <param name="runTime"></param>
        /// <returns></returns>
        public static bool AddTaskToRunOnce(string name, string url, DateTime runTime, string ParamJson="")
        {

            bool result = true;

            WebApiClientRequest client = new WebApiClientRequest();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("Name",name);
            dic.Add("NextRunTime", runTime.ToString());
            dic.Add("VisitUrl", url);
            dic.Add("Type", "2");
            dic.Add("ParamJson", ParamJson);
             client.Post(TaskSiteConfig.AddUrl,dic);


            return result;


        }

    }
}
