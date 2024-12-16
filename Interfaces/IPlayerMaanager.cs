using System.Collections.Generic;
using System.Drawing;
using GameClient.Models;

namespace GameClient.Interfaces
{
    public interface IPlayerManager
    {
        IEnumerable<Player> Players { get; }
        Player CreatePlayer(string name, Color color, PointF position);
        void SyncPlayersFromServer(IEnumerable<(string Name, string Color, float X, float Y)> serverPlayers);
    }
}
