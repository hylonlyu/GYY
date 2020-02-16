using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuaDan
{
    public partial class FrmWebbrowser : Form
    {
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetSetCookie(string lpszUrlName, string lbszCookieName, string lpszCookieData);

        public string Url
        {
            get;
            set;
        }
        public CookieContainer CC
        {
            get;
            set;
        }
        public FrmWebbrowser()
        {
            InitializeComponent();
        }

        private void FrmWebbrowser_Load(object sender, EventArgs e)
        {
            try
            {
                SetCookie();
                webBrowser1.ScriptErrorsSuppressed = true;
                webBrowser1.IsWebBrowserContextMenuEnabled = false;
                webBrowser1.Navigate(Url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void SetCookie()
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

        private void webBrowser1_BeforeNewWindow(object sender, WebBrowserExtendedNavigatingEventArgs e)
        {
            e.Cancel = true;
            SetCookie();
            webBrowser1.Navigate(e.Url);
        }

        private void toolForward_Click(object sender, EventArgs e)
        {
            SetCookie();
            webBrowser1.GoForward();
        }

        private void toolBack_Click(object sender, EventArgs e)
        {
            SetCookie();
            webBrowser1.GoBack();
        }

        private void toolRefresh_Click(object sender, EventArgs e)
        {
            SetCookie();
            webBrowser1.Refresh();
        }
    }
}
