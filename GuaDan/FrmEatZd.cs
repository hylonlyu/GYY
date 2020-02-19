using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuaDan
{
    public partial class FrmEatZd : Form
    {
        private FrmWebbrowser frmBrowser;
        /// <summary>
        /// 已经开的比赛场次
        /// </summary>
        List<int> lstOpenedRace = new List<int>();

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetSetCookie(string lpszUrlName, string lbszCookieName, string lpszCookieData);

        ZdConfig Config = new ZdConfig();
        private LoginStatus IsLogin = LoginStatus.LOGOUT;
        private CCMember cCmemberInstance;
        public CCMember CCmemberInstance
        {
            get
            {
                if (cCmemberInstance == null)
                {
                    cCmemberInstance = new ZdStrategy();
                }
                return cCmemberInstance;
            }
            set
            {
                cCmemberInstance = value;
            }
        }

        public bool bCheckOnline
        {
            get;
            set;
        }
        private int CurrentErrorTimes = 0;
        private readonly int ErrorTimesLimit = 3;
        UserInfo uInfo = new UserInfo();
        public FrmEatZd()
        {
            InitializeComponent();
            CCmemberInstance.OnLoginOk += CCmemberInstance_OnLoginOk;
            CCmemberInstance.OnLoginFail += CCmemberInstance_OnLoginFail;
            CCmemberInstance.OnLogout += CCmemberInstance_OnLogout;
            CCmemberInstance.OnBetOk += CCmemberInstance_OnBetOk;
            CCmemberInstance.ShowMsg += CCmemberInstance_ShowMsg;
            CCmemberInstance.OnNewTickt += CCmemberInstance_OnNewTickt;
        }

        private void CCmemberInstance_OnNewTickt(RaceInfoEnity enity)
        {
            DisplayBetInfo(enity);
        }

        private void CCmemberInstance_ShowMsg(string str)
        {
            ShowInfoMsg(str);
        }

        private void CCmemberInstance_OnBetOk(BettedItem item)
        {
            ShowInfoMsg(item.ToString());
            ShowBetResult(item);
        }

        private void CCmemberInstance_OnLogout()
        {
            ShowInfoMsg("账号退出");
            SetBtnStartEnable(false);
            IsLogin = LoginStatus.LOGOUT;

            Thread.Sleep(5000);
            ShowInfoMsg("账号退出，重新登陆");
            new Thread(new ParameterizedThreadStart(DoLogin)).Start(uInfo);
        }

        private void CCmemberInstance_OnLoginFail()
        {
            SetBtnStartEnable(false);
            IsLogin = LoginStatus.LOGOUT;
            Thread.Sleep(5000);

            ShowInfoMsg("登陆失败，重新登陆");
            new Thread(new ParameterizedThreadStart(DoLogin)).Start(uInfo);
        }

        private void CCmemberInstance_OnLoginOk()
        {
            ShowInfoMsg("登陆成功");
            SetBtnLoginText("退出");
            SetBtnStartEnable(true);
            IsLogin = LoginStatus.LOGIN;
        }

        private void ShowInfoMsg(string str)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(ShowInfoMsg), str);
            }
            else
            {
                string time = DateTime.Now.ToString("HH:mm:ss:ffff");
                lstInfo.Items.Add($"{time} ##  {str}");
                lstInfo.SelectedIndex = lstInfo.Items.Count - 1;
            }

        }
        private void SetBtnLoginText(string str)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(SetBtnLoginText), str);
            }
            else
            {
                this.btnLogin.Text = str;
            }
        }

        private void SetBtnStartEnable(bool bEnalbe)
        {
        }

        private void SetBtnEnable(Button btn, bool bEnalbe)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<Button,bool>(SetBtnEnable),btn, bEnalbe);
            }
            else
            {
                btn.Enabled = bEnalbe;
            }
        }
        #region Config
        private void SaveConfig()
        {
            if (!string.IsNullOrEmpty(cobMatch.SelectedText.Trim()))
            {
                Config.MatchCombol = $"{cobMatch.SelectedText.Trim()}|{cobMatch.SelectedValue.ToString().Trim()}";
            }
            if (cobMatch.SelectedValue != null)
            {
                Config.MatchUrl = cobMatch.SelectedValue.ToString();
            }
            Config.SiteUrl = txtUrl.Text.Trim();
            Config.Accout = txtAccount.Text.Trim();
            Config.Pwd = txtPwd.Text.Trim();
            Config.Pin = txtPin.Text.Trim();
            Config.Race = cobRace.Text.Trim();

            Config.KPSJ = Util.Text2Int(txtKPSJ.Text.Trim());
            Config.GDPL = Util.Text2Double(txtGDPL.Text.Trim());
            Config.DDJG = Util.Text2Int(txtDDJG.Text.Trim());
            Config.KSJE = Util.Text2Int(txtKSJE.Text.Trim());
            Config.GDJK = Util.Text2Double(txtGDJK.Text.Trim());
            Config.JXBL = Util.Text2Int(txtJXBL.Text.Trim());
            Config.ZGJX = Util.Text2Int(txtZGJX.Text.Trim());
            Config.PSBL = Util.Text2Double(txtPSBL.Text.Trim());
            Config.SCSJ = Util.Text2Int(txtSCSJ.Text.Trim());

            string file = string.Format(@"setting\{0}.jpg", "FrmEatZd");

            using (FileStream fs = new FileStream(file, FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, Config);
            }
        }

        private void LoadConfig()
        {
            string file = string.Format(@"setting\{0}.jpg", "FrmEatZd");
            if (File.Exists(file))
            {
                try
                {
                    using (FileStream fs = new FileStream(file, FileMode.Open))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        Config = bf.Deserialize(fs) as ZdConfig;
                    }
                }
                catch (Exception ex)
                {
                    Config = new ZdConfig();
                }

            }
            else
            {
                Config = new ZdConfig();
            }
            SetConfig();
        }

        private void SetConfig()
        {
            try
            {
                txtUrl.Text = Config.SiteUrl;
                txtAccount.Text = Config.Accout;
                txtPwd.Text = Config.Pwd;
                txtPin.Text = Config.Pin;
                cobRace.Text = Config.Race;

                txtKPSJ.Text = Config.KPSJ.ToString();
                txtGDPL.Text = Config.GDPL.ToString();
                txtDDJG.Text = Config.DDJG.ToString();
                txtKSJE.Text = Config.KSJE.ToString();
                txtGDJK.Text = Config.GDJK.ToString();
                txtJXBL.Text = Config.JXBL.ToString();
                txtZGJX.Text = Config.ZGJX.ToString();
                txtPSBL.Text = Config.PSBL.ToString();
                txtSCSJ.Text = Config.SCSJ.ToString();
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        private void FrmEatZd_Load(object sender, EventArgs e)
        {
            LoadConfig();
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (IsLogin != LoginStatus.DOINGLOGIN)
            {
                if (btnLogin.Text.Equals("登陆"))
                {
                    
                    uInfo.CUserName = txtAccount.Text.Trim();
                    uInfo.CPassword = txtPwd.Text.Trim();
                    uInfo.CPin = txtPin.Text.Trim();
                    uInfo.CUrl = txtUrl.Text.Trim();
                    new Thread(new ParameterizedThreadStart(DoLogin)).Start(uInfo);

                }
                if (btnLogin.Text.Equals("退出"))
                {
                    IsLogin = LoginStatus.LOGOUT;
                    SetBtnLoginText("登陆");
                }
            }
       
        }
        private void DoLogin(object obj)
        {
            if (IsLogin == LoginStatus.LOGOUT)
            {
                UserInfo uInfo = obj as UserInfo;
                IsLogin = LoginStatus.DOINGLOGIN;
                ShowInfoMsg("正在登陆");
                Hashtable ht = CCmemberInstance.DoLogin(uInfo.CUrl, uInfo);
                if (ht != null)
                {
                    BindMatchList(ht[4] as DataTable);
                }
            }
        }

        private void BindMatchList(DataTable dtMatch)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<DataTable>(BindMatchList), dtMatch);
            }
            else
            {
                try
                {
                    //加上上次的赛事
                    if (!string.IsNullOrEmpty(Config.MatchCombol))
                    {
                        string[] tmp = Config.MatchCombol.Split("|".ToCharArray());
                        if (tmp.Length > 1)
                        {
                            DataRow dr = dtMatch.NewRow();
                            dr["tip"] = tmp[0];
                            dr["url"] = tmp[1];
                            dtMatch.Rows.InsertAt(dr, 0);
                        }
                    }

                    //只打香港的比赛
                    DataTable TempDT = dtMatch.Clone();
                    DataRow[] drs = dtMatch.Select("tip like '%3H%'");
                    foreach (DataRow dr in drs)
                    {
                        TempDT.ImportRow(dr);
                    }
                    cobMatch.DataSource = TempDT;

                    //cobMatch.DataSource = dtMatch;
                    cobMatch.DisplayMember = "tip";
                    cobMatch.ValueMember = "url";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }



        private void ShowBetResult(BettedItem item)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<BettedItem>(ShowBetResult), item);
            }
            else
            {
                string bettime = item.BetTime.ToLongTimeString();
                string match = "";
                string[] tmp = cobMatch.Text.Split("_".ToCharArray());
                if (tmp.Length > 2)
                {
                    match = $"{tmp[0]}_{tmp[1]}";
                }

                string race = item.Race;
                string horse = item.Horse;
                string win = item.PlayType == PlayType.Q ? item.DBetCount.ToString() : "";
                string place = item.PlayType == PlayType.QP ? item.DBetCount.ToString() : "";
                string zhe = item.Zhe.ToString();
                string lwin = item.PlayType == PlayType.Q ? item.Lim.ToString() : "";
                string lplace = item.PlayType == PlayType.QP ? item.Lim.ToString() : "";
                string bettype = item.BetType == BetType.EAT ? "吃" : "赌";
                string odds = item.Odds.ToString();
                string total = item.TotalCount.ToString();
                string result = item.Result ? "成功" : "失败";
                string reason = item.Reason;

                //dgvBetResult.Rows.Insert(0, new object[] { bettime, match, race, horse, win, place, zhe, lwin, lplace, bettype, odds, total, result, reason });
            }
        }


        private void SetCookie(string Url, CookieContainer CC)
        {
            Uri uri = new Uri(Url);
            string cDomain = uri.Host;
            CookieContainer container = CC;
            CookieCollection cc = container.GetCookies(new Uri(Url));
            foreach (Cookie c in cc)
            {
                InternetSetCookie("http://" + cDomain, c.Name.ToString(), c.Value.ToString());
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void timerShowData_Tick(object sender, EventArgs e)
        {
      
        }

        /// <summary>
        /// 获取当前比赛的场次，并在cobrace中显示
        /// </summary>
        private void GetandDisplayRace()
        {
            string url = cobMatch.SelectedValue.ToString();
            BackgroundWorker bw = new BackgroundWorker();
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            bw.DoWork += Bw_DoWork;
            bw.RunWorkerAsync(url);
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            string url = e.Argument as string;
            List<int> res = CCmemberInstance.GetOpenedRace(url);
            e.Result = res;
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lstOpenedRace = e.Result as List<int>;
            if (lstOpenedRace.Count > 0)
            {
                cobRace.DataSource = lstOpenedRace;
                cobRace.Text = lstOpenedRace[0].ToString();
                Config.Race = lstOpenedRace[0].ToString();
                CCmemberInstance.Config = Config;
                ShowInfoMsg($"获取了{lstOpenedRace.Count}场，首场{lstOpenedRace[0]}");
            }
        }

        private void btnTrade_Click(object sender, EventArgs e)
        {
            if (frmBrowser == null || frmBrowser.IsDisposed)
            {
                frmBrowser = new FrmWebbrowser();
                frmBrowser.Url = $"http://{CCmemberInstance.DoMain}/new_history_live.jsp";
                frmBrowser.CC = CCmemberInstance.cc;
                frmBrowser.Show();
            }
            else
            {
                frmBrowser.WindowState = FormWindowState.Normal;
                frmBrowser.Show();
                frmBrowser.Activate();
            }
        }

        private void DisplayBetInfo(RaceInfoEnity htBetInfo)
        {
           
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<RaceInfoEnity>(DisplayBetInfo), htBetInfo);
            }
            else
            {
                if (htBetInfo.DicRaceInfo != null)
                {
                    foreach (KeyValuePair<string, RaceInfoItem> de in htBetInfo.DicRaceInfo)
                    {
                        //col.Add("snakehead", "会员");
                        //col.Add("date", "比赛日期");
                        //col.Add("playtype", "比赛类型");
                        //col.Add("country", "场地");
                        //col.Add("location", "赛事");
                        //col.Add("odds", "赔率");
                        //col.Add("race", "场");
                        //col.Add("horse", "马");
                        //col.Add("win", "W");
                        //col.Add("place", "P");
                        //col.Add("zhe", "%");
                        //col.Add("lwin", "W极");
                        //col.Add("lplace", "P极");
                        //col.Add("classType", "类型");
                        //col.Add("bettype", "下注");
                        //col.Add("time", "时间");
                        RaceInfoItem item = de.Value as RaceInfoItem;

                        string classType = item.ClassType;
                        //wp走地，显示 live
                        if (item.Playtype == PlayType.WP && !string.IsNullOrEmpty(item.Live))
                        {
                            classType = "L";
                        }

                        //dgvNewBet.Rows.Insert(0, htBetInfo.SnakeHead, item.Date, item.Playtype, item.Country, item.Location, item.OddsType, item.Race, item.Horse,
                        //          item.Win, item.Place, item.Zhe, item.LWin, item.LPlace, classType, item.Bettype, DateTime.Now.ToString("MM-dd HH:mm:ss ffff"));

                        //RaceInfoEnity enity = new RaceInfoEnity();
                        //enity.SnakeHead = htBetInfo.SnakeHead;
                        //enity.DicRaceInfo.Add(de.Key, de.Value);

                        //dgvNewBet.Rows[0].Tag = enity;
                    }
                }
            }

        }

        #region 注册相关
        private void DoInitRegInfo()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(this.bw_DoInitRegInfo);
            worker.ProgressChanged += new ProgressChangedEventHandler(this.bw_DoInitRegInfoChanged);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.bw_DoInitRegInfoCompleted);
            Hashtable argument = new Hashtable();
            worker.RunWorkerAsync(argument);
        }

        private void bw_DoInitRegInfo(object sender, DoWorkEventArgs e)
        {
            string machineCode = Security.GetMachineCode();
            if (machineCode == null)
            {
                MessageBox.Show("系統內部錯誤，錯誤代碼：100001");
                Application.Exit();
            }
            else
            {
                RegResult regStatus = Security.GetOnlineStatus();
                Security.PostUnRegUserData(machineCode, this.GetUsers());
                e.Result = regStatus;
            }
        }

        private void bw_DoInitRegInfoChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void bw_DoInitRegInfoCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RegResult result = (RegResult)e.Result;
            if (result != null && result.GetResult())
            {
                btnLogin.Enabled = true;
           
                this.Text = $"帳號到期: {result.GetExpiredTime().ToString()}";
                CurrentErrorTimes = 0;
            }
            else if (result != null && !result.GetResult())
            {
                MessageBox.Show(result.GetMsg());
                Environment.Exit(0);
            }
            else
            {
                //检测容错的次数
                CurrentErrorTimes++;
                if (CurrentErrorTimes >= ErrorTimesLimit)
                {
                    Environment.Exit(0);
                }
            }
        }

        string GetUsers()
        {
            return "";
        }
        #endregion

        private void timeReg_Tick(object sender, EventArgs e)
        {
            if (bCheckOnline)
            {
                this.DoInitRegInfo();
            }
        }

        private void FrmEatZd_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
