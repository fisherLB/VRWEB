using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace JN.Services.Tool
{
    public enum FileExtension
    {
        GIF = 7173,
        JPG = 255216,
        BMP = 6677,
        PNG = 13780,
        DOC = 208207,
        DOCX = 8075,
        XLSX = 8075,
        JS = 239187,
        XLS = 208207,
        SWF = 6787,
        MID = 7784,
        RAR = 8297,
        ZIP = 8075,
        XML = 6063,
        TXT = 7067,
        MP3 = 7368,
        WMA = 4838,
        JPEG = 255216,

        // 239187 aspx
        // 117115 cs
        // 119105 js
        // 210187 txt
        //255254 sql 		
        // 7790 exe dll,
        // 8297 rar
        // 6063 xml
        // 6033 html
    }

    public class FileValidation
    {
        public static bool IsAllowedExtension(HttpPostedFileBase fu, FileExtension[] fileEx)
        {
            var exts = new string[] { ".bmp", ".jpg", ".gif", ".jpeg", ".png", ".svg" };
            var fn = Path.GetExtension(fu.FileName).ToLower();
            if (!exts.Any(x => x == fn))
                return false;

            int fileLen = fu.ContentLength;
            byte[] imgArray = new byte[fileLen];
            fu.InputStream.Read(imgArray, 0, fileLen);
            MemoryStream ms = new MemoryStream(imgArray);
            System.IO.BinaryReader br = new System.IO.BinaryReader(ms);
            string fileclass = "";
            byte buffer;
            try
            {
                buffer = br.ReadByte();
                fileclass = buffer.ToString();
                buffer = br.ReadByte();
                fileclass += buffer.ToString();
            }
            catch
            {
            }
            br.Close();
            ms.Close();
            //注意将文件流指针还原
            fu.InputStream.Position = 0;
            foreach (FileExtension fe in fileEx)
            {
                if (Int32.Parse(fileclass) == (int)fe)
                    return true;
            }
            return false;
        }



        /// <summary>
        /// 根据文件头判断上传的文件类型
        /// </summary>
        /// <param name="filePath">filePath是文件的完整路径 </param>
        /// <returns>返回true或false</returns>
        public static bool IsPicture(string filePath)
        {
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(fs);
                string fileClass;
                byte buffer;
                buffer = reader.ReadByte();
                fileClass = buffer.ToString();
                buffer = reader.ReadByte();
                fileClass += buffer.ToString();
                reader.Close();
                fs.Close();
                if (fileClass == "255216" || fileClass == "7173" || fileClass == "13780" || fileClass == "6677")
                //255216是jpg;7173是gif;6677是BMP,13780是PNG;7790是exe,8297是rar 
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 判断文件是否为图片
        /// </summary>
        /// <param name="path">文件的完整路径</param>
        /// <returns>返回结果</returns>
        public static Boolean IsImage(string path)
        {
            try
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(path);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    
    }
}
