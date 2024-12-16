// Refactored GameClient.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GameClient.Models;
using GameClient.Utils.Managers;

namespace GameClient
{
    public partial class GameClient : Form
    {
        private GameManager gameManager;
        private NetworkManager networkManager;
        private PlayerManager playerManager;
        private System.Windows.Forms.Timer renderTimer;
        private System.Windows.Forms.Timer inputTimer;
        private HashSet<Keys> activeKeys = new HashSet<Keys>();
        private Point crosshairPosition = new Point(0, 0);
        private Player localPlayer;

        public GameClient()
        {
            InitializeComponent();

            Width = 800;
            Height = 600;
            Text = "Some Multiplayer Game";
            DoubleBuffered = true;

            string playerName = PromptForName();
            Color playerColor = PromptForColor();

            playerManager = new PlayerManager();
            localPlayer = playerManager.CreatePlayer(playerName, playerColor, new PointF(400, 300));

            gameManager = new GameManager();
            networkManager = new NetworkManager("50.54.113.242", 5555);
            networkManager.OnMessageReceived += UpdateFromServer;

            networkManager.SendMessage($"{playerName}|{ColorToHex(playerColor)}");

            SetupTimers();

            KeyDown += (s, e) => activeKeys.Add(e.KeyCode);
            KeyUp += (s, e) => activeKeys.Remove(e.KeyCode);
            MouseMove += (s, e) => crosshairPosition = e.Location;
            MouseClick += (s, e) => Shoot();
        }

        private void SetupTimers()
        {
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
                networkManager.SendMessage($"move,{localPlayer.Position.X},{localPlayer.Position.Y}");
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

            playerManager.SyncPlayersFromServer(serverPlayers);
            gameManager.UpdateBulletsFromServer(serverBullets);
            Invalidate(); // Redraw the game
        }


        private string PromptForName()
        {
            return Microsoft.VisualBasic.Interaction.InputBox("Enter your player name:", "Player Setup", "Player");
        }

        private Color PromptForColor()
        {
            ColorDialog colorDialog = new ColorDialog();
            return colorDialog.ShowDialog() == DialogResult.OK ? colorDialog.Color : Color.Blue;
        }

        private string ColorToHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private void Shoot()
        {
            // Calculate bullet direction based on crosshair position
            float deltaX = crosshairPosition.X - localPlayer.Position.X;
            float deltaY = crosshairPosition.Y - localPlayer.Position.Y;
            float magnitude = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            // Normalize the direction vector and scale to bullet speed
            float bulletSpeed = 10f;
            float velocityX = (deltaX / magnitude) * bulletSpeed;
            float velocityY = (deltaY / magnitude) * bulletSpeed;

            string bulletData = $"bullet,{localPlayer.Name},{localPlayer.Position.X},{localPlayer.Position.Y},{velocityX},{velocityY}";
            networkManager.SendMessage(bulletData);
        }



        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            var playersCopy = playerManager.Players.ToList(); // Create a copy of players
            var bulletsCopy = gameManager.Bullets.ToList();   // Create a copy of bullets

            foreach (var player in playersCopy)
            {
                g.FillEllipse(new SolidBrush(player.Color), player.Position.X, player.Position.Y, 20, 20);
                g.DrawString(player.Name, DefaultFont, Brushes.Black, player.Position.X, player.Position.Y - 25);
            }

            foreach (var bullet in bulletsCopy)
            {
                g.FillEllipse(Brushes.Red, bullet.Position.X, bullet.Position.Y, 5, 5);
            }

            // Crosshair drawing (example)
            g.DrawLine(Pens.Black, crosshairPosition.X, crosshairPosition.Y - 10, crosshairPosition.X, crosshairPosition.Y + 10);
            g.DrawLine(Pens.Black, crosshairPosition.X - 10, crosshairPosition.Y, crosshairPosition.X + 10, crosshairPosition.Y);
        }


    }
}