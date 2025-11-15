using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace GuaDan
{
    public partial class FrmWebbrowser : Form
    {
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

        private async void FrmWeb_Load(object sender, EventArgs e)
        {
            string userDataFolder1 = Path.Combine(Application.StartupPath, Text);
            CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions();
            CoreWebView2Environment environment1 = await CoreWebView2Environment.CreateAsync(null, userDataFolder1, options);
            await webView21.EnsureCoreWebView2Async(environment1);
            webView21.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            Init();
            webView21.CoreWebView2.Navigate(Url);
        }

        private void CoreWebView2_NewWindowRequested(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs e)
        {
            String url = e.Uri.ToString();
            if (!url.Contains("oauth"))
            {
                webView21.Source = new Uri(url);
                e.Handled = true;//禁止弹窗
            }
        }

        private void Init()
        {
            SetCookie();
        }

        private void SetCookie()
        {
            Uri uri = new Uri(Url);
            string cDomain = uri.Host;
            CookieContainer container = CC;
            CookieCollection cc = container.GetCookies(new Uri(Url));
            foreach (System.Net.Cookie c in cc)
            {
                var cookie = webView21.CoreWebView2.CookieManager.CreateCookie(c.Name.ToString(), c.Value.ToString(), cDomain, "/");
                cookie.IsHttpOnly = true;
                //cookie.IsSecure = true;
                webView21.CoreWebView2.CookieManager.AddOrUpdateCookie(cookie);
            }
        }

        private void toolForward_Click(object sender, EventArgs e)
        {
            SetCookie();
            webView21.GoForward();
        }

        private void toolBack_Click(object sender, EventArgs e)
        {
            SetCookie();
            webView21.GoBack();
        }

        private void toolRefresh_Click(object sender, EventArgs e)
        {
            SetCookie();
            webView21.CoreWebView2.Reload();
        }

        private void webView21_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            this.toolBack.Enabled = webView21.CanGoBack ? true : false;
            this.toolForward.Enabled = webView21.CanGoForward ? true : false;
        }
    }
}
