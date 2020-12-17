using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace APICore.Util
{
    public class APISignature
    {
        /** 默认编码字符集 */
        private static string DEFAULT_CHARSET = "GBK";

        public static string GetSignContent(IDictionary<string, string> parameters)
        {
            // 第一步：把字典按Key的字母顺序排序
            IDictionary<string, string> sortedParams = new SortedDictionary<string, string>(parameters);
            IEnumerator<KeyValuePair<string, string>> dem = sortedParams.GetEnumerator();

            // 第二步：把所有参数名和参数值串在一起
            StringBuilder query = new StringBuilder("");
            while (dem.MoveNext())
            {
                string key = dem.Current.Key;
                string value = dem.Current.Value;
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    query.Append(key).Append("=").Append(value).Append("&");
                }
            }
            string content = string.Empty;
            if (query.Length>0)
            {
                content = query.ToString().Substring(0, query.Length - 1);    
            }
            

            return content;
        }

        /// <summary>
        /// 使用MD5算法加密（不可逆，无法解密）32位小写加密
        /// </summary>
        /// <param name="password">明文</param>
        /// <returns>密文</returns>
        private static string EncryptionMD5Of32Bit( string password)
        {
            string cl = password;
            string pwd = "";
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();//实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(cl));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符
                pwd = pwd + s[i].ToString("x2");

            }
            return pwd;
        }
        public static string RSASign(IDictionary<string, string> parameters, string SecretKey)
        {
            string signContent = GetSignContent(parameters) + SecretKey;
            return EncryptionMD5Of32Bit(signContent);
        }

         
    }
}
