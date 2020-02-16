using System;
using System.Text;
using System.Net;
using System.Net.Security;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.IO.Compression;

namespace GuaDan
{
    public class Connect
    {
        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        public static string getDocument(string cUrl)
        {
            return getDocument(cUrl, null, null, null, null);
        }

        public static string getDocument(string cUrl, CookieContainer cc)
        {
            return getDocument(cUrl, cc, null, null, null);
        }

        public static string getDocument(string cUrl, CookieContainer cc, string referer)
        {
            return getDocument(cUrl, cc, referer, null, null);
        }

        public static string getDocument(string cUrl, CookieContainer cc, string referer, string en)
        {
            return getDocument(cUrl, cc, referer, en, null);
        }

        public static string getDocument(string cUrl, CookieContainer cc, string referer, string en, string cProxyStrings)
        {
            if (cUrl == null)
            {
                return null;
            }
            if (cUrl.ToLower().StartsWith("https://"))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(Connect.CheckValidationResult);
            }
            try
            {
                StreamReader reader;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cUrl);
                //if ((cProxyStrings == null) && (ConfigurationManager.AppSettings.Get("HttpProxy") != null))
                //{
                //    cProxyStrings = ConfigurationManager.AppSettings.Get("HttpProxy");
                //}
                if (cProxyStrings != null)
                {
                    WebProxy proxy = new WebProxy();
                    Uri uri = new Uri(cProxyStrings);
                    proxy.Address = uri;
                    request.Proxy = proxy;
                }
                request.Timeout = 0x4e20;
                request.CookieContainer = cc;
                if (referer != null)
                {
                    request.Referer = referer;
                }
                //request.Headers["Upgrade-Insecure-Requests"] = "1";
                //request.Headers["Cache-Control"] = "max-age=0";
                request.Headers["Pragma"] = "no-cache";
                request.Headers["Accept-Encoding"] = "gzip, deflate";
                request.Headers["Accept-Language"] = "zh-CN";
                request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; zh-CN; rv:1.8.1.14) Gecko/20080404 Firefox/2.0.0.14";
                request.Method = "GET";
                request.KeepAlive = true;

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream responseStream = response.GetResponseStream();
                string str = null;
                string contentEncoding = response.ContentEncoding;
                if (response.Headers["Location"] != null)
                {
                    return getDocument(cUrl, cc, cProxyStrings, referer, en);
                }
                if (contentEncoding.ToLower().IndexOf("gzip") >= 0)
                {
                    GZipStream stream = new GZipStream(responseStream, CompressionMode.Decompress, true);
                    reader = null;
                    if (en != null)
                    {
                        reader = new StreamReader(stream, Encoding.GetEncoding(en));
                    }
                    else
                    {
                        reader = new StreamReader(stream, Encoding.GetEncoding("gb2312"));
                    }
                    str = reader.ReadToEnd();
                    reader.Close();
                }
                else
                {
                    reader = null;
                    if (en != null)
                    {
                        reader = new StreamReader(responseStream, Encoding.GetEncoding(en));
                    }
                    else
                    {
                        reader = new StreamReader(responseStream, Encoding.GetEncoding("gb2312"));
                    }
                    str = reader.ReadToEnd();
                    reader.Close();
                }
                responseStream.Close();
                response.Close();
                return str;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string getDocument2(string cUrl)
        {
            return getDocument2(cUrl, null, null, null, null);
        }

        public static string getDocument2(string cUrl, string cc)
        {
            return getDocument2(cUrl, cc, null, null, null);
        }

        public static string getDocument2(string cUrl, string cc, string referer)
        {
            return getDocument2(cUrl, cc, referer, null, null);
        }
        public static string getDocument2(string cUrl, string cc, string referer, string en)
        {
            return getDocument2(cUrl, cc, referer, en, null);
        }

        public static string getDocument2(string cUrl, string cc, string referer, string en, string cProxyStrings)
        {
            if (cUrl == null)
            {
                return null;
            }
            if (cUrl.ToLower().StartsWith("https://"))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(Connect.CheckValidationResult);
            }
            try
            {
                StreamReader reader;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cUrl);
                //if ((cProxyStrings == null) && (ConfigurationManager.AppSettings.Get("HttpProxy") != null))
                //{
                //    cProxyStrings = ConfigurationManager.AppSettings.Get("HttpProxy");
                //}
                if (cProxyStrings != null)
                {
                    WebProxy proxy = new WebProxy();
                    Uri uri = new Uri(cProxyStrings);
                    proxy.Address = uri;
                    request.Proxy = proxy;
                }
                request.Timeout = 0x4e20;
                if (cc != null)
                {
                    request.Headers.Set(HttpRequestHeader.Cookie, cc);
                }
                if (referer != null)
                {
                    request.Referer = referer;
                }
                request.Headers["Pragma"] = "no-cache";
                request.Headers["Accept-Encoding"] = "gzip, deflate";
                request.Headers["Accept-Language"] = "zh-CN";
                request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; zh-CN; rv:1.8.1.14) Gecko/20080404 Firefox/2.0.0.14";
                request.Method = "GET";
                request.KeepAlive = true;

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream responseStream = response.GetResponseStream();
                string str = null;
                string contentEncoding = response.ContentEncoding;
                if (response.Headers["Location"] != null)
                {
                    return getDocument2(cUrl, cc, cProxyStrings, referer, en);
                }
                if (contentEncoding.ToLower().IndexOf("gzip") >= 0)
                {
                    GZipStream stream = new GZipStream(responseStream, CompressionMode.Decompress, true);
                    reader = null;
                    if (en != null)
                    {
                        reader = new StreamReader(stream, Encoding.GetEncoding(en));
                    }
                    else
                    {
                        reader = new StreamReader(stream, Encoding.GetEncoding("gb2312"));
                    }
                    str = reader.ReadToEnd();
                    reader.Close();
                }
                else
                {
                    reader = null;
                    if (en != null)
                    {
                        reader = new StreamReader(responseStream, Encoding.GetEncoding(en));
                    }
                    else
                    {
                        reader = new StreamReader(responseStream, Encoding.GetEncoding("gb2312"));
                    }
                    str = reader.ReadToEnd();
                    reader.Close();
                }
                responseStream.Close();
                response.Close();
                return str;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public static int getHttpState(string cUrl)
        {
            try
            {
                return getHttpState(cUrl, null);
            }
            catch (Exception exception)
            {
                exception = null;
                object obj2 = exception;
                return 0x1f5;
            }
        }

        public static int getHttpState(string cUrl, CookieContainer cc)
        {
            int num = 0;
            HttpWebResponse response = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cUrl);
                request.CookieContainer = cc;
                response = request.GetResponse() as HttpWebResponse;
                return Convert.ToInt32(response.StatusCode);
            }
            catch (WebException exception)
            {
                if (exception.Status.Equals(WebExceptionStatus.ProtocolError))
                {
                    response = (HttpWebResponse)exception.Response;
                    num = Convert.ToInt32(response.StatusCode);
                }
                return num;
            }
        }

        public static string getPic(string cUrl, CookieContainer cc, string referer, string cProxy, string cPath)
        {
            string strRet = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cUrl);
                if (cProxy != null)
                {
                    WebProxy proxy = new WebProxy();
                    Uri uri = new Uri(cProxy);
                    proxy.Address = uri;
                    request.Proxy = proxy;
                }
                if (cc != null)
                {
                    request.CookieContainer = cc;
                }
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Maxthon; .NET CLR 1.1.4322)";
                if (referer != null)
                {
                    request.Referer = referer;
                }
                request.Method = "GET";
                //request.Headers["Accept-Encoding"] = "gzip, deflate";
                Stream responseStream = (request.GetResponse() as HttpWebResponse).GetResponseStream();
                byte[] buffer = new byte[0x186a0];
                int length = buffer.Length;
                int offset = 0;
                while (length > 0)
                {
                    int num3 = responseStream.Read(buffer, offset, length);
                    if (num3 == 0)
                    {
                        break;
                    }
                    offset += num3;
                    length -= num3;
                }
                strRet = Encoding.Default.GetString(buffer);
                FileStream stream2 = new FileStream(cPath, FileMode.OpenOrCreate, FileAccess.Write);
                stream2.Write(buffer, 0, offset);
                responseStream.Close();
                stream2.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.StackTrace);
                Console.WriteLine(exception.ToString());
            }
            return strRet;
        }

        public static bool getRemoteFile(string cUrl, string cFilePath)
        {
            WebClient client = new WebClient();
            try
            {
                if (System.IO.File.Exists(cFilePath))
                {
                    System.IO.File.Delete(cFilePath);
                }
                client.DownloadFile(cUrl, cFilePath);
                return true;
            }
            catch (Exception)
            {
            }
            return false;
        }

        public static string postDocument(string cUrl, string data)
        {
            return postDocument(cUrl, data, null, null, null);
        }

        public static string postDocument(string cUrl, string data, CookieContainer cc)
        {
            return postDocument(cUrl, data, cc, null, null);
        }

        public static string postDocument(string cUrl, string data, CookieContainer cc, string cProxyStrings)
        {
            return postDocument(cUrl, data, cc, cProxyStrings, null);
        }

        public static string postDocument(string cUrl, string data, CookieContainer cc, string cProxyStrings, string referer)
        {
            return postDocument(cUrl, data, cc, cProxyStrings, null, null);
        }

        public static string postDocument(string cUrl, string data, CookieContainer cc, string cProxyStrings, string referer, string en)
        {
            if (en == null)
            {
                en = "gb2312";
            }
            if (cUrl.ToLower().StartsWith("https://"))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(Connect.CheckValidationResult);
            }
            try
            {
                Uri uri = new Uri(cUrl);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cUrl);
                if (cProxyStrings != null)
                {
                    WebProxy proxy = new WebProxy();
                    Uri uri2 = new Uri(cProxyStrings);
                    proxy.Address = uri2;
                    request.Proxy = proxy;
                }
                request.Timeout = 0x4e20;
                request.Method = "POST";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                request.AllowAutoRedirect = true;
                if (cc != null)
                {
                    request.CookieContainer = cc;
                }
                request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.2; zh-CN; rv:1.9) Gecko/2008052906 Firefox/3.0";
                if (referer == null)
                {
                    referer = "http://" + uri.Host;
                }
                //request.Headers["Accept-Encoding"] = "gzip, deflate";
                request.Referer = referer;
                request.ContentType = "application/x-www-form-urlencoded";
                request.KeepAlive = true;
                byte[] bytes = null;
                if (data != null)
                {
                    bytes = Encoding.ASCII.GetBytes(data);
                    request.ContentLength = bytes.Length;
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Close();
                }
                else
                {
                    request.ContentLength = 0L;
                }
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(en));
                string strRe = reader.ReadToEnd();
                reader.Close();
                request = null;
                response = null;

                return strRe;
            }
            catch (Exception exception)
            {
                Console.WriteLine("POST 数据错误 " + cUrl + ":\n" + exception.ToString());
                //MessageBox.Show("POST 数据错误 " + cUrl + ":\n" + exception.ToString());
                return null;
            }
        }

        public static string postDocument2(string cUrl, string data, string cc)
        {
            return postDocument2(cUrl, data, cc, null, null);
        }

        public static string postDocument2(string cUrl, string data, string cc, string cProxyStrings, string referer)
        {
            return postDocument2(cUrl, data, cc, cProxyStrings, referer, null);
        }

        public static string postDocument2(string cUrl, string data, string cc, string cProxyStrings, string referer, string en)
        {
            if (en == null)
            {
                en = "gb2312";
            }
            if (cUrl.ToLower().StartsWith("https://"))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(Connect.CheckValidationResult);
            }
            try
            {
                Uri uri = new Uri(cUrl);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(cUrl);
                if (cProxyStrings != null)
                {
                    WebProxy proxy = new WebProxy();
                    Uri uri2 = new Uri(cProxyStrings);
                    proxy.Address = uri2;
                    request.Proxy = proxy;
                }
                request.Timeout = 0x4e20;
                request.Method = "POST";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                request.AllowAutoRedirect = true;
                if (cc != null)
                {
                    request.Headers.Set(HttpRequestHeader.Cookie, cc);
                }
                request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.2; zh-CN; rv:1.9) Gecko/2008052906 Firefox/3.0";
                if (referer == null)
                {
                    referer = "http://" + uri.Host;
                }
                //request.Headers["Accept-Encoding"] = "gzip, deflate";
                request.Referer = referer;
                request.ContentType = "application/x-www-form-urlencoded";
                request.KeepAlive = true;
                byte[] bytes = null;
                if (data != null)
                {
                    bytes = Encoding.ASCII.GetBytes(data);
                    request.ContentLength = bytes.Length;
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Close();
                }
                else
                {
                    request.ContentLength = 0L;
                }
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(en));
                string strRe = reader.ReadToEnd();
                reader.Close();
                request = null;
                response = null;

                return strRe;
            }
            catch (Exception exception)
            {
                Console.WriteLine("POST 数据错误 " + cUrl + ":\n" + exception.ToString());
                return null;
            }
        }


    }
}
