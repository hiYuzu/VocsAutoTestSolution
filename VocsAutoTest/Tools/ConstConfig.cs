using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace VocsAutoTest.Tools
{
    public class ConstConfig
    {
        private static string appDir = Application.StartupPath;


        static System.Collections.Hashtable table = new Hashtable();
        static XmlNode appNode = null;
        //static IXmlEncrypt xmlEncrypt = AesCryptHelper.GetInstance();
        static XmlDocument xmlDoc = null;
        static ConstConfig()
        {
            LoadXml();
        }

        static void LoadXml()
        {
            TextWriter textWriter = null;
            try
            {
                if (!File.Exists(FileName))
                {
                    string topNodeName = "Const";
                    textWriter = File.CreateText(FileName);
                    string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<"
                                 + topNodeName + ">\r\n</" + topNodeName + ">";
                    textWriter.Write(xml);
                    textWriter.Close();
                    textWriter = null;
                }

                xmlDoc = new XmlDocument();
                xmlDoc.Load(FileName);
                //XmlNode appNode = xmlDoc.LastChild;
                //XmlResouces.GetInstance().TranslateXml("const", xmlDoc, null, Thread.CurrentThread.CurrentUICulture);
                appNode = xmlDoc.LastChild;
                //if (appNode.Name == xmlEncrypt.NodeNameplate)    //文件被加密
                //{
                //    string decrptXml = xmlEncrypt.Decrypt(appNode.InnerText.Trim());

                //    XmlDocument tXmlDoc = new XmlDocument();
                //    tXmlDoc.LoadXml(decrptXml);

                //    appNode = tXmlDoc.LastChild;
                //}

                foreach (XmlNode xn in appNode.ChildNodes) //遍历所有子节点 
                {
                    table.Add(xn.Name.ToLower(), xn.InnerText);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(string.Format(Resources.InitFail, FileName, ex.Message));
            }
            finally
            {
                if (textWriter != null)
                    textWriter.Close();
            }
        }

        public static string GetPath(string type)
        {
            return Path.Combine(appDir, type) + "\\";
        }
        //路径文件夹存在未创建可能，修改参见Line:133-135
        public static string AppPath
        {
            get { return appDir; }

            set { appDir = value; }
        }

        public static string DBPath
        {
            get { return GetPath("DB"); }
        }

        public static string LogPath
        {
            get { return GetPath("Log"); }
        }

        public static string BackPath
        {
            get { return GetPath("Back"); }
        }

        public static string XmlPath
        {
            get { return GetPath("Config"); }
        }

        public static string ImgPath
        {
            get
            {
                //为了支持多语言，图片根据区域信息不同放在不同路径下
                //add by zhangyq.2011-7-6
                string cultureName = System.Globalization.CultureInfo.CurrentCulture.Name;
                if (cultureName.Equals("zh-CN"))
                {
                    return GetPath("Images");
                }
                else
                {
                    return GetPath(@"Images\" + cultureName);
                }
            }
        }

        public static string TemplatePath
        {
            get { return GetPath("Template"); }
        }

        private static string FileName
        {
            get { 
                if (!Directory.Exists(XmlPath)){
                    Directory.CreateDirectory(XmlPath);
                }
                return Path.Combine(XmlPath, "Const.xml"); }
        }

        public static Hashtable GetKeyValueTable()
        {
            return table;
        }

        public static void ReLoad()
        {
            table.Clear();
#if WINCE	
            lock(FileSystem.fileLockObj)
			{
				LoadXml();
			}
#else
            LoadXml();
#endif
        }

        public static string GetValue(string name)
        {
            return (string)table[name.ToLower()];
        }

        static XmlNode GetXmlNode(string name)
        {
            if (name == null)
                return null;
            foreach (XmlNode xn in appNode.ChildNodes) //遍历所有子节点 
            {
                if (xn.Name.ToLower().Equals(name.ToLower()))
                    return xn;
            }
            return null;
        }

        public static void SaveValue(string name, string value)
        {
            SetValue(name, value);
            Save();
        }
        public static void SetValue(string name, string value)
        {
            XmlNode node = GetXmlNode(name);
            if (node == null)
            {
                XmlNode newNode = xmlDoc.CreateElement(name);
                newNode.InnerText = value;
                appNode.AppendChild(newNode);
            }
            else
            {
                node.InnerText = value;
            }
        }
        private static object syncObj = new object();
        public static void Save()
        {
            lock (syncObj)
            {
                StringBuilder sb = new StringBuilder();
                StringBuilder sbXmlText = new StringBuilder();

                sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n");
                //sb.Append("<").Append(xmlEncrypt.NodeNameplate).Append(">").Append("\r\n");

                sbXmlText.Append("<").Append(appNode.Name).Append(">").Append("\r\n");
                for (int i = 0; i < appNode.ChildNodes.Count; i++)
                {
                    XmlNode xmlNode = appNode.ChildNodes[i];
                    sbXmlText.Append("  " + xmlNode.OuterXml).Append("\r\n");
                }
                sbXmlText.Append("</").Append(appNode.Name).Append(">");

                //sb.Append(xmlEncrypt.Encrypt(sbXmlText.ToString())).Append("\r\n");
                sb.Append(sbXmlText.ToString()).Append("\r\n");
                //sb.Append("</").Append(xmlEncrypt.NodeNameplate).Append(">").Append("\r\n");

                using (TextWriter textWriter = File.CreateText(FileName))
                {
                    textWriter.Write(sb.ToString());

                    textWriter.Flush();
                }
            }

        }
    }
}
