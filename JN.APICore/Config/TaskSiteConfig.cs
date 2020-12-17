using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APICore
{
    public class TaskSiteConfig:BaseConfig
    {
        public static string AddUrl
        {
            get
            {

                return GetAppKey("TaskSite_AddUrl", "http://task.58wzj.com/api/Task/Add");
            }

        }

         public static string AddTaskToRunOnceUrl
         {
             get
             {

                 return GetAppKey("TaskSite_AddTaskToRunOnceUrl", "http://task.58wzj.com/api/Task/AddTaskToRun");
             }

         }

    }
}
