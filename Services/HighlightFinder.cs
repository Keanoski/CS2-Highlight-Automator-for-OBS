using DemoFile;
using DemoFile.Sdk;
using HighlightReel.Models;

namespace HighlightReel.Services
{
    public static class HighlightFinder
    {
        private const int TickRate = 64;
        private const int MaxTickGap = 10 * TickRate;
        private const int MinKills = 5;

        public static async Task<List<Highlight>> FindHighlightsAsync(string demoPath)
        {
            var kills = new Dictionary<string, List<int>>();
            var demo = new CsDemoParser();

            demo.Source1GameEvents.PlayerDeath += e =>
            {
                var name = e.Attacker?.PlayerName;
                if (name != null)
                {
                    if (!kills.ContainsKey(name))
                        kills[name] = new List<int>();

                    kills[name].Add(demo.CurrentDemoTick ?? 0);
                }
            };

            using var stream = File.OpenRead(demoPath);
            var reader = DemoFileReader.Create(demo, stream);
            await reader.ReadAllAsync();

            return ExtractHighlights(kills);
        }

        private static List<Highlight> ExtractHighlights(Dictionary<string, List<int>> kills)
        {
            var highlights = new List<Highlight>();

            foreach (var (player, ticks) in kills)
            {
                if (ticks.Count < MinKills) continue;

                ticks.Sort();
                var streak = new List<int>();

                foreach (var tick in ticks)
                {
                    if (!streak.Any() || tick - streak.Last() <= MaxTickGap)
                        streak.Add(tick);
                    else
                    {
                        AddHighlightIfValid(highlights, player, streak);
                        streak = new List<int> { tick };
                    }
                }

                AddHighlightIfValid(highlights, player, streak);
            }

            return highlights.OrderBy(h => h.StartTick).ToList();
        }

        private static void AddHighlightIfValid(List<Highlight> list, string player, List<int> streak)
        {
            if (streak.Count >= MinKills)
            {
                list.Add(new Highlight
                {
                    PlayerName = player,
                    StartTick = streak.First(),
                    EndTick = streak.Last(),
                    KillCount = streak.Count
                });
            }
        }
    }
}
