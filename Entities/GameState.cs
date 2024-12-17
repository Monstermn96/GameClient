using GameClient.Models;
using GameClient.Interfaces;
using Newtonsoft.Json;

namespace GameClient.Entities
{
    public class GameState : IGameState
    {
        [JsonProperty("PlayerList")]
        public Dictionary<string, Player> PlayerList { get; set; }

        [JsonProperty("BulletList")]
        public List<Bullet> BulletList { get; set; }

        public GameState()
        {
            PlayerList = new Dictionary<string, Player>();
            BulletList = new List<Bullet>();
        }
    }
}