using HighlightReel.Models;
using OBSWebsocketDotNet;
using System.Diagnostics;

namespace HighlightReel.Services
{
    public class OBSController
    {
        private readonly OBSWebsocket _obs = new();

        private const int TickRate = 64;
        private const int PreRollTicks = 4 * TickRate;
        private const int PostRollTicks = 3 * TickRate;

        public bool Connect()
        {
            var ip = ConsoleHelper.Prompt("OBS IP:", "192.168.0.118");
            var port = ConsoleHelper.Prompt("Port:", "4455");
            var password = ConsoleHelper.Prompt("Password:", "DcP6e0CT3QdHIiGm");

            try
            {
                _obs.ConnectAsync($"ws://{ip}:{port}", password);
                for (int i = 0; i < 50; i++)
                {
                    if (_obs.IsConnected)
                    {
                        Console.WriteLine("Connected to OBS.");
                        return true;
                    }

                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OBS connection failed: {ex.Message}");
            }

            return false;
        }

        public async Task RecordHighlightsAsync(Process cs2, List<Highlight> highlights)
        {
            var console = cs2.StandardInput;
            await console.WriteLineAsync("exec setup_script.cfg");

            foreach (var h in highlights)
            {
                Console.WriteLine($"\nRecording {h.PlayerName}, {h.KillCount} kills...");

                int start = h.StartTick - PreRollTicks;
                int end = h.EndTick + PostRollTicks;
                int duration = (int)((end - start) / (float)TickRate * 1000);

                await console.WriteLineAsync($"demo_gototick {start}");
                await console.WriteLineAsync($"spec_player_by_name \"{h.PlayerName}\"");
                await console.WriteLineAsync("demo_pause");
                await Task.Delay(500);

                _obs.StartRecord();
                await console.WriteLineAsync("demo_resume");
                await Task.Delay(duration);
                _obs.StopRecord();

                await console.WriteLineAsync("demo_pause");
                await Task.Delay(500);
            }
        }

        public void Disconnect() => _obs.Disconnect();
    }
}
