using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web;
namespace APICore.Controllers
{
    public class CacheController : WebApiBaseController
    {

        /// <summary>
        /// 获取所有缓存
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage Get()
        {


            List<string> list = new List<string>();
           var cache= HttpRuntime.Cache.GetEnumerator();
            while (cache.MoveNext())
            {

                list.Add(cache.Key.ToString());
            }

            return JsonResult(new
            {
                Data = list,
                Status = 200,
                Message = "操作成功"
            });

        }
        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="cache"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage Add([FromBody]CacheModel cache)
        {

            HttpRuntime.Cache.Add(cache.key, cache.value, null, System.Web.Caching.Cache.NoAbsoluteExpiration, cache.Expires-DateTime.Now, System.Web.Caching.CacheItemPriority.Default, null);

            return JsonResult(new
            {                
                Status = 200,
                Message = "操作成功"
            });
        }
        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage Remove([FromBody]CacheModel cache)
        {
            if (cache!=null)
            {

                HttpRuntime.Cache.Remove(cache.key);
                return JsonResult(new
                {
                    Status = 200,
                    Message = "操作成功"
                });


            }
            else
            {

                return JsonResult(new
                {
                    Status = 500,
                    Message = "操作失败，未能接收到参数"
                });

            }

        }
        /// <summary>
        /// 模糊删除
        /// </summary>
        /// <param name="cache"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage RemoveFuzzy([FromBody]CacheModel cache)
        {
            if (cache != null)
            {
                List<string> list = new List<string>();
                var cache1 = HttpRuntime.Cache.GetEnumerator();
                while (cache1.MoveNext())
                {
                    if (cache1.Key.ToString().Contains(cache.key))
                    {
                        list.Add(cache1.Key.ToString());
                    }
                }
                foreach (var item in list)
                {
                    HttpRuntime.Cache.Remove(item);    
                }
                
                return JsonResult(new
                {
                    Status = 200,
                    Message = "操作成功"
                });


            }
            else
            {

                return JsonResult(new
                {
                    Status = 500,
                    Message = "操作失败，未能接收到参数"
                });

            }

        }
        

    }

    public class CacheModel
    {

        public CacheModel()
        {
            Expires = DateTime.Now.AddHours(1);
        }

        public string key { get; set; 
        }

        public string value { get; set; }

        public DateTime Expires { get; set; }
    }
}
