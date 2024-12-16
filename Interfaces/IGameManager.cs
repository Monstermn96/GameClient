using System.Collections.Concurrent;
using GameClient.Models;

namespace GameClient.Interfaces
{
    public interface IGameManager
    {
        ConcurrentBag<Bullet> Bullets { get; }
        void UpdateBullets();
        void UpdateBulletsFromServer(IEnumerable<(float X, float Y, float VelocityX, float VelocityY)> serverBullets);
    }
}
