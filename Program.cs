// To run this code, you'll need to add the following NuGet packages:
// dotnet add package DemoFile.Game.Cs
// dotnet add package obs-websocket-dotnet

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DemoFile;
using DemoFile.Sdk;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;

public class HighlightReel
{
    // A simple class to store information about a highlight
    public class Highlight
    {
        public string PlayerName { get; set; }
        public int StartTick { get; set; }
        public int EndTick { get; set; }
        public int KillCount { get; set; }
    }

    private static readonly OBSWebsocket obs = new OBSWebsocket();

    public static async Task Main(string[] args)
    {
        Console.WriteLine("--- CS2 Highlight Automator for OBS ---");

        // Step 1: Find Highlights
        Console.WriteLine("\nEnter the full path to your CS2 demo file:");
        var demoPath = Console.ReadLine();

        if (string.IsNullOrEmpty(demoPath) || !File.Exists(demoPath))
        {
            Console.WriteLine("Invalid file path.");
            return;
        }

        var highlights = await FindHighlights(demoPath);

        if (!highlights.Any())
        {
            Console.WriteLine("No highlights found according to the defined criteria (streaks of 5+ kills).");
            return;
        }

        Console.WriteLine("\nFound the following highlights:");
        foreach (var highlight in highlights)
        {
            Console.WriteLine($"- {highlight.PlayerName} got {highlight.KillCount} kills. (Ticks: {highlight.StartTick} -> {highlight.EndTick})");
        }

        // Step 2: Connect to OBS
        if (!ConnectToObs())
        {
            return;
        }

        // Step 3: Get CS2 Path and generate a simple setup script
        Console.WriteLine("\n--- CS2 SETUP ---");
        string cs2Path = GetCs2Path();
        if (string.IsNullOrEmpty(cs2Path)) return;

        GenerateSetupScript(cs2Path);
        Console.WriteLine("\nSuccessfully generated setup script: 'setup_script.cfg'.");

        // Step 4: Launch CS2 and start the recording sequence
        Console.WriteLine("\n--- AUTOMATIC RECORDING ---");
        Console.WriteLine("The program will now launch CS2 and control OBS to record the clips.");
        Console.WriteLine("Press Enter to begin...");
        Console.ReadLine();

        var cs2Process = LaunchCs2ForPlayback(cs2Path, demoPath);
        if (cs2Process == null) return;

        Console.WriteLine("\nCS2 launched. Waiting for game to load before starting recording sequence (15 seconds)...");
        await Task.Delay(15000);

        await RecordHighlightsWithObs(cs2Process, highlights);

        Console.WriteLine("\n--- All highlights recorded! ---");
        obs.Disconnect();
        cs2Process.Kill();
    }

    private static bool ConnectToObs()
    {
        Console.WriteLine("\n--- OBS CONNECTION ---");
        Console.WriteLine("Please ensure OBS is running with the obs-websocket plugin installed and enabled.");

        Console.WriteLine("Enter OBS WebSocket Server IP (Default: 192.168.0.118):");
        string serverIp = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(serverIp)) serverIp = "192.168.0.118";

        Console.WriteLine("Enter OBS WebSocket Server Port (Default: 4455):");
        string serverPort = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(serverPort)) serverPort = "4455";

        Console.WriteLine("Enter OBS WebSocket Server Password (Default: DcP6e0CT3QdHIiGm):");
        string serverPassword = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(serverPassword)) serverPassword = "DcP6e0CT3QdHIiGm";

        string serverUrl = $"ws://{serverIp}:{serverPort}";

        try
        {
            obs.ConnectAsync(serverUrl, serverPassword);

            Console.Write("Connecting...");
            for (int i = 0; i < 50; i++) // Wait up to 5 seconds
            {
                if (obs.IsConnected)
                {
                    Console.WriteLine("\nSuccessfully connected to OBS.");
                    return true;
                }
                Thread.Sleep(100);
                Console.Write(".");
            }

            Console.WriteLine("\n");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Failed to connect to OBS: Connection timed out.");
            Console.WriteLine("Please check that OBS is running and the websocket server is enabled (Tools -> WebSocket Server Settings).");
            Console.ResetColor();
            return false;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nFailed to connect to OBS: {ex.Message}");
            Console.WriteLine("Please check that OBS is running and the websocket server is enabled (Tools -> WebSocket Server Settings).");
            Console.ResetColor();
        }
        return false;
    }

    private static async Task RecordHighlightsWithObs(Process cs2Process, List<Highlight> highlights)
    {
        var console = cs2Process.StandardInput;
        await console.WriteLineAsync("exec setup_script.cfg");

        foreach (var highlight in highlights)
        {
            Console.WriteLine($"\nRecording clip for {highlight.PlayerName}...");

            const int PRE_ROLL_TICKS = 4 * 64;
            const int POST_ROLL_TICKS = 3 * 64;
            int startTick = highlight.StartTick - PRE_ROLL_TICKS;
            int endTick = highlight.EndTick + POST_ROLL_TICKS;
            int durationTicks = endTick - startTick;
            int durationMs = (int)Math.Round(durationTicks / 64.0 * 1000);

            await console.WriteLineAsync($"demo_gototick {startTick}");
            await console.WriteLineAsync($"spec_player_by_name \"{highlight.PlayerName}\"");
            await console.WriteLineAsync("demo_pause");
            await Task.Delay(500);

            obs.StartRecord();
            Console.WriteLine("OBS recording started.");

            await console.WriteLineAsync("demo_resume");
            await Task.Delay(durationMs);

            obs.StopRecord();
            Console.WriteLine("OBS recording stopped.");

            await console.WriteLineAsync("demo_pause");
            await Task.Delay(500);
        }
    }

    public static async Task<List<Highlight>> FindHighlights(string demoPath)
    {
        var playerKills = new Dictionary<string, List<int>>();
        var demo = new CsDemoParser();
        demo.Source1GameEvents.PlayerDeath += e =>
        {
            var attackerName = e.Attacker?.PlayerName;
            if (attackerName != null && e.Player != null)
            {
                if (!playerKills.ContainsKey(attackerName))
                {
                    playerKills[attackerName] = new List<int>();
                }
                playerKills[attackerName].Add(demo.CurrentDemoTick.Value);
            }
        };

        using (var stream = File.OpenRead(demoPath))
        {
            var reader = DemoFileReader.Create(demo, stream);
            await reader.ReadAllAsync();
        }

        var highlights = new List<Highlight>();
        const int MAX_TICK_DIFFERENCE_FOR_STREAK = 10 * 64;
        const int MIN_KILLS_FOR_HIGHLIGHT = 5; // Changed from 3 to 5

        foreach (var (playerName, killTicks) in playerKills)
        {
            if (killTicks.Count < MIN_KILLS_FOR_HIGHLIGHT) continue;
            killTicks.Sort();
            var currentStreak = new List<int>();
            for (int i = 0; i < killTicks.Count; i++)
            {
                if (currentStreak.Count == 0 || killTicks[i] - currentStreak.Last() <= MAX_TICK_DIFFERENCE_FOR_STREAK)
                {
                    currentStreak.Add(killTicks[i]);
                }
                else
                {
                    if (currentStreak.Count >= MIN_KILLS_FOR_HIGHLIGHT)
                    {
                        highlights.Add(new Highlight { PlayerName = playerName, StartTick = currentStreak.First(), EndTick = currentStreak.Last(), KillCount = currentStreak.Count });
                    }
                    currentStreak = new List<int> { killTicks[i] };
                }
            }
            if (currentStreak.Count >= MIN_KILLS_FOR_HIGHLIGHT)
            {
                highlights.Add(new Highlight { PlayerName = playerName, StartTick = currentStreak.First(), EndTick = currentStreak.Last(), KillCount = currentStreak.Count });
            }
        }
        return highlights.OrderBy(h => h.StartTick).ToList();
    }

    public static string GetCs2Path()
    {
        string defaultPath = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\game\bin\win64\cs2.exe";
        while (true)
        {
            Console.WriteLine("\nPlease enter the full path to your cs2.exe");
            Console.WriteLine($"(Press Enter to use default: {defaultPath})");
            Console.Write("> ");
            string path = Console.ReadLine();
            if (string.IsNullOrEmpty(path)) path = defaultPath;
            if (File.Exists(path) && path.EndsWith("cs2.exe", StringComparison.OrdinalIgnoreCase)) return path;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid path.");
            Console.ResetColor();
            Console.WriteLine("Press Enter to try again, or type 'skip' to cancel.");
            if (Console.ReadLine()?.ToLower() == "skip") return null;
        }
    }

    public static void GenerateSetupScript(string cs2ExePath)
    {
        var gameDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(cs2ExePath), "..", ".."));
        var cfgDir = Path.Combine(gameDir, "csgo", "cfg");
        if (!Directory.Exists(cfgDir)) Directory.CreateDirectory(cfgDir);

        var scriptPath = Path.Combine(cfgDir, "setup_script.cfg");
        var scriptContent = new StringBuilder();
        scriptContent.AppendLine("sv_cheats 1");
        scriptContent.AppendLine("echo \"--- Applying cinematic settings... ---\"");
        scriptContent.AppendLine("cl_draw_only_deathnotices 1; mp_display_kill_assists 0; spec_show_xray 0; net_graph 0;");
        scriptContent.AppendLine("cl_viewmodel_shift_left_amt 0; cl_viewmodel_shift_right_amt 0; cl_bob_lower_amt 5; cl_bobamt_lat 0; cl_bobamt_vert 0; cl_bobcycle 2; viewmodel_offset_x 2; viewmodel_offset_y 0; viewmodel_offset_z -2;");
        scriptContent.AppendLine("voice_enable 0; bot_chatter off; snd_setmixer dialog vol 0; sv_ignoregrenaderadio 1;");
        File.WriteAllText(scriptPath, scriptContent.ToString());
    }

    public static Process LaunchCs2ForPlayback(string cs2ExePath, string demoPath)
    {
        try
        {
            string arguments = $"-insecure -condebug +playdemo \"{demoPath}\"";
            var startInfo = new ProcessStartInfo
            {
                FileName = cs2ExePath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardInput = true,
            };
            Console.WriteLine($"\nLaunching CS2 with arguments: {arguments}");
            return Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nFailed to launch CS2: {ex.Message}");
            Console.ResetColor();
            return null;
        }
    }

    public static void PrintPostLaunchInstructions(string cs2ExePath)
    {
        Console.WriteLine("\n--- All Done! ---");
        Console.WriteLine("Your clips have been recorded by OBS.");
        Console.WriteLine("You can now find the video files in the output directory you configured in OBS settings.");
    }
}
