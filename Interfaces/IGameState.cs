using GameClient.Models;

namespace GameClient.Interfaces
{
    public interface IGameState
    {
        public Dictionary<string, Player> PlayerList { get; set; }

        public List<Bullet> BulletList { get; set; }
    }
}