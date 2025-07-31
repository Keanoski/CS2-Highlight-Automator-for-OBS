# 🎥 CS2 Highlight Automator for OBS

A C# tool that automates the extraction and recording of highlight moments from CS2 demo files using **OBS Studio** via [`obs-websocket`](https://github.com/obsproject/obs-websocket). It detects kill streaks (5+ kills), launches CS2 for playback, and coordinates OBS to record cinematic highlight reels.

---

## 🚀 Features

- 📁 Parse `.dem` files from CS2 to identify high-kill streaks.
- 🔍 Automatically detect highlights with customizable kill thresholds.
- 🎮 Launch CS2 with demo playback and cinematic settings.
- 🎬 Use OBS Studio to record each highlight segment.
- 🛠️ Auto-generate config scripts to streamline visual setup in CS2.

---

## 🧰 Requirements

- [.NET 6 or later](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- [OBS Studio](https://obsproject.com/) with the [`obs-websocket`](https://github.com/obsproject/obs-websocket) plugin installed and enabled.

---

## 📦 NuGet Dependencies

Install the required packages:

```bash
dotnet add package DemoFile.Game.Cs
dotnet add package obs-websocket-dotnet
```

## How It Works

### ➤ Provide Demo File
Enter the full path to your `.dem` file from CS2.

### ➤ Highlight Detection
The tool parses the demo for player kill streaks (≥ 5 kills by default).

### ➤ OBS Connection
Prompts you to enter OBS WebSocket IP, port, and password.  
Connects to OBS Studio to control recording.

### ➤ CS2 Setup Script
You’ll be asked for the path to your `cs2.exe`.  
A cinematic configuration script (`setup_script.cfg`) is auto-generated.

### ➤ Playback & Recording
CS2 is launched with the demo.  
OBS is triggered to start/stop recording around each detected highlight.

### ➤ Output
Videos are saved in your OBS output folder.

## 📂 Output Example

Sample console output:
```bash

--- CS2 Highlight Automator for OBS ---

Found the following highlights:
- Player1 got 6 kills. (Ticks: 12345 -> 13000)

OBS recording started...
OBS recording stopped...

--- All highlights recorded! ---

```
