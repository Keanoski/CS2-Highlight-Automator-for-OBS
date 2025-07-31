using System;
using System.Diagnostics;
using HighlightReel.Utilities;

namespace HighlightReel.Services
{
    public static class CS2Launcher
    {
        public static string GetCs2Path()
        {
            const string defaultPath = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\game\bin\win64\cs2.exe";

            while (true)
            {
                var input = ConsoleHelper.Prompt("Path to cs2.exe:", defaultPath);

                if (File.Exists(input) && input.EndsWith("cs2.exe"))
                    return input;

                Console.WriteLine("Invalid path. Press Enter to try again, or type 'skip' to cancel.");
                if (Console.ReadLine()?.ToLower() == "skip") return null;
            }
        }

        public static Process Launch(string exePath, string demoPath)
        {
            try
            {
                var info = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = $"-insecure -condebug +playdemo \"{demoPath}\"",
                    UseShellExecute = false,
                    RedirectStandardInput = true
                };

                return Process.Start(info);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to launch CS2: {ex.Message}");
                return null;
            }
        }
    }
}
