using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace JN.WindowsService
{
    public partial class Service1 : ServiceBase
    {
        DataTable dt;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        //public void OnStart()
        {
            //EventLog.WriteEntry("我的服务启动"); //系统事件上查看器里的应用程序事件里来源描述
            WriteLog("服务启动");

            ////K线定时
            //System.Timers.Timer t3 = new System.Timers.Timer();
            //t3.Interval = 1000 * 60;
            //t3.Elapsed += new System.Timers.ElapsedEventHandler(KlineSetData); //到达时间执行事件;
            //t3.AutoReset = true; //设置是执行一次（false)还是一直执行(true);
            //t3.Enabled = true; //是否执行System.Timers.Timer.Elapsed事件;

            //System.Timers.Timer t = new System.Timers.Timer();
            //t.Interval = 1000 * 60;
            //t.Elapsed += new System.Timers.ElapsedEventHandler(jiancha); //到达时间执行事件;
            //t.AutoReset = true; //设置是执行一次（false)还是一直执行(true);
            //t.Enabled = true; //是否执行System.Timers.Timer.Elapsed事件;

            //每日释放+流通奖励
            System.Timers.Timer t2 = new System.Timers.Timer();
            t2.Interval = 1000 * 60;
            t2.Elapsed += new System.Timers.ElapsedEventHandler(ChkSrv); //到达时间执行事件;
            t2.AutoReset = true; //设置是执行一次（false)还是一直执行(true);
            t2.Enabled = true; //是否执行System.Timers.Timer.Elapsed事件;           

            //System.Timers.Timer t3 = new System.Timers.Timer();
            //t.Interval = 60000 * 5;//K图5分钟线
            //t.Elapsed += new System.Timers.ElapsedEventHandler(KlineSetData); //到达时间执行事件;
            //t.AutoReset = true; //设置是执行一次（false)还是一直执行(true);
            //t.Enabled = true; //是否执行System.Timers.Timer.Elapsed事件;  

        }

        protected override void OnStop()
        {
            WriteLog("服务停止");
            //EventLog.WriteEntry("我的服务停止");
        }

        public void ChkSrv(object source, System.Timers.ElapsedEventArgs e)
        {
            //int intHour = e.SignalTime.Hour;
            //int intMinute = e.SignalTime.Minute;
            //int intSecond = e.SignalTime.Second;
          

            string PlanXML = AppDomain.CurrentDomain.BaseDirectory + "Plan.xml";
            if (System.IO.File.Exists(PlanXML))
            {
                dt = XmlDataSet.GetDataSetByXml(PlanXML).Tables[0];
            }
            WriteLog("读取" + PlanXML + "(" + dt.Rows.Count + "条作业)");
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    bool isExec = false;
                    DateTime starttime = DateTime.Now;

                    bool tj1 = Convert.ToInt16(dt.Rows[i]["cycletype"]) == 0 && DateTime.Now.ToString("HH:mm") == Convert.ToDateTime(dt.Rows[i]["time"]).ToString("HH:mm");  //符合日结
                    bool tj2 = Convert.ToInt16(dt.Rows[i]["cycletype"]) == 1 && DateTime.Now.DayOfWeek.ToString() == dt.Rows[i]["cyclevalue"].ToString() && DateTime.Now.ToString("HH:mm") == Convert.ToDateTime(dt.Rows[i]["time"]).ToString("HH:mm"); //符合周结
                    bool tj3 = Convert.ToInt16(dt.Rows[i]["cycletype"]) == 2 && DateTime.Now.Day.ToString() == dt.Rows[i]["cyclevalue"].ToString() && DateTime.Now.ToString("HH:mm") == Convert.ToDateTime(dt.Rows[i]["time"]).ToString("HH:mm"); //符合月结
                    bool tj4 = Convert.ToInt16(dt.Rows[i]["cycletype"]) == 3 && DateTime.Now.Minute == Convert.ToDateTime(dt.Rows[i]["time"]).Minute;  //符合时结

                    if (tj1 || tj2 || tj3 || tj4)
                    {
                        WriteLog("触发定时作业");
                        string url = ReadFile(AppDomain.CurrentDomain.BaseDirectory + "url.txt");
                        if (!string.IsNullOrEmpty(url))
                        {
                            url += "timingplan/index?execprocess=" + dt.Rows[i]["name"].ToString();
                            WriteLog("读取访问地址：" + url);
                            try
                            {
                                System.Net.HttpWebRequest myHttpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                                System.Net.HttpWebResponse myHttpWebResponse = (System.Net.HttpWebResponse)myHttpWebRequest.GetResponse();
                                System.IO.Stream receiveStream = myHttpWebResponse.GetResponseStream();//得到回写的字节流
                                receiveStream.Close();
                                //WriteLog(receiveStream.ToString());
                                isExec = true;
                            }
                            catch (Exception ex)
                            {
                                WriteLog(ex.ToString());
                            }
                            if (isExec)
                                WriteLog("成功执行作业计划“" + dt.Rows[i]["name"].ToString() + "”，时间在" + DateTime.Now.ToString() + "");
                        }
                    }
                }
            }
            dt.Dispose();
            
        }




        /// <summary>
        /// 写入K线图数据
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>

        public void KlineSetData(object source, System.Timers.ElapsedEventArgs e)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            //WriteLog("执行K线数据写入测试时间：" + time);
            //WriteLog("执行K线数据写入测试时间2：" + time.Split(':')[2]);
            //bool zx = time.Split(':')[2].Contains("00");
            //WriteLog("是否执行k线：" + zx);


            //if (time.Split(':')[2].Contains("00"))
            // {
                 WriteLog("执行K线数据写入，时间：" + time);
                 string url = ReadFile(AppDomain.CurrentDomain.BaseDirectory + "url.txt");
                 if (!string.IsNullOrEmpty(url))
                 {
                     url += "timingplan/index?execprocess=plan4";
                    bool isExec = false;
                    try
                     {
                         System.Net.HttpWebRequest myHttpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                         System.Net.HttpWebResponse myHttpWebResponse = (System.Net.HttpWebResponse)myHttpWebRequest.GetResponse();
                         System.IO.Stream receiveStream = myHttpWebResponse.GetResponseStream();//得到回写的字节流
                         receiveStream.Close();
                        //WriteLog(receiveStream.ToString());
                        isExec = true;
                    }
                     catch (Exception ex)
                     {
                         WriteLog(ex.ToString());
                     }
                    if (isExec)
                        WriteLog("成功执行作业计划“plan4”，时间在" + DateTime.Now.ToString() + "");
                }
             //}
        }


        /// <summary>
        /// 检查超时付款和超时确认付款
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void jiancha(object source, System.Timers.ElapsedEventArgs e)
        {

            string url = ReadFile(AppDomain.CurrentDomain.BaseDirectory + "url.txt");
            if (!string.IsNullOrEmpty(url))
            {
                url += "timingplan/index?execprocess=plan2";
                bool isExec = false;
                try
                {
                    System.Net.HttpWebRequest myHttpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                    System.Net.HttpWebResponse myHttpWebResponse = (System.Net.HttpWebResponse)myHttpWebRequest.GetResponse();
                    System.IO.Stream receiveStream = myHttpWebResponse.GetResponseStream();//得到回写的字节流
                    //WriteLog(receiveStream.ToString());
                    receiveStream.Close();
                    isExec = true;
                }
                catch (Exception ex)
                {
                    WriteLog(ex.ToString());
                }
                if (isExec)
                    WriteLog("成功执行作业计划“plan2”，时间在" + DateTime.Now.ToString() + "");
            }

        }





        /// <summary>
        /// 10分钟更换价格
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void ChangePriceMinutes(object source, System.Timers.ElapsedEventArgs e)
        {

            string url = ReadFile(AppDomain.CurrentDomain.BaseDirectory + "url.txt");
            if (!string.IsNullOrEmpty(url))
            {
                url += "timingplan/index?execprocess=plan4";
                try
                {
                    System.Net.HttpWebRequest myHttpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                    System.Net.HttpWebResponse myHttpWebResponse = (System.Net.HttpWebResponse)myHttpWebRequest.GetResponse();
                    System.IO.Stream receiveStream = myHttpWebResponse.GetResponseStream();//得到回写的字节流
                    //WriteLog(receiveStream.ToString());
                }
                catch (Exception ex)
                {
                    WriteLog(ex.ToString());
                }
            }

        }


        public void Fenhong(object source, System.Timers.ElapsedEventArgs e)
        {
           
                        WriteLog("触发定时作业");
                        string url = ReadFile(AppDomain.CurrentDomain.BaseDirectory + "url.txt");
                        if (!string.IsNullOrEmpty(url))
                        {
                            url += "timingplan/index?execprocess=plan1" ;
                            WriteLog("读取访问地址：" + url);
                            try
                            {
                                System.Net.HttpWebRequest myHttpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                                System.Net.HttpWebResponse myHttpWebResponse = (System.Net.HttpWebResponse)myHttpWebRequest.GetResponse();
                                System.IO.Stream receiveStream = myHttpWebResponse.GetResponseStream();//得到回写的字节流
                                //WriteLog(receiveStream.ToString());
                            }
                            catch (Exception ex)
                            {
                                WriteLog(ex.ToString());
                            }
                            WriteLog("成功执行作业计划plan2，时间在" + DateTime.Now.ToString() + "");
                        }
        



        }

        //public void SendMessage()
        //{
        //    string url = "http://198.44.249.86:8099/";
        //    try
        //    {
        //        System.Net.HttpWebRequest myHttpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
        //        System.Net.HttpWebResponse myHttpWebResponse = (System.Net.HttpWebResponse)myHttpWebRequest.GetResponse();
        //        System.IO.Stream receiveStream = myHttpWebResponse.GetResponseStream();//得到回写的字节流
        //        WriteLog("- " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 模拟打开" + url + "\r\n");
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog("- " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message + "\r\n");
        //    }
        //}

        /// <summary>
        /// 从指定位置的文件中读取字符串。
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public string ReadFile(string path)
        {
            if (!File.Exists(path))
                return string.Empty;
            string rs = string.Empty;
            using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
            {
                rs = sr.ReadToEnd();
                sr.Close();
            }
            return rs;
        }

        public void WriteLog(string readme)
        {

            //logs.WriteTimepLanLog("- " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + readme + "\r\n", AppDomain.CurrentDomain.BaseDirectory + "Log\\");
            StreamWriter dout = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "WServ_InnPointLog.txt", true);
            dout.Write(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " >>" + readme + "\r\n");
            dout.Close(); //关闭StreamWriter对象
        }
    }

    /// <summary>
    /// OperateXmlByDataSet 的摘要说明。
    /// </summary>
    public class XmlDataSet
    {
        public XmlDataSet()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        #region GetDataSetByXml
        /// <summary>
        /// 读取xml直接返回DataSet
        /// </summary>
        /// <param name="strXmlPath">xml文件相对路径</param>
        /// <returns></returns>
        public static DataSet GetDataSetByXml(string strXmlPath)
        {
            try
            {
                DataSet ds = new DataSet();
                ds.ReadXml(strXmlPath);
                if (ds.Tables.Count > 0)
                {
                    return ds;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion

        #region GetDataViewByXml
        /// <summary>
        /// 读取Xml返回一个经排序或筛选后的DataView
        /// </summary>
        /// <param name="strXmlPath"></param>
        /// <param name="strWhere">筛选条件，如："name = 'kgdiwss'"</param>
        /// <param name="strSort">排序条件，如："Id desc"</param>
        /// <returns></returns>
        public static DataView GetDataViewByXml(string strXmlPath, string strWhere, string strSort)
        {
            try
            {
                DataSet ds = new DataSet();
                ds.ReadXml(strXmlPath);
                DataView dv = new DataView(ds.Tables[0]);
                if (strSort != null)
                {
                    dv.Sort = strSort;
                }
                if (strWhere != null)
                {
                    dv.RowFilter = strWhere;
                }
                return dv;
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion

    }
}
