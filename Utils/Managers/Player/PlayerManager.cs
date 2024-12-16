using System.Collections.Generic;
using System.Drawing;
using GameClient.Models;

namespace GameClient.Utils.Managers
{
    public class PlayerManager
    {
        private List<Player> playerList = new List<Player>();
        public IEnumerable<Player> Players => playerList;

        public Player CreatePlayer(string name, Color color, PointF position)
        {
            var player = new Player(name, color, position);
            playerList.Add(player);
            return player;
        }

        public void SyncPlayersFromServer(IEnumerable<(string Name, string Color, float X, float Y)> serverPlayers)
        {
            playerList.Clear();
            foreach (var sp in serverPlayers)
            {
                playerList.Add(new Player(sp.Name, ColorTranslator.FromHtml(sp.Color), new PointF(sp.X, sp.Y)));
            }
        }
    }
}
