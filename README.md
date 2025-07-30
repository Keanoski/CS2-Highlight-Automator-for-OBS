🎥 CS2 Highlight Automator for OBS
A C# tool that automates the extraction and recording of highlight moments from CS2 demo files using OBS Studio via obs-websocket. It detects kill streaks (5+ kills), launches CS2 for playback, and coordinates OBS to record cinematic highlight reels.

🚀 Features
📁 Parse .dem files from CS2 to identify high-kill streaks.

🔍 Automatically detect highlights with customizable kill thresholds.

🎮 Launch CS2 with demo playback and cinematic settings.

🎬 Use OBS Studio to record each highlight segment.

🛠️ Auto-generate config scripts to streamline visual setup in CS2.

🧰 Requirements
.NET 6 or later

OBS Studio with the obs-websocket plugin installed and enabled.

NuGet Dependencies
Install these packages:

bash
Kopiér
Rediger
dotnet add package DemoFile.Game.Cs
dotnet add package obs-websocket-dotnet
🛠️ Installation
Clone the repository:

bash
Kopiér
Rediger
git clone https://github.com/yourusername/cs2-highlight-automator.git
cd cs2-highlight-automator
Build the project:

bash
Kopiér
Rediger
dotnet build
Run the application:

bash
Kopiér
Rediger
dotnet run
🎮 How It Works
Provide Demo File:

Enter the full path to your .dem file from CS2.

Highlight Detection:

The tool parses the demo for player kill streaks (≥ 5 kills by default).

OBS Connection:

Prompts you to enter OBS WebSocket IP, port, and password.

Connects to OBS Studio to control recording.

CS2 Setup Script:

You’ll be asked for the path to your cs2.exe.

A cinematic configuration script (setup_script.cfg) is auto-generated.

Playback & Recording:

CS2 is launched with playback.

OBS is triggered to start/stop recording around each detected highlight.

Output:

Videos are saved in your OBS output folder.

📂 Output Example
Sample console output:

diff
Kopiér
Rediger
--- CS2 Highlight Automator for OBS ---

Found the following highlights:
- Player1 got 6 kills. (Ticks: 12345 -> 13000)

OBS recording started...
OBS recording stopped...

--- All highlights recorded! ---
⚙️ Configuration
Kill Threshold: Default is 5 kills. You can modify MIN_KILLS_FOR_HIGHLIGHT in FindHighlights() method.

OBS Settings: Ensure OBS Studio is running and the WebSocket plugin is enabled:

Default IP: 192.168.0.118

Default Port: 4455

Default Password: DcP6e0CT3QdHIiGm

You can change these defaults at runtime via console input.

🧪 Tested On
Windows 10 / 11

OBS Studio 30+ with obs-websocket plugin 5.0+

CS2 (via Steam)

❗ Known Limitations
CS2 must be installed and executable from the given path.

OBS must be properly configured before running the app.

Only supports .dem files from Counter-Strike 2.

🧑‍💻 Contributing
Pull requests and feedback are welcome. For major changes, please open an issue first to discuss what you would like to change.

📄 License
MIT License. See LICENSE for details.

🙌 Credits
DemoFile.Game.Cs for demo parsing.

obs-websocket-dotnet for OBS integration.

Let me know if you'd like this turned into a real Markdown file or need icons, badges, or extended customization (e.g. configuration options).
