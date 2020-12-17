using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Drawing;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Text;


namespace JN.Services.Tool
{
    public static class EmailHelper
    {

        /// <summary>发送email,默认是25端口
        /// 
        /// </summary>
        /// <param name="title">邮件标题</param>
        /// <param name="body">邮件内容</param>
        /// <param name="toAdress">收件人</param>
        public static bool SendMail(string title, string body, string toAdress, string smtpEmailAddress, string smtpUserName, string smtpPassword, string smtpServer, int smtpPort)
        {
            try
            {
                System.Web.Mail.MailMessage mail = new System.Web.Mail.MailMessage();
                try
                {
                    mail.To = toAdress;
                    mail.From = smtpEmailAddress;
                    mail.Subject = title;
                    mail.BodyFormat = System.Web.Mail.MailFormat.Html;
                    mail.Body = body;
                    mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", "1"); //basic authentication
                    mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", smtpUserName); //set your username here
                    mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", smtpPassword); //set your password here
                    mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserverport", smtpPort);//set port
                    mail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpusessl", "true");//set is ssl   
                    System.Web.Mail.SmtpMail.SmtpServer = smtpServer;
                    System.Web.Mail.SmtpMail.Send(mail);
                    return true;
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
                return false;

                //MailAddress to = new MailAddress(toAdress);
                //MailAddress from = new MailAddress(smtpEmailAddress);
                //System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage(from, to);
                //message.IsBodyHtml = true; // 如果不加上这句那发送的邮件内容中有HTML会原样输出 
                //message.Subject = title; message.Body = body;
                //SmtpClient smtp = new SmtpClient();
                //smtp.UseDefaultCredentials = true;
                //smtp.Port = smtpPort;
                //smtp.Credentials = new NetworkCredential(smtpUserName, smtpPassword);
                //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                //smtp.EnableSsl = true;
                //smtp.Host = smtpServer;
                //message.To.Add(toAdress);
                //smtp.Send(message);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// 发送电子邮件函数
        /// </summary>
        /// <param name="txtHost">电子邮件服务主机名称</param>
        /// <param name="txtFrom">发送人地址</param>
        /// <param name="txtPass">发信人密码</param>
        /// <param name="txtTo">收信人地址</param>
        /// <param name="txtSubject">邮件标题</param>
        /// <param name="txtBody">邮件内容</param>
        /// <param name="isBodyHtml">是否采用HTML编码</param>
        /// <param name="priority">电子邮件的优先级别</param>
        /// <param name="encoding">内容采用的编码方式</param>
        /// <param name="files">附件</param>
        /// <returns>操作结果</returns>
        public static string SendMail(string txtHost, string txtFrom, string txtPass, string txtTo, string txtSubject, string txtBody, bool isBodyHtml, MailPriority priority, System.Text.Encoding encoding, string[] files)
        {
            //电子邮件附件
            Attachment data = null;
            //传送的电子邮件类
            MailMessage message = new MailMessage(txtFrom, txtTo);
            //设置标题
            message.Subject = txtSubject;
            //设置内容
            message.Body = txtBody;
            //是否采用HTML编码
            message.IsBodyHtml = isBodyHtml;
            //电子邮件的优先级别
            message.Priority = priority;
            //内容采用的编码方式
            message.BodyEncoding = encoding;
            try
            {
                //添加附件
                if (files != null && files.Length > 0)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        //实例化电子邮件附件，并设置类型
                        data = new Attachment(files[i], System.Net.Mime.MediaTypeNames.Application.Octet);
                        //实例邮件内容
                        System.Net.Mime.ContentDisposition disposition = data.ContentDisposition;
                        //取得建档日期
                        disposition.CreationDate = System.IO.File.GetCreationTime(files[i]);
                        //取得附件修改日期
                        disposition.ModificationDate = System.IO.File.GetLastWriteTime(files[i]);
                        //取得读取日期
                        disposition.ReadDate = System.IO.File.GetLastAccessTime(files[i]);
                        //设定文件名称
                        System.IO.FileInfo fi = new System.IO.FileInfo(files[i]);
                        disposition.FileName = fi.Name.ToString();
                        //添加附件
                        message.Attachments.Add(data);
                    }
                }
                //实例从送电子邮件类
                SmtpClient client = new SmtpClient();
                //设置电子邮件主机名称
                client.Host = txtHost;
                //取得寄信人认证
                client.Credentials = new NetworkCredential(txtFrom, txtPass);
                //发送电子邮件
                client.Send(message);
                return "邮件发送成功";
            }
            catch (Exception Err)
            {
                //返回错误信息
                return Err.Message;
            }
            finally
            {
                //销毁电子邮件附件
                if (data != null)
                {
                    data.Dispose();
                }
                //销毁传送的电子邮件实例
                message.Dispose();
            }
        }
    }
}
