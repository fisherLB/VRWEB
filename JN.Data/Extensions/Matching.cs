using System;
using System.Linq;
using System.Text;

namespace JN.Data.Extensions
{
    public partial class Matching
    {
        #region 生成随机编号
        //生成随机编号
        public static string GetOrderNumber()
        {
            DateTime dateTime = DateTime.Now;
            string result = "R" + GetRandomNumber(7);//7位随机数字
            //int maxid = MvcCore.Unity.Get<JN.Data.Service.IMatchingService>().List().Count() > 0 ? MvcCore.Unity.Get<JN.Data.Service.IMatchingService>().List().Max(x => x.MatchingNo.Substring(x.AcceptNo.Length - 7)).ToInt() : 0;
            //if (maxid < 10000) maxid = 10000;
            //result += (maxid + 1).ToString().PadLeft(7, '0');
            if (IsHave(result))
            {
                return GetOrderNumber();
            }
            return result;
        }

        //检查订单号是否重复
        private static bool IsHave(string number)
        {
            return MvcCore.Unity.Get<JN.Data.Service.IMatchingService>().List(x => x.MatchingNo == number).Count() > 0;
        }

        /// <summary>生成随机数字
        /// 
        /// </summary>
        /// <param name="num">字符长度</param>
        /// <returns></returns>
        public static string GetRandomNumber(int num)
        {
            string a = "0123456789";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < num; i++)
            {
                sb.Append(a[new Random(Guid.NewGuid().GetHashCode()).Next(0, a.Length - 1)]);
            }
            return sb.ToString();
        }
        #endregion

    }
}