using JN.Data.Service;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class VRUsersController : BaseController
    {

        private readonly IVRUsersService VRUsersService;
        private readonly ISysDBTool SysDBTool;
        private readonly ICardService CardService;

        public VRUsersController(ISysDBTool SysDBTool,
            IVRUsersService VRUsersService,
            ICardService CardService
           )
        {
            this.VRUsersService = VRUsersService;
            this.SysDBTool = SysDBTool;
            this.CardService = CardService;
        }
        // GET: AdminCenter/VRUsers
        public ActionResult Index(int? page)
        {
            ActMessage = "VR用户管理";
            //动态构建查询
            var list = VRUsersService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            return View(list.OrderByDescending(x => x.Id).ToPagedList(page ?? 1, 20));
        }

        public ActionResult Card(int? page)
        {
            ActMessage = "卡密管理";
            //动态构建查询
            var list = CardService.List().WhereDynamic(FormatQueryString(HttpUtility.ParseQueryString(Request.Url.Query)));
            return View(list.OrderByDescending(x => x.Id).ToPagedList(page ?? 1, 20));
        }
    }
}