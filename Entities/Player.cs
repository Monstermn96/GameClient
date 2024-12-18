using Newtonsoft.Json;

namespace GameClient.Models
{
    public class Player
    {
        [JsonProperty("Type")]
        public string Type = "Player";

        [JsonProperty("UserName")]
        public string UserName { get; set; }
        [JsonProperty("Color")]
        public Color Color { get; set; }
        [JsonProperty("Position")]
        public PointF Position { get; set; }
        [JsonProperty("MovementSpeed")]
        public const float MovementSpeed = 5f;

        public Player(string userName, Color color, PointF initialPosition)
        {
            UserName = userName;
            Color = color;
            Position = initialPosition;
        }
    }
}
