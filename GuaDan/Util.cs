using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Web.Script.Serialization;
using MSScriptControl;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace GuaDan
{
    class Util
    {
        /// <summary>
        /// json字符串转换为Xml对象
        /// </summary>
        /// <param name="sJson"></param>
        /// <returns></returns>
        public static XmlDocument Json2Xml(string sJson)
        {

            JavaScriptSerializer oSerializer = new JavaScriptSerializer();
            Dictionary<string, object> Dic = (Dictionary<string, object>)oSerializer.DeserializeObject(sJson);
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDec;
            xmlDec = doc.CreateXmlDeclaration("1.0", "gb2312", "yes");
            doc.InsertBefore(xmlDec, doc.DocumentElement);
            XmlElement nRoot = doc.CreateElement("root");
            doc.AppendChild(nRoot);
            foreach (KeyValuePair<string, object> item in Dic)
            {
                XmlElement element = doc.CreateElement(item.Key);
                KeyValue2Xml(element, item);
                nRoot.AppendChild(element);
            }
            return doc;
        }

        private static void KeyValue2Xml(XmlElement node, KeyValuePair<string, object> Source)
        {
            object kValue = Source.Value;
            if (kValue.GetType() == typeof(Dictionary<string, object>))
            {
                foreach (KeyValuePair<string, object> item in kValue as Dictionary<string, object>)
                {
                    XmlElement element = node.OwnerDocument.CreateElement(item.Key);
                    KeyValue2Xml(element, item);
                    node.AppendChild(element);
                }
            }
            else if (kValue.GetType() == typeof(object[]))
            {
                object[] o = kValue as object[];
                for (int i = 0; i < o.Length; i++)
                {
                    XmlElement xitem = node.OwnerDocument.CreateElement("Item");
                    KeyValuePair<string, object> item = new KeyValuePair<string, object>("Item", o[i]);
                    KeyValue2Xml(xitem, item);
                    node.AppendChild(xitem);
                }

            }
            else
            {
                XmlText text = node.OwnerDocument.CreateTextNode(kValue.ToString());
                node.AppendChild(text);
            }
        }

        public static object GetCitiPwd(string valid, string code2, string uid2, string value)
        {
            ScriptControl sc = new ScriptControlClass();

            sc.Language = "JScript";


            string str = Properties.Resources.strMask;
            sc.AddCode(str);
            sc.AddCode(Properties.Resources.strHello);


            var parm = new object[] { valid, code2, uid2, value };
            object xRslt = sc.Run("hello1", parm);
            return xRslt;
        }

        public static object GetCitiPin(string r1, string r2, string u, string code)
        {
            MSScriptControl.ScriptControl sc = new MSScriptControl.ScriptControlClass();
            sc.Language = "JScript";


            string str = Properties.Resources.strMask;
            sc.AddCode(str);
            sc.AddCode(Properties.Resources.strPin);


            var parm = new object[] { r1, r2, u, code };
            object xRslt = sc.Run("pin", parm);
            return xRslt;
        }

        public static void DeleteOldImgFile(string path)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            foreach (FileInfo info2 in info.GetFiles())
            {
                if (info2.Extension.EndsWith("jpg"))
                {
                    long num2 = info2.LastWriteTime.Ticks / 0x2710L;
                    long num3 = DateTime.Now.Ticks / 0x2710L;
                    if ((num3 - num2) > 0x2710L)
                    {
                        try
                        {
                            info2.Delete();
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 最低交易额度为10
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static int Morethan10(double num)
        {
              //int ret = 0;
            //if (num > 2.5)
            //{

            //    double i = num / 5;
            //    ret = (int)Math.Round(i) * 5;
            //}
            return num >= 10? (int)num : 10;
        }

        public static int Closeto5(int num)
        {
            return num / 5 * 5;
        }

        /// <summary>
        /// 取偶数
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static int ClosetoEven(int num)
        {
            return  num%2==0?num : (num / 2+1) * 2;
        }
        public static DateTime ConvertToDateTime(Int32 d)
        {
            DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0);
            startTime = startTime.AddSeconds(d).ToLocalTime();
            return startTime;
        }
        public static Int32 ConvertDateTimeToInt32(string dt)
        {
            DateTime dt1 = new DateTime(1970, 1, 1, 8, 0, 0);
            DateTime dt2 = Convert.ToDateTime(dt);
            return Convert.ToInt32((dt2 - dt1).TotalSeconds);
        }

        public static Int32 ConvertDateTimeToInt32(DateTime dt)
        {
            DateTime dt1 = new DateTime(1970, 1, 1, 8, 0, 0);
            DateTime dt2 = dt;
            return Convert.ToInt32((dt2 - dt1).TotalSeconds);
        }

        public static int Text2Int(string txt)
        {
            int.TryParse(txt.Trim(),out int ret);
            return ret;
        }

        public static double Text2Double(string txt)
        {
            double.TryParse(txt.Trim(),out double ret);
            return ret;
        }

        public static object CloneObject(object obj)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter(null,
                new StreamingContext(StreamingContextStates.Clone));
                binaryFormatter.Serialize(memStream, obj);
                memStream.Seek(0, SeekOrigin.Begin);
                return binaryFormatter.Deserialize(memStream);
            }
        }
    }
}
