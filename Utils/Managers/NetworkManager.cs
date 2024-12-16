// NetworkManager.cs
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GameClient.Utils.Managers
{
    public class NetworkManager
    {
        private TcpClient client;
        private NetworkStream stream;

        public event Action<string> OnMessageReceived;

        public NetworkManager(string serverAddress, int port)
        {
            client = new TcpClient(serverAddress, port);
            stream = client.GetStream();

            Thread receiveThread = new Thread(ReceiveState);
            receiveThread.Start();
        }

        public void SendMessage(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            stream.Write(buffer, 0, buffer.Length);
        }

        private void ReceiveState()
        {
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string state = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    OnMessageReceived?.Invoke(state);
                }
            }
            catch
            {
                Console.WriteLine("Disconnected from server.");
            }
        }

        public void Disconnect()
        {
            stream?.Close();
            client?.Close();
        }
    }
}
