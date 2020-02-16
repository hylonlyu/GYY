using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;
namespace GuaDan
{
    public partial class LoadAccept2 : Form
    {
        private bool bChecekOneReg = true;
      
        private string cMachineCode = null;
  
        private string cServer = "http://localhost:55534/";
        private double dMoxSubmitRMB = 0.0;
       
        private FrmEatZd mainF = new FrmEatZd();
        public LoadAccept2()
        {
            InitializeComponent();
        }

        private void LoadSysConfig()
        {
            //try
            //{
            //    string strConfigFile = Yong.Util.StartupPath + @"\sysconfig.xml";
            //    if (!File.Exists(strConfigFile))
            //    {
            //        File.Create(strConfigFile);
            //    }

            //    SysConfig oConfig = new SysConfig(strConfigFile);
            //    //oConfig.LoadConfig();

            //    Cryp.CryptoHelper cry = new Cryp.CryptoHelper();
            //    this.cServer = cry.GetDecryptedValue(oConfig.Form.ToString());
            //    //this.cServer = "http://localhost:2274/";

            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
        }
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            this.Text = "正在登陸";
            Security.Account = txtAccount.Text.Trim();
            Security.Pwd = txtPwd.Text.Trim();
            this.DoInitRegInfo();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }


        private void bw_DoInitRegInfo(object sender, DoWorkEventArgs e)
        { 
                RegResult regStatus = Security.GetRegStatus();
                Security.PostUnRegUserData(this.cMachineCode, this.GetUserData());
                e.Result = regStatus;
        }

        private void bw_DoInitRegInfoChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void bw_DoInitRegInfoCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            this.Text = "";
            RegResult result = (RegResult)e.Result;
            if (result !=null && result.GetResult())
            {
                this.dMoxSubmitRMB = result.GetMaxSubmitRMB();
                this.mainF.Visible = true;
                this.mainF.ShowInTaskbar = true;
                this.mainF.WindowState = FormWindowState.Maximized;
                this.mainF.bCheckOnline = true;
                base.Visible = false;
                SaveSetting();
            }
            else if(result!=null && !result.GetResult())
            {
                MessageBox.Show(result.GetMsg());
                if (result.GetMsg().Equals("账号异常"))
                {
                    txtAccount.Text = "";
                    txtPwd.Text = "";
                }
                else
                {
                    Environment.Exit(0);
                }
            }
            else
            {
                txtAccount.Text = "";
                txtPwd.Text = "";
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

        private void ExitMe()
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string str = "taskkill /IM \"" + AppDomain.CurrentDomain.FriendlyName + "\"";
            process.StandardInput.WriteLine(str);
            process.StandardInput.WriteLine("exit");
        }

        public string GeServer()
        {
            return this.cServer;
        }

        private string GetLoactionCode()
        {
            return "";
        }

        public string GetMachineCode()
        {
            return this.cMachineCode;
        }

        private string GetUserData()
        {
            string str = "";
            //Hashtable hashtable = new DB().Select("Select iSiteID,cSiteUserName,cSitePassword From TSiteUser");
            Hashtable hashtable = new Hashtable();
            foreach (DictionaryEntry entry in hashtable)
            {
                Hashtable hashtable2 = (Hashtable)entry.Value;
                int num = Convert.ToInt32(hashtable2[0]);
                string str2 = Convert.ToString(hashtable2[1]);
                string str3 = Convert.ToString(hashtable2[2]);
                object obj2 = str;
                str = string.Concat(new object[] { obj2, num, "|", str2, "|", str3, "\n" });
            }
            return ("data=" + str);
        }


        private void InitRegInfo()
        {
            this.DoInitRegInfo();
        }

        private void LoadAccpet_Load(object sender, EventArgs e)
        {
            InitSaveSetting();
            this.mainF.ShowInTaskbar = false;
            this.mainF.WindowState = FormWindowState.Minimized;
            this.mainF.Visible = false;
            this.mainF.bCheckOnline = false;
            this.mainF.Show();
        }

        private void InitSaveSetting()
        {
           string autosave = IniFile.IniReadValue("CC", "autosave");
            bool.TryParse(autosave, out bool save);
            chkSave.Checked = save;
            txtAccount.Text = IniFile.IniReadValue("CC", "username");
            string pwd = IniFile.IniReadValue("CC","pwd");
            CryptoHelper ch = new CryptoHelper();
            string pwd2 = "";
            try
            {
                pwd2 = ch.GetDecryptedValue(pwd);
            }
            catch(Exception ex)
            {
                
            }
            txtPwd.Text = pwd2;
        }

        private void SaveSetting()
        {
            if (chkSave.Checked)
            {
                IniFile.IniWriteValue("CC", "autosave", "true");
                IniFile.IniWriteValue("CC", "username", txtAccount.Text.Trim());
                CryptoHelper ch = new CryptoHelper();
                string pwd = ch.GetEncryptedValue(txtPwd.Text.Trim());
                IniFile.IniWriteValue("CC", "pwd", pwd);
            }
            else
            {
                IniFile.IniWriteValue("CC", "autosave", "false");
                IniFile.IniWriteValue("CC", "username", "");
                IniFile.IniWriteValue("CC", "pwd", "");
            }
        }
        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void lblChangePwd_Click(object sender, EventArgs e)
        {
          
        }
    }
}
