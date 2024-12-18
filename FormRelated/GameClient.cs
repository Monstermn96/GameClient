using GameClient.Models;
using Newtonsoft.Json;
using GameClient.FormRelated;
using GameClient.Interfaces.Managers;

namespace GameClient
{
    public partial class GameClient : Form
    {
        private readonly IGameStateManager gameStateManager;
        private readonly INetworkManager networkManager;
        private readonly IPlayerManager playerManager;
        private readonly HashSet<Keys> activeKeys = new HashSet<Keys>();
        private Point crosshairPosition = new Point(0, 0);
        private Player localPlayer;

        private DateTime lastFrameTime = DateTime.Now;

        public GameClient(INetworkManager networkManager, IPlayerManager playerManager, IGameStateManager gameManager)
        {
            InitializeComponent();
            if (Logger.IsInitialized())
                Logger.ToggleConsoleLog();
            this.gameStateManager = gameManager;
            this.networkManager = networkManager;
            this.playerManager = playerManager;

            Width = 1920;
            Height = 1080;
            Text = "Some Multiplayer Game";
            DoubleBuffered = true;

            string playerName = PromptForName();
            Color playerColor = PromptForColor();

            localPlayer = playerManager.CreatePlayer(playerName, playerColor, new PointF(400, 300));
            networkManager.OnMessageReceived += HandleServerMessage;
            networkManager.ConnectAsync();

            // Serialize the localPlayer into JSON object
            networkManager.SendMessage(JsonConvert.SerializeObject(new
            {
                Type = "Player",
                UserName = localPlayer.UserName,
                Color = ColorToHex(localPlayer.Color),
                Position = new { X = localPlayer.Position.X, Y = localPlayer.Position.Y }
            }));
            gameStateManager.AddPlayer(localPlayer.UserName, localPlayer);

            Application.Idle += GameLoop;

            KeyDown += (s, e) => activeKeys.Add(e.KeyCode);
            KeyUp += (s, e) => activeKeys.Remove(e.KeyCode);
            MouseMove += (s, e) => crosshairPosition = e.Location;
            MouseClick += (s, e) => Shoot();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            float deltaTime = (float)(now - lastFrameTime).TotalSeconds;
            lastFrameTime = now;

            HandleInput(deltaTime);
            gameStateManager.UpdateBullets();
            gameStateManager.GetPlayersFromServer();
            Invalidate();
        }

        private void HandleServerMessage(string message)
        {
            // Log the message and update UI or game state
            if (Logger.IsInitialized())
            {
                Logger.Log($"Server says: {message}");
            }
        }

        private void HandleInput(float deltaTime)
        {
            float deltaX = 0, deltaY = 0;
            float speed = Player.MovementSpeed * deltaTime * 60;

            if (activeKeys.Contains(Keys.W)) deltaY -= speed;
            if (activeKeys.Contains(Keys.S)) deltaY += speed;
            if (activeKeys.Contains(Keys.A)) deltaX -= speed;
            if (activeKeys.Contains(Keys.D)) deltaX += speed;

            if (deltaX != 0 || deltaY != 0)
            {
                float magnitude = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                deltaX /= magnitude;
                deltaY /= magnitude;

                localPlayer.Position = new PointF(
                    localPlayer.Position.X + deltaX * speed,
                    localPlayer.Position.Y + deltaY * speed
                );

                var movementData = new
                {
                    Type = "Player",
                    UserName = localPlayer.UserName,
                    Position = new { X = localPlayer.Position.X, Y = localPlayer.Position.Y }
                };
                Invalidate();
                networkManager.SendMessage(JsonConvert.SerializeObject(movementData));
            }

            // Toggle console log form (F1 key) on UI thread
            if (activeKeys.Contains(Keys.F1))
            {
                this.BeginInvoke(new Action(() =>
                {
                    if (Logger.IsInitialized())
                        Logger.ToggleConsoleLog();
                }));
                activeKeys.Remove(Keys.F1); // Prevent repeated toggle
            }
        }



        private void Shoot()
        {
            float deltaX = crosshairPosition.X - localPlayer.Position.X;
            float deltaY = crosshairPosition.Y - localPlayer.Position.Y;
            float magnitude = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            float bulletSpeed = 10f;
            float velocityX = (deltaX / magnitude) * bulletSpeed;
            float velocityY = (deltaY / magnitude) * bulletSpeed;

            var bulletData = new Bullet(localPlayer.Position, new PointF(velocityX, velocityY));
            gameStateManager.LocalShoot(localPlayer.Position, new PointF(velocityX, velocityY));
            networkManager.SendMessage(JsonConvert.SerializeObject(bulletData));
        }

        private string PromptForName() =>
            Microsoft.VisualBasic.Interaction.InputBox("Enter your Username:", "Player Setup", "");

        private Color PromptForColor()
        {
            using var colorDialog = new ColorDialog();
            return colorDialog.ShowDialog() == DialogResult.OK ? colorDialog.Color : Color.Blue;
        }

        private string ColorToHex(Color color) =>
            $"#{color.R:X2}{color.G:X2}{color.B:X2}";

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            var playerList = gameStateManager.GetPlayersFromServer();
            var bulletsCopy = gameStateManager.GetBulletsFromServer();

            foreach (var player in playerList)
            {
                g.FillEllipse(new SolidBrush(player.Value.Color), player.Value.Position.X, player.Value.Position.Y, 20, 20);
            }

            foreach (var bullet in bulletsCopy)
            {
                g.FillEllipse(Brushes.Red, bullet.Position.X, bullet.Position.Y, 5, 5);
            }

            g.DrawLine(Pens.Black, crosshairPosition.X, crosshairPosition.Y - 10, crosshairPosition.X, crosshairPosition.Y + 10);
            g.DrawLine(Pens.Black, crosshairPosition.X - 10, crosshairPosition.Y, crosshairPosition.X + 10, crosshairPosition.Y);
        }
    }
}
