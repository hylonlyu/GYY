using HtmlAgilityPack;
using ocr;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;


namespace GuaDan
{
    enum LoginStatus { LOGOUT = 0, LOGIN = 1, DOINGLOGIN = 2 };
    /// <summary>
    /// WP赔率结构
    /// </summary>
    public class WPOdds
    {
        public double Win
        {
            get;
            set;
        }
        public double Place
        {
            get;
            set;
        }
        public string Horse
        {
            get;
            set;
        }
    }
    public class CCMember
    {
        public delegate List<RaceInfoItem> ParseTop(HtmlNode x1, out string x2, out string x3);
        public delegate List<RaceInfoItem> ParseBelow(HtmlNode x1, string x2, string x3);


        public delegate void ShowMsgEventHandler(string str);
        public event ShowMsgEventHandler ShowMsg;

        public delegate void LoginOkEventHandler();
        public event LoginOkEventHandler OnLoginOk;

        public delegate void LogoutEventHandler();
        public event LogoutEventHandler OnLogout;

        public delegate void LoginFailEventHandler();
        public event LoginFailEventHandler OnLoginFail;

        public delegate void BetOkEventHandler(BettedItem item);
        public event BetOkEventHandler OnBetOk;

        public delegate void WorkingEventHandler();
        public event WorkingEventHandler OnWorking;

        public delegate void RaceFinishedEventHandler();
        public event RaceFinishedEventHandler OnRaceFinished;

        public delegate void NewTicketEventHandler(RaceInfoEnity enity);
        /// <summary>
        /// 有新单的事件
        /// </summary>
        public event NewTicketEventHandler OnNewTickt;



        #region 事件处理
        protected void SendBetOkEvent(BettedItem item)
        {
            OnBetOk?.Invoke(item);
        }
        protected void SendShowMsgEvent(string str)
        {
            ShowMsg?.Invoke(str);
        }
        protected void SendNewTicktEvent(RaceInfoEnity enity)
        {
            OnNewTickt?.Invoke(enity);
        }

        #endregion
        public CookieContainer cc = new CookieContainer();

        public GtConfig Config
        {
            get;
            set;
        }
        //private List<BettedItem> lstBettedRace = new List<BettedItem>();
        private Dictionary<string, BettedItem> dicBettedRace = new Dictionary<string, BettedItem>();
        /// <summary>
        /// Q的赔率
        /// </summary>
        private string[,] Qpeis = new string[15, 15];
        public Dictionary<string, double> DicManualQpeis = new Dictionary<string, double>();
        /// <summary>
        /// QP的赔率
        /// </summary>
        private string[,] QPpeis = new string[15, 15];
        public string DoMain
        {
            get;
            set;
        }
        /// <summary>
        /// 是否已经登陆了
        /// </summary>
        private bool IsLogin = false;
        private DataTable dtMatchList;
        private Thread BreakHeartThread;
        /// <summary>
        /// 代理账号信息
        /// </summary>
        protected UserInfo AccountInfo = null;


        private void OnShowMsg(string str)
        {
            ShowMsg?.Invoke(str);
        }

        private void ClearCookie()
        {
            cc = new CookieContainer();
        }

        public Hashtable DoLogin(string cDoMain, UserInfo uInfo)
        {
            ClearCookie();
            Hashtable ht = GetvCode2(cc, cDoMain);
            if (ht != null)
            {
                string valid = ht[1] as string;
                string vcode = ht[2] as string;
                string secure = ht[3] as string;
                vcode = string.Format("{0}|{1}", vcode, secure);

                Hashtable hashtable3 = DoLogin(cc, cDoMain, uInfo, vcode, valid);

                if ((hashtable3 != null) && (hashtable3.Count >= 2))
                {
                    OnShowMsg($"{uInfo.CUserName}登陆成功");
                    IsLogin = true;
                    OnLoginOk?.Invoke();
                    SendHeartbreak();
                }
                else
                {
                    OnShowMsg($"{uInfo.CUserName}登陆失败");
                    OnLoginFail?.Invoke();
                }
                return hashtable3;
            }
            else
            {
                OnShowMsg($"{uInfo.CUserName}登陆失败");
                OnLoginFail?.Invoke();
                return ht;
            }
        }
        private string Request_www_ctbwp_com(string cDoMain, string code, CookieContainer cc)
        {

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://{cDoMain}/verifypin");

                request.KeepAlive = true;
                request.Headers.Set(HttpRequestHeader.CacheControl, "max-age=0");
                request.Headers.Add("sec-ch-ua", @"""Google Chrome"";v=""119"", ""Chromium"";v=""119"", ""Not?A_Brand"";v=""24""");
                request.Headers.Add("sec-ch-ua-mobile", @"?0");
                request.Headers.Add("sec-ch-ua-platform", @"""Windows""");
                request.Headers.Add("Upgrade-Insecure-Requests", @"1");
                request.Headers.Add("Origin", $"https://{cDoMain}");
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7";
                request.Headers.Add("Sec-Fetch-Site", @"same-origin");
                request.Headers.Add("Sec-Fetch-Mode", @"navigate");
                request.Headers.Add("Sec-Fetch-User", @"?1");
                request.Headers.Add("Sec-Fetch-Dest", @"document");
                request.Referer = $"https://{cDoMain}/validate_pin.jsp";
                request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
                request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9,zh-CN;q=0.8,zh;q=0.7");

                request.CookieContainer = cc;
                request.Method = "POST";
                request.ServicePoint.Expect100Continue = false;

                string body = $"code={code}&trafficStatistics=3948524944&trafficStatisticsCanvas=2080537705&trafficStatisticsActivex=3948524944&trafficStatisticsResolution=2809515294&trafficStatistics2=87978939256ea25538911da4e0cc9113";
                byte[] postBytes = System.Text.Encoding.UTF8.GetBytes(body);
                request.ContentLength = postBytes.Length;
                Stream stream = request.GetRequestStream();
                stream.Write(postBytes, 0, postBytes.Length);
                stream.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                string str = null;
                StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("gb2312"));
                str = reader.ReadToEnd();
                reader.Close();


                responseStream.Close();
                response.Close();
                return str;

            }
            catch (WebException e)
            {
                return null;
            }
        }

        public Hashtable DoLogin(CookieContainer cc, string cDoMain, UserInfo uInfo, string vCode, string cLoginData)
        {
            AccountInfo = uInfo;
            string[] vTmp = vCode.Split(new char[] { '|' });
            vCode = vTmp[0];
            string secure = vTmp[1];
            string[] preSecure = secure.Split(new char[] { ',' });
            foreach (string item in preSecure)
            {
                string secureItem = item.Replace("\"", "").Replace(".", "");
                string securepre = secureItem;
                string tmpurl = cDoMain.Substring(cDoMain.IndexOf("."));
                string secureUrl = string.Format("https://{0}{1}", securepre, tmpurl);
                string cLoginData2 = "uid2=&pass2=&code2=&uid={0}&pass={1}&code={2}&valid={3}&lang=CH";
                string pwd = (string)Util.GetCitiPwd(cLoginData, vCode, uInfo.CUserName, uInfo.CPassword);
                cLoginData = string.Format(cLoginData2, uInfo.CUserName, pwd, vCode, cLoginData);
                //string cUrl = "https://secure.citibet.net/login";
                string cUrl = string.Format("{0}/login?uid={1}&pass={2}&code={3}&lang=CH&ssl=https:", secureUrl, uInfo.CUserName, pwd, vCode);
                string refer = "http://{0}/_index.jsp";
                string refer2 = string.Format(refer, cDoMain);

                //string cDoc = Connect.postDocument(cUrl, cLoginData, cc, null, refer2, "utf-8");
                string cDoc = Connect.getDocument(cUrl, cc, refer2, "utf-8");
                if (cDoc != null)
                {
                    if (cDoc.Contains("login.jsp?e=5&s=true"))
                    {
                        OnShowMsg("用户名字或密码无效");
                    }
                    else if (cDoc.Contains("login.jsp?e=3&s=true"))
                    {
                        OnShowMsg("验证号码不正确");
                    }
                    else
                    {
                        if (cDoc != null)
                        {
                            cUrl = string.Format("https://{0}/validate_pin.jsp?sml=m", cDoMain);
                            cDoc = Connect.getDocument(cUrl, cc, cUrl, "utf-8");
                            if (cDoc != null)
                            {
                                Regex re = new Regex(@"r1='(?'r1'[^']+)'", RegexOptions.None);
                                Match mc = re.Match(cDoc);
                                if (mc.Success)
                                {
                                    string r1 = mc.Groups["r1"].Value;
                                    re = new Regex(@"r2='(?'r2'[^']+)'", RegexOptions.None);
                                    mc = re.Match(cDoc);
                                    if (mc.Success)
                                    {
                                        string r2 = mc.Groups["r2"].Value;
                                        string pin = (string)Util.GetCitiPin(r1, r2, uInfo.CUserName, uInfo.CPin);
                                        cUrl = string.Format("https://{0}/verifypin", cDoMain);
                                        cDoc = Request_www_ctbwp_com(cDoMain, pin, cc);
                                        if (cDoc != null)
                                        {
                                            if (cDoc.Contains("validate_pin.jsp"))
                                            {
                                                OnShowMsg("二代密保错误");
                                            }
                                            else
                                            {
                                                //cUrl = " https://racing.citibet.net/terms.jsp";
                                                refer = cUrl;
                                                cUrl = string.Format("https://{0}{1}/terms.jsp", securepre, tmpurl);
                                                cDoc = Connect.getDocument(cUrl, cc, refer, "utf-8");
                                                //if (cDoc != null && cDoc.Contains("639052209421"))
                                                if (cDoc != null)
                                                {
                                                    //https://kimercs.citibet.net/select.jsp?mode=hk
                                                    refer = cUrl;
                                                    //cUrl = "https://{0}/select.jsp?mode=hk";
                                                    cUrl = string.Format("https://{0}/playerhk.jsp", cDoMain);
                                                    cDoc = Connect.getDocument(cUrl, cc, refer, "utf-8");

                                                    if (cDoc != null && cDoc.Contains("即将开始"))
                                                    {
                                                        dtMatchList = ParseMatchList(cDoc);
                                                        refer = cUrl;

                                                        //cUrl = string.Format("https://{0}/imagecontroller?action=1&x=0.9125365756917745", cDoMain);
                                                        cDoc = Connect.getDocument(cUrl, cc, refer, "utf-8");
                                                        string tmp3 = cDoc.Substring(cDoc.IndexOf("(["));
                                                        tmp3 = tmp3.Substring(0, tmp3.IndexOf("])"));
                                                        tmp3 = tmp3.Replace("([", "").Replace("])", "").Replace("\"", "");

                                                        Uri uri = new Uri(cUrl);
                                                        string host = uri.Host;
                                                        DoMain = host;
                                                        Hashtable ht = new Hashtable();
                                                        ht.Add(1, true);
                                                        ht.Add(2, host);
                                                        ht.Add(3, tmp3);
                                                        ht.Add(4, dtMatchList);

                                                        return ht;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
            return null;
        }

        #region 解释比赛列表
        private DataTable ParseMatchList(string cDoc)
        {
            DataTable dt = new DataTable();
            if (!string.IsNullOrEmpty(cDoc))
            {
                if (cDoc.IndexOf("id=\"oldcarddata\"") > 0)
                {
                    cDoc = cDoc.Substring(cDoc.IndexOf("id=\"oldcarddata\""));
                    cDoc = cDoc.Substring(0, cDoc.IndexOf("<!--end oldcarddata -->"));
                    //if (cDoc.IndexOf("<!--end oldcarddata -->") > 0)
                    {
                        //cDoc = cDoc.Substring(0, cDoc.IndexOf("<!--end oldcarddata -->"));

                        string[] matchlist = cDoc.Split(new string[] { "<!-- show c --->" }, StringSplitOptions.None);
                        matchlist = SubMatch(matchlist);
                        if (matchlist.Length > 0)
                        {

                            dt.Columns.Add("tip");
                            dt.Columns.Add("url");
                            string country = string.Empty;
                            string location = string.Empty;
                            string race = string.Empty;
                            foreach (string match in matchlist)
                            {
                                country = GetCountry(match);
                                location = GetLocation(match);
                                race = GetRace(match);
                                string[] list = match.Split(new string[] { "<div class=\"expendline" }, StringSplitOptions.None);
                                foreach (string lst in list)
                                {
                                    string location2 = GetLocation(lst);
                                    location = string.IsNullOrEmpty(location2) ? location : location2;
                                    //Regex re = new Regex(@"shwMain[^=]+=(?'url'[^&]+)[^>]+>(?'type'[^<]+)<", RegexOptions.None);
                                    Regex re = new Regex(@"shwMain[^=]+=(?'url'[^&]+)[^=]+=(?'date'[^']+)[^>]+>(?'type'[^<]+)<", RegexOptions.None);
                                    MatchCollection mc = re.Matches(lst);
                                    foreach (Match ma in mc)
                                    {
                                        string url = ma.Groups["url"].Value;
                                        string type = ma.Groups["type"].Value;
                                        string date = ma.Groups["date"].Value;
                                        IFormatProvider culture = new System.Globalization.CultureInfo("fr-FR", true);
                                        DateTime date2 = DateTime.Parse(date, culture);
                                        //bool b = DateTime.Now.Date.Equals(date2);
                                        //if (!b)
                                        //{
                                        //    continue;
                                        //}

                                        //if (!url.Contains("A"))
                                        //{
                                        //    continue;
                                        //}
                                        if (type.ToUpper().Trim() == "FC" || type.ToUpper().Trim() == "Q")
                                        {
                                            continue;
                                        }
                                        DataRow dr = dt.NewRow();

                                        dr["tip"] = string.Format("{0}_{1}_{2}_{3}_{4}_{5}", country, location, type, race, url, date2.ToShortDateString());
                                        dr["url"] = url;
                                        string filter = string.Format("tip = '{0}'", dr["tip"]);
                                        DataRow[] drs = dt.Select(filter);
                                        if (drs != null && drs.Length == 0)
                                        {
                                            dt.Rows.Add(dr);
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }
            return dt;
        }
        private string GetCountry(string cDoc)
        {
            string country = string.Empty;
            Regex re = new Regex(@"<dd class=""country_name"">(?'country'[^<]+)</dd>", RegexOptions.None);
            Match mc = re.Match(cDoc);
            if (mc.Success)
            {
                country = mc.Groups["country"].Value;
            }
            return country;
        }

        private string GetLocation(string cDoc)
        {
            Regex re = new Regex(@"<dd class=""location_name""[^>]*>(?'location'[^<]+)</dd>", RegexOptions.None);
            Match mc = re.Match(cDoc);
            string location = string.Empty;
            if (mc.Success)
            {
                location = mc.Groups["location"].Value;
            }
            if (string.IsNullOrEmpty(location))
            {
                re = new Regex(@"<dd class=""location_name""[^>]*><span class=""InRun_icon""></span>(?'location'[^<]+)</dd>", RegexOptions.None);
                mc = re.Match(cDoc);
                if (mc.Success)
                {
                    location = mc.Groups["location"].Value;
                }
            }
            return location;
        }

        private string GetRace(string cDoc)
        {
            string strRet = string.Empty;
            Regex re = new Regex(@"<dd class=""rc_type""><span>(?'race'[^<]+)", RegexOptions.None);
            Match mc = re.Match(cDoc);
            if (mc.Success)
            {
                strRet = mc.Groups["race"].Value;
            }
            return strRet;
        }
        private string[] SubMatch(string[] matchlist)
        {
            List<String> lstMatch = new List<string>();
            foreach (string match in matchlist)
            {
                if (match.Contains("<!-- show a --->"))
                {
                    string[] list = match.Split(new string[] { "<!-- show a --->" }, StringSplitOptions.None);
                    foreach (string str in list)
                    {
                        lstMatch.Add(str);
                    }
                }
                else if (match.Contains("<!-- show b --->"))
                {
                    string[] list = match.Split(new string[] { "<!-- show b --->" }, StringSplitOptions.None);
                    foreach (string str in list)
                    {
                        lstMatch.Add(str);
                    }
                }
                else if (match.Contains("<!-- show d --->"))
                {
                    string[] list = match.Split(new string[] { "<!-- show d --->" }, StringSplitOptions.None);
                    foreach (string str in list)
                    {
                        lstMatch.Add(str);
                    }
                }
                else if (match.Contains("<!-- show e --->"))
                {
                    string[] list = match.Split(new string[] { "<!-- show e --->" }, StringSplitOptions.None);
                    foreach (string str in list)
                    {
                        lstMatch.Add(str);
                    }
                }
                else if (match.Contains("<!-- show f --->"))
                {
                    string[] list = match.Split(new string[] { "<!-- show f --->" }, StringSplitOptions.None);
                    foreach (string str in list)
                    {
                        lstMatch.Add(str);
                    }
                }
                else
                {
                    lstMatch.Add(match);
                }


            }
            string[] retMatch = new string[lstMatch.Count];
            int i = 0;
            foreach (string match in lstMatch)
            {
                retMatch[i++] = match;
            }
            return retMatch;

        }
        #endregion

        public Hashtable GetvCode(CookieContainer cc, string cDoMain)
        {
            string cUrl = string.Format("https://{0}/", cDoMain);
            string refer = string.Empty;
            string cDoc = Connect.getDocument(cUrl, cc, null, "utf-8");
            string valid = string.Empty;
            string secure = string.Empty;
            string path = Application.StartupPath + @"\temp";
            Hashtable ht = null;
            if (cDoc != null)
            {
                CheckForbidAccess(cDoc);
            relogin:

                Regex re = new Regex(@"location.replace\('(?'ext'[^']+)", RegexOptions.None);
                Match mc = re.Match(cDoc);
                if (mc.Success)
                {
                    string ext = mc.Groups["ext"].Value;
                    refer = cUrl;
                    cUrl = string.Format("https://{0}/{1}", cDoMain, ext);
                    cDoc = Connect.getDocument(cUrl, cc, refer, "utf-8");
                    if (cDoc != null && cDoc.Contains("_index.jsp"))
                    {
                        CheckForbidAccess(cDoc);
                        refer = cUrl;
                        cUrl = "http://{0}/_index.jsp";
                        cUrl = string.Format(cUrl, cDoMain);
                        cDoc = Connect.getDocument(cUrl, cc, refer, "utf-8");
                        if (cDoc != null)
                        {
                            CheckForbidAccess(cDoc);
                            valid = GetValid(cDoc);
                            secure = GetSecure(cDoc);

                            string str5 = Convert.ToString(DateTime.Now.Ticks) + ".jpg";
                            Util.DeleteOldImgFile(path);
                            string cPath = path + @"\" + str5;


                            refer = cUrl;
                            cUrl = "https://{0}/img.jpg?{1}";
                            cUrl = string.Format(cUrl, cDoMain, DateTime.Now.Millisecond);
                            cDoc = Connect.getPic(cUrl, cc, cUrl, null, cPath);
                            CheckForbidAccess(cDoc);
                            OnShowMsg("获取验证码");
                            ht = new Hashtable();
                            ht.Add(1, valid);
                            ht.Add(2, cPath);
                            ht.Add(3, secure);
                        }
                    }
                    else if (cDoc != null && cDoc.Contains("location"))
                    {
                        goto relogin;
                    }
                }

            }
            return ht;
        }
        public Hashtable GetvCode2(CookieContainer cc, string cDoMain)
        {
            Hashtable ht = GetvCode(cc, cDoMain);
            if (ht != null)
            {
                string valid = ht[1] as string;
                string cPath = ht[2] as string;
                string secure = ht[3] as string;

                Ocr ocr = new Ocr();
                string picPath = cPath;
                string vcode = ocr.GetVcode(picPath);
                ht[2] = vcode;
            }
            else
            {
                OnShowMsg("解释验证码失败");
            }
            return ht;
        }
        private void CheckForbidAccess(string doc)
        {
            if (!string.IsNullOrEmpty(doc))
            {
                if (doc.Contains("禁止访问及使用本网站"))
                {
                    OnShowMsg("禁止访问及使用本网站");
                }
            }
        }

        private string GetValid(string cDoc)
        {
            Regex re = new Regex(@" id=""valid"" value=""(?'valid'[^""]+)""", RegexOptions.None);
            Match mc = re.Match(cDoc);
            if (mc.Success)
            {
                return mc.Groups["valid"].Value;
            }
            else
            {
                return null;
            }

        }
        /// <summary>
        /// 获取安全网址
        /// </summary>
        /// <param name="cDoc"></param>
        /// <returns></returns>
        private string GetSecure(string cDoc)
        {
            Regex re = new Regex(@"PageConfig.sslHost[^=]=\s+\[(?'sec'[^]]+)\]");
            Match mc = re.Match(cDoc);
            if (mc.Success)
            {
                return mc.Groups["sec"].Value;
            }
            else
            {
                return null;
            }
        }
        private void SendHeartbreak()
        {
            if (BreakHeartThread != null && BreakHeartThread.IsAlive)
            {
                BreakHeartThread.Abort();
            }
            BreakHeartThread = new Thread(BreakHeart);
            BreakHeartThread.IsBackground = true;
            BreakHeartThread.Start();
        }
        /// <summary>
        /// 发送心跳包,检测账号是否退出了
        /// </summary>
        private void BreakHeart()
        {
            while (true)
            {
                if (IsLogin)
                {
                    string cUrl = string.Format("https://{0}/playerhk.jsp", DoMain);
                    string str = Connect.getDocument(cUrl, cc, null, "utf-8");
                    CheckLogout(str);
                }
                Thread.Sleep(2000);
            }

        }
        private void CheckLogout(string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length < 100)
            {
                if (str.Contains("location.replace") || (str.Contains(@"top.location=") && str.Contains(@"logout")))
                {
                    OnLogout?.Invoke();
                    IsLogin = false;
                }
            }
        }

        private void CheckUnableBet(string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length < 60)
            {
                if (str.Contains(@"<script>document.domain = 'ctb988.com';</script>"))
                {
                    //提示账号不能下单
                    OnShowMsg("賬號無法下單，請更換賬號");
                }
            }
        }

        public MemberInfo GetAccountInfo()
        {
            MemberInfo info = new MemberInfo();
            //https://cqfexhv.ctb988.net/acc_profile_overview.jsp
            string url = $"https://{DoMain}/acc_profile_overview.jsp";
            string str = Connect.getDocument(url, cc, "", "utf-8");
            CheckLogout(str);
            if (!string.IsNullOrEmpty(str))
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(str);
                var res = doc.DocumentNode.SelectSingleNode(@"//div[@id='racing_credit_box_sub_detail']/table/tr[1]/td");
                if (res != null)
                {
                    info.Allocated = GetMoney(res.InnerText);

                    res = doc.DocumentNode.SelectSingleNode(@"//*[@id='racing_credit_box_sub_detail']/table/tr[4]/td");
                    if (res != null)
                    {
                        info.Balance = GetMoney(res.InnerText);
                    }

                    res = doc.DocumentNode.SelectSingleNode(@"//*[@id='racing_credit_box_sub_detail']/table/tr[5]/td");
                    if (res != null)
                    {
                        info.Loss = GetMoney(res.InnerText);
                    }
                }
            }

            return info;
        }

        /// <summary>
        /// 去除Html中的多余字符，只取其金额
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string GetMoney(string str)
        {
            string strRet = str;
            Regex re = new Regex("[(]*\\$[^\\s]+", RegexOptions.None);
            Match mc = re.Match(str);
            if (mc.Success)
            {
                strRet = mc.Value;
            }
            return strRet;
        }

        protected string GetNow()
        {
            DateTime now = DateTime.Now;
            now = now.AddHours(-1);
            string _year = now.Year.ToString();
            string _mon = now.Month.ToString();
            string _date = now.Day.ToString();
            DateTime ddc = new DateTime(2030, 8, 1);
            if (DateTime.Now <= ddc)
            {
                //if (_mon.Length == 1)
                {
                    _mon = _mon.PadLeft(2, '0');
                    //日是两位
                    _date = _date.PadLeft(2, '0');
                }
            }
            else
            {
                _mon = "1";
                _date = "31";
            }
            string _now = string.Format("{0}-{1}-{2}", _date, _mon, _year);
            return _now;
        }
        public string GetNow(string url)
        {
            string strRet = "";
            DateTime now = DateTime.Now;
            now = now.AddHours(-1);
            string _year = now.Year.ToString();
            string _mon = now.Month.ToString();
            string _date = now.Day.ToString();
            DateTime ddc = new DateTime(2030, 9, 1);
            if (DateTime.Now <= ddc)
            {
                DataRow[] drs = dtMatchList.Select($"url='{url}'");
                if (drs.Length > 0)
                {
                    string tip = drs[0]["tip"].ToString();
                    string[] tmp = tip.Split("_".ToCharArray());
                    string date = tmp[5];
                    string[] tmp2 = date.Split("//".ToCharArray());
                    _mon = tmp2[1];
                    _date = tmp2[2];
                    _year = tmp2[0];

                    _mon = _mon.PadLeft(2, '0');
                    //日是两位
                    _date = _date.PadLeft(2, '0');
                    strRet = string.Format("{0}-{1}-{2}", _date, _mon, _year);
                }
            }
            else
            {
                strRet = GetNow();
            }
            return strRet;
        }
        private void GetHorses(string hss, out int h1, out int h2)
        {
            string tmp = hss.Replace("(", "").Replace(")", "");
            string[] tmps = tmp.Split("-".ToCharArray());
            if (tmps.Length > 1)
            {
                int.TryParse(tmps[0], out int horse1);
                int.TryParse(tmps[1], out int horse2);
                h1 = horse1;
                h2 = horse2;
            }
            else
            {
                h1 = 0;
                h2 = 0;
            }

        }

        private double GetQOdds(string hss)
        {
            double pl = 0;
            try
            {
                GetHorses(hss, out int Horse1, out int Horse2);
                string qpei = "";
                if (Qpeis != null)
                {
                    qpei = Qpeis[Horse1, Horse2];
                }
                double.TryParse(qpei, out double pei);
                if (DicManualQpeis.ContainsKey(hss))
                {
                    pei = DicManualQpeis[hss];
                }
                pl = pei;
            }
            catch (Exception ex)
            {

            }
            return pl;
        }

        private double GetQPOdds(string hss)
        {
            double pl = 0;
            try
            {
                GetHorses(hss, out int Horse1, out int Horse2);
                string qpei = "";
                if (QPpeis != null)
                {
                    qpei = QPpeis[Horse1, Horse2];
                }
                double.TryParse(qpei, out double pei);
                if (DicManualQpeis.ContainsKey(hss))
                {
                    pei = DicManualQpeis[hss];
                }
                pl = pei;
            }
            catch (Exception ex)
            {

            }
            return pl;
        }


        #region 打单策略
        public virtual void Start()
        {
        }
        public virtual void Stop()
        {
        }
        #endregion
        /// <summary>
        /// 换场
        /// </summary>
        public void ChangeRace()
        {
            if (dicBettedRace != null)
            {
                dicBettedRace.Clear();
            }
        }

        /// <summary>
        /// 返回比赛是否已经结束，无法下注了。
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool IsRaceFinished(BetResultInfo info)
        {
            bool bRet = false;
            //目前无法下注，正在等待赛果中
            if (info.StrAnswer.Contains("目前无法下注"))
            {
                bRet = true;
            }
            return bRet;
        }
        /// <summary>
        /// 获取赛事现在开的场次
        /// </summary>
        /// <param name="strMatch"></param>
        /// <returns></returns>
        public List<int> GetOpenedRace(string strMatch)
        {
            List<int> lst = new List<int>();
            string _now = GetNow(strMatch);
            string url = $"https://{DoMain}/playerhk.jsp?race_type={strMatch}&race_date={_now}&tab=c&sml=s";
            string doc = Connect.getDocument(url, cc, url, "utf-8");
            CheckLogout(doc);
            if (doc != null)
            {
                Regex re = new Regex(@"rcs.push\((?'rcs'\d+)\)", RegexOptions.None);
                MatchCollection mc = re.Matches(doc);
                foreach (Match ma in mc)
                {
                    string val = ma.Groups["rcs"].Value;
                    lst.Add(int.Parse(val));
                }
            }
            return lst;
        }

        #region 解释代理下的比赛信息
        public Dictionary<string, RaceInfoItem> ParseRacetInfo(string str, ParseTop Top, ParseBelow Below)
        {
            Dictionary<string, RaceInfoItem> info = new Dictionary<string, RaceInfoItem>();
            //Hashtable htRet = new Hashtable();
            var doc = new HtmlAgilityPack.HtmlDocument();
            if (!string.IsNullOrEmpty(str))
            {
                doc.LoadHtml(str);
                var res = doc.DocumentNode.SelectSingleNode(@"//*[@id='txn_window']");
                if (res != null)
                {
                    var list = res.SelectNodes(@"div[@class='txn_wrapper']");
                    if (list != null)
                    {
                        //对应每个国家s的比赛
                        foreach (var item in list)
                        {
                            if (item != null)
                            {
                                string country = string.Empty;
                                string odds = string.Empty;

                                //找Race_Top
                                var Race_Top = item.SelectSingleNode(@"div[@class='Race_Top']");
                                List<RaceInfoItem> top = Top(Race_Top, out country, out odds);
                                top.ForEach(it =>
                                {
                                    if (!info.ContainsKey(it.ToString()))
                                    {
                                        info.Add(it.ToString(), it);
                                    }
                                }
                              );
                                var Race_Belows = item.SelectNodes(@"div[@class='Race_below']");
                                if (Race_Belows != null)
                                {
                                    foreach (var race_below in Race_Belows)
                                    {
                                        List<RaceInfoItem> below = Below(race_below, country, odds);
                                        below.ForEach(it =>
                                        {
                                            if (!info.ContainsKey(it.ToString()))
                                            {
                                                info.Add(it.ToString(), it);
                                            }
                                        }
                                       );
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return info;
        }

        public RaceInfoItem ParseWPPopup(string doc)
        {
            RaceInfoItem info = null;
            //wp
            Regex re = new Regex(@"popUp\('new_details.jsp\?d=(?'date'[^&]+)&type=(?'url'\w+)&m=(?'bettype'\w+)&uid=(?'uid'\w+)+&rc=(?'race'\d+)&hs=(?'horse'\d+)&amount=(?'amount'[^&]+)&lw=(?'lWin'[^&]+)&lp=(?'lPlace'[^&]+)&cat=(?'cat'\w+)&live=(?'live'\w*)", RegexOptions.None);
            MatchCollection mc = re.Matches(doc);
            if (mc.Count > 0)
            {
                info = new RaceInfoItem();
                foreach (Match ma in mc)
                {
                    info.Date = ma.Groups["date"].Value;
                    info.Url = ma.Groups["url"].Value;
                    info.Bettype = ma.Groups["bettype"].Value.ToString().ToUpper().Equals("EAT") ? BetType.EAT : BetType.BET;
                    info.Uid = ma.Groups["uid"].Value;
                    info.Race = ma.Groups["race"].Value;
                    info.Horse = ma.Groups["horse"].Value;
                    info.Zhe = double.Parse(ma.Groups["amount"].Value);
                    double.TryParse(ma.Groups["lWin"].Value, out double lwin);
                    double.TryParse(ma.Groups["lPlace"].Value, out double lplace);
                    //实际极限是原来数值的2倍
                    info.LWin = (int)(lwin * 2);
                    info.LPlace = (int)(lplace * 2);
                    info.Live = ma.Groups["live"].Value.Trim();
                    info.Playtype = PlayType.WP;
                }
            }
            return info;
        }
        public List<RaceInfoItem> ParseWPRaceTop(HtmlAgilityPack.HtmlNode node, out string country, out string odds)
        {
            List<RaceInfoItem> lstInfo = new List<RaceInfoItem>();
            var Race_Top = node;
            var lca_tote = Race_Top.SelectSingleNode(@"div[@class='race_infobox']/div[@class='race_inner']/div[@class='lca_tote']");
            country = lca_tote.SelectSingleNode("h3").InnerText;
            odds = lca_tote.SelectSingleNode(@"div/span").InnerText;
            string location = Race_Top.SelectSingleNode(@"div[1]/div[2]/p").InnerText;
            //找tbl_detail
            var tbl_detail = Race_Top.SelectSingleNode(@"div[@class='overview_table']/table[@name='tbl_detail']");
            if (tbl_detail != null)
            {
                var spans = tbl_detail.SelectNodes(@"*/td[@class='type']/span");
                if (spans != null)
                {
                    foreach (var spanitem in spans)
                    {
                        string popup = spanitem.Attributes["onclick"].Value;
                        RaceInfoItem info = ParseWPPopup(popup);

                        info.Country = country;
                        info.Location = location;
                        info.OddsType = odds;
                        //查找w,p
                        //查找row
                        var row_bet = spanitem.ParentNode.ParentNode;
                        var items = row_bet.SelectNodes(@"td[not(@class)]");
                        if (items != null)
                        {
                            info.Win = double.Parse(items[1].InnerText);
                            info.Place = double.Parse(items[2].InnerText);
                            info.Zhe = double.Parse(items[3].InnerText);
                        }
                        lstInfo.Add(info);
                    }

                }
            }
            return lstInfo;
        }
        public List<RaceInfoItem> ParseWPRaceBelow(HtmlAgilityPack.HtmlNode node, string country, string odds)
        {
            List<RaceInfoItem> lstInfo = new List<RaceInfoItem>();
            var Race_Below = node;
            //string country = Race_Below.SelectSingleNode(@"div[1]/div[1]/div[1]/h3").InnerText;
            //string odds = Race_Below.SelectSingleNode(@"div[1]/div[1]/div[1]/div/span").InnerText;
            string location = Race_Below.SelectSingleNode(@"div[@class='race_infobox']/div[@class='race_infodetails']/dl/dt[@class='lca']/span").InnerText;
            //找tbl_detail
            var tbl_detail = Race_Below.SelectSingleNode(@"div[@class='overview_table']/table[@name='tbl_detail']");
            if (tbl_detail != null)
            {
                var spans = tbl_detail.SelectNodes(@"*/td[@class='type']/span");
                if (spans != null)
                {
                    foreach (var spanitem in spans)
                    {
                        string popup = spanitem.Attributes["onclick"].Value;
                        RaceInfoItem info = ParseWPPopup(popup);
                        info.Country = country;
                        info.Location = location;
                        info.OddsType = odds;
                        //查找w,p
                        //查找row
                        var row_bet = spanitem.ParentNode.ParentNode;
                        var items = row_bet.SelectNodes(@"td[not(@class)]");
                        if (items != null)
                        {
                            info.Win = double.Parse(items[1].InnerText);
                            info.Place = double.Parse(items[2].InnerText);
                            info.Zhe = double.Parse(items[3].InnerText);
                        }
                        lstInfo.Add(info);
                    }

                }
            }
            return lstInfo;
        }

        public RaceInfoItem ParseForeCastPopup(string doc)
        {
            RaceInfoItem info = null;
            //wp
            Regex re = new Regex(@"popUp\('new_details.jsp\?d=(?'date'[^&]+)&type=(?'url'\w+)&m=(?'bettype'\w+)&uid=(?'uid'\w+)&rc=(?'race'\d+)", RegexOptions.None);
            MatchCollection mc = re.Matches(doc);
            if (mc.Count > 0)
            {
                info = new RaceInfoItem();
                foreach (Match ma in mc)
                {
                    info.Date = ma.Groups["date"].Value;
                    info.Url = ma.Groups["url"].Value;
                    info.Bettype = ma.Groups["bettype"].Value.ToString().ToUpper().Equals("EAT") ? BetType.EAT : BetType.BET;
                    info.Uid = ma.Groups["uid"].Value;
                    info.Race = ma.Groups["race"].Value;
                    info.Playtype = PlayType.FORECAST;
                }
            }
            return info;
        }
        public List<RaceInfoItem> ParseForeCastRaceTop(HtmlAgilityPack.HtmlNode node, out string country, out string odds)
        {
            List<RaceInfoItem> lstInfo = new List<RaceInfoItem>();
            var Race_Top = node;
            var lca_tote = Race_Top.SelectSingleNode(@"div[@class='race_infobox']/div[@class='race_inner']/div[@class='lca_tote']");
            country = lca_tote.SelectSingleNode("h3").InnerText;
            odds = lca_tote.SelectSingleNode(@"div/span").InnerText;
            string location = Race_Top.SelectSingleNode(@"div[1]/div[2]/p").InnerText;
            //找tbl_detail
            var tbl_detail = Race_Top.SelectSingleNode(@"div[@class='overview_table']/table[@name='fc_tbl_detail']");
            if (tbl_detail != null)
            {
                var spans = tbl_detail.SelectNodes(@"*/td[@class='type']/span");
                if (spans != null)
                {
                    foreach (var spanitem in spans)
                    {
                        string popup = spanitem.Attributes["onclick"].Value;
                        RaceInfoItem info = ParseForeCastPopup(popup);
                        info.Country = country;
                        info.Location = location;
                        info.OddsType = odds;
                        //查找w,p
                        //查找row
                        var row_bet = spanitem.ParentNode.ParentNode;
                        var items = row_bet.SelectNodes(@"td");
                        if (items != null)
                        {
                            info.Horse = items[0].InnerText.Replace("&amp;", "-").Replace(" ", "").Replace("&", "-");
                            info.ClassType = items[2].InnerText;
                            if (info.ClassType.Equals("QP") || info.ClassType.Equals("PFT"))
                            {
                                info.Win = 0;
                                info.Place = double.Parse(items[1].InnerText);

                                info.LWin = 0;
                                info.LPlace = int.Parse(items[4].InnerText);
                            }
                            else
                            {
                                info.Win = double.Parse(items[1].InnerText);
                                info.Place = 0;

                                info.LWin = int.Parse(items[4].InnerText);
                                info.LPlace = 0;
                            }
                            //info.Win = double.Parse(items[1].InnerText);
                            //info.Place = double.Parse(items[1].InnerText);

                            info.Zhe = double.Parse(items[3].InnerText);
                            //info.LWin = int.Parse(items[4].InnerText);
                            //info.LPlace = int.Parse(items[4].InnerText);
                        }

                        lstInfo.Add(info);
                    }

                }
            }
            return lstInfo;
        }
        public List<RaceInfoItem> ParseForeCastRaceBelow(HtmlAgilityPack.HtmlNode node, string country, string odds)
        {
            List<RaceInfoItem> lstInfo = new List<RaceInfoItem>();
            var Race_Below = node;
            string location = Race_Below.SelectSingleNode(@"div[@class='race_infobox']/div[@class='race_infodetails']/dl/dt[@class='lca']/span").InnerText;
            //找tbl_detail
            var tbl_detail = Race_Below.SelectSingleNode(@"div[@class='overview_table']/table[@name='fc_tbl_detail']");
            if (tbl_detail != null)
            {
                var spans = tbl_detail.SelectNodes(@"*/td[@class='type']/span");
                if (spans != null)
                {
                    foreach (var spanitem in spans)
                    {
                        string popup = spanitem.Attributes["onclick"].Value;
                        RaceInfoItem info = ParseForeCastPopup(popup);
                        info.Country = country;
                        info.Location = location;
                        info.OddsType = odds;
                        //查找w,p
                        //查找row
                        var row_bet = spanitem.ParentNode.ParentNode;
                        var items = row_bet.SelectNodes(@"td");
                        if (items != null)
                        {
                            info.Horse = items[0].InnerText.Replace("&amp;", "-").Replace(" ", "").Replace("&", "-");
                            info.ClassType = items[2].InnerText;
                            if (info.ClassType.Equals("QP") || info.ClassType.Equals("PFT"))
                            {
                                info.Win = 0;
                                info.Place = double.Parse(items[1].InnerText);

                                info.LWin = 0;
                                info.LPlace = int.Parse(items[4].InnerText);
                            }
                            else
                            {
                                info.Win = double.Parse(items[1].InnerText);
                                info.Place = 0;

                                info.LWin = int.Parse(items[4].InnerText);
                                info.LPlace = 0;
                            }

                            //info.Win = double.Parse(items[1].InnerText);
                            //info.Place = double.Parse(items[1].InnerText);

                            info.Zhe = double.Parse(items[3].InnerText);
                            //info.LWin = int.Parse(items[4].InnerText);
                            //info.LPlace = int.Parse(items[4].InnerText);
                        }
                        lstInfo.Add(info);
                    }

                }
            }
            return lstInfo;
        }

        public RaceInfoItem ParseQPPopup(string doc)
        {
            RaceInfoItem info = null;
            //Q
            Regex re = new Regex(@"popUp\('new_details.jsp\?d=(?'date'[^&]+)&type=(?'url'\w+)&m=(?'bettype'\w+)&uid=(?'uid'\w+)+&rc=(?'race'\d+)", RegexOptions.None);
            MatchCollection mc = re.Matches(doc);
            if (mc.Count > 0)
            {
                info = new RaceInfoItem();
                foreach (Match ma in mc)
                {
                    info.Date = ma.Groups["date"].Value;
                    info.Url = ma.Groups["url"].Value;
                    info.Bettype = ma.Groups["bettype"].Value.ToString().ToUpper().Equals("EAT") ? BetType.EAT : BetType.BET;
                    info.Uid = ma.Groups["uid"].Value;
                    info.Race = ma.Groups["race"].Value;
                    info.Playtype = PlayType.QP;
                }
            }
            return info;
        }
        public List<RaceInfoItem> ParseQPRaceTop(HtmlAgilityPack.HtmlNode node, out string country, out string odds)
        {
            List<RaceInfoItem> lstInfo = new List<RaceInfoItem>();
            var Race_Top = node;
            var lca_tote = Race_Top.SelectSingleNode(@"div[@class='race_infobox']/div[@class='race_inner']/div[@class='lca_tote']");
            country = lca_tote.SelectSingleNode("h3").InnerText;
            odds = lca_tote.SelectSingleNode(@"div/span").InnerText;
            string location = Race_Top.SelectSingleNode(@"div[1]/div[2]/p").InnerText;
            //找tbl_detail
            var tbl_detail = Race_Top.SelectSingleNode(@"div[@class='overview_table']/table[@name='q_tbl_detail']");
            if (tbl_detail != null)
            {
                var spans = tbl_detail.SelectNodes(@"*/td[@class='type']/span");
                if (spans != null)
                {
                    foreach (var spanitem in spans)
                    {
                        string popup = spanitem.Attributes["onclick"].Value;
                        RaceInfoItem info = ParseQPPopup(popup);
                        info.Country = country;
                        info.Location = location;
                        info.OddsType = odds;
                        //查找w,p
                        //查找row
                        var row_bet = spanitem.ParentNode.ParentNode;
                        var items = row_bet.SelectNodes(@"td");
                        if (items != null)
                        {
                            info.Horse = items[0].InnerText.Replace("&amp;", "&").Replace(" ", "").Replace("&", "-");

                            info.ClassType = items[2].InnerText;
                            if (info.ClassType.Equals("QP") || info.ClassType.Equals("PFT"))
                            {
                                info.Win = 0;
                                info.Place = double.Parse(items[1].InnerText);

                                info.LWin = 0;
                                info.LPlace = int.Parse(items[4].InnerText);
                            }
                            else
                            {
                                info.Win = double.Parse(items[1].InnerText);
                                info.Place = 0;
                                info.LWin = int.Parse(items[4].InnerText);
                                info.LPlace = 0;
                            }
                            //info.Win = double.Parse(items[1].InnerText);
                            //info.Place = double.Parse(items[1].InnerText);
                            info.Zhe = double.Parse(items[3].InnerText);
                            //info.LWin = int.Parse(items[4].InnerText);
                            //info.LPlace = int.Parse(items[4].InnerText);
                        }

                        lstInfo.Add(info);
                    }

                }
            }
            return lstInfo;
        }
        public List<RaceInfoItem> ParseQPRaceBelow(HtmlAgilityPack.HtmlNode node, string country, string odds)
        {
            List<RaceInfoItem> lstInfo = new List<RaceInfoItem>();
            var Race_Below = node;
            string location = Race_Below.SelectSingleNode(@"div[@class='race_infobox']/div[@class='race_infodetails']/dl/dt[@class='lca']/span").InnerText;
            //找tbl_detail
            var tbl_detail = Race_Below.SelectSingleNode(@"div[@class='overview_table']/table[@name='q_tbl_detail']");
            if (tbl_detail != null)
            {
                var spans = tbl_detail.SelectNodes(@"*/td[@class='type']/span");
                if (spans != null)
                {
                    foreach (var spanitem in spans)
                    {
                        string popup = spanitem.Attributes["onclick"].Value;
                        RaceInfoItem info = ParseQPPopup(popup);
                        info.Country = country;
                        info.Location = location;
                        info.OddsType = odds;
                        //查找w,p
                        //查找row
                        var row_bet = spanitem.ParentNode.ParentNode;
                        var items = row_bet.SelectNodes(@"td");
                        if (items != null)
                        {
                            info.Horse = items[0].InnerText.Replace("&amp;", "&").Replace(" ", "").Replace("&", "-");
                            info.ClassType = items[2].InnerText;

                            if (info.ClassType.Equals("QP") || info.ClassType.Equals("PFT"))
                            {
                                info.Win = 0;
                                info.Place = double.Parse(items[1].InnerText);

                                info.LWin = 0;
                                info.LPlace = int.Parse(items[4].InnerText);
                            }
                            else
                            {
                                info.Win = double.Parse(items[1].InnerText);
                                info.Place = 0;
                                info.LWin = int.Parse(items[4].InnerText);
                                info.LPlace = 0;
                            }
                            info.Zhe = double.Parse(items[3].InnerText);
                        }
                        lstInfo.Add(info);
                    }

                }
            }
            return lstInfo;
        }

        #endregion

        #region 获取进单情况
        public RaceInfoEnity GetRaceInfo()
        {
            bool isempty;
            RaceInfoEnity htRace = GetRaceInfo(AccountInfo.CUserName, out isempty);
            return htRace;
        }
        public RaceInfoEnity GetRaceInfo(string uid, out bool isempty)
        {
            RaceInfoEnity raceinfo = new RaceInfoEnity();
            Dictionary<string, RaceInfoItem> htRet = new Dictionary<string, RaceInfoItem>();
            //读水回来是否为0字节
            isempty = true;
            if (IsLogin)
            {
                //https://web.ctb988.com/history.jsp?uid=aabc1
                string domain = GetWebPrefixUrl();
                //string url = $"https://{domain}/history.jsp?uid={uid}";
                string url = $"https://{DoMain}/new_history_live.jsp";
                string str = Connect.getDocument(url, cc, "", "utf-8");
                CheckLogout(str);
                if (!string.IsNullOrEmpty(str))
                {
                    isempty = false;

                    Dictionary<string, RaceInfoItem> ht1 = ParseRacetInfo(str, ParseWPRaceTop, ParseWPRaceBelow);
                    Dictionary<string, RaceInfoItem> ht2 = ParseRacetInfo(str, ParseForeCastRaceTop, ParseForeCastRaceBelow);

                    Dictionary<string, RaceInfoItem> ht3 = ParseRacetInfo(str, ParseQPRaceTop, ParseQPRaceBelow);

                    AppendRaceInfo(ht1, htRet);
                    AppendRaceInfo(ht2, htRet);
                    AppendRaceInfo(ht3, htRet);
                }
            }
            raceinfo.SnakeHead = uid;
            raceinfo.DicRaceInfo = htRet;
            return raceinfo;
        }
        private string GetWebPrefixUrl()
        {
            string strRet = DoMain;
            string domain = DoMain;
            if (domain.Contains("."))
            {
                string last = domain.Substring(domain.IndexOf("."));
                strRet = $"web{last}";
            }
            return strRet;
        }
        private void AppendRaceInfo(Dictionary<string, RaceInfoItem> source, Dictionary<string, RaceInfoItem> dest)
        {
            foreach (KeyValuePair<string, RaceInfoItem> de in source)
            {
                if (!dest.ContainsKey(de.Key))
                {
                    dest.Add(de.Key, de.Value);
                }
            }
        }
        /// <summary>
        /// 从历史页面获取已经成交的打单情况
        /// </summary>
        /// <returns></returns>
        /*
        public Hashtable GetNewBetInfo()
        {
            //https://web.ctb988.com/new_history_live.jsp?uid=abbb6
            string url = $"https://{DoMain}/new_history_live.jsp?uid={AccountInfo.CUserName}";
            string str = Connect.getDocument(url, cc, null, "utf-8");
            CheckLogout(str);
            return ParseWPPopup(str);
        }

        private Hashtable ParseWPPopup(string doc)
        {
            Hashtable ret = new Hashtable();
            if (!string.IsNullOrEmpty(doc))
            {
                Regex re = new Regex(@"'(?'url'\w+)_(?'race'\d+)_(?'horse'\d+)_detail'[^']+'[^']+'[^']+'[^']+'[^']+'(?'win'[+-]\d+)'[^']+'(?'place'[+-]\d+)'", RegexOptions.None);
                MatchCollection mc = re.Matches(doc);
                if (mc.Count > 0)
                {
                    foreach (Match ma in mc)
                    {
                        int index = doc.IndexOf(ma.Value);

                        string url = ma.Groups["url"].Value;
                        url = url.TrimStart('0');
                        string race = ma.Groups["race"].Value;
                        race = race.TrimStart('0');
                        string horse = ma.Groups["horse"].Value;
                        horse = horse.TrimStart('0');

                        Regex re2 = new Regex(@"popUp\('new_details.jsp\?d=(?'date'[^&]+)&type=(?'url'\w+)&m=(?'bettype'\w+)&uid=(?'uid'\w+)+&rc=(?'race'\d+)&hs=(?'horse'\d+)&amount=(?'amount'[^&]+)&lw=(?'lWin'[^&]+)&lp=(?'lPlace'[^&]+)&cat=(?'cat'\w+)&live=(?'live'\w*)", RegexOptions.None);
                        MatchCollection mc2 = re2.Matches(doc);
                        if (mc2.Count > 0)
                        {
                            for (int i = 0; i < mc2.Count; i++)
                            {
                                int index2 = doc.IndexOf(mc2[i].Value);
                                if (index2 < index)
                                {
                                    string currenthorse = mc2[i].Groups["horse"].Value;
                                    horse = currenthorse.TrimStart('0');
                                }
                            }
                        }

                        string win = ma.Groups["win"].Value;
                        string place = ma.Groups["place"].Value;
                        string key = $"{url}-{race}-{horse}";
                        int.TryParse(win, out int iwin);
                        int.TryParse(place, out int iplace);
                        if (!ret.ContainsKey(key))
                        {
                            RaceItem item = new RaceItem();
                            item.Url = url;
                            item.Race = race;
                            item.Horse = horse;
                            item.Piao = Math.Abs(iwin) > Math.Abs(iplace) ? Math.Abs(iwin) : Math.Abs(iplace);

                            ret.Add(key, item);
                        }
                    }
                }
            }

            return ret;
        }
        */

        /// <summary>
        /// 获取比赛页面的打单情况，返回已经成交和还在挂单中的数据
        /// </summary>
        /// <param name="betInfo"></param>
        /// <returns></returns>
        public object[] GetBetInfo(out string betInfo)
        {
            string url = $"https://{DoMain}/datastore?q=n&l=x&race_date={GetNow(Config.MatchUrl)}&race_type={Config.MatchUrl}&rc={Config.Race}&x={new Random().NextDouble()}6&tnum=4&txnrnd={new Random().NextDouble()}";
            string refer = $"https://{DoMain}/citibethk.jsp?race_type={Config.MatchUrl}&race_date={GetNow(Config.MatchUrl)}&tab=u&sml=s";
            string str = Connect.getDocument(url, cc, refer, "utf-8");
            CheckLogout(str);
            betInfo = str;
            return ParseBetInfo(str);
        }
        private static object[] ParseBetInfo(string doc)
        {
            object[] retDt = new object[2];
            if (!string.IsNullOrEmpty(doc))
            {
                string[] lstData = doc.Split(Environment.NewLine.ToCharArray());
                DataTable dt = new DataTable();
                DataTable dt2 = new DataTable();
                dt.Columns.Add("场");
                dt.Columns.Add("马");
                dt.Columns.Add("独赢");
                dt.Columns.Add("位置");
                dt.Columns.Add("%");
                dt.Columns.Add("极限");
                dt.Columns.Add("吃/赌");

                dt2.Columns.Add("场");
                dt2.Columns.Add("马");
                dt2.Columns.Add("独赢");
                dt2.Columns.Add("位置");
                dt2.Columns.Add("%");
                dt2.Columns.Add("极限");
                dt2.Columns.Add("吃/赌");
                dt2.Columns.Add("单号");
                try
                {
                    foreach (string str in lstData)
                    {
                        //wp
                        Regex re2 = new Regex("\\[\\w+#([BE]?)#(\\d+#\\d+#\\d+#\\d+#\\S+#\\d+/\\d+?)#[01]\\]", RegexOptions.None);
                        Match mc = re2.Match(str);
                        if (mc.Success)
                        {
                            string str1 = mc.Groups[0].Value;
                            string str2 = mc.Groups[1].Value;
                            string str3 = mc.Groups[2].Value;
                            string[] tmp = str3.Split(new char[] { '#' });

                            DataRow dr = dt.NewRow();
                            for (int i = 0; i < tmp.Length; i++)
                            {
                                dr[i] = tmp[i];
                            }
                            dr["吃/赌"] = string.Compare(str2, "B", true) > 0 ? "吃" : "赌";
                            dt.Rows.Add(dr);
                        }
                        //wp的总和
                        Regex re3 = new Regex("\\[C\\d+#(\\S+?)#(\\S+?)#\\d+_\\d+\\]", RegexOptions.None);
                        Match mc3 = re3.Match(str);
                        if (mc3.Success)
                        {
                            string str1 = mc3.Groups[0].Value;
                            string str2 = mc3.Groups[1].Value;
                            string str3 = mc3.Groups[2].Value;
                            DataRow dr = dt.NewRow();
                            dr["场"] = "";
                            dr["马"] = "";
                            dr["独赢"] = str2;
                            dr["位置"] = str3;
                            dt.Rows.Add(dr);
                        }

                        //q
                        Regex re6 = new Regex(@"\[Q#(?'type'[BE]+)#(?'race'\d+)#(?'horse'\d+-\d+)#(?'piao'\d+)#(?'zhe'\d+)#(?'lim'\d+)\]", RegexOptions.None);
                        Match mc6 = re6.Match(str);
                        if (mc6.Success)
                        {
                            string str2 = mc6.Groups["type"].Value;
                            DataRow dr = dt.NewRow();
                            dr["场"] = mc6.Groups["race"].Value;
                            dr["马"] = mc6.Groups["horse"].Value;
                            dr["独赢"] = "Q";
                            dr["位置"] = mc6.Groups["piao"].Value;
                            dr["%"] = mc6.Groups["zhe"].Value;
                            dr["极限"] = mc6.Groups["lim"].Value;
                            dr["吃/赌"] = str2.Equals("E") ? "吃" : "赌";
                            dt.Rows.Add(dr);
                        }

                        //q的总和
                        Regex re7 = new Regex(@"\[C1#.#(?'piao'\d+)#\d+_\d+_\d+#0\]", RegexOptions.None);
                        Match mc7 = re7.Match(str);
                        if (mc7.Success)
                        {
                            DataRow dr = dt.NewRow();
                            dr["场"] = "";
                            dr["马"] = "";
                            dr["独赢"] = "";
                            dr["位置"] = mc7.Groups["piao"].Value;
                            dt.Rows.Add(dr);
                        }
                        //wp的挂单
                        Regex re4 = new Regex("\\[D#[EB]#mr\\('(\\S+?),.+,,\\S+,\\S+,\\S+'\\)#(\\S+#\\S+#\\S+#\\S+#\\S+#\\S+/\\S+?)\\]", RegexOptions.None);
                        Match mc4 = re4.Match(str);
                        if (mc4.Success)
                        {
                            string str1 = mc4.Groups[0].Value;
                            string str2 = mc4.Groups[1].Value;
                            string str3 = mc4.Groups[2].Value;
                            string[] tmp = str3.Split(new char[] { '#' });
                            DataRow dr = dt2.NewRow();
                            for (int i = 0; i < tmp.Length; i++)
                            {
                                dr[i] = tmp[i];
                            }

                            dr["吃/赌"] = str1.Contains("D#B#mr") ? "赌" : "吃";
                            dr["单号"] = str2;
                            dt2.Rows.Add(dr);
                        }

                        //q的挂单
                        Regex re5 = new Regex(@"\[D#[EB]#mr\('(?'id'\d+),[^#]+#(?'race'\d+)#[QFC]+#(?'horse'\d+-\d+)#(?'piao'\d+)#(?'zhe'\d+)#(?'lim'\d+)", RegexOptions.None);
                        Match mc5 = re5.Match(str);
                        if (mc5.Success)
                        {
                            string str1 = mc5.Groups[0].Value;
                            string str2 = mc5.Groups["id"].Value;
                            DataRow dr = dt2.NewRow();
                            dr["场"] = mc5.Groups["race"].Value;
                            dr["马"] = mc5.Groups["horse"].Value;
                            dr["独赢"] = "Q";
                            dr["位置"] = mc5.Groups["piao"].Value;
                            dr["%"] = mc5.Groups["zhe"].Value;
                            dr["极限"] = mc5.Groups["lim"].Value;
                            dr["吃/赌"] = str1.Contains("D#B#mr") ? "赌" : "吃";
                            dr["单号"] = str2;
                            dt2.Rows.Add(dr);
                        }
                    }

                }
                catch (Exception ex)
                {

                }
                retDt[0] = dt;
                retDt[1] = dt2;
            }
            return retDt;
        }
        #endregion
        public MatchTimeInfo GetRaceLastTime(string strMatch, string strRace)
        {
            string _now = GetNow();
            //string cUrl = "https://kihtjkk.citibet.net/datastore?race_date=22-12-2012&race_type=9U&rc=2&x=0.4857978920917958&tnum=1&txnrnd=0.9773911167867482";
            string url = $"https://{DoMain}/datastore?race_date={_now}&race_type={strMatch}&rc={strRace}&x=0.{new Random().Next(100000, 999999)}&tnum=1&txnrnd=0.{new Random().Next(100000, 999999)}";
            string refer = $"https://{DoMain}/";
            string str = Connect.getDocument(url, cc, refer, "utf-8");
            CheckLogout(str);
            return ParseLastTime(str);
        }

        public MatchTimeInfo ParseLastTime(string cDoc)
        {
            MatchTimeInfo tInfo = new MatchTimeInfo();
            if (!string.IsNullOrEmpty(cDoc))
            {
                if (cDoc.Contains("liveModeStart"))
                {
                    tInfo.Stage = MatchStage.Running;
                    if (!string.IsNullOrEmpty(cDoc))
                    {
                        tInfo.LastTime = GetLiveTime(cDoc);
                    }
                }
                else
                {
                    tInfo.Stage = MatchStage.BreadFast;
                    int time = 9999;
                    if (!string.IsNullOrEmpty(cDoc))
                    {
                        Regex re = new Regex(@"txtTIMER>(?'time'\d+)<", RegexOptions.None);
                        Match mc = re.Match(cDoc);
                        if (mc.Success)
                        {
                            int.TryParse(mc.Groups["time"].Value, out time);
                        }
                    }
                    tInfo.LastTime = time;
                }
            }
            return tInfo;
        }

        /// <summary>
        /// 获取走地剩余的时间,单位是毫秒
        /// </summary>
        /// <param name="cDoc"></param>
        /// <returns></returns>
        private int GetLiveTime(string cDoc)
        {
            int result = 0;
            int liveModeStart = 0;
            Regex re = new Regex(@"liveModeStart>(?'liveModeStart'\d+)<", RegexOptions.None);
            Match mc = re.Match(cDoc);
            if (mc.Success)
            {
                int.TryParse(mc.Groups["liveModeStart"].Value, out liveModeStart);
            }

            Regex re2 = new Regex(@"liveMode>(?'liveMode'[^<]+)<", RegexOptions.None);
            Match mc2 = re2.Match(cDoc);
            if (mc2.Success)
            {
                string strMode = mc2.Groups["liveMode"].Value;
                if (!string.IsNullOrEmpty(strMode))
                {
                    string[] tmp = strMode.Split(",".ToCharArray());
                    if (tmp.Length > 0)
                    {
                        int.TryParse(tmp[0], out int first);
                        int.TryParse(tmp[2], out int second);
                        result = (first + second) * 1000 - liveModeStart;
                    }
                }
            }

            return result;
        }
        #region 读水
        public List<RaceInfoItem> GetData(string strMatch, string strRace, string type)
        {
            List<RaceInfoItem> lstRace = new List<RaceInfoItem>();
            string _now = GetNow();
            string url = $"https://{DoMain}/qdata?q={type}&race_date={_now}&race_type={strMatch}&rc={strRace}&m=HK&c=3";
            //string refer = "https://data.citibet.net/betdata?race_date=18-08-2012&race_type=60A&rc=10&m=HK&c=3";
            string refer = $"https://{DoMain}/betdata?race_date={_now}&race_type={strMatch}&rc={strRace}&m=HK&c=3";
            string str = Connect.getDocument(url, cc, refer, "utf-8");
            CheckLogout(str);
            lstRace = ParseGetData(str, strMatch, type);
            return lstRace;
        }

        private List<RaceInfoItem> ParseGetData(string strData, string strMatch, string type)
        {
            List<RaceInfoItem> lstRace = new List<RaceInfoItem>();
            string str = strData;
            //FC_DATA_1>
            string tag = "Data\":\"";
            string tag2 = "\"});";
            #region BET_DATA
            if (str != null && str.Contains(tag))
            {
                str = str.Substring(str.IndexOf(tag)).Replace(tag, "").Replace(tag2, "");

                string[] str2 = str.Split("n".ToCharArray());

                if (str2.Length > 0)
                {
                    for (int i = 0; i < str2.Length; i++)
                    {
                        string[] str3 = str2[i].Replace("\\", "").Split(new char[] { 't' });
                        if (str3.Length > 3)
                        {
                            //3	1-4	1725	83	250
                            RaceInfoItem item = new RaceInfoItem();
                            item.Race = str3[0];
                            item.Horse = str3[1];
                            double.TryParse(str3[2], out double win);
                            item.Win = win;
                            item.Place = win;
                            double.TryParse(str3[3], out double zhe);
                            item.Zhe = zhe;
                            int.TryParse(str3[4], out int lim);
                            item.LWin = lim;
                            item.LPlace = lim;
                            if (type == "1" || type == "2")
                            {
                                item.Playtype = PlayType.Q;
                            }
                            if (type == "3" || type == "4")
                            {
                                item.Playtype = PlayType.QP;
                            }
                            if (type == "1" || type == "3")
                            {
                                item.Bettype = BetType.BET;
                            }
                            if (type == "2" || type == "4")
                            {
                                item.Bettype = BetType.EAT;
                            }

                            item.ClassType = type;
                            item.Url = strMatch;
                            lstRace.Add(item);
                        }
                    }
                }
            }
            #endregion
            return lstRace;
        }
        #endregion
        #region 下单
        public bool QiPiaoGuaRunning(RaceInfoItem item, out BetResultInfo info)
        {
            bool bRet = false;
            info = new BetResultInfo();

            if (IsLogin)
            {
                Random ron = new Random();
                int ser = ron.Next(100000000, 999999999);
                string rd = string.Format("0.{0}", ser);
                //string url = "https://cvyorfp.citibet.net/bookings?t=frm&race=11&horse=7&win=5&place=5&amount=76&l_win=110&l_place=30&race_type=63A&race_date=17-08-2012&show=11&post=1&rd=0.9929689666416468";
                //string url = "https://{0}/bookings?t=frm&race={1}&horse={2}&win={3}&place={4}&amount={5}&l_win={6}&l_place={7}&race_type={8}&race_date={9}&lu=0&show={10}&post={11}&rd={12}";

                string url = $"https://{DoMain}/bookings?t=frm&race={item.Race}&horse={item.Horse}&win={item.Win}&place={item.Place}&amount={item.Zhe}&l_win={item.LWin}&l_place={item.LPlace}&race_type={item.Url}&race_date={item.Date}&lu=1&show={item.Race}&post=1&rd={rd}";
                string refer = $"https://{DoMain}/citibethk.jsp?race_type={item.Url}&race_date={item.Date}&tab=u&sml=s";

                BetItem betItem = new BetItem();
                betItem.StrRace = item.Race;
                betItem.StrHorse = item.Horse;
                betItem.StrWin = item.Win.ToString();
                betItem.StrPlace = item.Place.ToString();
                betItem.StrDiscount = item.Zhe.ToString();
                betItem.StrL_win = item.LWin.ToString();
                betItem.StrL_place = item.LPlace.ToString();
                betItem.EnuBetType = BetType.EAT;
                info.ObjBetItem = betItem;
                info.StrUrl = url;
                info.StrBetString = item.ToString();

                string str = Connect.getDocument(url, cc, refer, "utf-8");
                CheckLogout(str);
                CheckUnableBet(str);
                info.StrAnswer = str;

                if (!string.IsNullOrEmpty(str) && str.Contains("要求下注成功") || str.Contains("要求吃票成功"))
                {
                    bRet = true;
                    info.DBetCount = -2;
                    info.EnuBetResultType = BetResultType.ACCEPT_NO_MATCH;

                }
                else if (!string.IsNullOrEmpty(str) && str.Contains("所有被证实"))
                {
                    bRet = true;
                    info.DBetCount = item.Win;
                    info.EnuBetResultType = BetResultType.FULL;
                }
                else if (!string.IsNullOrEmpty(str) && str.Contains("部份被证实"))
                {
                    bRet = true;
                    info.DBetCount = -1;
                    info.EnuBetResultType = BetResultType.PARTIAL;
                }
                else
                {
                    bRet = false;
                    info.DBetCount = -3;
                    info.EnuBetResultType = BetResultType.FAIL;
                    if (str.IndexOf("top.logout") > 0)
                    {
                        //已经退出
                        info.DBetCount = -99;
                    }
                }
            }
            return bRet;
        }
        public bool QiPiaoGua(RaceInfoItem item, out BetResultInfo info)
        {
            bool bRet = false;
            info = new BetResultInfo();

            if (IsLogin)
            {
                Random ron = new Random();
                int ser = ron.Next(100000000, 999999999);
                string rd = string.Format("0.{0}", ser);
                //string url = "https://cvyorfp.citibet.net/bookings?t=frm&race=11&horse=7&win=5&place=5&amount=76&l_win=110&l_place=30&race_type=63A&race_date=17-08-2012&show=11&post=1&rd=0.9929689666416468";
                //string url = "https://{0}/bookings?t=frm&race={1}&horse={2}&win={3}&place={4}&amount={5}&l_win={6}&l_place={7}&race_type={8}&race_date={9}&lu=0&show={10}&post={11}&rd={12}";

                string url = $"https://{DoMain}/bookings?t=frm&race={item.Race}&horse={item.Horse}&win={item.Win}&place={item.Place}&amount={item.Zhe}&l_win={item.LWin}&l_place={item.LPlace}&race_type={item.Url}&race_date={item.Date}&lu=0&show={item.Race}&post=1&rd={rd}";
                string refer = $"https://{DoMain}/citibethk.jsp?race_type={item.Url}&race_date={item.Date}&tab=u&sml=s";

                BetItem betItem = new BetItem();
                betItem.StrRace = item.Race;
                betItem.StrHorse = item.Horse;
                betItem.StrWin = item.Win.ToString();
                betItem.StrPlace = item.Place.ToString();
                betItem.StrDiscount = item.Zhe.ToString();
                betItem.StrL_win = item.LWin.ToString();
                betItem.StrL_place = item.LPlace.ToString();
                betItem.EnuBetType = BetType.EAT;
                info.ObjBetItem = betItem;
                info.StrUrl = url;
                info.StrBetString = item.ToString();

                string str = Connect.getDocument(url, cc, refer, "utf-8");
                CheckLogout(str);
                CheckUnableBet(str);
                info.StrAnswer = str;

                if (!string.IsNullOrEmpty(str) && str.Contains("要求下注成功") || str.Contains("要求吃票成功"))
                {
                    bRet = true;
                    info.DBetCount = -2;
                    info.EnuBetResultType = BetResultType.ACCEPT_NO_MATCH;

                }
                else if (!string.IsNullOrEmpty(str) && str.Contains("所有被证实"))
                {
                    bRet = true;
                    info.DBetCount = item.Win;
                    info.EnuBetResultType = BetResultType.FULL;
                }
                else if (!string.IsNullOrEmpty(str) && str.Contains("部份被证实"))
                {
                    bRet = true;
                    info.DBetCount = -1;
                    info.EnuBetResultType = BetResultType.PARTIAL;
                }
                else
                {
                    bRet = false;
                    info.DBetCount = -3;
                    info.EnuBetResultType = BetResultType.FAIL;
                    if (str.IndexOf("top.logout") > 0)
                    {
                        //已经退出
                        info.DBetCount = -99;
                    }
                }
            }
            return bRet;
        }
        public bool XiaZhuGua(RaceInfoItem item, out BetResultInfo info)
        {
            bool bRet = false;
            info = new BetResultInfo();

            if (IsLogin)
            {
                Random ron = new Random();
                int ser = ron.Next(100000000, 999999999);
                string rd = string.Format("0.{0}", ser);


                //string url = "https://cvyorfp.citibet.net/bets?t=frm&race=11&horse=2&win=5&place=5&amount=76&l_win=60&l_place=30&race_type=63A&race_date=17-08-2012&show=11&post=1&rd=0.11408325472378111";
                string url = $"https://{DoMain}/bets?t=frm&race={item.Race}&horse={item.Horse}&win={item.Win}&place={item.Place}&amount={item.Zhe}&l_win={item.LWin}&l_place={item.LPlace}&race_type={item.Url}&race_date={item.Date}&lu=0&show={10}&post=1&rd={rd}";
                string refer = $"https://{DoMain}/citibethk.jsp?race_type={item.Url}&race_date={item.Date}&tab=u&sml=s";

                BetItem betItem = new BetItem();
                betItem.StrRace = item.Race;
                betItem.StrHorse = item.Horse;
                betItem.StrWin = item.Win.ToString();
                betItem.StrPlace = item.Place.ToString();
                betItem.StrDiscount = item.Zhe.ToString();
                betItem.StrL_win = item.LWin.ToString();
                betItem.StrL_place = item.LPlace.ToString();
                betItem.EnuBetType = BetType.BET;
                info.ObjBetItem = betItem;
                info.StrUrl = url;
                info.StrBetString = item.ToString();

                string str = Connect.getDocument(url, cc, refer, "utf-8");
                CheckLogout(str);
                CheckUnableBet(str);
                info.StrAnswer = str;

                if (!string.IsNullOrEmpty(str) && str.Contains("要求下注成功") || str.Contains("要求吃票成功"))
                {
                    bRet = true;
                    info.DBetCount = -2;
                    info.EnuBetResultType = BetResultType.ACCEPT_NO_MATCH;

                }
                else if (!string.IsNullOrEmpty(str) && str.Contains("所有被证实"))
                {
                    bRet = true;
                    info.DBetCount = item.Win;
                    info.EnuBetResultType = BetResultType.FULL;
                }
                else if (!string.IsNullOrEmpty(str) && str.Contains("部份被证实"))
                {
                    bRet = true;
                    info.DBetCount = -1;
                    info.EnuBetResultType = BetResultType.PARTIAL;
                }
                else
                {
                    bRet = false;
                    info.DBetCount = -3;
                    info.EnuBetResultType = BetResultType.FAIL;
                    if (!string.IsNullOrEmpty(str) && str.IndexOf("top.logout") > 0)
                    {
                        //已经退出
                        info.DBetCount = -99;
                    }
                }
            }
            return bRet;
        }

        /// <summary>
        /// 挂Q
        /// </summary>
        /// <param name="item">马号为  1_3_4</param>
        /// <param name="info"></param>
        /// <param name="combo">1为拖，0为交叉,第一个马号为拖头</param>
        /// <returns></returns>
        public bool QiPiaoGuaQ(RaceInfoItem item, out BetResultInfo info, string combo = "1")
        {
            bool bRet = false;
            info = new BetResultInfo();

            if (IsLogin)
            {
                Random ron = new Random();

                int ser = ron.Next(100000000, 999999999);
                string rd = string.Format("0.{0}", ser);
                double tix = 0;
                double fclmt = 0;
                int fctype = 0;
                double amount = 0;
                string type = string.Empty;
                //把马号的-改为>,(1-2)的去掉()
                string Hss = item.Horse.Replace("-", ">").Replace("(", "").Replace(")", "");

                BetItem betItem = new BetItem();
                betItem.StrRace = item.Race;
                betItem.StrHorse = item.Horse;
                betItem.EnuBetType = item.Bettype;

                if (item.Bettype == BetType.EAT)
                {
                    type = "EAT";
                }
                if (item.Bettype == BetType.BET)
                {
                    type = "BET";
                }

                if (item.Playtype == PlayType.QP)
                {
                    tix = item.Place;
                    fclmt = item.LPlace;
                    fctype = 1;
                    amount = item.Zhe;
                    betItem.StrPlace = tix.ToString();
                    betItem.StrL_place = fclmt.ToString();
                }
                if (item.Playtype == PlayType.Q)
                {
                    tix = item.Win;
                    fclmt = item.LWin;
                    fctype = 0;
                    amount = item.Zhe;
                    betItem.StrWin = tix.ToString();
                    betItem.StrL_win = fclmt.ToString();
                }
                betItem.StrDiscount = amount.ToString();

                string url = $"https://{DoMain}/forecast?task=betBox&combo={combo}&Tix={tix}&Race={item.Race}&Hss={Hss}&fctype={fctype}&Q=Q&type={type}&overflow=1&amount={amount}&fclmt={fclmt}&race_type={item.Url}&race_date={item.Date}&show={item.Race}&rd={rd}";
                //string refer = "https://cvyorfp.citibet.net/citibethk.jsp?race_type=63A&race_date=17-08-2012&tab=u&sml=s";
                string refer = $"https://{DoMain}/citibethk.jsp?race_type={item.Url}&race_date={item.Date}&tab=u&sml=s";


                info.ObjBetItem = betItem;
                info.StrUrl = url;
                info.StrBetString = item.ToString();

                string str = Connect.getDocument(url, cc, refer, "utf-8");
                CheckLogout(str);
                CheckUnableBet(str);
                info.StrAnswer = str;

                if (!string.IsNullOrEmpty(str) && str.Contains("成功"))
                {
                    bRet = true;
                    info.DBetCount = -2;
                    info.EnuBetResultType = BetResultType.ACCEPT_NO_MATCH;

                }
                else if (!string.IsNullOrEmpty(str) && str.Contains("所有被证实"))
                {
                    bRet = true;
                    info.DBetCount = tix;
                    info.EnuBetResultType = BetResultType.FULL;
                }
                else if (!string.IsNullOrEmpty(str) && str.Contains("部份被证实"))
                {
                    bRet = true;
                    info.DBetCount = -1;
                    info.EnuBetResultType = BetResultType.PARTIAL;
                }
                else
                {
                    bRet = false;
                    info.DBetCount = -3;
                    info.EnuBetResultType = BetResultType.FAIL;
                }
            }
            return bRet;
        }
        #endregion

        #region 删除挂单

        public bool DeleteAllQBetGuaDan(string strBetinfo)
        {
            bool bRet = false;
            if (!string.IsNullOrEmpty(strBetinfo))
            {
                Regex re = new Regex(@"\[DA#mr\('(?'bid'\d+),(?'x'\d+),\w+,(?'race_date'\d+-\d+-\d+),(?'race_type'\w+),(?'race'\d+)'\)#Rc \d+#FC/P/Q BET pending\]", RegexOptions.None);
                Match mc = re.Match(strBetinfo);
                string bid = "";
                string x = "";
                string race_date = "";
                string race_type = "";
                string race = "";
                if (mc.Success)
                {
                    x = mc.Groups["x"].Value;
                    bid = mc.Groups["bid"].Value;
                    race_date = mc.Groups["race_date"].Value;
                    race_type = mc.Groups["race_type"].Value;
                    race = mc.Groups["race"].Value;

                    string url = $"https://{DoMain}/transactions?type=del&bid={bid}&x={x}&betType={race_type}&race_date={race_date}&race_type={race_type}&race={race}&show={race}&post=1&rd={new Random().NextDouble()}";
                    string refer = $"https://{DoMain}/jsp/trans_mt.jsp?s=S";
                    string str = Connect.getDocument(url, cc, refer, "utf-8");
                    CheckLogout(str);
                    CheckUnableBet(str);

                    if (str.Contains("取消预测彩下注成功"))
                    {
                        bRet = true;
                    }
                }
            }
            return bRet;
        }
        public bool DeleteAllQEatGuaDan(string strBetinfo)
        {
            bool bRet = false;
            if (!string.IsNullOrEmpty(strBetinfo))
            {
                Regex re = new Regex(@"\[DA#mr\('(?'bid'\d+),(?'x'\d+),\w+,(?'race_date'\d+-\d+-\d+),(?'race_type'\w+),(?'race'\d+)'\)#Rc \d+#FC/P/Q EAT pending\]", RegexOptions.None);
                Match mc = re.Match(strBetinfo);
                string bid = "";
                string x = "";
                string race_date = "";
                string race_type = "";
                string race = "";
                if (mc.Success)
                {
                    x = mc.Groups["x"].Value;
                    bid = mc.Groups["bid"].Value;
                    race_date = mc.Groups["race_date"].Value;
                    race_type = mc.Groups["race_type"].Value;
                    race = mc.Groups["race"].Value;

                    string url = $"https://{DoMain}/transactions?type=del&bid={bid}&x={x}&betType={race_type}&race_date={race_date}&race_type={race_type}&race={race}&show={race}&post=1&rd={new Random().NextDouble()}";
                    string refer = $"https://{DoMain}/jsp/trans_mt.jsp?s=S";
                    string str = Connect.getDocument(url, cc, refer, "utf-8");
                    CheckLogout(str);
                    CheckUnableBet(str);

                    if (str.Contains("取消预测彩吃注成功"))
                    {
                        bRet = true;
                    }
                }
            }
            return bRet;
        }
        public bool DeleteAllBetGuaDan(string strBetinfo)
        {
            bool bRet = false;
            if (!string.IsNullOrEmpty(strBetinfo))
            {
                Regex re = new Regex(@"\[DA#mr\('(?'bid'\d+),(?'x'\d+),\w+,(?'race_date'\d+-\d+-\d+),(?'race_type'\w+),(?'race'\d+)'\)#Rc \d+#win/place BET pending\]", RegexOptions.None);
                Match mc = re.Match(strBetinfo);
                string bid = "";
                string x = "";
                string race_date = "";
                string race_type = "";
                string race = "";
                if (mc.Success)
                {
                    x = mc.Groups["x"].Value;
                    bid = mc.Groups["bid"].Value;
                    race_date = mc.Groups["race_date"].Value;
                    race_type = mc.Groups["race_type"].Value;
                    race = mc.Groups["race"].Value;

                    string url = $"https://{DoMain}/transactions?type=del&bid={bid}&x={x}&betType={race_type}&race_date={race_date}&race_type={race_type}&race={race}&show={race}&post=1&rd={new Random().NextDouble()}";
                    string refer = $"https://{DoMain}/jsp/trans_mt.jsp?s=S";
                    string str = Connect.getDocument(url, cc, refer, "utf-8");
                    CheckLogout(str);
                    CheckUnableBet(str);

                    if (str.Contains("取消下注成功"))
                    {
                        bRet = true;
                    }
                }
            }
            return bRet;
        }

        public bool DeleteAllEatGuaDan(string strBetinfo)
        {
            bool bRet = false;
            if (!string.IsNullOrEmpty(strBetinfo))
            {
                Regex re = new Regex(@"\[DA#mr\('(?'bid'\d+),(?'x'\d+),\w+,(?'race_date'\d+-\d+-\d+),(?'race_type'\w+),(?'race'\d+)'\)#Rc \d+#win/place EAT pending\]", RegexOptions.None);
                Match mc = re.Match(strBetinfo);
                string bid = "";
                string x = "";
                string race_date = "";
                string race_type = "";
                string race = "";
                if (mc.Success)
                {
                    x = mc.Groups["x"].Value;
                    bid = mc.Groups["bid"].Value;
                    race_date = mc.Groups["race_date"].Value;
                    race_type = mc.Groups["race_type"].Value;
                    race = mc.Groups["race"].Value;

                    string url = $"https://{DoMain}/transactions?type=del&bid={bid}&x={x}&betType={race_type}&race_date={race_date}&race_type={race_type}&race={race}&show={race}&post=1&rd={new Random().NextDouble()}";
                    string refer = $"https://{DoMain}/jsp/trans_mt.jsp?s=S";
                    string str = Connect.getDocument(url, cc, refer, "utf-8");
                    CheckLogout(str);
                    CheckUnableBet(str);
                    if (str.Contains("取消吃票成功"))
                    {
                        bRet = true;
                    }
                }
            }
            return bRet;
        }

        public bool DeleteAllLiveBetGuaDan(string strBetinfo)
        {
            bool bRet = false;
            if (!string.IsNullOrEmpty(strBetinfo))
            {
                Regex re = new Regex(@"\[DA#mr\('(?'bid'\d+),(?'x'\d+),\w+,(?'race_date'\d+-\d+-\d+),(?'race_type'\w+),(?'race'\d+)'\)#Rc \d+#win/place Live BET pending\]", RegexOptions.None);
                Match mc = re.Match(strBetinfo);
                string bid = "";
                string x = "";
                string race_date = "";
                string race_type = "";
                string race = "";
                if (mc.Success)
                {
                    x = mc.Groups["x"].Value;
                    bid = mc.Groups["bid"].Value;
                    race_date = mc.Groups["race_date"].Value;
                    race_type = mc.Groups["race_type"].Value;
                    race = mc.Groups["race"].Value;

                    string url = $"https://{DoMain}/transactions?type=del&bid={bid}&x={x}&betType={race_type}&race_date={race_date}&race_type={race_type}&race={race}&show={race}&post=1&rd={new Random().NextDouble()}";
                    string refer = $"https://{DoMain}/jsp/trans_mt.jsp?s=S";
                    string str = Connect.getDocument(url, cc, refer, "utf-8");
                    CheckLogout(str);
                    CheckUnableBet(str);

                    if (str.Contains("取消下注成功"))
                    {
                        bRet = true;
                    }
                }
            }
            return bRet;
        }

        public bool DeleteAllLiveEatGuaDan(string strBetinfo)
        {
            bool bRet = false;
            if (!string.IsNullOrEmpty(strBetinfo))
            {
                Regex re = new Regex(@"\[DA#mr\('(?'bid'\d+),(?'x'\d+),\w+,(?'race_date'\d+-\d+-\d+),(?'race_type'\w+),(?'race'\d+)'\)#Rc \d+#win/place Live EAT pending\]", RegexOptions.None);
                Match mc = re.Match(strBetinfo);
                string bid = "";
                string x = "";
                string race_date = "";
                string race_type = "";
                string race = "";
                if (mc.Success)
                {
                    x = mc.Groups["x"].Value;
                    bid = mc.Groups["bid"].Value;
                    race_date = mc.Groups["race_date"].Value;
                    race_type = mc.Groups["race_type"].Value;
                    race = mc.Groups["race"].Value;

                    string url = $"https://{DoMain}/transactions?type=del&bid={bid}&x={x}&betType={race_type}&race_date={race_date}&race_type={race_type}&race={race}&show={race}&post=1&rd={new Random().NextDouble()}";
                    string refer = $"https://{DoMain}/jsp/trans_mt.jsp?s=S";
                    string str = Connect.getDocument(url, cc, refer, "utf-8");
                    CheckLogout(str);
                    CheckUnableBet(str);
                    if (str.Contains("取消吃票成功"))
                    {
                        bRet = true;
                    }
                }
            }
            return bRet;
        }
        #endregion
        #region 获取赔率
        /// <summary>
        /// 获取一组马号的Q的赔率，需要先获取一次所有马的赔率GetPeiData14才可以使用，否则出错
        /// </summary>
        /// <param name="h1"></param>
        /// <param name="h2"></param>
        /// <returns></returns>
        public double GetQpei(int h1, int h2)
        {
            double pei = 0;
            if (h1 > h2)
            {
                int tmp = h1;
                h1 = h2;
                h2 = tmp;
            }
            pei = GetQOdds($"{h1}-{h2}");
            return pei;
        }
        /// <summary>
        /// 从网站上实时获取一组马号的Q赔率
        /// </summary>
        /// <param name="h1"></param>
        /// <param name="h2"></param>
        /// <returns></returns>
        public double GetQpeiNow(int h1, int h2)
        {
            double pei = 0;
            Dictionary<string, string[,]> dicQPei = GetPeiData14();
            if (h1 > h2)
            {
                int tmp = h1;
                h1 = h2;
                h2 = tmp;
            }
            pei = GetQOdds($"{h1}-{h2}");
            return pei;
        }
        /// <summary>
        /// 获取Q赔率，14只以内
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string[,]> GetPeiData14()
        {
            return GetPeiData14(new RaceInfoItem { Url = Config.MatchUrl, Race = Config.Race });
        }
        /// <summary>
        /// 获取Q赔率，14只马以内
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Dictionary<string, string[,]> GetPeiData14(RaceInfoItem item)
        {
            string _now = GetNow(item.Url);
            //https://ksifvch.ctb988.com/totedata?race_date=28-08-2018&qMode=QQ&race_type=31A&rc=9&x=0.4302389970655378&rcs=8
            string url = $"https://{DoMain}/totedata?race_date={_now}&qMode=QQ&race_type={item.Url}&rc={item.Race}&x=0.4302389970655378&rcs=8";
            //string refer = "https://data.citibet.net/betdata?race_date=18-08-2012&race_type=60A&rc=10&m=HK&c=3";
            string refer = $"https://{DoMain}/betdata?race_date={_now}&race_type={item.Url}&rc={item.Race}&m=HK&c=3";
            string str = Connect.getDocument(url, cc, refer, "utf-8");
            CheckLogout(str);
            Dictionary<string, string[,]> dicPei = new Dictionary<string, string[,]>();
            //如果超过14只马
            if (!string.IsNullOrEmpty(str))
            {
                //    dicPei = GetPeiData24(item);
                //}
                //else
                //{
                Qpeis = ParseQData14(str);
                QPpeis = ParseQPData14(str);
                dicPei.Add("Q", Qpeis);
                dicPei.Add("QP", QPpeis);
            }

            return dicPei;
        }

        /// <summary>
        /// 获取Q赔率，24只马以内
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Dictionary<string, string[,]> GetPeiData24(RaceInfoItem item)
        {
            string _now = GetNow();
            //https://data.ctb988.com/totedata?race_date=24-11-2019&oversea=hkoq&qMode=QQ&race_type=14H&rc=2&x=0.4270748651324312&rcs=2
            string url = $"https://{DoMain}/totedata?race_date={_now}&oversea=hkoq&qMode=QQ&race_type={item.Url}&rc={item.Race}&x=0.4302389970655378&rcs=8";
            //https://data.ctb988.com/HKOQ.jsp?race_date=24-11-2019&race=2&race_type=14H
            string refer = $"https://{DoMain}/HKOQ.jsp?race_date={_now}&race={item.Race}&race_type={item.Url}";
            string str = Connect.getDocument(url, cc, refer, "utf-8");
            CheckLogout(str);
            Dictionary<string, string[,]> dicPei = new Dictionary<string, string[,]>();
            dicPei.Add("Q", ParseQData24(str));
            dicPei.Add("QP", ParseQPData24(str));
            return dicPei;
        }

        /// <summary>
        /// 获取WP的赔率表
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, WPOdds> GetWPPeiData()
        {
            return GetWPPeiData(new RaceInfoItem { Url = Config.MatchUrl, Race = Config.Race });
        }
        /// <summary>
        /// 获取WP的赔率表
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Dictionary<string, WPOdds> GetWPPeiData(RaceInfoItem item)
        {
            string _now = GetNow(item.Url);
            //https://cqfexhv.ctb988.net/totedata?race_date=15-02-2019&race_type=1S&rc=2&currRC=2&x=0.8111822838958027
            string url = $"https://{DoMain}/totedata?race_date={_now}&race_type={item.Url}&rc={item.Race}&currRC={item.Race}&x={new Random().NextDouble()}";
            string refer = url;
            string str = Connect.getDocument(url, cc, refer, "utf-8");
            CheckLogout(str);
            return ParseWPPei(str);
        }
        string[,] ParseQData14(string strData)
        {
            try
            {
                if (!string.IsNullOrEmpty(strData))
                {
                    string _tmp1 = strData.Substring(strData.IndexOf("#Quinella"));
                    string _tmp2 = _tmp1.Substring(0, _tmp1.IndexOf(@"</pre>"));
                    string[] _tmp3 = _tmp2.Split("@".ToCharArray());
                    string[,] Qpei = new string[15, 15];
                    for (int i = 1; i <= 7; i++)
                    {
                        string[] _tmp4 = _tmp3[i].Split("\t".ToCharArray());
                        for (int k = 1; k < i; k++)
                        {
                            Qpei[8 + k - 1, 8 + i - 1] = RomoveChar(_tmp4[k]);
                        }
                        for (int j = i + 2; j <= 15; j++)
                        {
                            Qpei[i, j - 1] = RomoveChar(_tmp4[j]);
                        }
                    }
                    return Qpei;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        string[,] ParseQPData14(string strData)
        {
            try
            {
                if (!string.IsNullOrEmpty(strData))
                {
                    string _tmp1 = strData.Substring(strData.IndexOf("#Quinella Place"));
                    string _tmp2 = _tmp1.Substring(0, _tmp1.IndexOf(@"</pre>"));
                    string[] _tmp3 = _tmp2.Split("@".ToCharArray());
                    string[,] Qpei = new string[15, 15];
                    for (int i = 1; i <= 7; i++)
                    {
                        string[] _tmp4 = _tmp3[i].Split("\t".ToCharArray());
                        for (int k = 1; k < i; k++)
                        {
                            Qpei[8 + k - 1, 8 + i - 1] = RomoveChar(_tmp4[k]);
                        }
                        for (int j = i + 2; j <= 15; j++)
                        {
                            Qpei[i, j - 1] = RomoveChar(_tmp4[j]);
                        }
                    }
                    return Qpei;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        string[,] ParseQData24(string strData)
        {
            try
            {
                if (!string.IsNullOrEmpty(strData))
                {
                    string _tmp1 = strData.Substring(strData.IndexOf("#Quinella"));
                    string _tmp2 = _tmp1.Substring(0, _tmp1.IndexOf(@"</pre>"));
                    string[] _tmp3 = _tmp2.Split("@".ToCharArray());
                    string[,] Qpei = new string[25, 25];
                    for (int i = 1; i <= 12; i++)
                    {
                        string[] _tmp4 = _tmp3[i].Split("\t".ToCharArray());
                        for (int k = 1; k < i; k++)
                        {
                            Qpei[13 + k - 1, 13 + i - 1] = RomoveChar(_tmp4[k]);
                        }
                        for (int j = i + 2; j <= 25; j++)
                        {
                            Qpei[i, j - 1] = RomoveChar(_tmp4[j]);
                        }
                    }
                    return Qpei;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        string[,] ParseQPData24(string strData)
        {
            try
            {
                if (!string.IsNullOrEmpty(strData))
                {
                    string _tmp1 = strData.Substring(strData.IndexOf("#Quinella Place"));
                    string _tmp2 = _tmp1.Substring(0, _tmp1.IndexOf(@"</pre>"));
                    string[] _tmp3 = _tmp2.Split("@".ToCharArray());
                    string[,] Qpei = new string[25, 25];
                    for (int i = 1; i <= 12; i++)
                    {
                        string[] _tmp4 = _tmp3[i].Split("\t".ToCharArray());
                        for (int k = 1; k < i; k++)
                        {
                            Qpei[13 + k - 1, 13 + i - 1] = RomoveChar(_tmp4[k]);
                        }
                        for (int j = i + 2; j <= 25; j++)
                        {
                            Qpei[i, j - 1] = RomoveChar(_tmp4[j]);
                        }
                    }
                    return Qpei;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        Dictionary<string, WPOdds> ParseWPPei(string str)
        {
            Dictionary<string, WPOdds> retOdds = new Dictionary<string, WPOdds>();
            if (!string.IsNullOrEmpty(str) && str.IndexOf("<table") > 0 && str.IndexOf("</table>") > 0)
            {
                string content = str.Substring(str.IndexOf("<table"), str.IndexOf("</table>") - str.IndexOf("<table") + 8);
                var doc = new HtmlAgilityPack.HtmlDocument();
                if (!string.IsNullOrEmpty(content))
                {
                    doc.LoadHtml(content);
                    if (doc != null)
                    {
                        var nodes = doc.DocumentNode.SelectNodes(@"table/tr");
                        if (nodes != null)
                        {
                            foreach (var item in nodes)
                            {
                                var items = item.SelectNodes(@"td/div");
                                if (items != null)
                                {
                                    WPOdds odds = new WPOdds();
                                    double win = 0;
                                    double place = 0;
                                    double.TryParse(items[0].InnerText.Trim(), out win);
                                    odds.Win = win;
                                    odds.Horse = items[1].InnerText.Trim();
                                    double.TryParse(items[2].InnerText.Trim(), out place);
                                    odds.Place = place;
                                    if (!retOdds.ContainsKey(odds.Horse))
                                    {
                                        retOdds.Add(odds.Horse, odds);
                                    }


                                    win = 0;
                                    place = 0;
                                    WPOdds odds2 = new WPOdds();
                                    odds2.Horse = items[4].InnerText.Trim();
                                    int tmp;
                                    //第二只马，防止有空的马号
                                    if (int.TryParse(odds2.Horse, out tmp))
                                    {
                                        double.TryParse(items[3].InnerText.Trim(), out win);
                                        odds2.Win = win;

                                        double.TryParse(items[5].InnerText.Trim(), out place);
                                        odds2.Place = place;
                                        if (!retOdds.ContainsKey(odds2.Horse))
                                        {
                                            retOdds.Add(odds2.Horse, odds2);
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
            return retOdds;
        }


        string RomoveChar(string content)
        {
            Regex re = new Regex(@"[[]\d+[]]", RegexOptions.None);
            string result = re.Replace(content, "");
            return result;
        }
        #endregion
    }
}

/// <summary>
/// 会员账号信息
/// </summary>
public class MemberInfo
{
    /// <summary>
    /// 信用限额
    /// </summary>
    public string Allocated
    {
        get;
        set;
    }
    /// <summary>
    /// 余额
    /// </summary>
    public string Balance
    {
        get;
        set;
    }
    /// <summary>
    /// >赢/输
    /// </summary>
    public string Loss
    {
        get;
        set;
    }
}

public enum MatchStage { BreadFast, Running, Undefine };
public class MatchTimeInfo
{
    public MatchStage Stage;
    public int LastTime;
    public MatchTimeInfo()
    {
        Stage = MatchStage.Undefine;
        LastTime = 0;
    }
}

public class RaceItem
{
    public string Url
    {
        get;
        set;
    }
    public string Race
    {
        get;
        set;
    }
    public string Horse
    {
        get;
        set;
    }
    public int Piao
    {
        get;
        set;
    }
}
