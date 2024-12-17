using System.Collections.Concurrent;
using GameClient.Models;

namespace GameClient.Interfaces.Managers
{
    public interface IGameStateManager
    {
        void AddPlayer(string userName, Player player);
        void UpdateBullets();
        Dictionary<string, Player> GetPlayersFromServer();
        List<Bullet> GetBulletsFromServer();
        void LocalShoot(PointF position, PointF velocity);
    }
}
