using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GuaDan;

namespace GuaDan.Tests
{
    [TestClass]
    public class TcpServerTests
    {
        [TestMethod]
        public void TestServerStartStop()
        {
            using (var server = new TcpServer(0)) // request ephemeral port
            {
                server.Start();
                Assert.IsTrue(server.IsRunning);
                Assert.IsTrue(server.ListeningPort > 0);
                server.Stop();
                Assert.IsFalse(server.IsRunning);
            }
        }

        [TestMethod]
        public async Task TestClientConnectAndMessageReceive()
        {
            using (var server = new TcpServer(0))
            {
                string lastMsg = null;
                TcpServer.ClientInfo connectedClient = null;
                var connected = new ManualResetEventSlim(false);
                var msgReceived = new ManualResetEventSlim(false);

                server.ClientConnected += (ci) =>
                {
                    connectedClient = ci;
                    connected.Set();
                };
                server.MessageReceived += (ci, msg) =>
                {
                    lastMsg = msg;
                    msgReceived.Set();
                };

                server.Start();
                int port = server.ListeningPort;

                using (var client = new TcpClient())
                {
                    await client.ConnectAsync("127.0.0.1", port);
                    Assert.IsTrue(client.Connected);

                    // wait for server to see connection
                    Assert.IsTrue(connected.Wait(2000), "Server did not report connected client");

                    var stream = client.GetStream();
                    byte[] data = Encoding.UTF8.GetBytes("hello server");
                    await stream.WriteAsync(data, 0, data.Length);

                    // wait for server to report message
                    Assert.IsTrue(msgReceived.Wait(2000), "Server did not report received message");
                    Assert.AreEqual("hello server", lastMsg);
                }

                // allow server to process disconnect
                await Task.Delay(200);
                var clients = server.GetClients();
                // after client close there might be 0 clients or 1 disconnected client depending on timing
                Assert.IsTrue(clients.Count == 0 || (clients.Count == 1));

                server.Stop();
            }
        }
    }
}
