
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GameClient.Models;
using GameClient.Interfaces;
using GameClient.FormRelated;
using GameClient.Utils;

namespace GameClient
{
    public partial class GameClient : Form
    {
        private readonly IGameManager gameManager;
        private readonly INetworkManager networkManager;
        private readonly IPlayerManager playerManager;
        private readonly System.Windows.Forms.Timer renderTimer;
        private readonly System.Windows.Forms.Timer inputTimer;
        private readonly HashSet<Keys> activeKeys = new HashSet<Keys>();
        private Point crosshairPosition = new Point(0, 0);
        private Player localPlayer;

        public GameClient(INetworkManager networkManager, IPlayerManager playerManager, IGameManager gameManager)
        {
            InitializeComponent();
            

            // Add menu strip
            MenuStrip menuStrip = new MenuStrip();
            ToolStripMenuItem viewMenuItem = new ToolStripMenuItem("View");
            ToolStripMenuItem toggleConsoleLogMenuItem = new ToolStripMenuItem("Toggle Console Log");
            toggleConsoleLogMenuItem.Click += (s, e) => Logger.ToggleConsoleLog();
            viewMenuItem.DropDownItems.Add(toggleConsoleLogMenuItem);
            menuStrip.Items.Add(viewMenuItem);
            MainMenuStrip = menuStrip;
            Controls.Add(menuStrip);

            this.gameManager = gameManager;
            this.networkManager = networkManager;
            this.playerManager = playerManager;

            Width = 800;
            Height = 600;
            Text = "Some Multiplayer Game";
            DoubleBuffered = true;

            string playerName = PromptForName();
            Color playerColor = PromptForColor();

            localPlayer = playerManager.CreatePlayer(playerName, playerColor, new PointF(400, 300));
            networkManager.OnMessageReceived += UpdateFromServer;
            //Connect to server
            networkManager.ConnectAsync();
            networkManager.SendMessage($"{playerName}|{ColorToHex(playerColor)}");

            renderTimer = new System.Windows.Forms.Timer { Interval = 12 };
            renderTimer.Tick += (s, e) =>
            {
                gameManager.UpdateBullets();
                Invalidate();
            };
            renderTimer.Start();

            inputTimer = new System.Windows.Forms.Timer { Interval = 40 };
            inputTimer.Tick += (s, e) => HandleInput();
            inputTimer.Start();

            KeyDown += (s, e) => activeKeys.Add(e.KeyCode);
            KeyUp += (s, e) => activeKeys.Remove(e.KeyCode);
            MouseMove += (s, e) => crosshairPosition = e.Location;
            MouseClick += (s, e) => Shoot();
        }

        private void HandleInput()
        {
            float deltaX = 0;
            float deltaY = 0;

            foreach (var key in activeKeys)
            {
                switch (key)
                {
                    case Keys.W: deltaY -= Player.MovementSpeed; break;
                    case Keys.S: deltaY += Player.MovementSpeed; break;
                    case Keys.A: deltaX -= Player.MovementSpeed; break;
                    case Keys.D: deltaX += Player.MovementSpeed; break;
                }
            }

            if (deltaX != 0 || deltaY != 0)
            {
                localPlayer.Position = new PointF(localPlayer.Position.X + deltaX, localPlayer.Position.Y + deltaY);
                networkManager.SendMessage($"player,{localPlayer.Name},{localPlayer.Position.X},{localPlayer.Position.Y}");
            }
        }

        private void UpdateFromServer(string serverState)
        {
            var serverPlayers = new List<(string Name, string Color, float X, float Y)>();
            var serverBullets = new List<(float X, float Y, float VelocityX, float VelocityY)>();

            var entries = serverState.Split('|');

            foreach (var entry in entries)
            {
                var parts = entry.Split(',');
                if (parts[0] == "player")
                {
                    serverPlayers.Add((parts[1], parts[2], float.Parse(parts[3]), float.Parse(parts[4])));
                }
                else if (parts[0] == "bullet")
                {
                    serverBullets.Add((float.Parse(parts[2]), float.Parse(parts[3]), float.Parse(parts[4]), float.Parse(parts[5])));
                }
            }

            // Sync players and bullets
            playerManager.SyncPlayersFromServer(serverPlayers);
            gameManager.UpdateBulletsFromServer(serverBullets);
            Invalidate();
        }

        private void Shoot()
        {
            float deltaX = crosshairPosition.X - localPlayer.Position.X;
            float deltaY = crosshairPosition.Y - localPlayer.Position.Y;
            float magnitude = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            float bulletSpeed = 10f;
            float velocityX = (deltaX / magnitude) * bulletSpeed;
            float velocityY = (deltaY / magnitude) * bulletSpeed;

            string bulletData = $"bullet,{localPlayer.Name},{localPlayer.Position.X},{localPlayer.Position.Y},{velocityX},{velocityY}";
            networkManager.SendMessage(bulletData);
        }

        private string PromptForName() =>
            Microsoft.VisualBasic.Interaction.InputBox("Enter your player name:", "Player Setup", "Player");

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

            var playersCopy = playerManager.Players.ToList();
            var bulletsCopy = gameManager.Bullets.ToList();

            foreach (var player in playersCopy)
            {
                g.FillEllipse(new SolidBrush(player.Color), player.Position.X, player.Position.Y, 20, 20);
                g.DrawString(player.Name, DefaultFont, Brushes.Black, player.Position.X, player.Position.Y - 25);
            }

            foreach (var bullet in bulletsCopy)
            {
                g.FillEllipse(Brushes.Red, bullet.Position.X, bullet.Position.Y, 5, 5);
            }

            g.DrawLine(Pens.Black, crosshairPosition.X, crosshairPosition.Y - 10, crosshairPosition.X, crosshairPosition.Y + 10);
            g.DrawLine(Pens.Black, crosshairPosition.X - 10, crosshairPosition.Y, crosshairPosition.X + 10, crosshairPosition.Y);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            // Ensure the ConsoleLogForm is hidden when the main form closes
            Logger.ToggleConsoleLog();

            Application.Exit(); // Exit the application cleanly
        }

    }
}
