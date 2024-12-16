using System.Drawing;

namespace GameClient.Models
{
    public class Bullet
    {
        public PointF Position { get; set; }
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
