// Updated Player.cs for Movement
using System.Drawing;

namespace GameClient.Models
{
    public class Player
    {
        public string Name { get; private set; }
        public Color Color { get; private set; }
        public PointF Position { get; set; }
        public const float MovementSpeed = 5f;

        public Player(string name, Color color, PointF initialPosition)
        {
            Name = name;
            Color = color;
            Position = initialPosition;
        }
        public void SmoothMove(PointF targetPosition, float lerpFactor = 0.1f)
        {
            Position = new PointF(
                Position.X + (targetPosition.X - Position.X) * lerpFactor,
                Position.Y + (targetPosition.Y - Position.Y) * lerpFactor
            );
        }

        public void MoveUp() => Position = new PointF(Position.X, Position.Y - MovementSpeed);
        public void MoveDown() => Position = new PointF(Position.X, Position.Y + MovementSpeed);
        public void MoveLeft() => Position = new PointF(Position.X - MovementSpeed, Position.Y);
        public void MoveRight() => Position = new PointF(Position.X + MovementSpeed, Position.Y);
    }
}