namespace GameClient.Interfaces
{
    public interface INetworkManager
    {
        event Action<string> OnMessageReceived;
        void SendMessage(string message);
        Task ConnectAsync();
        void Disconnect();
    }
}
