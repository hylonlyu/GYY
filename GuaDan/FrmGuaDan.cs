using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        private GtConfig Config = new GtConfig();
        /// <summary>
        /// 二次下单时的Q赔率
        /// </summary>
        Dictionary<string, WPOdds> dicWpOdds = new Dictionary<string, WPOdds>();

        // TCP服务器相关字段
        private TcpServer tcpServer;
        private Dictionary<string, TcpServer.ClientInfo> connectedClients = new Dictionary<string, TcpServer.ClientInfo>();
        public FrmGuaDan()
        {
            InitializeComponent();

            // 初始化TCP服务器UI状态
            UpdateServerStatus(false);

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
                    //string[] FilterMatch = new string[] { "香港","澳洲" };
                    string[] FilterMatch = new string[] { "香港" };
                    DataTable TempDT = dtMatch.Clone();
                    foreach (DataRow dr in dtMatch.Rows)
                    {
                        foreach (var item in FilterMatch)
                        {
                            if (dr["tip"].ToString().StartsWith(item))
                            {
                                TempDT.ImportRow(dr);
                                break;
                            }
                        }

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
            // 保存 panel4 和 lstIP 的额外设置
            SaveExtraSettings();
        }

        #region Config
        /* private void SaveConfig()
        {
            if (!string.IsNullOrEmpty(cobMatch.Text.Trim()))
            {
                Config.MatchCombol = $"{cobMatch.Text.Trim()}|{cobMatch.SelectedValue.ToString().Trim()}";
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
        */
        private void SaveConfig() {

            if (!string.IsNullOrEmpty(cobMatch.Text.Trim()))
            {
                Config.MatchCombol = $"{cobMatch.Text.Trim()}|{cobMatch.SelectedValue.ToString().Trim()}";
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

            Config.ZheQ =Util.Text2Double(txtZheQ.Text.Trim());
            Config.LlimQ =Util.Text2Int(txtLlimQ.Text.Trim());
            Config.RLimQ = Util.Text2Int(txtRlimQ.Text.Trim());

            Config.ZheQp =Util.Text2Double(txtZheQp.Text.Trim());
            Config.LlimQp =Util.Text2Int(txtLlimQp.Text.Trim());
            Config.RLimQp =Util.Text2Int(txtRlimQp.Text.Trim());

            Config.Gdz = Util.Text2Int(txtGdz.Text.Trim());

            // 保存TCP服务器端口配置
            Config.TcpServerPort = Util.Text2Int(txtPort.Text.Trim());

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
                        Config = bf.Deserialize(fs) as GtConfig;
                    }
                }
                catch (Exception ex)
                {
                    Config = new GtConfig();
                }

            }
            else
            {
                Config = new GtConfig();
            }
            SetConfig();
            // 加载 panel4 和 lstIP 的额外设置
            LoadExtraSettings();
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

                txtZheQ.Text = Config.ZheQ.ToString();
                txtLlimQ.Text = Config.LlimQ.ToString();
                txtRlimQ.Text = Config.RLimQ.ToString();

                txtZheQp.Text = Config.ZheQp.ToString();
                txtLlimQp.Text = Config.LlimQp.ToString();
                txtRlimQp.Text = Config.RLimQp.ToString();

                txtGdz.Text = Config.Gdz.ToString();

                // 加载TCP服务器端口配置
                txtPort.Text = Config.TcpServerPort.ToString();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        [Serializable]
        private class FrmExtraSetting
        {
            public Dictionary<string, bool> ControlChecks = new Dictionary<string, bool>();
            public Dictionary<string, string> ControlTexts = new Dictionary<string, string>();
            public List<string> IPs = new List<string>();
        }

        private void SaveExtraSettings()
        {
            try
            {
                if (!Directory.Exists("setting")) Directory.CreateDirectory("setting");
                FrmExtraSetting extra = new FrmExtraSetting();
                // 保存 panel4 中的选项状态和文本
                if (this.panel4 != null)
                {
                    foreach (Control c in panel4.Controls)
                    {
                        if (c is CheckBox cb)
                        {
                            extra.ControlChecks[c.Name] = cb.Checked;
                        }
                        else if (c is RadioButton rb)
                        {
                            extra.ControlChecks[c.Name] = rb.Checked;
                        }
                        else if (c is TextBox tb)
                        {
                            extra.ControlTexts[c.Name] = tb.Text;
                        }
                    }
                }

            

                string file = "setting\\FrmGuaDan_extra.bin";
                using (FileStream fs = new FileStream(file, FileMode.Create))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs, extra);
                }
                ShowInfoMsg("已保存额外设置");
            }
            catch (Exception ex)
            {
                ShowInfoMsg($"保存额外设置失败: {ex.Message}");
            }
        }

        private void LoadExtraSettings()
        {
            try
            {
                string file = "setting\\FrmGuaDan_extra.bin";
                if (!File.Exists(file)) return;
                FrmExtraSetting extra = null;
                using (FileStream fs = new FileStream(file, FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    extra = bf.Deserialize(fs) as FrmExtraSetting;
                }
                if (extra == null) return;

                // 恢复 panel4 的状态
                if (this.panel4 != null)
                {
                    foreach (Control c in panel4.Controls)
                    {
                        if (c is CheckBox cb)
                        {
                            if (extra.ControlChecks.ContainsKey(c.Name))
                            {
                                cb.Checked = extra.ControlChecks[c.Name];
                            }
                        }
                        else if (c is RadioButton rb)
                        {
                            if (extra.ControlChecks.ContainsKey(c.Name))
                            {
                                rb.Checked = extra.ControlChecks[c.Name];
                            }
                        }
                        else if (c is TextBox tb)
                        {
                            if (extra.ControlTexts.ContainsKey(c.Name))
                            {
                                tb.Text = extra.ControlTexts[c.Name];
                            }
                        }
                    }
                }

          
            }
            catch (Exception ex)
            {
                ShowInfoMsg($"加载额外设置失败: {ex.Message}");
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
            //    switch (type)
            //    {
            //        case "Q":
            //            DoBetQ(horse, member, playtype, Config.Qgdps,"");
            //            break;
            //        case "WP":
            //            DoBetWP(horse, member, playtype, Config.WPgdps);
            //            break;
            //        case "W":
            //            DoBetW(horse, member, playtype, Config.Wgdps,"");
            //            break;
            //        case "P":
            //            DoBetP(horse, member, playtype, Config.Pgdps,"");
            //            break;
            //    }
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
            /*
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
                                DoBetW(h, member, playtype, piao,pei.ToString(),times);
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
 */
        }

        private void DoBetPbyZK(string horse, string member, string playtype, Dictionary<string, WPOdds> dicWpOdds,bool next = false)
        {
            /*
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
                                DoBetP(h, member, playtype, piao,pei.ToString(),times);
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
           */
        }
        /// <summary>
        /// Q的做孔打法
        /// </summary>
        /// <param name="horse"></param>
        /// <param name="member"></param>
        /// <param name="playtype"></param>
        private void DoBetQbyZK(string horse, string member, string playtype,bool next = false)
        {
            /*
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
                    DoBetQ($"{item.Item1}_{item.Item2}", member, playtype, piao,pei.ToString(),times);
                }
                else
                {
                    BetInfo info = new BetInfo { horse=$"{item.Item1}-{item.Item2}",
                    bettype="Q",playtype= call };
                    AddBetFail(info, "赔率为0");
                }
            }
*/
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
        private void DoBetQ(string horse, string member, string playtype, int amount,string odds, int times = 1)
        {
            /*
            string qtype = horse.Contains("*") ? "1" : "0";
            RaceInfoItem item = new RaceInfoItem();
            item.Url = Config.MatchUrl;
            item.Horse = horse.Replace("*", "_");
            item.Race = Config.Race;
            item.Win = amount;
            item.Place = 0;
            item.Odds = odds;
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
            */
        }
        private void DoBetWP(string horse, string member, string playtype, int amount, int times = 1)
        {
            /*
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
            */
        }

        private void DoBetW(string horse, string member, string playtype, int amount,string odds, int times = 1)
        {
            /*
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
                        item.Odds = odds;
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
            */
        }

        private void DoBetP(string horse, string member, string playtype, int amount,string odds, int times = 1)
        {
            /*
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
                        item.Odds = odds;
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
            */
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
                            ShowInfoMsg($"会员1{b}# {item.ToString()}#赔率{item.Odds}");
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
                            ShowInfoMsg($"会员1{b}# {item.ToString()}#赔率{item.Odds}");
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
                            ShowInfoMsg($"会员2{b}# {item.ToString()}#赔率{item.Odds}");
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
                            ShowInfoMsg($"会员2{b}# {item.ToString()}#赔率{item.Odds}");
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
                            ShowInfoMsg($"会员1{b}# {item.ToString()}#赔率{item.Odds}");
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
                            ShowInfoMsg($"会员2{b}# {item.ToString()}#赔率{item.Odds}");
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
                            ShowInfoMsg($"会员1{b}# {item.ToString()}#赔率{item.Odds}");

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
                            ShowInfoMsg($"会员2{b}# {item.ToString()}#赔率{item.Odds}");
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
                        ShowInfoMsg($"会员1{b}# {item.ToString()}#赔率{item.Odds}");
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
                        ShowInfoMsg($"会员2{b}# {item.ToString()}#赔率{item.Odds}");
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
                        ShowInfoMsg($"会员1{b}# {item.ToString()}#赔率{item.Odds}");

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
                        ShowInfoMsg($"会员2{b}# {item.ToString()}#赔率{item.Odds}");
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
            frmBrowser.Url = $"http://{CCmemberInstance.DoMain}/new_history_live.jsp?{new Random().NextDouble()}";
            frmBrowser.CC = CCmemberInstance.cc;
            frmBrowser.Text = "1";
            frmBrowser.Show();
        }

        private void btnMatch_Click(object sender, EventArgs e)
        {
            FrmWebbrowser frmMatch = new FrmWebbrowser();
            frmMatch.Url = $"http://{CCmemberInstance.DoMain}/playerhk.jsp?{new Random().NextDouble()}";
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
            /*
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
                        DoBetQ(item.horse, member, playtype, piao,pei.ToString() ,2);
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
            */
        }
        private void DoBetWbyZK2(BetInfo item, CCMember cc, Dictionary<string, WPOdds> dicWpOdds)
        {
            /*
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
                            DoBetW(item.horse, member, playtype, piao, pei.ToString(),2);
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
            */
        }
        private void DoBetPbyZK2(BetInfo item, CCMember cc, Dictionary<string, WPOdds> dicWpOdds)
        {
            /*
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
                            DoBetP(item.horse, member, playtype, piao,pei.ToString(), 2);
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
            */
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
                                DoBetQ(horse, member, playtype, iplace, "",2);
                            }
                            else
                            {
                                if (iwin == iplace)
                                {
                                    DoBetWP(horse, member, playtype, iwin, 2);
                                }
                                else if (iwin == 0)
                                {
                                    DoBetP(horse, member, playtype, iplace,"", 2);
                                }
                                else
                                {
                                    DoBetW(horse, member, playtype, iwin,"", 2);
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
                ShowInfoMsg($"会员{member}获取{Config.MatchCombol},第{Config.Race}场Q赔率成功");
            }
            else
            {
                ShowInfoMsg($"会员{member}获取{Config.MatchCombol},第{Config.Race}场Q赔率失败");
            }

            if(dicWpOdds!=null && dicWpOdds.Count>0)
            {
                ShowInfoMsg($"会员{member}获取{Config.MatchCombol},第{Config.Race}场WP赔率成功");
            }
            else
            {
                ShowInfoMsg($"会员{member}获取{Config.MatchCombol},第{Config.Race}场WP赔率失败");
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

        private void btnQdz_Click(object sender, EventArgs e)
        {
            string race = cobRace.Text.Trim();
            var tuEat = GetQRaceInfo(CCmemberInstance, "EAT", race);
            //var tuBet = GetQRaceInfo(CCmemberInstance2, "BET", race);

            Dictionary<string, RaceInfoItem> dicQEat = tuEat.Item1;
            Dictionary<string, RaceInfoItem> dicQPEat = tuEat.Item2;

            //Dictionary<string, RaceInfoItem> dicQBet = tuBet.Item1;
            //Dictionary<string, RaceInfoItem> dicQPBet = tuBet.Item2;

            List<string> lstQEat = GetHorseList(dicQEat);
            List<string> lstQPEat = GetHorseList(dicQPEat);

           List<string> lstQGroup = GroupByFrequency(lstQEat);
            List<string> lstQPGroup = GroupByFrequency(lstQPEat);

            string ret = "";
            if (radZfq.Checked)
            {
                foreach (var item in lstQGroup) 
                { 
                    ret += $"{item}正" + "\r\n";
                }
                foreach (var item in lstQPGroup)
                {
                    ret += $"{item}位" + "\r\n";
                }
            }
            if (radZs.Checked)
            {
                foreach (var item in lstQGroup)
                {
                    ret += $"{item}双" + "\r\n";
                }
            }
            if (radFs.Checked)
            {
                foreach (var item in lstQPGroup)
                {
                    ret += $"{item}双" + "\r\n";
                }
            }
            txtReport.Text = ret;
            if (!string.IsNullOrEmpty(ret))
            {
                Clipboard.SetText(ret);
            }
            string message = txtReport.Text.Trim();
            BroadcastMessage(message);
        }

        private List<string> GetHorseList(Dictionary<string, RaceInfoItem> dicRi)
        {
            List<string> lstHorse = new List<string>();
            foreach(var item in dicRi)
            {
                string horse = item.Value.Horse.Replace("(", "").Replace(")", "");
                if (!lstHorse.Contains(horse))
                {
                    lstHorse.Add(horse);
                }
            }
            return lstHorse;
        }

        public   List<string> GroupByFrequency(List<string> pairs)
        {
            if (pairs == null || pairs.Count == 0)
                return new List<string>();

            var pairData = new List<(int first, int second, string original)>();
            foreach (var pair in pairs)
            {
                var parts = pair.Split('-');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int a) &&
                    int.TryParse(parts[1], out int b))
                {
                    pairData.Add((a, b, pair));
                }
            }

            var results = new List<string>();
            var processedIndices = new HashSet<int>();

            while (processedIndices.Count < pairData.Count)
            {
                var frequency = new Dictionary<int, int>();
                for (int i = 0; i < pairData.Count; i++)
                {
                    if (processedIndices.Contains(i)) continue;

                    var data = pairData[i];

                    // 改用 TryGetValue 替代 GetValueOrDefault
                    if (frequency.TryGetValue(data.first, out int count1))
                        frequency[data.first] = count1 + 1;
                    else
                        frequency[data.first] = 1;

                    if (frequency.TryGetValue(data.second, out int count2))
                        frequency[data.second] = count2 + 1;
                    else
                        frequency[data.second] = 1;
                }

                if (!frequency.Any()) break;

                int maxFreq = frequency.Values.Max();
                int head = frequency
                    .Where(kvp => kvp.Value == maxFreq)
                    .Select(kvp => kvp.Key)
                    .Min();

                var tails = new HashSet<int>();
                for (int i = 0; i < pairData.Count; i++)
                {
                    if (processedIndices.Contains(i)) continue;

                    var data = pairData[i];
                    if (data.first == head)
                    {
                        tails.Add(data.second);
                        processedIndices.Add(i);
                    }
                    else if (data.second == head)
                    {
                        tails.Add(data.first);
                        processedIndices.Add(i);
                    }
                }

                if (tails.Any())
                {
                    var sortedTails = string.Join(",", tails.OrderBy(t => t));
                    results.Add($"{head}-{sortedTails}");
                }
            }

            return results;
        }
        private Tuple<Dictionary<string, RaceInfoItem>, Dictionary<string, RaceInfoItem>> GetQRaceInfo(CCMember ccm,string bettype,string race)
        {
            RaceInfoEnity re = ccm.GetRaceInfo();
            Dictionary<string, RaceInfoItem> dicQP = new Dictionary<string, RaceInfoItem>();
            Dictionary<string, RaceInfoItem> dicQ = new Dictionary<string, RaceInfoItem>();
           

            foreach (var item in re.DicRaceInfo)
            {
                RaceInfoItem ri = item.Value as RaceInfoItem;
                if (ri != null && ri.Bettype.ToString().Equals(bettype) && ri.Race.Equals(race))
                {
                    if ((ri.Playtype.ToString().Equals("QP") && ri.ClassType.ToString().Equals("QP")) || (ri.Playtype.ToString().Equals("FORECAST") && ri.ClassType.ToString().Equals("PFT")))
                    {
                        dicQP.Add(item.Key, ri);
                    }
                    if ((ri.Playtype.ToString().Equals("QP") && ri.ClassType.ToString().Equals("Q")) || (ri.Playtype.ToString().Equals("FORECAST") && ri.ClassType.ToString().Equals("FC")))
                    {
                        dicQ.Add(item.Key, ri);
                    }
                }
            }

            return Tuple.Create(dicQ, dicQP);
        }

        private Dictionary<string, RaceInfoItem> GetQiDaZhi(Dictionary<string, RaceInfoItem> dicQEat, Dictionary<string, RaceInfoItem> dicQBet,string bettype="Q")
        {
            Dictionary<string, RaceInfoItem> dicQ = new Dictionary<string, RaceInfoItem>();
            dicQEat= GetSumRaceInfo(dicQEat, bettype);
            dicQBet = GetSumRaceInfo(dicQBet, bettype);
            foreach (var item in dicQEat)
            {
                string horse = item.Value.Horse;
                double piao = bettype.Equals("Q") ? item.Value.Win * -1.0 : item.Value.Place * -1.0;
                bool bfind = false;
                foreach(var item2 in dicQBet)
                {
                    string horse2 = item2.Value.Horse;
                    double piao2 = bettype.Equals("Q") ? item2.Value.Win  : item2.Value.Place ;
                    if (horse == horse2)
                    {
                        if(Math.Abs(piao+piao2)>= Config.Gdz)
                        {
                            RaceInfoItem ri = item.Value.Clone();
                            if(bettype.Equals("Q"))
                            {
                                ri.Win = Math.Abs(piao + piao2);
                            }
                            else
                            {
                                ri.Place = Math.Abs(piao + piao2);
                            }
                            dicQ.Add(item.Key,ri);
                        }
                        bfind = true;
                        break;
                    }
                }
                if (!bfind)
                {
                    if (Math.Abs(piao) >= Config.Gdz)
                    {
                        dicQ.Add(item.Key, item.Value);
                    }
                }
            }
            return dicQ;
        }

        private  Dictionary<string, RaceInfoItem> GetSumRaceInfo(Dictionary<string, RaceInfoItem> dicRi,string bettype ="Q")
        {
            Dictionary<string, RaceInfoItem> dicRet = new Dictionary<string, RaceInfoItem>();
            Dictionary<string, KeyValuePair<string, RaceInfoItem>> dicPiao = new Dictionary<string, KeyValuePair<string, RaceInfoItem>> ();
            foreach (var item in dicRi)
            {
                if (!dicPiao.ContainsKey(item.Value.Horse))
                {
                    dicPiao.Add(item.Value.Horse, item);
                }
                else
                {
                    if (bettype.Equals("Q"))
                    {
                        dicPiao[item.Value.Horse].Value.Win += item.Value.Win;
                    }
                    else
                    {
                        dicPiao[item.Value.Horse].Value.Place += item.Value.Place;
                    }
                }
            }
            foreach(var item in dicPiao)
            {
                dicRet.Add(item.Value.Key,item.Value.Value);
            }
            return dicRet;
        }

        private string ProcessQP(Dictionary<string, RaceInfoItem> dicQP)
        {
            string strRet = "负Q:" + Environment.NewLine;
            Dictionary<double, List<string>> dic = new Dictionary<double, List<string>>();
            Dictionary<string, double> dicPiao = new Dictionary<string, double>();
            foreach (var item in dicQP)
            {
                if (!dicPiao.ContainsKey(item.Value.Horse))
                {
                    dicPiao.Add(item.Value.Horse, item.Value.Place);
                }
                else
                {
                    dicPiao[item.Value.Horse] += item.Value.Place;
                }
            }
            foreach (var item in dicPiao)
            {
                if (!dic.ContainsKey(item.Value))
                {
                    List<string> strings = new List<string>();
                    strings.Add(item.Key);
                    dic.Add(item.Value, strings);
                }
                else
                {
                    dic[item.Value].Add(item.Key);
                }
            }
            string tip = ProcessQitem(dic);
            strRet = string.IsNullOrEmpty(tip) ? "" : strRet + tip;
            return strRet;
        }

        private string ProcessQ(Dictionary<string, RaceInfoItem> dicQ)
        {
            string strRet = "正Q:" + Environment.NewLine;
            Dictionary<double, List<string>> dic = new Dictionary<double, List<string>>();
            Dictionary<string, double> dicPiao = new Dictionary<string, double>();
            foreach (var item in dicQ)
            {
                if (!dicPiao.ContainsKey(item.Value.Horse))
                {
                    dicPiao.Add(item.Value.Horse, item.Value.Win);
                }
                else
                {
                    dicPiao[item.Value.Horse] += item.Value.Win;
                }
            }
            foreach (var item in dicPiao)
            {
                if (!dic.ContainsKey(item.Value))
                {
                    List<string> strings = new List<string>();
                    strings.Add(item.Key);
                    dic.Add(item.Value, strings);
                }
                else
                {
                    dic[item.Value].Add(item.Key);
                }
            }
            string tip = ProcessQitem(dic);
            strRet = string.IsNullOrEmpty(tip) ? "" : strRet + tip;

            return strRet;
        }
        private string ProcessQitem(Dictionary<double, List<string>> dicQ)
        {
            string strRet = "";
            foreach (var item in dicQ)
            {
                double piao = item.Key;
                List<string> lstHorses = item.Value;

                string tip = ProcessQitemDetail(piao, lstHorses);

                strRet += tip;
            }
            return strRet;
        }

        private string ProcessQitemDetail(double piao, List<string> lstHorses)
        {
            string strRet = "";
            while (lstHorses.Count > 0)
            {

                Dictionary<string, int> dicHorseCount = new Dictionary<string, int>();
                foreach (var horses in lstHorses)
                {
                    string h = horses.Replace("(", "").Replace(")", "");
                    string h1 = h.Split("-".ToCharArray())[0].Trim();
                    string h2 = h.Split("-".ToCharArray())[1].Trim();
                    if (!dicHorseCount.ContainsKey(h1))
                    {
                        dicHorseCount.Add(h1, 1);
                    }
                    else
                    {
                        dicHorseCount[h1] += 1;
                    }

                    if (!dicHorseCount.ContainsKey(h2))
                    {
                        dicHorseCount.Add(h2, 1);
                    }
                    else
                    {
                        dicHorseCount[h2] += 1;
                    }
                }

                int max = 0;
                string Head = "";
                foreach (var horsecount in dicHorseCount)
                {
                    if (horsecount.Value >= max)
                    {
                        Head = horsecount.Key;
                        max = horsecount.Value;
                    }
                }

                string tip = $"{Head}拖";
                List<string> lstHorses2 = new List<string>();
                foreach (var horses in lstHorses)
                {
                    string h = horses.Replace("(", "").Replace(")", "");
                    string h1 = h.Split("-".ToCharArray())[0].Trim();
                    string h2 = h.Split("-".ToCharArray())[1].Trim();
                    if (Head.Equals(h1))
                    {
                        tip += $"{h2},";
                    }
                    else if (Head.Equals(h2))
                    {
                        tip += $"{h1},";
                    }
                    else
                    {
                        lstHorses2.Add(horses);
                    }
                }
                tip += $"#{RoundToNearestTen(piao)}";
                lstHorses = lstHorses2;
                strRet += tip + Environment.NewLine;
            }
            return strRet;
        }

        public int RoundToNearestTen(double number)
        {
            if (number % 10 == 0)
            {
                return (int)number;
            }
            else
            {
                return (int)(((int)(number / 10) + 1) * 10);
            }
        }

        private void btnXie_Click(object sender, EventArgs e)
        {
            cCmemberInstance.GetBetInfo(out string betinfo);
            if (string.IsNullOrEmpty(betinfo))
            {
                ShowInfoMsg("获取吃进单情况失败");
            }
            CCmemberInstance.DeleteAllQEatGuaDan(betinfo);
            cCmemberInstance2.GetBetInfo(out string betinfo2);
            if (string.IsNullOrEmpty(betinfo2))
            {
                ShowInfoMsg("获取赌进单情况失败");
            }

            bool bq = cCmemberInstance2.DeleteAllQEatGuaDan(betinfo2);
            CCmemberInstance2.DeleteAllQBetGuaDan(betinfo2);
           
            if (bq)
            {
                ShowInfoMsg("删除Q/PQ挂单成功");
            }

            string race = cobRace.Text.Trim();
            var tuEat = GetQRaceInfo(CCmemberInstance, "EAT", race);
            var tuBet = GetQRaceInfo(CCmemberInstance2, "BET", race);

            Dictionary<string, RaceInfoItem> dicQEat = tuEat.Item1;
            Dictionary<string, RaceInfoItem> dicQPEat = tuEat.Item2;

            Dictionary<string, RaceInfoItem> dicQBet = tuBet.Item1;
            Dictionary<string, RaceInfoItem> dicQPBet = tuBet.Item2;


            Dictionary<string, RaceInfoItem> dicQ = GetQiDaZhi(dicQEat, dicQBet);
            Xie(dicQ);
    
            Dictionary<string, RaceInfoItem> dicQP = GetQiDaZhi(dicQPEat, dicQPBet, "QP");
            Xie(dicQP,"QP");
        }

        private void Xie(Dictionary<string, RaceInfoItem> dicQ, string bettype = "Q")
        {
            foreach (var item in dicQ)
            {
                RaceInfoItem ri = item.Value;
                ri.Bettype = BetType.BET;
                if (bettype.Equals("Q"))
                {
                    ri.Playtype = PlayType.Q;
                    ri.Win = RoundToNearestTen(ri.Win);
                    //ri.Place = RoundToNearestTen(ri.Win);
                    ri.Zhe = Config.ZheQ;
                    ri.LWin = Config.LlimQ;
                    ri.LPlace = Config.RLimQ;
                }
                else
                {
                    ri.Playtype = PlayType.QP;
                   // ri.Win= RoundToNearestTen(ri.Place);
                    ri.Place = RoundToNearestTen(ri.Place);
                    ri.Zhe = Config.ZheQp;
                    ri.LWin = Config.LlimQp;
                    ri.LPlace = Config.RLimQp;
                }
                bool b = CCmemberInstance2.QiPiaoGuaQ(ri, out BetResultInfo info);
                if(b)
                {
                    //ShowInfoMsg(ri.ToString());
                }
                else
                {
                    ShowInfoMsg(info.StrAnswer);
                }
            }
        }

        #region TCP服务器相关方法

        /// <summary>
        /// 初始化TCP服务器
        /// </summary>
        private void InitializeTcpServer()
        {
            try
            {
                int port = int.Parse(txtPort.Text);
                tcpServer = new TcpServer(port);

                // 订阅TCP服务器事件
                tcpServer.ClientConnected += OnClientConnected;
                tcpServer.ClientDisconnected += OnClientDisconnected;
                tcpServer.MessageReceived += OnMessageReceived;
                tcpServer.ServerError += OnServerError;
            }
            catch (Exception ex)
            {
                AppendTcpLog($"TCP服务器初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        private void StartServer()
        {
            try
            {
                if (tcpServer == null)
                {
                    InitializeTcpServer();
                }

                if (tcpServer != null && !tcpServer.IsRunning)
                {
                    tcpServer.Start();
                    UpdateServerStatus(true);
                    AppendTcpLog($"TCP服务器启动成功，监听端口: {tcpServer.ListeningPort}");
                }
            }
            catch (Exception ex)
            {
                AppendTcpLog($"服务器启动失败: {ex.Message}");
                UpdateServerStatus(false);
            }
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        private void StopServer()
        {
            try
            {
                if (tcpServer != null && tcpServer.IsRunning)
                {
                    tcpServer.Stop();
                    UpdateServerStatus(false);
                    UpdateClientList();
                    AppendTcpLog("TCP服务器已停止");
                }
            }
            catch (Exception ex)
            {
                AppendTcpLog($"服务器停止失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 客户端连接事件处理
        /// </summary>
        private void OnClientConnected(TcpServer.ClientInfo client)
        {
            connectedClients[client.Id] = client;
            UpdateClientList();
            AppendTcpLog($"客户端连接: {client.RemoteEndPoint} (ID: {client.Id.Substring(0, 8)})");
        }

        /// <summary>
        /// 客户端断开连接事件处理
        /// </summary>
        private void OnClientDisconnected(TcpServer.ClientInfo client)
        {
            connectedClients.Remove(client.Id);
            UpdateClientList();
            AppendTcpLog($"客户端断开: {client.RemoteEndPoint} (ID: {client.Id.Substring(0, 8)})");
        }

        /// <summary>
        /// 收到消息事件处理
        /// </summary>
        private void OnMessageReceived(TcpServer.ClientInfo client, string message)
        {
            // 检查是否为心跳包
            if (message == "HEARTBEAT")
            {
                // 回复心跳包
                bool success = tcpServer.TrySendToClient(client.Id, "HEARTBEAT_ACK");
                if (success)
                {
                    AppendTcpLog($"收到并回复来自 {client.RemoteEndPoint} 的心跳");
                }
                else
                {
                    AppendTcpLog($"回复来自 {client.RemoteEndPoint} 的心跳失败");
                }
                return;
            }

            AppendTcpLog($"收到来自 {client.RemoteEndPoint} 的消息: {message}");
        }

        /// <summary>
        /// 服务器错误事件处理
        /// </summary>
        private void OnServerError(Exception ex)
        {
            AppendTcpLog($"服务器错误: {ex.Message}");
        }

        /// <summary>
        /// 更新服务器状态显示
        /// </summary>
        private void UpdateServerStatus(bool isRunning)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<bool>(UpdateServerStatus), isRunning);
                return;
            }

            btnStartServer.Enabled = !isRunning;
            btnStopServer.Enabled = isRunning;
            txtPort.Enabled = !isRunning;
            btnSendMessage.Enabled = isRunning && lstClients.SelectedIndex >= 0;
            btnBroadcastMessage.Enabled = isRunning;
            btnDisconnectClient.Enabled = isRunning && lstClients.SelectedIndex >= 0;

            lblServerStatus.Text = isRunning ? $"服务器运行中 (端口: {tcpServer?.ListeningPort})" : "服务器停止";
            lblServerStatus.ForeColor = isRunning ? Color.Green : Color.Red;
        }

        /// <summary>
        /// 更新客户端列表
        /// </summary>
        private void UpdateClientList()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(UpdateClientList));
                return;
            }

            lstClients.Items.Clear();
            foreach (var client in connectedClients.Values)
            {
                string item = $"{client.RemoteEndPoint} | 连接时间: {client.ConnectedAt:HH:mm:ss}";
                lstClients.Items.Add(item);
            }

            // 确保lblClientCount也在主线程中更新
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    lblClientCount.Text = $"客户端数量: {connectedClients.Count}";
                }));
            }
            else
            {
                lblClientCount.Text = $"客户端数量: {connectedClients.Count}";
            }
        }

        /// <summary>
        /// 添加TCP日志
        /// </summary>
        private void AppendTcpLog(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(AppendTcpLog), message);
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtTcpLog.AppendText($"[{timestamp}] {message}{Environment.NewLine}");

            // 限制日志行数，避免内存占用过多
            if (txtTcpLog.Lines.Length > 500)
            {
                var lines = txtTcpLog.Lines;
                var newLines = new string[lines.Length - 100];
                Array.Copy(lines, 100, newLines, 0, newLines.Length);
                txtTcpLog.Lines = newLines;
            }
        }

        /// <summary>
        /// 获取选中的客户端ID
        /// </summary>
        private string GetSelectedClientId()
        {
            if (lstClients.SelectedIndex >= 0)
            {
                var selectedKeys = connectedClients.Keys.ToList();
                if (lstClients.SelectedIndex < selectedKeys.Count)
                {
                    return selectedKeys[lstClients.SelectedIndex];
                }
            }
            return null;
        }

        /// <summary>
        /// 发送消息给指定客户端
        /// </summary>
        private void SendToSelectedClient()
        {
            string clientId = GetSelectedClientId();
            if (clientId != null)
            {
                string message = txtMessage.Text.Trim();
                if (!string.IsNullOrEmpty(message))
                {
                    if (tcpServer.TrySendToClient(clientId, message))
                    {
                        AppendTcpLog($"发送消息到 {connectedClients[clientId].RemoteEndPoint}: {message}");
                        txtMessage.Clear();
                    }
                    else
                    {
                        AppendTcpLog("发送消息失败");
                    }
                }
            }
            else
            {
                MessageBox.Show("请先选择一个客户端");
            }
        }

        /// <summary>
        /// 广播消息给所有客户端
        /// </summary>
        private void BroadcastMessage(string message)
        {
           
            if (!string.IsNullOrEmpty(message))
            {
                int successCount = 0;
                int failCount = 0;

                foreach (var clientId in connectedClients.Keys.ToList())
                {
                    if (tcpServer.TrySendToClient(clientId, message))
                    {
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                    }
                }

                AppendTcpLog($"广播消息: {message} (成功: {successCount}, 失败: {failCount})");
                txtMessage.Clear();
            }
        }

        /// <summary>
        /// 断开选中的客户端
        /// </summary>
        private void DisconnectSelectedClient()
        {
            string clientId = GetSelectedClientId();
            if (clientId != null)
            {
                string clientEndpoint = connectedClients[clientId].RemoteEndPoint;
                tcpServer.DisconnectClient(clientId);
                AppendTcpLog($"手动断开客户端: {clientEndpoint}");
            }
            else
            {
                MessageBox.Show("请先选择一个客户端");
            }
        }

        #endregion

        #region TCP控件事件处理程序

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            try
            {
                int port = int.Parse(txtPort.Text);
                if (port < 1 || port > 65535)
                {
                    MessageBox.Show("端口号必须在1-65535之间");
                    return;
                }

                StartServer();
            }
            catch (FormatException)
            {
                MessageBox.Show("请输入有效的端口号");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动服务器时发生错误: {ex.Message}");
            }
        }

        private void btnStopServer_Click(object sender, EventArgs e)
        {
            StopServer();
        }

        private void btnDisconnectClient_Click(object sender, EventArgs e)
        {
            DisconnectSelectedClient();
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            SendToSelectedClient();
        }

        private void btnBroadcastMessage_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text.Trim();
            BroadcastMessage(message);
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            txtTcpLog.Clear();
            AppendTcpLog("日志已清空");
        }

        private void lstClients_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool hasSelection = lstClients.SelectedIndex >= 0;
            btnDisconnectClient.Enabled = tcpServer?.IsRunning == true && hasSelection;
            btnSendMessage.Enabled = tcpServer?.IsRunning == true && hasSelection;
        }

        #endregion

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
