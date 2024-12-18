using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using GameClient.Models;
using GameClient.Interfaces.Managers;
using GameClient.Interfaces;

namespace GameClient.Utils.Managers
{
    public class GameStateManager : IGameStateManager
    {
        private readonly IGameState gameState;

        public GameStateManager(IGameState gameState) 
        { 
            this.gameState = gameState;
        }
        public void AddPlayer(string userName, Player player)
        {
            gameState.PlayerList.Add(userName, player);
        }
        public void AddBullet(Bullet bullet)
        {
            gameState.BulletList.Add(bullet);
        }

        public void ServerShoot(PointF position, PointF velocity)
        {
            gameState.BulletList.Add(new Bullet(position, velocity));
        }

        public void LocalShoot(PointF position, PointF velocity)
        {
            gameState.BulletList.Add(new Bullet(position, velocity));
        }

        public Dictionary<string, Player> GetPlayersFromServer()
        {
            return gameState.PlayerList;
        }
        public List<Bullet> GetBulletsFromServer()
        {
            return gameState.BulletList;
        }

        public void UpdateBullets()
        {
            foreach (var bullet in gameState.BulletList)
            {
                bullet.Update();
            }
        }
    }
}
