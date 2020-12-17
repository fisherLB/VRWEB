using System;
using System.Collections;
using System.Xml;
namespace JN.Services.Tool
{
    public class XmlHelper
    {
        #region 公共变量
        public static XmlDocument xmldoc;
        public static XmlNode xmlnode;
        public static XmlElement xmlelem;
        #endregion
        #region 创建Xml文档
        /// <summary>
        /// 创建一个带有根用户的Xml文件
        /// </summary>
        /// <param name="FileName">Xml文件名称</param>
        /// <param name="rootName">根用户名称</param>
        /// <param name="Encode">编码方式:gb2312，UTF-8等常见的</param>
        /// <param name="DirPath">保存的目录路径</param>
        /// <returns></returns>
        public static bool CreateXmlDocument(string FileName, string RootName, string Encode)
        {
            try
            {
                xmldoc = new XmlDocument();
                XmlDeclaration xmldecl;
                xmldecl = xmldoc.CreateXmlDeclaration("1.0", Encode, null);
                xmldoc.AppendChild(xmldecl);
                xmlelem = xmldoc.CreateElement("", RootName, "");
                xmldoc.AppendChild(xmlelem);
                xmldoc.Save(FileName);
                return true;
            }
            catch (Exception e)
            {
                return false;
                throw new Exception(e.Message);
            }
        }
        #endregion
        #region 常用操作方法(增删改)
        /// <summary>
        /// 插入一个用户和它的若干子用户
        /// </summary>
        /// <param name="XmlFile">Xml文件路径</param>
        /// <param name="NewNodeName">插入的用户名称</param>
        /// <param name="HasAttributes">此用户是否具有属性，True为有，False为无</param>
        /// <param name="fatherNode">此插入用户的父用户,要匹配的XPath表达式(例如:"//用户名//子用户名)</param>
        /// <param name="htAtt">此用户的属性，Key为属性名，Value为属性值</param>
        /// <param name="htSubNode">子用户的属性，Key为Name,Value为InnerText</param>
        /// <returns>返回真为更新成功，否则失败</returns>
        public static bool InsertNode(string XmlFile, string NewNodeName, bool HasAttributes, string fatherNode, Hashtable htAtt, Hashtable htSubNode)
        {
            try
            {
                xmldoc = new XmlDocument();
                xmldoc.Load(XmlFile);
                XmlNode root = xmldoc.SelectSingleNode(fatherNode);
                xmlelem = xmldoc.CreateElement(NewNodeName);
                if (htAtt != null && HasAttributes)//若此用户有属性，则先添加属性
                {
                    SetAttributes(xmlelem, htAtt);
                    SetNodes(xmlelem.Name, xmldoc, xmlelem, htSubNode);//添加完此用户属性后，再添加它的子用户和它们的InnerText
                }
                else
                {
                    SetNodes(xmlelem.Name, xmldoc, xmlelem, htSubNode);//若此用户无属性，那么直接添加它的子用户
                }
                root.AppendChild(xmlelem);
                xmldoc.Save(XmlFile);
                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 更新用户
        /// </summary>
        /// <param name="XmlFile">Xml文件路径</param>
        /// <param name="fatherNode">需要更新用户的上级用户,要匹配的XPath表达式(例如:"//用户名//子用户名)</param>
        /// <param name="htAtt">需要更新的属性表，Key代表需要更新的属性，Value代表更新后的值</param>
        /// <param name="htSubNode">需要更新的子用户的属性表，Key代表需要更新的子用户名字Name,Value代表更新后的值InnerText</param>
        /// <returns>返回真为更新成功，否则失败</returns>
        public static bool UpdateNode(string XmlFile, string fatherNode, Hashtable htAtt, Hashtable htSubNode)
        {
            try
            {
                xmldoc = new XmlDocument();
                xmldoc.Load(XmlFile);
                XmlNodeList root = xmldoc.SelectSingleNode(fatherNode).ChildNodes;
                UpdateNodes(root, htAtt, htSubNode);
                xmldoc.Save(XmlFile);
                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 删除指定用户下的子用户
        /// </summary>
        /// <param name="XmlFile">Xml文件路径</param>
        /// <param name="fatherNode">制定用户,要匹配的XPath表达式(例如:"//用户名//子用户名)</param>
        /// <returns>返回真为更新成功，否则失败</returns>
        public static bool DeleteNodes(string XmlFile, string fatherNode)
        {
            try
            {
                xmldoc = new XmlDocument();
                xmldoc.Load(XmlFile);
                xmlnode = xmldoc.SelectSingleNode(fatherNode);
                xmlnode.RemoveAll();
                xmldoc.Save(XmlFile);
                return true;
            }
            catch (XmlException xe)
            {
                throw new XmlException(xe.Message);
            }
        }
        /// <summary>
        /// 删除匹配XPath表达式的第一个用户(用户中的子元素同时会被删除)
        /// </summary>
        /// <param name="xmlFileName">XML文档完全文件名(包含物理路径)</param>
        /// <param name="xpath">要匹配的XPath表达式(例如:"//用户名//子用户名</param>
        /// <returns>成功返回true,失败返回false</returns>
        public static bool DeleteXmlNodeByXPath(string xmlFileName, string xpath)
        {
            bool isSuccess = false;
            xmldoc = new XmlDocument();
            try
            {
                xmldoc.Load(xmlFileName); //加载XML文档
                XmlNode xmlNode = xmldoc.SelectSingleNode(xpath);
                if (xmlNode != null)
                {
                    //删除用户
                    xmldoc.ParentNode.RemoveChild(xmlNode);
                }
                xmldoc.Save(xmlFileName); //保存到XML文档
                isSuccess = true;
            }
            catch (Exception ex)
            {
                throw ex; //这里可以定义你自己的异常处理
            }
            return isSuccess;
        }
        /// <summary>
        /// 删除匹配XPath表达式的第一个用户中的匹配参数xmlAttributeName的属性
        /// </summary>
        /// <param name="xmlFileName">XML文档完全文件名(包含物理路径)</param>
        /// <param name="xpath">要匹配的XPath表达式(例如:"//用户名//子用户名</param>
        /// <param name="xmlAttributeName">要删除的xmlAttributeName的属性名称</param>
        /// <returns>成功返回true,失败返回false</returns>
        public static bool DeleteXmlAttributeByXPath(string xmlFileName, string xpath, string xmlAttributeName)
        {
            bool isSuccess = false;
            bool isExistsAttribute = false;
            xmldoc = new XmlDocument();
            try
            {
                xmldoc.Load(xmlFileName); //加载XML文档
                XmlNode xmlNode = xmldoc.SelectSingleNode(xpath);
                XmlAttribute xmlAttribute = null;
                if (xmlNode != null)
                {
                    //遍历xpath用户中的所有属性
                    foreach (XmlAttribute attribute in xmlNode.Attributes)
                    {
                        if (attribute.Name.ToLower() == xmlAttributeName.ToLower())
                        {
                            //用户中存在此属性
                            xmlAttribute = attribute;
                            isExistsAttribute = true;
                            break;
                        }
                    }
                    if (isExistsAttribute)
                    {
                        //删除用户中的属性
                        xmlNode.Attributes.Remove(xmlAttribute);
                    }
                }
                xmldoc.Save(xmlFileName); //保存到XML文档
                isSuccess = true;
            }
            catch (Exception ex)
            {
                throw ex; //这里可以定义你自己的异常处理
            }
            return isSuccess;
        }
        /*柯乐义*/
        /// <summary>
        /// 删除匹配XPath表达式的第一个用户中的所有属性
        /// </summary>
        /// <param name="xmlFileName">XML文档完全文件名(包含物理路径)</param>
        /// <param name="xpath">要匹配的XPath表达式(例如:"//用户名//子用户名</param>
        /// <returns>成功返回true,失败返回false</returns>
        public static bool DeleteAllXmlAttributeByXPath(string xmlFileName, string xpath)
        {
            bool isSuccess = false;
            xmldoc = new XmlDocument();
            try
            {
                xmldoc.Load(xmlFileName); //加载XML文档
                XmlNode xmlNode = xmldoc.SelectSingleNode(xpath);
                if (xmlNode != null)
                {
                    //遍历xpath用户中的所有属性
                    xmlNode.Attributes.RemoveAll();
                }
                xmldoc.Save(xmlFileName); //保存到XML文档
                isSuccess = true;
            }
            catch (Exception ex)
            {
                throw ex; //这里可以定义你自己的异常处理
            }
            return isSuccess;
        }
        #endregion
        #region 私有方法
        /// <summary>
        /// 设置用户属性
        /// </summary>
        /// <param name="xe">用户所处的Element</param>
        /// <param name="htAttribute">用户属性，Key代表属性名称，Value代表属性值</param>
        private static void SetAttributes(XmlElement xe, Hashtable htAttribute)
        {
            foreach (DictionaryEntry de in htAttribute)
            {
                xe.SetAttribute(de.Key.ToString(), de.Value.ToString());
            }
        }
        /// <summary>
        /// 增加子用户到根用户下
        /// </summary>
        /// <param name="rootNode">上级用户名称</param>
        /// <param name="XmlDoc">Xml文档</param>
        /// <param name="rootXe">父根用户所属的Element</param>
        /// <param name="SubNodes">子用户属性，Key为Name值，Value为InnerText值</param>
        private static void SetNodes(string rootNode, XmlDocument XmlDoc, XmlElement rootXe, Hashtable SubNodes)
        {
            if (SubNodes == null)
                return;
            foreach (DictionaryEntry de in SubNodes)
            {
                xmlnode = XmlDoc.SelectSingleNode(rootNode);
                XmlElement subNode = XmlDoc.CreateElement(de.Key.ToString());
                subNode.InnerText = de.Value.ToString();
                rootXe.AppendChild(subNode);
            }
        }
        /// <summary>
        /// 更新用户属性和子用户InnerText值。柯 乐 义
        /// </summary>
        /// <param name="root">根用户名字</param>
        /// <param name="htAtt">需要更改的属性名称和值</param>
        /// <param name="htSubNode">需要更改InnerText的子用户名字和值</param>
        private static void UpdateNodes(XmlNodeList root, Hashtable htAtt, Hashtable htSubNode)
        {
            foreach (XmlNode xn in root)
            {
                xmlelem = (XmlElement)xn;
                if (xmlelem.HasAttributes)//如果用户如属性，则先更改它的属性
                {
                    foreach (DictionaryEntry de in htAtt)//遍历属性哈希表
                    {
                        if (xmlelem.HasAttribute(de.Key.ToString()))//如果用户有需要更改的属性
                        {
                            xmlelem.SetAttribute(de.Key.ToString(), de.Value.ToString());//则把哈希表中相应的值Value赋给此属性Key
                        }
                    }
                }
                if (xmlelem.HasChildNodes)//如果有子用户，则修改其子用户的InnerText
                {
                    XmlNodeList xnl = xmlelem.ChildNodes;
                    foreach (XmlNode xn1 in xnl)
                    {
                        XmlElement xe = (XmlElement)xn1;
                        foreach (DictionaryEntry de in htSubNode)
                        {
                            if (xe.Name == de.Key.ToString())//htSubNode中的key存储了需要更改的用户名称，
                            {
                                xe.InnerText = de.Value.ToString();//htSubNode中的Value存储了Key用户更新后的数据
                            }
                        }
                    }
                }
            }
        }
        #endregion
        #region XML文档用户查询和读取
        /// <summary>
        /// 选择匹配XPath表达式的第一个用户XmlNode.
        /// </summary>
        /// <param name="xmlFileName">XML文档完全文件名(包含物理路径)</param>
        /// <param name="xpath">要匹配的XPath表达式(例如:"//用户名//子用户名")</param>
        /// <returns>返回XmlNode</returns>
        public static XmlNode GetXmlNodeByXpath(string xmlFileName, string xpath)
        {
            xmldoc = new XmlDocument();
            try
            {
                xmldoc.Load(xmlFileName); //加载XML文档
                XmlNode xmlNode = xmldoc.SelectSingleNode(xpath);
                return xmlNode;
            }
            catch (Exception ex)
            {
                return null;
                //throw ex; //这里可以定义你自己的异常处理
            }
        }
        /// <summary>
        /// 选择匹配XPath表达式的用户列表XmlNodeList.
        /// </summary>
        /// <param name="xmlFileName">XML文档完全文件名(包含物理路径)</param>
        /// <param name="xpath">要匹配的XPath表达式(例如:"//用户名//子用户名")</param>
        /// <returns>返回XmlNodeList</returns>
        public static XmlNodeList GetXmlNodeListByXpath(string xmlFileName, string xpath)
        {
            xmldoc = new XmlDocument();
            try
            {
                xmldoc.Load(xmlFileName); //加载XML文档
                XmlNodeList xmlNodeList = xmldoc.SelectNodes(xpath);
                return xmlNodeList;
            }
            catch (Exception ex)
            {
                return null;
                //throw ex; //这里可以定义你自己的异常处理
            }
        }
        /// <summary>
        /// 选择匹配XPath表达式的第一个用户的匹配xmlAttributeName的属性XmlAttribute. 柯乐义
        /// </summary>
        /// <param name="xmlFileName">XML文档完全文件名(包含物理路径)</param>
        /// <param name="xpath">要匹配的XPath表达式(例如:"//用户名//子用户名</param>
        /// <param name="xmlAttributeName">要匹配xmlAttributeName的属性名称</param>
        /// <returns>返回xmlAttributeName</returns>
        public static XmlAttribute GetXmlAttribute(string xmlFileName, string xpath, string xmlAttributeName)
        {
            string content = string.Empty;
            xmldoc = new XmlDocument();
            XmlAttribute xmlAttribute = null;
            try
            {
                xmldoc.Load(xmlFileName); //加载XML文档
                XmlNode xmlNode = xmldoc.SelectSingleNode(xpath);
                if (xmlNode != null)
                {
                    if (xmlNode.Attributes.Count > 0)
                    {
                        xmlAttribute = xmlNode.Attributes[xmlAttributeName];
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex; //这里可以定义你自己的异常处理
            }
            return xmlAttribute;
        }
        #endregion
    }
}

//XmlHelper m_menu_keleyi_com = new XmlHelper();
//m_menu_keleyi_com.CreateXmlDocument(@"D:\kel"+"eyimenu.xml", "ke"+"leyimenu", "utf-8");