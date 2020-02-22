using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GuaDan
{
    public partial class FrmLogin : Form
    {
        public FrmLogin()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(txtAccount.Text.Trim()) && txtPwd.Text.Equals("a1189"))
            {
                FrmGuaDan frmMain = new FrmGuaDan();
                frmMain.Show();
                this.Hide();
            }
        }
    }
}
