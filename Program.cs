using HighlightReel.Models;
using HighlightReel.Services;
using HighlightReel.Utilities;

class Program
{
    public static async Task Main()
    {
        Console.WriteLine("--- CS2 Highlight Automator for OBS ---");

        var demoPath = ConsoleHelper.Prompt("Enter the full path to your CS2 demo file:");
        if (!File.Exists(demoPath))
        {
            Console.WriteLine("Invalid demo file path.");
            return;
        }

        var highlights = await HighlightFinder.FindHighlightsAsync(demoPath);
        if (!highlights.Any())
        {
            Console.WriteLine("No valid highlights found.");
            return;
        }

        ConsoleHelper.DisplayHighlights(highlights);

        var obs = new OBSController();
        if (!obs.Connect()) return;

        var cs2Path = CS2Launcher.GetCs2Path();
        if (string.IsNullOrEmpty(cs2Path)) return;

        SetupScriptGenerator.Generate(cs2Path);

        Console.WriteLine("Press Enter to begin recording...");
        Console.ReadLine();

        var cs2Process = CS2Launcher.Launch(cs2Path, demoPath);
        if (cs2Process == null) return;

        await Task.Delay(15000); // Wait for CS2 to load

        await obs.RecordHighlightsAsync(cs2Process, highlights);

        Console.WriteLine("All highlights recorded.");
        obs.Disconnect();
        cs2Process.Kill();
    }
}
