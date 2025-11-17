using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WeControl
{
    public partial class Form1 : Form
    {
        private int _listenPort = 9000;
        private UdpClient _udpClient;
        private CancellationTokenSource _cts;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            this.FormClosing += Form1_FormClosing;
     
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Load saved port if available
            try
            {
                var saved = Properties.Settings.Default.Port;
                if (!string.IsNullOrWhiteSpace(saved))
                {
                    txtPort.Text = saved;
                }
            }
            catch
            {
                // ignore
            }

            // parse port
            if (!int.TryParse(txtPort.Text, out _listenPort))
            {
                _listenPort = 9000;
                txtPort.Text = _listenPort.ToString();
            }

            StartListener();
        }

        private void BtnBind_Click(object sender, EventArgs e)
        {
            // Save the port to settings and restart listener
            if (!int.TryParse(txtPort.Text, out int port))
            {
                MessageBox.Show("端口格式不正确。请输入一个数字端口。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Properties.Settings.Default.Port = txtPort.Text;
            try
            {
                Properties.Settings.Default.Save();
                AddMessageToListBox($"已保存端口: {txtPort.Text}");
            }
            catch (Exception ex)
            {
                AddMessageToListBox($"保存端口失败: {ex.Message}");
            }

            // restart listener on new port
            StopListener();
            StartListener();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopListener();
        }

        private void StartListener()
        {
            try
            {
                if (!int.TryParse(txtPort.Text, out _listenPort))
                {
                    _listenPort = 9000;
                    txtPort.Text = _listenPort.ToString();
                }

                _cts = new CancellationTokenSource();
                _udpClient = new UdpClient(_listenPort);
                Task.Run(() => ReceiveLoop(_cts.Token));
                AddMessageToListBox($"监听 UDP 端口 {_listenPort}...");
            }
            catch (Exception ex)
            {
                AddMessageToListBox($"启动监听失败: {ex.Message}");
            }
        }

        private void StopListener()
        {
            try
            {
                if (_cts != null)
                {
                    _cts.Cancel();
                    _cts.Dispose();
                    _cts = null;
                }

                if (_udpClient != null)
                {
                    try { _udpClient.Close(); } catch { }
                    _udpClient = null;
                }
            }
            catch { }
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    UdpReceiveResult result;
                    try
                    {
                        result = await _udpClient.ReceiveAsync();
                    }
                    catch (ObjectDisposedException)
                    {
                        break; // socket closed
                    }
                    catch (SocketException)
                    {
                        break;
                    }

                    string msg;
                    try
                    {
                        msg = Encoding.UTF8.GetString(result.Buffer);
                    }
                    catch
                    {
                        msg = BitConverter.ToString(result.Buffer);
                    }

                    AddMessageToListBox(msg);
                }
            }
            catch (Exception ex)
            {
                AddMessageToListBox($"接收循环异常: {ex.Message}");
            }
        }

        private void AddMessageToListBox(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string line = $"[{timestamp}] {message}";
            if (lstInfo.InvokeRequired)
            {
                lstInfo.BeginInvoke(new Action(() =>
                {
                    lstInfo.Items.Add(line);
                    lstInfo.TopIndex = lstInfo.Items.Count - 1;
                }));
            }
            else
            {
                lstInfo.Items.Add(line);
                lstInfo.TopIndex = lstInfo.Items.Count - 1;
            }
        }
    }
}
