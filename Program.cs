using Microsoft.Extensions.DependencyInjection;
using GameClient.Utils.Managers;
using GameClient.FormRelated;
using GameClient.Utils;
using GameClient.Interfaces.Managers;
using GameClient.Interfaces;
using GameClient.Entities;

namespace GameClient
{
    internal static class Program
    {

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var services = ConfigureServices();
            using var serviceProvider = services.BuildServiceProvider();

            var mainForm = serviceProvider.GetRequiredService<GameClient>();
            Application.Run(mainForm);
        }

        private static IServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();

            //Logger
            var consoleLogForm = new ConsoleLogForm();
            Logger.Initialize(consoleLogForm);
            services.AddSingleton(consoleLogForm);

            // Register interfaces and implementations
            services.AddSingleton<IPlayerManager, PlayerManager>();
            services.AddSingleton<IGameStateManager, GameStateManager>();
            services.AddSingleton<IGameState, GameState>();

            // Register NetworkManager with parameters for serverAddress and port
            services.AddSingleton<INetworkManager>(provider =>
                new NetworkManager("50.54.113.242", 5555, consoleLogForm)); // Provide serverAddress and port here

            // Register the GameClient form
            services.AddTransient<GameClient>();

            return services;
        }
    }
}