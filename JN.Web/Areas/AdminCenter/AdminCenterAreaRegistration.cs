using JN.Data.Extensions;
using JN.Services.Tool;
using System.Web.Mvc;

namespace JN.Web.Areas.AdminCenter
{
    public class AdminCenterAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "AdminCenter";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            string customAreaName = "";
            if (!ConfigHelper.GetConfigBool("HaveEncrypt"))
                customAreaName = ConfigHelper.GetConfigString("AdminPath");
            else
                customAreaName = DESEncrypt.Decrypt(ConfigHelper.GetConfigString("AdminPath"), ConfigHelper.GetConfigString("EncryptKey"));

            context.MapRoute(
                "Admin_default",
                customAreaName + "/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
                , new string[] { "JN.Web.Areas.AdminCenter.Controllers" }
            );
        }
    }
}