using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GuaDan
{
    /// <summary>
    /// Simple TCP server to accept multiple client connections, receive messages and manage connection state.
    /// Designed for .NET Framework 4.8 / 4.7.2 environments.
    /// </summary>
    public class TcpServer : IDisposable
    {
        private readonly ConcurrentDictionary<string, ClientState> _clients = new ConcurrentDictionary<string, ClientState>();
        private TcpListener _listener;
        private CancellationTokenSource _cts;

        public bool IsRunning { get; private set; }

        public int Port { get; private set; }

        // Actual listening port (useful when Port was 0 to request an ephemeral port)
        public int ListeningPort { get; private set; }

        // Events to notify about client state changes and incoming messages
        public event Action<ClientInfo> ClientConnected;
        public event Action<ClientInfo> ClientDisconnected;
        public event Action<ClientInfo, string> MessageReceived;
        public event Action<Exception> ServerError;

        public TcpServer(int port)
        {
            Port = port;
            ListeningPort = port;
        }

        public void Start()
        {
            if (IsRunning) return;
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Start();
            try
            {
                ListeningPort = ((IPEndPoint)_listener.LocalEndpoint).Port;
            }
            catch
            {
                ListeningPort = Port;
            }
            IsRunning = true;
            Task.Run(() => AcceptLoopAsync(_cts.Token));
        }

        public void Stop()
        {
            if (!IsRunning) return;
            _cts.Cancel();
            try
            {
                _listener.Stop();
            }
            catch { }

            foreach (var kv in _clients)
            {
                try
                {
                    kv.Value.TcpClient.Close();
                }
                catch { }
            }
            _clients.Clear();
            IsRunning = false;
        }

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var tcp = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    _ = Task.Run(() => HandleClientAsync(tcp, token));
                }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex)
                {
                    ServerError?.Invoke(ex);
                    await Task.Delay(200).ConfigureAwait(false);
                }
            }
        }

        private async Task HandleClientAsync(TcpClient tcp, CancellationToken serverToken)
        {
            string id = Guid.NewGuid().ToString();
            var state = new ClientState(id, tcp);
            _clients.TryAdd(id, state);
            OnClientConnected(state);

            try
            {
                using (tcp)
                using (var stream = tcp.GetStream())
                {
                    var buffer = new byte[8192];
                    while (tcp.Connected && !serverToken.IsCancellationRequested)
                    {
                        int read = 0;
                        try
                        {
                            read = await stream.ReadAsync(buffer, 0, buffer.Length, serverToken).ConfigureAwait(false);
                        }
                        catch (IOException)
                        {
                            // client disconnected abruptly
                            break;
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }

                        if (read == 0) // disconnected gracefully
                            break;

                        string msg = Encoding.UTF8.GetString(buffer, 0, read);
                        MessageReceived?.Invoke(state.ToClientInfo(), msg);
                    }
                }
            }
            catch (Exception ex)
            {
                ServerError?.Invoke(ex);
            }
            finally
            {
                _clients.TryRemove(id, out var _);
                OnClientDisconnected(state);
            }
        }

        private void OnClientConnected(ClientState state)
        {
            ClientConnected?.Invoke(state.ToClientInfo());
        }
        private void OnClientDisconnected(ClientState state)
        {
            ClientDisconnected?.Invoke(state.ToClientInfo());
        }

        public IReadOnlyCollection<ClientInfo> GetClients()
        {
            var list = new List<ClientInfo>();
            foreach (var kv in _clients)
            {
                list.Add(kv.Value.ToClientInfo());
            }
            return list;
        }

        public bool TrySendToClient(string clientId, string message)
        {
            if (!_clients.TryGetValue(clientId, out var state)) return false;
            try
            {
                var stream = state.TcpClient.GetStream();
                var data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                return true;
            }
            catch (Exception ex)
            {
                ServerError?.Invoke(ex);
                return false;
            }
        }

        public void DisconnectClient(string clientId)
        {
            if (_clients.TryRemove(clientId, out var state))
            {
                try
                {
                    state.TcpClient.Close();
                }
                catch { }
                OnClientDisconnected(state);
            }
        }

        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }

        private class ClientState
        {
            public string Id { get; }
            public TcpClient TcpClient { get; }
            public DateTime ConnectedAt { get; }

            public ClientState(string id, TcpClient client)
            {
                Id = id;
                TcpClient = client;
                ConnectedAt = DateTime.UtcNow;
            }

            public ClientInfo ToClientInfo()
            {
                string endpoint = string.Empty;
                try
                {
                    endpoint = TcpClient.Client.RemoteEndPoint?.ToString();
                }
                catch { }
                return new ClientInfo
                {
                    Id = Id,
                    RemoteEndPoint = endpoint,
                    ConnectedAt = ConnectedAt,
                    Connected = TcpClient.Connected
                };
            }
        }
        public class ClientInfo
        {
            public string Id { get; set; }
            public string RemoteEndPoint { get; set; }
            public DateTime ConnectedAt { get; set; }
            public bool Connected { get; set; }
        }
    }
}
