namespace GameClient.Interfaces.Managers
{
    public interface INetworkManager
    {
        event Action<string> OnMessageReceived;
        void SendMessage(string message);
        Task ConnectAsync();
        void Disconnect();
    }
}
