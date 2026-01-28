using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GuaDan
{
    /// <summary>
    /// 简单的HTTP消息服务器，用于向客户端推送消息
    /// 使用HTTP轮询方式，避免复杂的TCP连接管理
    /// </summary>
    public class HttpMessageServer : IDisposable
    {
        private HttpListener _httpListener;
        private bool _isRunning;
        private int _port;

        // 消息队列，存储待发送的消息
        private readonly ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();

        // 客户端记录，用于管理连接的客户端
        private readonly ConcurrentDictionary<string, ClientInfo> _connectedClients = new ConcurrentDictionary<string, ClientInfo>();

        public bool IsRunning { get { return _isRunning; } }
        public int Port { get { return _port; } }
        public int ListeningPort { get { return _port; } }

        // 事件通知
        public event Action<string> ClientConnected;
        public event Action<string> ClientDisconnected;
        public event Action<string> ServerError;
        public event Action<string> ServerLog;

        public HttpMessageServer(int port)
        {
            _port = port;
        }

        public async Task<bool> StartAsync()
        {
            if (_isRunning) return true;

            try
            {
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add("http://+:" + _port.ToString() + "/");
                _httpListener.Start();
                _isRunning = true;

                if (ServerLog != null)
                {
                    ServerLog("HTTP消息服务器启动成功，监听端口: " + _port.ToString());
                }

                // 启动监听任务
                _ = Task.Run(() => ListenForClientsAsync());

                return true;
            }
            catch (Exception ex)
            {
                if (ServerError != null)
                {
                    ServerError("启动HTTP服务器失败: " + ex.Message);
                }
                return false;
            }
        }

        public void Stop()
        {
            if (!_isRunning) return;

            try
            {
                _isRunning = false;
                _httpListener?.Stop();
                _httpListener?.Close();

                // 清理客户端记录
                _connectedClients.Clear();

                if (ServerLog != null)
                {
                    ServerLog("HTTP消息服务器已停止");
                }
            }
            catch (Exception ex)
            {
                if (ServerError != null)
                {
                    ServerError("停止HTTP服务器失败: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// 向所有客户端发送消息
        /// </summary>
        public void SendMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            _messageQueue.Enqueue(message);
            if (ServerLog != null)
            {
                ServerLog("消息已加入队列: " + message);
            }
        }

        /// <summary>
        /// 获取待发送的消息
        /// </summary>
        public string GetNextMessage()
        {
            string message = null;
            if (_messageQueue.TryDequeue(out message))
            {
                return message;
            }
            return null;
        }

        /// <summary>
        /// 检查是否有待发送的消息
        /// </summary>
        public bool HasMessages()
        {
            return !_messageQueue.IsEmpty;
        }

        /// <summary>
        /// 获取连接的客户端数量
        /// </summary>
        public int GetClientCount()
        {
            return _connectedClients.Count;
        }

        /// <summary>
        /// 监听客户端请求
        /// </summary>
        private async Task ListenForClientsAsync()
        {
            while (_isRunning)
            {
                try
                {
                    var context = await _httpListener.GetContextAsync();
                    _ = Task.Run(() => HandleClientRequestAsync(context));
                }
                catch (ObjectDisposedException)
                {
                    // 服务器已停止，正常退出
                    break;
                }
                catch (Exception ex)
                {
                    if (ServerError != null)
                    {
                        ServerError("接受客户端请求时出错: " + ex.Message);
                    }
                    await Task.Delay(100); // 短暂延迟后继续
                }
            }
        }

        /// <summary>
        /// 处理客户端请求
        /// </summary>
        private async Task HandleClientRequestAsync(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;

                // 记录客户端访问
                string clientId = request.RemoteEndPoint != null ? request.RemoteEndPoint.ToString() : "Unknown";
                DateTime clientTime = DateTime.Now;

                // 根据请求路径处理
                string responseContent = "";
                string contentType = "text/plain; charset=utf-8";

                if (request.Url != null && request.Url.AbsolutePath.EndsWith("/check"))
                {
                    // 检查是否有新消息
                    string message = GetNextMessage();
                    if (message != null)
                    {
                        responseContent = message;
                        if (ServerLog != null)
                        {
                            ServerLog("向客户端 " + clientId + " 发送消息: " + message);
                        }
                    }
                    else
                    {
                        responseContent = "NO_MESSAGE";
                    }
                }
                else if (request.Url != null && request.Url.AbsolutePath.EndsWith("/status"))
                {
                    // 返回服务器状态
                    responseContent = "OK|" + GetClientCount().ToString() + "|" + _messageQueue.Count.ToString();
                }
                else
                {
                    // 默认响应
                    responseContent = "HTTP消息服务器运行中";
                }

                // 记录客户端连接
                if (!_connectedClients.ContainsKey(clientId))
                {
                    var clientInfo = new ClientInfo
                    {
                        Id = clientId,
                        RemoteEndPoint = clientId,
                        ConnectedAt = clientTime,
                        LastRequestAt = clientTime
                    };
                    _connectedClients[clientId] = clientInfo;
                    if (ClientConnected != null)
                    {
                        ClientConnected(clientId);
                    }
                }
                else
                {
                    _connectedClients[clientId].LastRequestAt = clientTime;
                }

                // 清理超过5分钟未活动的客户端
                CleanupInactiveClients();

                // 发送响应
                byte[] buffer = Encoding.UTF8.GetBytes(responseContent);
                response.ContentLength64 = buffer.Length;
                response.ContentType = contentType;
                response.StatusCode = 200;

                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.Close();
            }
            catch (Exception ex)
            {
                if (ServerError != null)
                {
                    ServerError("处理客户端请求时出错: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// 清理超过5分钟未活动的客户端
        /// </summary>
        private void CleanupInactiveClients()
        {
            var cutoffTime = DateTime.Now.AddMinutes(-5);
            var toRemove = new List<string>();

            foreach (var client in _connectedClients)
            {
                if (client.Value.LastRequestAt < cutoffTime)
                {
                    toRemove.Add(client.Key);
                }
            }

            foreach (var clientId in toRemove)
            {
                ClientInfo removedClient;
                if (_connectedClients.TryRemove(clientId, out removedClient))
                {
                    if (ClientDisconnected != null)
                    {
                        ClientDisconnected(clientId);
                    }
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// 客户端信息类
        /// </summary>
        public class ClientInfo
        {
            public string Id { get; set; }
            public string RemoteEndPoint { get; set; }
            public DateTime ConnectedAt { get; set; }
            public DateTime LastRequestAt { get; set; }
        }
    }
}