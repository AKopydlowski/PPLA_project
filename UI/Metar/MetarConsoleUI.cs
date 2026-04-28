using System;
using PPLA.Project.Core.Metar;

namespace PPLA.Project.UI.Metar
{
    public static class MetarConsoleUI
    {
        public static void Run()
        {
            var parser = new MetarParser();

            Console.WriteLine("Podaj METAR/TAF:");
            var raw = Console.ReadLine() ?? string.Empty;

            var report = parser.Parse(raw);
            Console.WriteLine("--- Wynik parsera ---");
            Console.WriteLine($"Wiatr: {report.Wind ?? "brak"}");
            Console.WriteLine($"Widzialność: {report.Visibility ?? "brak"}");
            Console.WriteLine($"Chmury: {report.Clouds ?? "brak"}");
            Console.WriteLine($"QNH/Altimeter: {report.Qnh ?? "brak"}");
        }
    }
}
