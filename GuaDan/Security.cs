using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace GuaDan
{
    public class Security
    {
        public static string Account = "";
        public static string Pwd = "";
        public static string PreTag = "eatZD";
        /// <summary>
        /// 是否为测试账号
        /// </summary>
        public static bool IsTry = true;
        public static bool IsRepair = false;

        private static string cServer = "https://reg789.top/";
        //private static string cServer = "http://localhost:60374/";

        private static string cSession = "";

        public static bool DoReg(string cServer2, string cMachineCode)
        {
            RegResult result = new RegResult();
            string str2 = Connect.getDocument(cServer + "Reg.aspx?cMachineCode=" + cMachineCode, null, null, "utf-8");
            if (string.IsNullOrEmpty(str2))
            {
                return false;
            }
            else
            {
                //CryptoHelper cry = new CryptoHelper();
                //str2 = cry.GetDecryptedValue(str2);
            }
            return (str2.Trim().Length == 40);
        }

        public static string GetMachineCode()
        {
            string src = PreTag;
            string str4 = "";
            string str5 = "";
            ManagementObjectCollection instances = new ManagementClass("Win32_DiskDrive").GetInstances();
            foreach (ManagementObject obj2 in instances)
            {
                str4 = (string)obj2.Properties["Model"].Value;
                src = src + "_HDID:" + str4;
            }
            ManagementObjectCollection objects2 = new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances();
            foreach (ManagementObject obj2 in objects2)
            {
                if ((bool)obj2["IPEnabled"])
                {
                    str5 = obj2["MacAddress"].ToString();
                    src = src + "_MACID:" + str5;
                }
                obj2.Dispose();
            }
            return Encryption.SHA1(src);
        }

        public static RegResult GetRegStatus()
        {
            RegResult result = null;
            System.Threading.Thread.Sleep(500);
            CryptoHelper cry = new CryptoHelper();
            string cMachineCode = $"{PreTag}|{Account}|{Pwd}";
            string strData = $"{cMachineCode}|{cSession}|{DateTime.Now.Millisecond}";
            string encryData = cry.GetEncryptedValue(strData);
            //处理“+”的情况
            encryData = encryData.Replace("+", "%2B");
            Int32 d = Util.ConvertDateTimeToInt32(DateTime.Now);
            string url = cServer + "api/CheckStatus/login?code=" + encryData + "&d=" + d + "&r=" + new Random(DateTime.Now.Millisecond).Next(100, 99999);
            string str2 = Connect.getDocument(url, null, null, "utf-8");
            if (!string.IsNullOrEmpty(str2))
            {
                result = new RegResult();

                str2 = cry.GetDecryptedValue(str2);
                string[] strArray = str2.Split(new char[] { '|' });
                if (strArray.Length < 3)
                {
                    return result;
                }
                string Type = strArray[0];
                string Account = strArray[1];
                Int32 ExpireDate = Int32.Parse(strArray[2]);
                double Money = double.Parse(strArray[3]);
                bool IsOk = bool.Parse(strArray[4]);
                bool istry = bool.Parse(strArray[5]);
                IsRepair = !string.IsNullOrEmpty(strArray[6]);
                string Msg = strArray[7];

                DateTime dExpiredTime = Util.ConvertToDateTime(ExpireDate);
                result.SetExpiredTime(dExpiredTime);
                result.SetMsg(Msg);
                result.SetMaxSubmitRMB(Money);
                result.SetResult(IsOk);
                result.IsTry = istry;
                IsTry = istry;
            }
            return result;
        }

        public static RegResult GetOnlineStatus()
        {
            RegResult result = null;
            System.Threading.Thread.Sleep(500);
            CryptoHelper cry = new CryptoHelper();

            string cMachineCode = $"{PreTag}|{Account}|{Pwd}";
            string strData = $"{cMachineCode}|{cSession}|{DateTime.Now.Millisecond}";
            string encryData = cry.GetEncryptedValue(strData);
            //处理“+”的情况
            encryData = encryData.Replace("+", "%2B");
            Int32 d = Util.ConvertDateTimeToInt32(DateTime.Now);
            string url = cServer + "api/CheckStatus/online?code=" + encryData + "&d=" + d + "&r=" + new Random(DateTime.Now.Millisecond).Next(100, 99999);
            string str2 = Connect.getDocument(url, null, null, "utf-8");
            if (!string.IsNullOrEmpty(str2))
            {
                result = new RegResult();

                str2 = cry.GetDecryptedValue(str2);
                string[] strArray = str2.Split(new char[] { '|' });
                if (strArray.Length < 3)
                {
                    return result;
                }
                string Type = strArray[0];
                string Account = strArray[1];
                Int32 ExpireDate = Int32.Parse(strArray[2]);
                double Money = double.Parse(strArray[3]);
                bool IsOk = bool.Parse(strArray[4]);
                bool istry = bool.Parse(strArray[5]);
                IsRepair = !string.IsNullOrEmpty(strArray[6]);
                string Msg = strArray[7];

                DateTime dExpiredTime = Util.ConvertToDateTime(ExpireDate);
                result.SetExpiredTime(dExpiredTime);
                result.SetMsg(Msg);
                result.SetMaxSubmitRMB(Money);
                result.SetResult(IsOk);
                result.IsTry = istry;
                IsTry = istry;
            }
            return result;
        }

        public static RegResult ChangePwd(string pwd, string pwd2)
        {
            RegResult result = null;
            System.Threading.Thread.Sleep(500);
            CryptoHelper cry = new CryptoHelper();

            string cMachineCode = $"{PreTag}|{Account}|{pwd}";
            string strData = $"{cMachineCode}|{cSession}|{DateTime.Now.Millisecond}";
            string encryData = cry.GetEncryptedValue(strData);
            //处理“+”的情况
            encryData = encryData.Replace("+", "%2B");
            Int32 d = Util.ConvertDateTimeToInt32(DateTime.Now);
            string url = cServer + "api/CheckStatus/ChangePwd?code=" + encryData + "&pwd2=" + pwd2 + "&d=" + d + "&r=" + new Random(DateTime.Now.Millisecond).Next(100, 99999);
            string str2 = Connect.getDocument(url, null, null, "utf-8");
            if (!string.IsNullOrEmpty(str2))
            {
                result = new RegResult();

                str2 = cry.GetDecryptedValue(str2);
                string[] strArray = str2.Split(new char[] { '|' });
                if (strArray.Length < 3)
                {
                    return result;
                }
                string Type = strArray[0];
                string Account = strArray[1];
                Int32 ExpireDate = Int32.Parse(strArray[2]);
                double Money = double.Parse(strArray[3]);
                bool IsOk = bool.Parse(strArray[4]);
                bool istry = bool.Parse(strArray[5]);
                IsRepair = !string.IsNullOrEmpty(strArray[6]);
                string Msg = strArray[7];

                DateTime dExpiredTime = Util.ConvertToDateTime(ExpireDate);
                result.SetExpiredTime(dExpiredTime);
                result.SetMsg(Msg);
                result.SetMaxSubmitRMB(Money);
                result.SetResult(IsOk);
                result.IsTry = istry;
                IsTry = istry;
            }
            return result;
        }


        public static int GetThisTaskCount()
        {
            int num = 0;
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.StandardInput.WriteLine("tasklist");
            process.StandardInput.WriteLine("exit");
            string[] strArray = process.StandardOutput.ReadToEnd().Split(new char[] { '\n' });
            for (int i = 0; i < strArray.Length; i++)
            {
                if (strArray[i].ToLower().Trim().StartsWith(AppDomain.CurrentDomain.FriendlyName.ToLower()))
                {
                    num++;
                }
            }
            return num;
        }

        public static bool PostUnRegUserData(string cMachineCode, string data)
        {
            string cUrl = cServer + "PostUserData.aspx";
            data = data + "&cMachineCode=" + cMachineCode;
            string str2 = Connect.postDocument(cUrl, data);
            if (str2 == null)
            {
                return false;
            }
            else
            {
                try
                {
                    CryptoHelper cry = new CryptoHelper();
                    str2 = cry.GetDecryptedValue(str2);
                }
                catch (Exception ex)
                {
                }
            }
            return str2.Trim().Equals("true");
        }
    }
}
