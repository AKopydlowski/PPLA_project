using System;
using PPLA.Project.Core.Vfr;
using PPLA.Project.UI.Common;

namespace PPLA.Project.UI.Vfr
{
    public static class VfrConsoleUI
    {
        public static void Run()
        {
            var calc = new VfrLegCalculator();

            var result = calc.Calculate(new VfrLegInput
            {
                TrueCourseDeg = ConsoleInput.ReadDouble("True course (deg): "),
                DistanceNm = ConsoleInput.ReadDouble("Distance (NM): "),
                TrueAirspeedKt = ConsoleInput.ReadDouble("TAS (kt): "),
                WindDirectionDeg = ConsoleInput.ReadDouble("Wind direction (deg): "),
                WindSpeedKt = ConsoleInput.ReadDouble("Wind speed (kt): "),
                MagneticVariationDeg = ConsoleInput.ReadDouble("Magnetic variation (deg, East + / West -): ")
            });

            Console.WriteLine($"WCA: {result.WindCorrectionAngleDeg:F1}°");
            Console.WriteLine($"True heading: {result.TrueHeadingDeg:F1}°");
            Console.WriteLine($"Magnetic heading: {result.MagneticHeadingDeg:F1}°");
            Console.WriteLine($"Ground speed: {result.GroundSpeedKt:F1} kt");
            Console.WriteLine($"Time: {result.TimeMinutes:F1} min");
        }
    }
}
