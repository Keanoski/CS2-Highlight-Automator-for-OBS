namespace HighlightReel.Utilities
{
    public static class ConsoleHelper
    {
        public static string Prompt(string message, string fallback = "")
        {
            Console.WriteLine(message);
            Console.Write("> ");
            var input = Console.ReadLine();
            return string.IsNullOrWhiteSpace(input) ? fallback : input;
        }

        public static void DisplayHighlights(IEnumerable<HighlightReel.Models.Highlight> highlights)
        {
            Console.WriteLine("\nDetected Highlights:");
            foreach (var h in highlights)
            {
                Console.WriteLine($"- {h.PlayerName} got {h.KillCount} kills (Ticks: {h.StartTick} → {h.EndTick})");
            }
        }
    }
}
