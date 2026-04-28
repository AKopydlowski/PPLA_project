using System;
using PPLA.Project.Core.Wind;
using PPLA.Project.UI.Common;

namespace PPLA.Project.UI.Wind
{
    public static class WindConsoleUI
    {
        public static void Run()
        {
            var calc = new RunwayWindCalculator();

            var runway = ConsoleInput.ReadInt("Runway heading (deg): ");
            var windDir = ConsoleInput.ReadInt("Wind direction (deg): ");
            var windSpeed = ConsoleInput.ReadDouble("Wind speed (kt): ");
            var limit = ConsoleInput.ReadDouble("Crosswind limit (kt): ");

            var result = calc.Calculate(runway, windDir, windSpeed, limit);
            Console.WriteLine($"Headwind/Tailwind component: {result.HeadwindKt:F1} kt");
            Console.WriteLine($"Crosswind component: {result.CrosswindKt:F1} kt");
            Console.WriteLine(result.ExceedsCrosswindLimit ? "WARNING: Crosswind limit exceeded" : "Crosswind within limit");
        }
    }
}
