using GameClient.FormRelated;
using GameClient.Interfaces;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using System.Windows.Forms;

public class NetworkManager : INetworkManager
{
    private readonly string serverAddress;
    private readonly int port;
    private TcpClient client;
    private StreamReader reader;
    private StreamWriter writer;
    private Channel<string> messageChannel = Channel.CreateUnbounded<string>();
    public ConsoleLogForm consoleLogForm;
    private bool isConnected => client?.Connected == true;
    public event Action<string> OnMessageReceived;
    public event Action<object> OnJsonReceived; // Event for handling JSON objects

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

            if (Logger.IsInitialized())
            {
                Logger.Log("Connected to server.");
            }
            else
            {
                Console.WriteLine("Connected to server.");
            }

            // Start message processing
            _ = Task.Run(() => ProcessMessagesAsync());

            // Start listening for messages
            _ = Task.Run(async () => await ListenForMessagesAsync());
        }
        catch (Exception ex)
        {
            if (Logger.IsInitialized())
            {
                Logger.Log($"Error connecting to server: {ex.Message}");
            }
            else
            {
                Console.WriteLine($"Error connecting to server: {ex.Message}");
            }
        }
    }

    private async Task ProcessMessagesAsync()
    {
        await foreach (var message in messageChannel.Reader.ReadAllAsync())
        {
            try
            {
                // Attempt JSON processing
                TryProcessJson(message);
            }
            catch (Exception)
            {
                // Invoke plain message handler
                OnMessageReceived?.Invoke(message);
            }
        }
    }

    private async Task ListenForMessagesAsync()
    {
        try
        {
            while (client.Connected)
            {
                string message = await reader.ReadLineAsync();
                if (message == null) break;

                if (Logger.IsInitialized())
                {
                    Logger.Log($"Received: {message}");
                }
                else
                {
                    Console.WriteLine($"Received: {message}");
                }

                // Queue message for processing
                await messageChannel.Writer.WriteAsync(message);
            }
        }
        catch (Exception ex)
        {
            if (Logger.IsInitialized())
            {
                Logger.Log($"Disconnected from server: {ex.Message}");
            }
            else
            {
                Console.WriteLine($"Disconnected from server: {ex.Message}");
            }
        }
        finally
        {
            messageChannel.Writer.Complete();
        }
    }

    /// <summary>
    /// Attempts to process the message as JSON without breaking the loop on failure.
    /// </summary>
    private void TryProcessJson(string message)
    {
        try
        {
            var jsonObject = JsonConvert.DeserializeObject<object>(message);
            if (jsonObject != null)
            {
                if (Logger.IsInitialized())
                {
                    Logger.Log("Successfully processed message as JSON.");
                }
                else
                {
                    Console.WriteLine("Successfully processed message as JSON.");
                }
                OnJsonReceived?.Invoke(jsonObject);
            }
        }
        catch (JsonException)
        {
            if (Logger.IsInitialized())
            {
                Logger.Log("Message is not valid JSON. Processing as plain text.");
            }
            else
            {
                Console.WriteLine("Message is not valid JSON. Processing as plain text.");
            }
            throw;
        }
    }

    private string SerializeObject(object obj)
    {
        string message;
        try
        {
            message = JsonConvert.SerializeObject(obj);
        }
        catch (Exception)
        {
            return "Failed to serialize JSON.";
        }
        return message;
    }

    public async Task SendMessageAsync(string message)
    {
        if (client?.Connected == true && writer != null)
        {
            await writer.WriteLineAsync(message);
            if (Logger.IsInitialized())
            {
                Logger.Log($"Sent: {message}");
            }
            else
            {
                Console.WriteLine($"Sent: {message}");
            }
        }
        else
        {
            if (Logger.IsInitialized())
            {
                Logger.Log("Failed to send message: Not connected to server or writer not initialized.");
            }
            else
            {
                Console.WriteLine("Failed to send message: Not connected to server or writer not initialized.");
            }
        }
    }

    public void Disconnect()
    {
        client?.Close();
        if (Logger.IsInitialized())
        {
            Logger.Log("Disconnected from server.");
        }
        else
        {
            Console.WriteLine("Disconnected from server.");
        }
    }

    public void SendMessage(string message)
    {
        if (client?.Connected == true && writer != null)
        {
            Task.Run(async () => await SendMessageAsync(message));
        }
        else
        {
            if (Logger.IsInitialized())
            {
                Logger.Log("Cannot send message: Not connected to the server.");
            }
            else
            {
                Console.WriteLine("Cannot send message: Not connected to the server.");
            }
        }
    }
}
