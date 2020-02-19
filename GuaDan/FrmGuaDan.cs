using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuaDan
{
    public partial class FrmGuaDan : Form
    {
        private CCMember cCmemberInstance;
        public CCMember CCmemberInstance
        {
            get
            {
                if (cCmemberInstance == null)
                {
                    cCmemberInstance = new CCMember();
                }
                return cCmemberInstance;
            }
            set
            {
                cCmemberInstance = value;
            }
        }

        private CCMember cCmemberInstance2;
        public CCMember CCmemberInstance2
        {
            get
            {
                if (cCmemberInstance2 == null)
                {
                    cCmemberInstance2 = new CCMember();
                }
                return cCmemberInstance2;
            }
            set
            {
                cCmemberInstance2 = value;
            }
        }

        UserInfo uInfo = new UserInfo();
        UserInfo uInfo2 = new UserInfo();
        private List<Task> lstTask = new List<Task>();
        private GdConfig Config = new GdConfig();
        public FrmGuaDan()
        {
            InitializeComponent();
            CCmemberInstance.OnLoginOk += CCmemberInstance_OnLoginOk;
            CCmemberInstance.OnLoginFail += CCmemberInstance_OnLoginFail;
            CCmemberInstance.OnLogout += CCmemberInstance_OnLogout;

            CCmemberInstance2.OnLoginOk += CCmemberInstance2_OnLoginOk;
            CCmemberInstance2.OnLoginFail += CCmemberInstance2_OnLoginFail;
            CCmemberInstance2.OnLogout += CCmemberInstance2_OnLogout;
        }

        private void CCmemberInstance2_OnLogout()
        {
            Thread.Sleep(5000);
            ShowInfoMsg("账号2退出，重新登陆");
            new Thread(new ParameterizedThreadStart(DoLogin2)).Start(uInfo2);
        }

        private void CCmemberInstance2_OnLoginFail()
        {
            Thread.Sleep(5000);

            ShowInfoMsg("账号2登陆失败，重新登陆");
            new Thread(new ParameterizedThreadStart(DoLogin2)).Start(uInfo2);
        }

        private void CCmemberInstance2_OnLoginOk()
        {
            ShowInfoMsg("账号2登陆成功");
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

        private void DoLogin(object obj)
        {
            UserInfo uInfo = obj as UserInfo;
            ShowInfoMsg($"{uInfo.CUserName}正在登陆");
            Hashtable ht = CCmemberInstance.DoLogin(uInfo.CUrl, uInfo);
            if (ht != null)
            {
                BindMatchList(ht[4] as DataTable);
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
                    //DataTable TempDT = dtMatch.Clone();
                    //DataRow[] drs = dtMatch.Select("tip like '%3H%'");
                    //foreach (DataRow dr in drs)
                    //{
                    //    TempDT.ImportRow(dr);
                    //}
                    //cobMatch.DataSource = TempDT;

                    cobMatch.DataSource = dtMatch;
                    cobMatch.DisplayMember = "tip";
                    cobMatch.ValueMember = "url";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }


        private void DoLogin2(object obj)
        {
            UserInfo uInfo2 = obj as UserInfo;
            ShowInfoMsg($"{uInfo2.CUserName}正在登陆");
            Hashtable ht = CCmemberInstance2.DoLogin(uInfo2.CUrl, uInfo2);
        }

        private void CCmemberInstance_OnLogout()
        {
            Thread.Sleep(5000);
            ShowInfoMsg("账号1退出，重新登陆");
            new Thread(new ParameterizedThreadStart(DoLogin)).Start(uInfo);
        }

        private void CCmemberInstance_OnLoginFail()
        {
            Thread.Sleep(5000);

            ShowInfoMsg("账号1登陆失败，重新登陆");
            new Thread(new ParameterizedThreadStart(DoLogin)).Start(uInfo);
        }

        private void CCmemberInstance_OnLoginOk()
        {
            ShowInfoMsg("账号1登陆成功");
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveConfig();
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
            Config.Accout2 = txtAccount2.Text.Trim();
            Config.Pwd2 = txtPwd2.Text.Trim();
            Config.Pin2 = txtPin2.Text.Trim();

            Config.Qgqzk =Util.Text2Double(txtQgqzk.Text.Trim());
            Config.Qgdzk = Util.Text2Double(txtQgdzk.Text.Trim());
            Config.Qzkps = Util.Text2Int(txtQzkps.Text.Trim());
            Config.Qgdps = Util.Text2Int(txtQgdps.Text.Trim());

            Config.WPgqzk = Util.Text2Double(txtWPgqzk.Text.Trim());
            Config.WPgdzk = Util.Text2Double(txtWPgdzk.Text.Trim());
            Config.WPgdps = Util.Text2Int(txtWPgdps.Text.Trim());

            Config.Wgqzk = Util.Text2Double(txtWgqzk.Text.Trim());
            Config.Wgdzk = Util.Text2Double(txtWgdzk.Text.Trim());
            Config.Wzkps = Util.Text2Int(txtWzkps.Text.Trim());
            Config.Wgdps = Util.Text2Int(txtWgdps.Text.Trim());

            Config.Pgqzk = Util.Text2Double(txtPgqzk.Text.Trim());
            Config.Pgdzk = Util.Text2Double(txtPgdzk.Text.Trim());
            Config.Pzkps = Util.Text2Int(txtPzkps.Text.Trim());
            Config.Pgdps = Util.Text2Int(txtPgdps.Text.Trim());

            Config.Qgqzk2 = Util.Text2Double(txtQgqzk2.Text.Trim());
            Config.Qgdzk2 = Util.Text2Double(txtQgdzk2.Text.Trim());

            Config.WPgqzk2 = Util.Text2Double(txtWPgqzk2.Text.Trim());
            Config.WPgdzk2 = Util.Text2Double(txtWPgdzk2.Text.Trim());

            Config.Wgqzk2 = Util.Text2Double(txtWgqzk2.Text.Trim());
            Config.Wgdzk2 = Util.Text2Double(txtWgdzk2.Text.Trim());

            Config.Pgqzk2 = Util.Text2Double(txtPgqzk2.Text.Trim());
            Config.Pgdzk2 = Util.Text2Double(txtPgdzk2.Text.Trim());

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
                        Config = bf.Deserialize(fs) as GdConfig;
                    }
                }
                catch (Exception ex)
                {
                    Config = new GdConfig();
                }

            }
            else
            {
                Config = new GdConfig();
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

                txtAccount2.Text = Config.Accout2;
                txtPwd2.Text = Config.Pwd2;
                txtPin2.Text = Config.Pin2;


                txtQgqzk.Text = Config.Qgqzk.ToString();
                txtQgdzk.Text = Config.Qgdzk.ToString();
                txtQzkps.Text = Config.Qzkps.ToString();
                txtQgdps.Text = Config.Qgdps.ToString();

                txtWPgqzk.Text = Config.WPgqzk.ToString();
                txtWPgdzk.Text = Config.WPgdzk.ToString();
                txtWPgdps.Text = Config.WPgdps.ToString();

                txtWgqzk.Text = Config.Wgqzk.ToString();
                txtWgdzk.Text = Config.Wgdzk.ToString();
                txtWzkps.Text = Config.Wzkps.ToString();
                txtWgdps.Text = Config.Wgdps.ToString();

                txtPgqzk.Text = Config.Pgqzk.ToString();
                txtPgdzk.Text = Config.Pgdzk.ToString();
                txtPzkps.Text = Config.Pzkps.ToString();
                txtPgdps.Text = Config.Pgdps.ToString();

                txtQgqzk2.Text = Config.Qgqzk2.ToString();
                txtQgdzk2.Text = Config.Qgdzk2.ToString();

                txtWPgqzk2.Text = Config.WPgqzk2.ToString();
                txtWPgdzk2.Text = Config.WPgdzk2.ToString();

                txtWgqzk2.Text = Config.Wgqzk2.ToString();
                txtWgdzk2.Text = Config.Wgdzk2.ToString();

                txtPgqzk2.Text = Config.Pgqzk2.ToString();
                txtPgdzk2.Text = Config.Pgdzk2.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        private void FrmGuaDan_Load(object sender, EventArgs e)
        {
            LoadConfig();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string type = "";
            string horse = "";
            string member = "";
            string bettype = "";
            foreach(Control c in panType.Controls)
            {
                if(c is RadioButton)
                {
                    RadioButton rad = c as RadioButton;
                    if(rad.Checked)
                    {
                        type = rad.Text;
                        break;
                    }
                }
            }

            if(chkM1.Checked && !chkM2.Checked)
            {
                member = "1";
            }
            if (!chkM1.Checked && chkM2.Checked)
            {
                member = "2";
            }

            if (chkM1.Checked && chkM2.Checked)
            {
                member = "3";
            }

            foreach(Control c in panHorse.Controls)
            {
                if(c is CheckBox)
                {
                    CheckBox cc = c as CheckBox;
                    if(cc.Checked)
                    {
                        horse += $"{cc.Text}_";
                    }
                }
            }
            horse = horse.Trim("_".ToCharArray());
            if (radQ.Checked && radT.Checked)
            {
                horse = $"{txtT.Text.Trim()}*{horse}";
            }

            if(radEat.Checked )
            {
                bettype = "吃";
            }
            else if(radBet.Checked)
            {
                bettype = "赌";
            }
            dgvDan.Rows.Add(new object[] { horse, type, member, bettype });
        }

        private string GetSelectedHorse()
        {
            string strRet = null;
            return strRet;
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            lstTask.Clear();
            for (int i = 0; i < this.dgvDan.Rows.Count; i++)
            {
                string horse = dgvDan.Rows[i].Cells[0].Value.ToString();
                string type = dgvDan.Rows[i].Cells[1].Value.ToString();
                string member = dgvDan.Rows[i].Cells[2].Value.ToString();
                string playtype = dgvDan.Rows[i].Cells[3].Value.ToString();
                switch (type)
                {
                    case "Q":
                        DoBetQ(horse, member, playtype);
                        break;
                    case "WP":
                        DoBetWP(horse,member,playtype);
                        break;
                    case "W":
                        DoBetW(horse, member, playtype);
                        break;
                    case "P":
                        DoBetP(horse, member, playtype);
                        break;
                }
            }
            Task.WhenAll(lstTask).ContinueWith((t) =>
            {
                ShowInfoMsg("完成任务");
            });
        }

        private void DoBetQ(string horse, string member, string playtype)
        {
            string qtype = horse.Contains("*") ? "1" : "0";
            RaceInfoItem item = new RaceInfoItem();
            item.Url = Config.MatchUrl;
            item.Horse = horse.Replace("*","_");
            item.Race = Config.Race;
            item.Win = Config.Qgdps;
            item.Place = 0;
            item.Zhe = playtype.Equals("吃") ? Config.Qgqzk : Config.Qgdzk;
            item.LWin = 700;
            item.LPlace = 0;
            item.Playtype = PlayType.Q;
            if(playtype.Equals("吃"))
            {
                item.Bettype = BetType.EAT;
            }
            else
            {
                item.Bettype = BetType.BET;
            }
            
            item.Date = CCmemberInstance.GetNow(Config.MatchUrl);

            DoBetQ(member, item, qtype);
        }
        private void DoBetWP(string horse,string member,string playtype)
        {
            if(!string.IsNullOrEmpty(horse))
            {
                string[] hs = horse.Split(",".ToCharArray());
                foreach(string h in hs)
                {
                    if(!string.IsNullOrEmpty(h))
                    {
                        RaceInfoItem item = new RaceInfoItem();
                        item.Url = Config.MatchUrl;
                        item.Horse = h;
                        item.Race = Config.Race;
                        item.Win = Config.WPgdps;
                        item.Place = Config.WPgdps;
                        item.Zhe = playtype.Equals("吃") ? Config.WPgqzk : Config.WPgdzk;
                        item.LWin = 300;
                        item.LPlace = 100;
                        item.Date = CCmemberInstance.GetNow(Config.MatchUrl);
                        DoBetWP(member, playtype, item);
                    }

                }
            }
        }

        private void DoBetW(string horse, string member, string playtype)
        {
            if (!string.IsNullOrEmpty(horse))
            {
                string[] hs = horse.Split(",".ToCharArray());
                foreach (string h in hs)
                {
                    if (!string.IsNullOrEmpty(h))
                    {
                        RaceInfoItem item = new RaceInfoItem();
                        item.Url = Config.MatchUrl;
                        item.Horse = h;
                        item.Race = Config.Race;
                        item.Win = Config.Wgdps;
                        item.Place = 0;
                        item.Zhe = playtype.Equals("吃") ? Config.Wgqzk : Config.Wgdzk;
                        item.LWin = 300;
                        item.LPlace = 0;
                        item.Date = CCmemberInstance.GetNow(Config.MatchUrl);
                        DoBetWP(member, playtype, item);
                    }

                }
            }
        }

        private void DoBetP(string horse, string member, string playtype)
        {
            if (!string.IsNullOrEmpty(horse))
            {
                string[] hs = horse.Split(",".ToCharArray());
                foreach (string h in hs)
                {
                    if (!string.IsNullOrEmpty(h))
                    {
                        RaceInfoItem item = new RaceInfoItem();
                        item.Url = Config.MatchUrl;
                        item.Horse = h;
                        item.Race = Config.Race;
                        item.Win = 0;
                        item.Place = Config.Pgdps;
                        item.Zhe = playtype.Equals("吃") ? Config.Pgqzk : Config.Pgdzk;
                        item.LWin = 0;
                        item.LPlace = 100;
                        item.Date = CCmemberInstance.GetNow(Config.MatchUrl);
                        DoBetWP(member, playtype, item);
                    }
                }
            }
        }


        private void DoBetWP(string member, string playtype, RaceInfoItem item)
        {
            switch (member)
            {
                case "1":
                    if (playtype.Equals("吃"))
                    {
                        Task t = Task.Run<bool>(() =>
                        {
                            bool b = CCmemberInstance.QiPiaoGua(item, out BetResultInfo info);
                            ShowInfoMsg($"会员1{b}# {item.ToString()}");
                            return b;
                        });
                        lstTask.Add(t);
                    }
                    else
                    {
                        Task t = Task.Run<bool>(() =>
                        {
                            bool b = CCmemberInstance.XiaZhuGua(item, out BetResultInfo info);
                            ShowInfoMsg($"会员1{b}# {item.ToString()}");
                            return b;

                        });

                    }
                    break;
                case "2":
                    if (playtype.Equals("吃"))
                    {
                        Task t = Task.Run<bool>(() =>
                        {
                            bool b = CCmemberInstance2.QiPiaoGua(item, out BetResultInfo info);
                            ShowInfoMsg($"会员2{b}# {item.ToString()}");
                            return b;
                        });
                        lstTask.Add(t);
                    }
                    else
                    {
                        Task t = Task.Run<bool>(() =>
                        {
                            bool b = CCmemberInstance2.XiaZhuGua(item, out BetResultInfo info);
                            ShowInfoMsg($"会员2{b}# {item.ToString()}");
                            return b;

                        });

                    }
                    break;
                case "3":
                    if (playtype.Equals("吃"))
                    {
                        Task t = Task.Run<bool>(() =>
                        {
                            bool b = CCmemberInstance.QiPiaoGua(item, out BetResultInfo info);
                            ShowInfoMsg($"会员1{b}# {item.ToString()}");

                            b = CCmemberInstance2.QiPiaoGua(item, out BetResultInfo info2);
                            ShowInfoMsg($"会员2{b}# {item.ToString()}");
                            return b;
                        });
                        lstTask.Add(t);
                    }
                    else
                    {
                        Task t = Task.Run<bool>(() =>
                        {
                            bool b = CCmemberInstance.XiaZhuGua(item, out BetResultInfo info);
                            ShowInfoMsg($"会员1{b}# {item.ToString()}");

                            b = CCmemberInstance2.XiaZhuGua(item, out BetResultInfo info2);
                            ShowInfoMsg($"会员2{b}# {item.ToString()}");
                            return b;

                        });
                    }
                    break;
            }
        }
        private void DoBetQ(string member,  RaceInfoItem item,string qtype)
        {
            switch (member)
            {
                case "1":
                    Task t1 = Task.Run<bool>(() =>
                    {
                        bool b = CCmemberInstance.QiPiaoGuaQ(item, out BetResultInfo info,qtype);
                        ShowInfoMsg($"会员1{b}# {item.ToString()}");
                        return b;
                    });
                    lstTask.Add(t1);
                    break;
                case "2":
                    Task t2 = Task.Run<bool>(() =>
                    {
                        bool b = CCmemberInstance2.QiPiaoGuaQ(item, out BetResultInfo info,qtype);
                        ShowInfoMsg($"会员2{b}# {item.ToString()}");
                        return b;
                    });
                    lstTask.Add(t2);
                    break;
                case "3":
                    Task t = Task.Run<bool>(() =>
                    {
                        bool b = CCmemberInstance.QiPiaoGuaQ(item, out BetResultInfo info,qtype);
                        ShowInfoMsg($"会员1{b}# {item.ToString()}");

                        b = CCmemberInstance2.QiPiaoGuaQ(item, out BetResultInfo info2,qtype);
                        ShowInfoMsg($"会员2{b}# {item.ToString()}");
                        return b;
                    });
                    lstTask.Add(t);
                    break;
            }
        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            uInfo.CUserName = txtAccount.Text.Trim();
            uInfo.CPassword = txtPwd.Text.Trim();
            uInfo.CPin = txtPin.Text.Trim();
            uInfo.CUrl = txtUrl.Text.Trim();
            new Thread(new ParameterizedThreadStart(DoLogin)).Start(uInfo);
        }

        private void btnLogin2_Click(object sender, EventArgs e)
        {
            uInfo2.CUserName = txtAccount2.Text.Trim();
            uInfo2.CPassword = txtPwd2.Text.Trim();
            uInfo2.CPin = txtPin2.Text.Trim();
            uInfo2.CUrl = txtUrl.Text.Trim();
            new Thread(new ParameterizedThreadStart(DoLogin2)).Start(uInfo2);
        }

        private void btnTrade_Click(object sender, EventArgs e)
        {
            FrmWebbrowser frmBrowser = new FrmWebbrowser();
            frmBrowser.Url = $"http://{CCmemberInstance.DoMain}/new_history_live.jsp";
            frmBrowser.CC = CCmemberInstance.cc;
            frmBrowser.Show();
        }

        private void btnMatch_Click(object sender, EventArgs e)
        {
            FrmWebbrowser frmMatch = new FrmWebbrowser();
            frmMatch.Url = $"http://{CCmemberInstance.DoMain}/playerhk.jsp";
            frmMatch.CC = CCmemberInstance.cc;
            frmMatch.Show();
        }

        private void btnTrade2_Click(object sender, EventArgs e)
        {
            FrmWebbrowser frmBrowser = new FrmWebbrowser();
            frmBrowser.Url = $"http://{CCmemberInstance.DoMain}/new_history_live.jsp";
            frmBrowser.CC = CCmemberInstance2.cc;
            frmBrowser.Show();
        }

        private void btnMatch2_Click(object sender, EventArgs e)
        {
            FrmWebbrowser frmMatch = new FrmWebbrowser();
            frmMatch.Url = $"http://{CCmemberInstance.DoMain}/playerhk.jsp";
            frmMatch.CC = CCmemberInstance2.cc;
            frmMatch.Show();
        }
    }
}
