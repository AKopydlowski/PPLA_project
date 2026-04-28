using System;
using PPLA.Project.Core.FuelPlanning;
using PPLA.Project.UI.Common;

namespace PPLA.Project.UI.FuelPlanning
{
    public static class FuelPlanningConsoleUI
    {
        public static void Run()
        {
            var calc = new FuelPlannerCalculator();

            var result = calc.Calculate(new FuelPlanInput
            {
                DistanceNm = ConsoleInput.ReadDouble("Distance (NM): "),
                TrueAirspeedKt = ConsoleInput.ReadDouble("TAS (kt): "),
                WindComponentKt = ConsoleInput.ReadDouble("Wind component (kt, + tailwind / - headwind): "),
                FuelBurnPerHourL = ConsoleInput.ReadDouble("Fuel burn (L/h): "),
                TaxiFuelL = ConsoleInput.ReadDouble("Taxi fuel (L): "),
                ReserveFuelL = ConsoleInput.ReadDouble("Reserve fuel loaded (L): "),
                ReserveMinutesRequired = ConsoleInput.ReadDouble("Required reserve time (min): "),
                ContingencyPercent = ConsoleInput.ReadDouble("Contingency (%): ")
            });

            Console.WriteLine($"Ground speed: {result.GroundSpeedKt:F1} kt");
            Console.WriteLine($"Flight time: {result.FlightTimeHours:F2} h");
            Console.WriteLine($"Trip fuel: {result.TripFuelL:F1} L");
            Console.WriteLine($"Contingency fuel: {result.ContingencyFuelL:F1} L");
            Console.WriteLine($"Reserve required: {result.ReserveRequiredFuelL:F1} L");
            Console.WriteLine($"Estimated landing fuel: {result.EstimatedLandingFuelL:F1} L");
            Console.WriteLine($"Block fuel: {result.BlockFuelL:F1} L");
            Console.WriteLine(result.HasReserveMargin ? "Status: RESERVE OK" : "Status: RESERVE BELOW MINIMUM");
        }
    }
}
