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
