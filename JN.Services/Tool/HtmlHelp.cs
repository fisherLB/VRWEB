using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace JN.Services.Tool
{
    public class HtmlHelp
    {
        /// <summary>
        /// 保存HTML
        /// </summary>
        /// <param name="html"></param>
        public static void SaveCategoryHtml(string html,string theme)
        {
            Page page = new Page();
            HttpServerUtility server = page.Server;
            string dir = server.MapPath("/Areas/ShopCenter/Views/Shared/");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string path = dir + "CategoryHtml" + theme + ".cshtml";

            StreamWriter FileWriter = new StreamWriter(path, true, System.Text.Encoding.UTF8); //创建日志文件
            FileWriter.Write(html);
            FileWriter.Close(); //关闭StreamWriter对象
        }



    }
}
