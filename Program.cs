using System;
using PPLA.Project.UI.FuelPlanning;
using PPLA.Project.UI.Metar;
using PPLA.Project.UI.Vfr;
using PPLA.Project.UI.WeightBalance;
using PPLA.Project.UI.Wind;

namespace PPLA.Project
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("=== PPLA Planner ===");
                Console.WriteLine("1. Weight & Balance");
                Console.WriteLine("2. Fuel & Flight Planner");
                Console.WriteLine("3. Runway Wind / Crosswind");
                Console.WriteLine("4. METAR/TAF Parser");
                Console.WriteLine("5. VFR Planner");
                Console.WriteLine("0. Exit");
                Console.Write("Choose module: ");

                var choice = Console.ReadLine();
                Console.WriteLine();

                if (choice == "0")
                {
                    Console.WriteLine("Koniec. Safe flights!");
                    break;
                }

                RunModule(choice);
            }
        }

        private static void RunModule(string? choice)
        {
            try
            {
                switch (choice)
                {
                    case "1":
                        WeightBalanceConsoleUI.Run();
                        break;
                    case "2":
                        FuelPlanningConsoleUI.Run();
                        break;
                    case "3":
                        WindConsoleUI.Run();
                        break;
                    case "4":
                        MetarConsoleUI.Run();
                        break;
                    case "5":
                        VfrConsoleUI.Run();
                        break;
                    default:
                        Console.WriteLine("Nieznana opcja.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd: {ex.Message}");
            }
        }
    }
}
