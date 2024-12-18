using GameClient.FormRelated;
using GameClient.Interfaces.Managers;
using GameClient.Models;
using GameClient.Utils.Managers;
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
    private IGameStateManager gameStateManager;
    private bool isConnected => client?.Connected == true;
    public event Action<string> OnMessageReceived;
    public event Action<object> OnJsonReceived; // Event for handling JSON objects

    public NetworkManager(string serverAddress, int port, ConsoleLogForm consoleLogForm, IGameStateManager gameStateManager)
    {
        this.serverAddress = serverAddress;
        this.port = port;
        this.consoleLogForm = consoleLogForm;
        this.gameStateManager = gameStateManager;
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
            //_ = Task.Run(() => ProcessMessagesAsync());

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
                    TryProcessJson(message);
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

    private void TryProcessJson(string message)
    {
        try
        {
            // Strip BOM character and trim
            message = message.Trim().TrimStart('\uFEFF');

            if (string.IsNullOrEmpty(message))
            {
                Logger.Log("Empty message received. Skipping processing.");
                return;
            }

            // Deserialize into a Dictionary<string, dynamic>
            var rootObject = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(message);

            if (rootObject == null)
            {
                Logger.Log("Invalid JSON format.");
                return;
            }

            // Iterate through each item in the root object
            foreach (var entry in rootObject)
            {
                string key = entry.Key;
                dynamic nestedObject = entry.Value;

                // Check if the nested object contains a "Type" property
                if (nestedObject?.Type != null)
                {
                    string type = nestedObject.Type.ToString();

                    switch (type.ToLower())
                    {
                        case "player":
                            HandlePlayerMessage(nestedObject);
                            break;

                        case "bullet":
                            HandleBulletMessage(nestedObject);
                            break;

                        default:
                            Logger.Log($"Unknown object type '{type}' for key '{key}'.");
                            break;
                    }
                }
                else
                {
                    Logger.Log($"Skipping entry '{key}': Missing 'Type' field.");
                }
            }
        }
        catch (JsonException ex)
        {
            Logger.Log($"Message is not valid JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Log($"Unexpected error: {ex.Message}");
        }
    }


    private void HandlePlayerMessage(dynamic jsonObject)
    {
        try
        {
            // Extract data from the dynamic JSON object
            string userName = jsonObject.UserName;
            float x = (float)jsonObject.Position.X;
            float y = (float)jsonObject.Position.Y;

            // Provide default color if none is included in the JSON
            Color playerColor = Color.Blue; // Default color

            if (jsonObject.Color != null)
            {
                playerColor = Color.FromName((string)jsonObject.Color);
            }

            // Create the Player object using the required constructor
            var player = new Player(userName, playerColor, new PointF(x, y));

            // Add player to the game state
            gameStateManager.AddPlayer(userName, player);

            Logger.Log($"Processed Player: {userName} at Position ({x}, {y}).");
        }
        catch (Exception ex)
        {
            Logger.Log($"Failed to process Player message: {ex.Message}");
        }
    }


    private void HandleBulletMessage(dynamic jsonObject)
    {
        try
        {
            // Extract position and velocity from the dynamic JSON object
            float x = (float)jsonObject.Position.X;
            float y = (float)jsonObject.Position.Y;
            float velocityX = (float)jsonObject.Velocity.X;
            float velocityY = (float)jsonObject.Velocity.Y;

            // Create the Bullet object using the required constructor
            var bullet = new Bullet(new PointF(x, y), new PointF(velocityX, velocityY));

            // Add the bullet to the game state
            gameStateManager.AddBullet(bullet);

            Logger.Log($"Processed Bullet at Position ({x}, {y}) with Velocity ({velocityX}, {velocityY}).");
        }
        catch (Exception ex)
        {
            Logger.Log($"Failed to process Bullet message: {ex.Message}");
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
