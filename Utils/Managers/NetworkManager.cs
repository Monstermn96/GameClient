using GameClient.FormRelated;
using GameClient.Interfaces;
using GameClient.Utils;
using System.Net.Sockets;
using System.Text;

public class NetworkManager : INetworkManager
{
    private readonly string serverAddress;
    private readonly int port;
    private TcpClient client;
    private StreamReader reader;
    private StreamWriter writer;
    public ConsoleLogForm consoleLogForm;

    public event Action<string> OnMessageReceived;

    public NetworkManager(string serverAddress, int port, ConsoleLogForm consoleLogForm)
    {
        this.serverAddress = serverAddress;
        this.port = port;
        this.consoleLogForm = consoleLogForm;
    }

    public async Task ConnectAsync()
    {
        try
        {
            client = new TcpClient();
            await client.ConnectAsync(serverAddress, port);
            var stream = client.GetStream();
            reader = new StreamReader(stream, Encoding.UTF8);
            writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            Logger.Log("Connected to server.");

            // Start listening for messages
            _ = Task.Run(async () =>
            {
                try
                {
                    while (client.Connected)
                    {
                        string message = await reader.ReadLineAsync();
                        if (message == null) break;

                        OnMessageReceived?.Invoke(message);
                        Logger.Log($"Received: {message}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Disconnected from server: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            Logger.Log($"Error connecting to server: {ex.Message}");
        }
    }




    public async Task SendMessageAsync(string message)
    {
        if (client?.Connected == true && writer != null)
        {
            await writer.WriteLineAsync(message);
            Logger.Log($"Sent: {message}");
        }
        else
        {
            Logger.Log("Failed to send message: Not connected to server or writer not initialized.");
        }
    }


    public void Disconnect()
    {
        client?.Close();
        Logger.Log("Disconnected from server.");
    }

    public void SendMessage(string message)
    {
        if (client?.Connected == true && writer != null)
        {
            Task.Run(async () => await SendMessageAsync(message));
        }
        else
        {
            Logger.Log("Cannot send message: Not connected to the server.");
        }
    }

}
