# GameClient

### Purpose

The _GameClient_ project is the **first iteration** of a client for a multiplayer game server. After several refactors and iterations, the current structure focuses on modularity, maintainability, and scalability while keeping it fun and educational. This project is part of my journey to learn C# and game development concepts.

---

### Current Features

- **Separation of Concerns**: Logic is broken into distinct components and interfaces:

  - `GameClient`: The main form handling UI, input, rendering, and game updates.
  - `NetworkManager`: Handles communication with the game server, including sending and receiving messages.
  - `PlayerManager`: Manages player creation, state, and synchronization with the server.
  - `GameManager`: Updates and manages bullet states in the game.
  - `Logger`: Provides logging to both the console and a log form for debugging.
  - `ConsoleLogForm`: Displays a log console in the UI for easier debugging.

- **Dependency Injection (DI)**: Microsoft.Extensions.DependencyInjection is used for managing dependencies across components.

- **Network Communication**: TCP communication with the server allows real-time sending and receiving of player and bullet states.

- **Smooth Player Movement and Shooting**:

  - Players can move using the `W`, `A`, `S`, `D` keys.
  - A crosshair follows the mouse cursor, and clicking shoots bullets toward the cursor.

- **Console Logging**:

  - Logs all key events, including server connections, received data, and errors.
  - Users can toggle the visibility of the log form through the menu.

- **Refactored Modular Design**:
  - Interfaces (`IPlayerManager`, `IGameManager`, `INetworkManager`) allow flexibility for future changes or enhancements.

---

### Project Structure

```
GameClient/
├── Interfaces/
│   ├── IPlayerManager.cs       # Interface for managing players
│   ├── IGameManager.cs         # Interface for managing bullets
│   └── INetworkManager.cs      # Interface for network communication
│
├── Models/
│   ├── Player.cs               # Represents a player with movement capabilities
│   └── Bullet.cs               # Represents a bullet with position and velocity
│
├── Utils/
│   ├── Logger.cs               # Static logger for console and log form
│   └── Managers/
│       ├── PlayerManager.cs    # Manages player creation and state
│       └── GameManager.cs      # Manages bullets and updates their state
│
├── FormRelated/
│   └── ConsoleLogForm.cs       # Log form to display messages
│
├── NetworkManager.cs           # Handles TCP communication with the server
├── GameClient.cs               # Main form for UI, input handling, and rendering
├── Program.cs                  # Entry point with dependency injection setup
│
└── README.md                   # Project documentation
```

---

### Key Technologies

- **C#**
- **.NET 6 / .NET Core**
- **Microsoft.Extensions.DependencyInjection** (for DI)
- **TCP Communication** (`System.Net.Sockets`)
- **WinForms** (for the game client UI)
- **Asynchronous Programming** (`async/await`)

---

### Running the Client

1. Clone the repository:

   ```bash
   git clone <repository-url>
   cd GameClient
   ```

2. Build the project using Visual Studio or the .NET CLI:

   ```bash
   dotnet build
   ```

3. Run the project:

   ```bash
   dotnet run
   ```

4. Use the client UI to:
   - Connect to the server (IP and port are hardcoded in `NetworkManager`).
   - Move the player using `W`, `A`, `S`, `D`.
   - Shoot bullets by clicking toward the mouse cursor.

---

### Known Issues & Future Plans

- **Known Issues**:

  - Currently, the server IP and port are hardcoded.
  - Bullet synchronization may experience minor delays with high latency.

- **Future Features**:
  - Add proper server discovery and dynamic configuration for server connection.
  - Implement player health, scoring, and additional game mechanics.
  - Enhance client-side prediction for smoother player movement.
  - Improve error handling and reconnection logic.

---

### Learning Process

The project has gone through several refactors to improve modularity and structure. Key takeaways so far:

1. Proper use of **interfaces** to decouple logic and enable better testing.
2. Managing **asynchronous communication** for real-time updates.
3. Applying **Dependency Injection** to simplify initialization and improve maintainability.

This is the **first iteration**, and there are many exciting things to add and refine as I continue learning!

---

### Notes

This project is part of my journey to learn C#, game development, and software architecture. Feedback and suggestions are always welcome!
