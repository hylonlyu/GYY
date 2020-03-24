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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuaDan
{
    public partial class FrmGuaDan : Form
    {
        public bool bCheckOnline = false;
        private int CurrentErrorTimes = 0;
        private readonly int ErrorTimesLimit = 3;
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
        /// <summary>
        /// 二次下单时的Q赔率
        /// </summary>
        Dictionary<string, WPOdds> dicWpOdds = new Dictionary<string, WPOdds>();
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
            SetInstanceConfig();
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

            Config.Qgqzk = Util.Text2Double(txtQgqzk.Text.Trim());
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

            Config.Qgqzk3 = Util.Text2Double(txtQgqzk3.Text.Trim());
            Config.Qgdzk3 = Util.Text2Double(txtQgdzk3.Text.Trim());
            Config.Qzkps3 = Util.Text2Int(txtQzkps3.Text.Trim());

            Config.Wgqzk3 = Util.Text2Double(txtWgqzk3.Text.Trim());
            Config.Wgdzk3 = Util.Text2Double(txtWgdzk3.Text.Trim());
            Config.Wzkps3 = Util.Text2Int(txtWzkps3.Text.Trim());

            Config.Pgqzk3 = Util.Text2Double(txtPgqzk3.Text.Trim());
            Config.Pgdzk3 = Util.Text2Double(txtPgdzk3.Text.Trim());
            Config.Pzkps3 = Util.Text2Int(txtPzkps3.Text.Trim());

            Config.Qgqzk2 = Util.Text2Double(txtQgqzk2.Text.Trim());
            Config.Qgdzk2 = Util.Text2Double(txtQgdzk2.Text.Trim());

            Config.WPgqzk2 = Util.Text2Double(txtWPgqzk2.Text.Trim());
            Config.WPgdzk2 = Util.Text2Double(txtWPgdzk2.Text.Trim());

            Config.Wgqzk2 = Util.Text2Double(txtWgqzk2.Text.Trim());
            Config.Wgdzk2 = Util.Text2Double(txtWgdzk2.Text.Trim());

            Config.Pgqzk2 = Util.Text2Double(txtPgqzk2.Text.Trim());
            Config.Pgdzk2 = Util.Text2Double(txtPgdzk2.Text.Trim());

            Config.bGp = radGp.Checked;
            Config.bZk = radZk.Checked;
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


                txtQgqzk3.Text = Config.Qgqzk3.ToString();
                txtQgdzk3.Text = Config.Qgdzk3.ToString();
                txtQzkps3.Text = Config.Qzkps3.ToString();

                txtWgqzk3.Text = Config.Wgqzk3.ToString();
                txtWgdzk3.Text = Config.Wgdzk3.ToString();
                txtWzkps3.Text = Config.Wzkps3.ToString();

                txtPgqzk3.Text = Config.Pgqzk3.ToString();
                txtPgdzk3.Text = Config.Pgdzk3.ToString();
                txtPzkps3.Text = Config.Pzkps3.ToString();

                txtQgqzk2.Text = Config.Qgqzk2.ToString();
                txtQgdzk2.Text = Config.Qgdzk2.ToString();

                txtWPgqzk2.Text = Config.WPgqzk2.ToString();
                txtWPgdzk2.Text = Config.WPgdzk2.ToString();

                txtWgqzk2.Text = Config.Wgqzk2.ToString();
                txtWgdzk2.Text = Config.Wgdzk2.ToString();

                txtPgqzk2.Text = Config.Pgqzk2.ToString();
                txtPgdzk2.Text = Config.Pgdzk2.ToString();

                radZk.Checked = Config.bZk;
                radGp.Checked = Config.bGp;
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
            foreach (Control c in panType.Controls)
            {
                if (c is RadioButton)
                {
                    RadioButton rad = c as RadioButton;
                    if (rad.Checked)
                    {
                        type = rad.Text;
                        break;
                    }
                }
            }

            if (chkM1.Checked && !chkM2.Checked)
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


            Stack<string> stack = new Stack<string>();
            foreach (Control c in panHorse.Controls)
            {
                if (c is CheckBox)
                {

                    CheckBox cc = c as CheckBox;
                    if (cc.Checked)
                    {
                        stack.Push(cc.Text);
                    }
                }
            }
            while (stack.Count > 0)
            {
                horse += $"{stack.Pop()}_";
            }
            horse = horse.Trim("_".ToCharArray());
            if (radQ.Checked && radT.Checked)
            {
                horse = $"{txtT.Text.Trim()}*{horse}";
            }

            if (radEat.Checked)
            {
                bettype = "吃";
            }
            else if (radBet.Checked)
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

        private void SetInstanceConfig()
        {
            CCmemberInstance.Config = Config;
            CCmemberInstance2.Config = Config;
        }
        private void btn1_Click(object sender, EventArgs e)
        {
            SaveConfig();
            SetInstanceConfig();
            Button btnThis = sender as Button;
            btnThis.Enabled = false;

            ShowInfoMsg("一次下单开始");
            if (radGp.Checked)
            {
                DoBetGuPiao();
            }

            if (radZk.Checked)
            {
                DoBetZuoKong();
            }
            btnThis.Enabled = true;
            ShowInfoMsg("一次下单结束");

        }

        private void DoBetGuPiao()
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
                        DoBetQ(horse, member, playtype, Config.Qgdps);
                        break;
                    case "WP":
                        DoBetWP(horse, member, playtype, Config.WPgdps);
                        break;
                    case "W":
                        DoBetW(horse, member, playtype, Config.Wgdps);
                        break;
                    case "P":
                        DoBetP(horse, member, playtype, Config.Pgdps);
                        break;
                }
            }
            Task.WhenAll(lstTask).ContinueWith((t) =>
            {
                ShowInfoMsg("完成任务");
            });
        }

        private void DoBetZuoKong(bool next = false)
        {
            lstTask.Clear();
            if (!next)
            {
                //先获取一次Q的赔率
                CCmemberInstance.GetPeiData14();
                //获取wp的赔率
                dicWpOdds = CCmemberInstance.GetWPPeiData();
            }

            for (int i = 0; i < this.dgvDan.Rows.Count; i++)
            {
                string horse = dgvDan.Rows[i].Cells[0].Value.ToString();
                string type = dgvDan.Rows[i].Cells[1].Value.ToString();
                string member = dgvDan.Rows[i].Cells[2].Value.ToString();
                string playtype = dgvDan.Rows[i].Cells[3].Value.ToString();

                switch (type)
                {
                    case "Q":
                        DoBetQbyZK(horse, member, playtype,next);
                        break;
                    case "W":
                        DoBetWbyZK(horse, member, playtype, dicWpOdds,next);
                        break;
                    case "P":
                        DoBetPbyZK(horse, member, playtype, dicWpOdds,next);
                        break;
                }
            }
            Task.WhenAll(lstTask).ContinueWith((t) =>
            {
                ShowInfoMsg("完成任务");
            });
        }
        private void DoBetWbyZK(string horse, string member, string playtype, Dictionary<string, WPOdds> dicWpOdds,bool next = false)
        {
            if (!string.IsNullOrEmpty(horse))
            {
                string[] hs = horse.Split("_".ToCharArray());
                foreach (string h in hs)
                {
                    if (!string.IsNullOrEmpty(h))
                    {
                        if (dicWpOdds.ContainsKey(h))
                        {
                            double pei = dicWpOdds[h].Win;
                            if (pei != 0)
                            {
                                pei = pei > 30 ? 30 : pei;
                                int piao = (int)(Config.Wzkps / pei);
                                if(next)
                                {
                                    piao = (int)(Config.Wzkps3 / pei);
                                }
                                piao = Util.Closeto5(piao);
                                int times = next ? 3 : 1;
                                DoBetW(h, member, playtype, piao,times);
                            }
                            else
                            {
                                BetInfo info = new BetInfo
                                {
                                    horse = $"{h}",
                                    bettype = "W",
                                    playtype = playtype
                                };
                                AddBetFail(info, "赔率为0");
                            }
                        }
                        else
                        {
                            BetInfo info = new BetInfo
                            {
                                horse = $"{h}",
                                bettype = "W",
                                playtype = playtype
                            };
                            AddBetFail(info, "无此马赔率");
                        }
                    }
                }
            }

        }

        private void DoBetPbyZK(string horse, string member, string playtype, Dictionary<string, WPOdds> dicWpOdds,bool next = false)
        {
            if (!string.IsNullOrEmpty(horse))
            {
                string[] hs = horse.Split("_".ToCharArray());
                foreach (string h in hs)
                {
                    if (!string.IsNullOrEmpty(h))
                    {
                        if (dicWpOdds.ContainsKey(h))
                        {
                            double pei = dicWpOdds[h].Place;
                            if (pei != 0)
                            {
                                pei = pei > 10 ? 10 : pei;
                                int piao = (int)(Config.Pzkps / pei);
                                if(next)
                                {
                                    piao = (int)(Config.Pzkps3 / pei);
                                }
                                piao = Util.Closeto5(piao);
                                int times = next ? 3 : 1;
                                DoBetP(h, member, playtype, piao,times);
                            }
                            else
                            {
                                BetInfo info = new BetInfo
                                {
                                    horse = $"{h}",
                                    bettype = "P",
                                    playtype = playtype
                                };
                                AddBetFail(info, "赔率为0");
                            }
                        }
                        else
                        {
                            BetInfo info = new BetInfo
                            {
                                horse = $"{h}",
                                bettype = "P",
                                playtype = playtype
                            };
                            AddBetFail(info, "无此马赔率");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Q的做孔打法
        /// </summary>
        /// <param name="horse"></param>
        /// <param name="member"></param>
        /// <param name="playtype"></param>
        private void DoBetQbyZK(string horse, string member, string playtype,bool next = false)
        {
            List<Tuple<int, int>> lstHorses = GetHorsePair(horse);
            foreach (var item in lstHorses)
            {
                double pei = CCmemberInstance.GetQpei(item.Item1, item.Item2);
                if (pei != 0)
                {
                    pei = pei > 70 ? 70 : pei;
                    int piao = (int)(Config.Qzkps / pei);
                    if(next)
                    {
                        piao = (int)(Config.Qzkps3 / pei);
                    }
                    piao = piao >= 10 ? piao : 10;
                    int times = next ? 3 : 1;
                    DoBetQ($"{item.Item1}_{item.Item2}", member, playtype, piao,times);
                }
                else
                {
                    BetInfo info = new BetInfo { horse=$"{item.Item1}-{item.Item2}",
                    bettype="Q",playtype= playtype};
                    AddBetFail(info, "赔率为0");
                }
            }

        }

        /// <summary>
        /// 根据马的字符串，得到马对
        /// </summary>
        /// <param name="horse"></param>
        /// <returns></returns>
        private List<Tuple<int, int>> GetHorsePair(string horse)
        {
            List<Tuple<int, int>> lstHorses = new List<Tuple<int, int>>();
            Regex re = new Regex(@"\d+", RegexOptions.None);
            MatchCollection mc = re.Matches(horse);
            if (mc.Count > 0)
            {
                //拖
                if (horse.Contains("*"))
                {
                    int.TryParse(mc[0].Value, out int head);
                    for (int i = 1; i < mc.Count; i++)
                    {
                        int.TryParse(mc[i].Value, out int h);
                        lstHorses.Add(new Tuple<int, int>(head, h));
                    }
                }
                //交叉
                else
                {
                    for (int i = 0; i < mc.Count; i++)
                    {
                        int.TryParse(mc[i].Value, out int h1);
                        for (int j = i + 1; j < mc.Count; j++)
                        {
                            int.TryParse(mc[j].Value, out int h2);
                            lstHorses.Add(new Tuple<int, int>(h1, h2));
                        }
                    }
                }
            }
            return lstHorses;
        }
        private void GetHorses(string horses, out int h1, out int h2)
        {
            h1 = 0;
            h2 = 0;
            if (horses.Contains("_"))
            {
                string[] arrHorse = horses.Split("_".ToCharArray());
                int.TryParse(arrHorse[0], out h1);
                int.TryParse(arrHorse[1], out h2);
                if (h1 > h2)
                {
                    int tmp = h1;
                    h1 = h2;
                    h2 = tmp;
                }
            }
        }
        private void DoBetQ(string horse, string member, string playtype, int amount, int times = 1)
        {
            string qtype = horse.Contains("*") ? "1" : "0";
            RaceInfoItem item = new RaceInfoItem();
            item.Url = Config.MatchUrl;
            item.Horse = horse.Replace("*", "_");
            item.Race = Config.Race;
            item.Win = amount;
            item.Place = 0;
            if (times == 1)
            {
                item.Zhe = playtype.Equals("吃") ? Config.Qgqzk : Config.Qgdzk;
            }
            if (times == 2)
            {
                item.Zhe = playtype.Equals("吃") ? Config.Qgqzk2 : Config.Qgdzk2;
            }
            if (times == 3)
            {
                item.Zhe = playtype.Equals("吃") ? Config.Qgqzk3 : Config.Qgdzk3;
            }
            item.LWin = 700;
            item.LPlace = 0;
            item.Playtype = PlayType.Q;
            if (playtype.Equals("吃"))
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
        private void DoBetWP(string horse, string member, string playtype, int amount, int times = 1)
        {
            if (!string.IsNullOrEmpty(horse))
            {
                string[] hs = horse.Split("_".ToCharArray());
                foreach (string h in hs)
                {
                    if (!string.IsNullOrEmpty(h))
                    {
                        RaceInfoItem item = new RaceInfoItem();
                        item.Url = Config.MatchUrl;
                        item.Horse = h;
                        item.Race = Config.Race;
                        item.Win = amount;
                        item.Place = amount;
                        if (times == 1)
                        {
                            item.Zhe = playtype.Equals("吃") ? Config.WPgqzk : Config.WPgdzk;
                        }
                        if (times == 2)
                        {
                            item.Zhe = playtype.Equals("吃") ? Config.WPgqzk2 : Config.WPgdzk2;
                        }
                        item.LWin = 300;
                        item.LPlace = 100;
                        item.Date = CCmemberInstance.GetNow(Config.MatchUrl);
                        DoBetWP(member, playtype, item);
                    }

                }
            }
        }

        private void DoBetW(string horse, string member, string playtype, int amount, int times = 1)
        {
            if (!string.IsNullOrEmpty(horse))
            {
                string[] hs = horse.Split("_".ToCharArray());
                foreach (string h in hs)
                {
                    if (!string.IsNullOrEmpty(h))
                    {
                        RaceInfoItem item = new RaceInfoItem();
                        item.Url = Config.MatchUrl;
                        item.Horse = h;
                        item.Race = Config.Race;
                        item.Win = amount;
                        item.Place = 0;
                        if (times == 1)
                        {
                            item.Zhe = playtype.Equals("吃") ? Config.Wgqzk : Config.Wgdzk;
                        }
                        if (times == 2)
                        {
                            item.Zhe = playtype.Equals("吃") ? Config.Wgqzk2 : Config.Wgdzk2;
                        }
                        if (times == 3)
                        {
                            item.Zhe = playtype.Equals("吃") ? Config.Wgqzk3 : Config.Wgdzk3;
                        }
                        item.LWin = 300;
                        item.LPlace = 0;
                        item.Date = CCmemberInstance.GetNow(Config.MatchUrl);
                        DoBetWP(member, playtype, item);
                    }

                }
            }
        }

        private void DoBetP(string horse, string member, string playtype, int amount, int times = 1)
        {
            if (!string.IsNullOrEmpty(horse))
            {
                string[] hs = horse.Split("_".ToCharArray());
                foreach (string h in hs)
                {
                    if (!string.IsNullOrEmpty(h))
                    {
                        RaceInfoItem item = new RaceInfoItem();
                        item.Url = Config.MatchUrl;
                        item.Horse = h;
                        item.Race = Config.Race;
                        item.Win = 0;
                        item.Place = amount;
                        if (times == 1)
                        {
                            item.Zhe = playtype.Equals("吃") ? Config.Pgqzk : Config.Pgdzk;
                        }
                        if (times == 2)
                        {
                            item.Zhe = playtype.Equals("吃") ? Config.Pgqzk2 : Config.Pgdzk2;
                        }
                        if (times == 3)
                        {
                            item.Zhe = playtype.Equals("吃") ? Config.Pgqzk3 : Config.Pgdzk3;
                        }
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
                            if (!b)
                            {
                                BetInfo binfo = new BetInfo
                                {
                                    horse = $"{item.Horse}",
                                    bettype = "WP",
                                    playtype = item.Bettype.ToString()
                                };
                                AddBetFail(binfo, info.StrAnswer);
                            };
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
                            if (!b)
                            {
                                BetInfo binfo = new BetInfo
                                {
                                    horse = $"{item.Horse}",
                                    bettype = "WP",
                                    playtype = item.Bettype.ToString()
                                };
                                AddBetFail(binfo, info.StrAnswer);
                            };
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
                            if (!b)
                            {
                                BetInfo binfo = new BetInfo
                                {
                                    horse = $"{item.Horse}",
                                    bettype = "WP",
                                    playtype = item.Bettype.ToString()
                                };
                                AddBetFail(binfo, info.StrAnswer);
                            };
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
                            if (!b)
                            {
                                BetInfo binfo = new BetInfo
                                {
                                    horse = $"{item.Horse}",
                                    bettype = "WP",
                                    playtype = item.Bettype.ToString()
                                };
                                AddBetFail(binfo, info.StrAnswer);
                            };
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
                            if (!b)
                            {
                                BetInfo binfo = new BetInfo
                                {
                                    horse = $"{item.Horse}",
                                    bettype = "WP",
                                    playtype = item.Bettype.ToString()
                                };
                                AddBetFail(binfo, info.StrAnswer);
                            };

                            b = CCmemberInstance2.QiPiaoGua(item, out BetResultInfo info2);
                            ShowInfoMsg($"会员2{b}# {item.ToString()}");
                            if (!b)
                            {
                                BetInfo binfo = new BetInfo
                                {
                                    horse = $"{item.Horse}",
                                    bettype = "WP",
                                    playtype = item.Bettype.ToString()
                                };
                                AddBetFail(binfo, info.StrAnswer);
                            };
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

                            if (!b)
                            {
                                BetInfo binfo = new BetInfo
                                {
                                    horse = $"{item.Horse}",
                                    bettype = "WP",
                                    playtype = item.Bettype.ToString()
                                };
                                AddBetFail(binfo, info.StrAnswer);
                            };
                            b = CCmemberInstance2.XiaZhuGua(item, out BetResultInfo info2);
                            ShowInfoMsg($"会员2{b}# {item.ToString()}");
                            if (!b)
                            {
                                BetInfo binfo = new BetInfo
                                {
                                    horse = $"{item.Horse}",
                                    bettype = "WP",
                                    playtype = item.Bettype.ToString()
                                };
                                AddBetFail(binfo, info.StrAnswer);
                            };
                            return b;

                        });
                    }
                    break;
            }
        }
        private void DoBetQ(string member, RaceInfoItem item, string qtype)
        {
            switch (member)
            {
                case "1":
                    Task t1 = Task.Run<bool>(() =>
                    {
                        bool b = CCmemberInstance.QiPiaoGuaQ(item, out BetResultInfo info, qtype);
                        ShowInfoMsg($"会员1{b}# {item.ToString()}");
                        if (!b)
                        {
                            BetInfo binfo = new BetInfo
                            {
                                horse = $"{item.Horse}",
                                bettype = "Q",
                                playtype = item.Bettype.ToString()
                            };
                            AddBetFail(binfo, info.StrAnswer);
                        };
                        return b;
                    }

                    );
                    lstTask.Add(t1);
                    break;
                case "2":
                    Task t2 = Task.Run<bool>(() =>
                    {
                        bool b = CCmemberInstance2.QiPiaoGuaQ(item, out BetResultInfo info, qtype);
                        ShowInfoMsg($"会员2{b}# {item.ToString()}");
                        if (!b)
                        {
                            BetInfo binfo = new BetInfo
                            {
                                horse = $"{item.Horse}",
                                bettype = "Q",
                                playtype = item.Bettype.ToString()
                            };
                            AddBetFail(binfo, info.StrAnswer);
                        };
                        return b;
                    });
                    lstTask.Add(t2);
                    break;
                case "3":
                    Task t = Task.Run<bool>(() =>
                    {
                        bool b = CCmemberInstance.QiPiaoGuaQ(item, out BetResultInfo info, qtype);
                        ShowInfoMsg($"会员1{b}# {item.ToString()}");

                        if (!b)
                        {
                            BetInfo binfo = new BetInfo
                            {
                                horse = $"{item.Horse}",
                                bettype = "Q",
                                playtype = item.Bettype.ToString()
                            };
                            AddBetFail(binfo, info.StrAnswer);
                        };

                        b = CCmemberInstance2.QiPiaoGuaQ(item, out BetResultInfo info2, qtype);
                        if (!b)
                        {
                            BetInfo binfo = new BetInfo
                            {
                                horse = $"{item.Horse}",
                                bettype = "Q",
                                playtype = item.Bettype.ToString()
                            };
                            AddBetFail(binfo, info.StrAnswer);
                        };
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
            frmBrowser.Text = "1";
            frmBrowser.Show();
        }

        private void btnMatch_Click(object sender, EventArgs e)
        {
            FrmWebbrowser frmMatch = new FrmWebbrowser();
            frmMatch.Url = $"http://{CCmemberInstance.DoMain}/playerhk.jsp";
            frmMatch.CC = CCmemberInstance.cc;
            frmMatch.Text = "1";
            frmMatch.Show();
        }

        private void btnTrade2_Click(object sender, EventArgs e)
        {
            FrmWebbrowser frmBrowser = new FrmWebbrowser();
            frmBrowser.Url = $"http://{CCmemberInstance.DoMain}/new_history_live.jsp";
            frmBrowser.CC = CCmemberInstance2.cc;
            frmBrowser.Text = "2";
            frmBrowser.Show();
        }
         
        private void btnMatch2_Click(object sender, EventArgs e)
        {
            FrmWebbrowser frmMatch = new FrmWebbrowser();
            frmMatch.Url = $"http://{CCmemberInstance.DoMain}/playerhk.jsp";
            frmMatch.CC = CCmemberInstance2.cc;
            frmMatch.Text = "2";
            frmMatch.Show();
        }

        private void btn2_Click(object sender, EventArgs e)
        {
            SaveConfig();
            SetInstanceConfig();
            lstTask.Clear();
            Button btnThis = sender as Button;
            btnThis.Enabled = false;
            ShowInfoMsg("二次下单开始");
            if (radGp.Checked)
            {
                Ecxd(CCmemberInstance);
                Ecxd(CCmemberInstance2);
            }
            else if (radZk.Checked)
            {
                Zkecxd(CCmemberInstance);
                Zkecxd(CCmemberInstance2);
            }
            btnThis.Enabled = true;
            ShowInfoMsg("二次下单结束");
        }
        /// <summary>
        /// 做孔二次下单
        /// </summary>
        /// <param name="cc"></param>
        private void Zkecxd(CCMember cc)
        {
            object[] betInfo = cc.GetBetInfo(out string betString);
            Dictionary<string, BetInfo> dicBetinfo = GetBetPiao(betInfo);
            //删除所有的挂单
            cc.DeleteAllBetGuaDan(betString);
            cc.DeleteAllEatGuaDan(betString);
            cc.DeleteAllQBetGuaDan(betString);
            cc.DeleteAllQEatGuaDan(betString);
            /*
            //先获取一次Q的赔率
            cc.GetPeiData14();
            //获取wp的赔率
            dicWpOdds = cc.GetWPPeiData();
            */
            foreach (var item in dicBetinfo)
            {
                switch (item.Value.bettype)
                {
                    case "Q":
                        DoBetQbyZK2(item.Value, cc);
                        break;
                    case "W":
                        DoBetWbyZK2(item.Value, cc, dicWpOdds);
                        break;
                    case "P":
                        DoBetPbyZK2(item.Value, cc, dicWpOdds);
                        break;

                }
            }
        }

        private void DoBetQbyZK2(BetInfo item, CCMember cc)
        {
            string member = cc.Equals(CCmemberInstance) ? "1" : "2";
            //得到现在赔率下应该下多少票
            string[] horses = item.horse.Split("-".ToCharArray());
            if (horses.Length > 0)
            {
                int.TryParse(horses[0], out int h1);
                int.TryParse(horses[1], out int h2);
                double pei = cc.GetQpei(h1, h2);
                if (pei != 0)
                {
                    pei = pei > 70 ? 70 : pei;
                    int piao = (int)(Config.Qzkps / pei);
                    int gap = piao - item.piao;
                    if (Math.Abs(gap) >= 10)
                    {
                        string playtype = "";
                        if (gap > 0)
                        {
                            playtype = item.playtype;
                        }
                        else
                        {
                            playtype = item.playtype.Equals("吃") ? "赌" : "吃";
                        }
                        piao = Math.Abs(gap) >= 10 ? Math.Abs(gap) : 10;
                        DoBetQ(item.horse, member, playtype, piao, 2);
                    }
                    else
                    {
                        //DoBetQ(item.horse, member, item.playtype, item.piao, 2);
                    }
                }
                else
                {
                    AddBetFail(item, "赔率为0");
                }
            }

        }
        private void DoBetWbyZK2(BetInfo item, CCMember cc, Dictionary<string, WPOdds> dicWpOdds)
        {
            string member = cc.Equals(CCmemberInstance) ? "1" : "2";
            if (!string.IsNullOrEmpty(item.horse))
            {
                if (dicWpOdds.ContainsKey(item.horse))
                {
                    double pei = dicWpOdds[item.horse].Win;
                    if (pei != 0)
                    {
                        pei = pei > 30 ? 30 : pei;
                        int piao = (int)(Config.Wzkps / pei);
                        int gap = piao - item.piao;
                        if (Math.Abs(gap) >= 5)
                        {
                            string playtype = "";
                            if (gap > 0)
                            {
                                playtype = item.playtype;
                            }
                            else
                            {
                                playtype = item.playtype.Equals("吃") ? "赌" : "吃";
                            }
                            piao = Util.Closeto5(Math.Abs(gap));
                            DoBetW(item.horse, member, playtype, piao, 2);
                        }
                        else
                        {
                            //DoBetW(item.horse, member, item.playtype, item.piao, 2);
                        }

                    }
                    else
                    {
                        AddBetFail(item, "赔率为0");
                    }
                }
                else
                {
                    AddBetFail(item, "不包含此马赔率");
                }
            }
        }
        private void DoBetPbyZK2(BetInfo item, CCMember cc, Dictionary<string, WPOdds> dicWpOdds)
        {
            string member = cc.Equals(CCmemberInstance) ? "1" : "2";
            if (!string.IsNullOrEmpty(item.horse))
            {
                if (dicWpOdds.ContainsKey(item.horse))
                {
                    double pei = dicWpOdds[item.horse].Place;
                    if (pei != 0)
                    {
                        pei = pei > 10 ? 10 : pei;
                        int piao = (int)(Config.Pzkps / pei);
                        int gap = piao - item.piao;
                        if (Math.Abs(gap) >= 5)
                        {
                            string playtype = "";
                            if (gap > 0)
                            {
                                playtype = item.playtype;
                            }
                            else
                            {
                                playtype = item.playtype.Equals("吃") ? "赌" : "吃";
                            }
                            piao = Util.Closeto5(Math.Abs(gap));
                            DoBetP(item.horse, member, playtype, piao, 2);
                        }
                        else
                        {
                            //DoBetP(item.horse, member, item.playtype, item.piao, 2);
                        }
                    }
                    else
                    {
                        AddBetFail(item, "赔率为0");
                    }
                }
                else
                {
                    AddBetFail(item, "不包含此马赔率");
                }
            }
        }
        /// <summary>
        /// 根据下单情况，获取每只马获得了多少票
        /// </summary>
        /// <param name="betInfo"></param>
        /// <returns></returns>
        private Dictionary<string, BetInfo> GetBetPiao(object[] betInfo)
        {
            Dictionary<string, BetInfo> dicBetInfo = new Dictionary<string, BetInfo>();
            if (betInfo.Length > 0)
            {
                DataTable dt1 = betInfo[0] as DataTable;
                foreach (DataRow dr in dt1.Rows)
                {
                    string qian = dr["场"].ToString();
                    if (!string.IsNullOrEmpty(qian))
                    {
                        string h = dr["马"].ToString();
                        string win = dr["独赢"].ToString();
                        int.TryParse(win, out int iwin);
                        string place = dr["位置"].ToString();
                        int.TryParse(place, out int iplace);
                        string pt = dr["吃/赌"].ToString();
                        if (win.Equals("Q"))
                        {
                            BetInfo info = new BetInfo { horse = h, piao = iplace, playtype = pt, bettype = "Q" };
                            if (!dicBetInfo.ContainsKey(info.ToString()))
                            {
                                dicBetInfo.Add(info.ToString(), info);
                            }
                            else
                            {
                                dicBetInfo[info.ToString()].piao += info.piao;
                            }

                        }
                        else
                        {
                            if (iwin == 0)
                            {
                                BetInfo info = new BetInfo { horse = h, piao = iplace, playtype = pt, bettype = "P" };
                                if (!dicBetInfo.ContainsKey(info.ToString()))
                                {
                                    dicBetInfo.Add(info.ToString(), info);
                                }
                                else
                                {
                                    dicBetInfo[info.ToString()].piao += info.piao;
                                }

                            }
                            else
                            {
                                BetInfo info = new BetInfo { horse = h, piao = iwin, playtype = pt, bettype = "W" };
                                if (!dicBetInfo.ContainsKey(info.ToString()))
                                {
                                    dicBetInfo.Add(info.ToString(), info);
                                }
                                else
                                {
                                    dicBetInfo[info.ToString()].piao += info.piao;
                                }

                            }
                        }
                    }
                }
                DataTable dt2 = betInfo[1] as DataTable;
                foreach (DataRow dr in dt2.Rows)
                {
                    string qian = dr["场"].ToString();
                    if (!string.IsNullOrEmpty(qian))
                    {
                        string h = dr["马"].ToString();
                        string win = dr["独赢"].ToString();
                        int.TryParse(win, out int iwin);
                        string place = dr["位置"].ToString();
                        int.TryParse(place, out int iplace);
                        string pt = dr["吃/赌"].ToString();
                        if (win.Equals("Q"))
                        {
                            BetInfo info = new BetInfo { horse = h, piao = 0, playtype = pt, bettype = "Q" };
                            if (!dicBetInfo.ContainsKey(info.ToString()))
                            {
                                dicBetInfo.Add(info.ToString(), info);
                            }
                        }
                        else
                        {
                            if (iwin == 0)
                            {
                                BetInfo info = new BetInfo { horse = h, piao = 0, playtype = pt, bettype = "P" };
                                if (!dicBetInfo.ContainsKey(info.ToString()))
                                {
                                    dicBetInfo.Add(info.ToString(), info);
                                }
                            }
                            else
                            {

                                BetInfo info = new BetInfo { horse = h, piao = 0, playtype = pt, bettype = "W" };
                                if (!dicBetInfo.ContainsKey(info.ToString()))
                                {
                                    dicBetInfo.Add(info.ToString(), info);
                                }
                            }
                        }
                    }
                }
            }
            return dicBetInfo;
        }
        /// <summary>
        /// 二次下单
        /// </summary>
        private void Ecxd(CCMember cc)
        {
            if (radGp.Checked)
            {
                string member = cc.Equals(CCmemberInstance) ? "1" : "2";
                object[] betInfo = cc.GetBetInfo(out string betString);
                //删除所有的挂单
                cc.DeleteAllBetGuaDan(betString);
                cc.DeleteAllEatGuaDan(betString);
                cc.DeleteAllQBetGuaDan(betString);
                cc.DeleteAllQEatGuaDan(betString);
                //根据挂单改折扣后重新挂
                if (betInfo != null)
                {
                    DataTable dtGua = betInfo[1] as DataTable;
                    if (dtGua != null)
                    {
                        foreach (DataRow dr in dtGua.Rows)
                        {

                            string horse = dr["马"].ToString();
                            string win = dr["独赢"].ToString();
                            string place = dr["位置"].ToString();
                            int.TryParse(win, out int iwin);
                            int.TryParse(place, out int iplace);
                            string playtype = dr["吃/赌"].ToString();
                            //Q
                            if (win.Equals("Q"))
                            {
                                DoBetQ(horse, member, playtype, iplace, 2);
                            }
                            else
                            {
                                if (iwin == iplace)
                                {
                                    DoBetWP(horse, member, playtype, iwin, 2);
                                }
                                else if (iwin == 0)
                                {
                                    DoBetP(horse, member, playtype, iplace, 2);
                                }
                                else
                                {
                                    DoBetW(horse, member, playtype, iwin, 2);
                                }
                            }
                        }
                    }
                }
            }

        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            SaveConfig();
            SetInstanceConfig();

            Button btnThis = sender as Button;
            btnThis.Enabled = false;
            ShowInfoMsg("再次下单开始");
            if (radZk.Checked)
            {
                DoBetZuoKong(true);
            }
            btnThis.Enabled = true;
            ShowInfoMsg("再次下单结束");
        }

        private void AddBetFail(BetInfo info,string reason)
        {
            if(this.InvokeRequired)
            {
                this.Invoke(new Action<BetInfo,string>(AddBetFail), info,reason);
            }
            else
            {
                this.dgvBetResult.Rows.Add(new string[] {DateTime.Now.ToLongTimeString(),
                Config.MatchUrl,Config.Race,info.horse,info.bettype,info.playtype,reason});
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            dgvDan.Rows.Clear();
        }

        private void cobMatch_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void cobRace_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void btnLockOdds_Click(object sender, EventArgs e)
        {
            Button btnThis = sender as Button;
            btnThis.Enabled = false;
            LockOdds(CCmemberInstance);
            LockOdds(CCmemberInstance2);
            btnThis.Enabled = true;
        }

        private void LockOdds(CCMember cc)
        {
            //先获取一次Q的赔率
            Dictionary<string,string[,]> dicQodds =  cc.GetPeiData14();
            //获取wp的赔率
            dicWpOdds = cc.GetWPPeiData();
            string member = cc.Equals(CCmemberInstance) ? "1" : "2";
            if(dicQodds!=null && dicQodds["Q"] != null)
            {
                ShowInfoMsg($"会员{member}获取Q赔率成功");
            }
            else
            {
                ShowInfoMsg($"会员{member}获取Q赔率失败");
            }

            if(dicWpOdds!=null && dicWpOdds.Count>0)
            {
                ShowInfoMsg($"会员{member}获取WP赔率成功");
            }
            else
            {
                ShowInfoMsg($"会员{member}获取WP赔率失败");
            }
        }

        private void timeReg_Tick(object sender, EventArgs e)
        {
            if (bCheckOnline)
            {
                this.DoInitRegInfo();
            }
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
                Security.PostUnRegUserData(machineCode, "");
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
                //btnLogin.Enabled = true;
                //btnLogin2.Enabled = true;
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
                    //TextFile.WriteFile("Log.bin", $"失去和服务器联系{CurrentErrorTimes}次");
                    Environment.Exit(0);
                }
            }
        }


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

        private void FrmGuaDan_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }
    }

    class BetInfo
    {
        public string horse;
        public int piao;
        public string playtype;
        public string bettype;

        public override string ToString()
        {
            return $"{horse}-{playtype}-{bettype}";
        }
    }

  
         

}
