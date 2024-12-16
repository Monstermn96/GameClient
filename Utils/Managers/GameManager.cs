using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using GameClient.Models;

namespace GameClient.Utils.Managers
{
    public class GameManager
    {
        public ConcurrentBag<Player> Players { get; private set; } = new ConcurrentBag<Player>();
        public ConcurrentBag<Bullet> Bullets { get; private set; } = new ConcurrentBag<Bullet>();

        public GameManager() { }

        public void UpdateBulletsFromServer(IEnumerable<(float X, float Y, float VelocityX, float VelocityY)> serverBullets)
        {
            Bullets = new ConcurrentBag<Bullet>();
            foreach (var b in serverBullets)
            {
                Bullets.Add(new Bullet(new PointF(b.X, b.Y), new PointF(b.VelocityX, b.VelocityY)));
            }
        }

        public void UpdateBullets()
        {
            foreach (var bullet in Bullets)
            {
                bullet.Update();
            }
        }
    }
}
