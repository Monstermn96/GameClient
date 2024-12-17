using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using GameClient.Models;
using GameClient.Interfaces;

namespace GameClient.Utils.Managers
{
    public class GameManager : IGameManager
    {
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
        public void LocalShoot(PointF position, PointF velocity)
        {
            Bullets.Add(new Bullet(position, velocity));
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
