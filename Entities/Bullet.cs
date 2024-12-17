using Newtonsoft.Json;
using System.Drawing;

namespace GameClient.Models
{
    public class Bullet
    {
        [JsonProperty("Type")]
        public string Type = "Bullet";

        [JsonProperty("Position")]
        public PointF Position { get; set; }

        [JsonProperty("Velocity")]
        public PointF Velocity { get; set; }

        public Bullet(PointF position, PointF velocity)
        {
            Position = position;
            Velocity = velocity;
        }

        public void Update()
        {
            Position = new PointF(Position.X + Velocity.X, Position.Y + Velocity.Y);
        }
    }
}
