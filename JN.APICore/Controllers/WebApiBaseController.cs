using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace APICore.Controllers
{
   // [ApiAuthority]
    public class WebApiBaseController : ApiController
    {

        public  HttpResponseMessage JsonResult(object result)
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            //设置忽视循环检测
            jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            jsonSerializerSettings.Converters.Add(timeFormat);

            var response = this.Request.CreateResponse();
            response.Content = new StringContent(JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented, jsonSerializerSettings), System.Text.Encoding.UTF8, "application/json");
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

    }
}
