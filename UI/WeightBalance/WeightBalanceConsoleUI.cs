using System;
using PPLA.Project.Core.WeightBalance;
using PPLA.Project.UI.Common;

namespace PPLA.Project.UI.WeightBalance
{
    public static class WeightBalanceConsoleUI
    {
        public static void Run()
        {
            var calc = new WeightBalanceCalculator
            {
                EmptyWeightKg = ConsoleInput.ReadDouble("Empty weight (kg): "),
                EmptyWeightArm = ConsoleInput.ReadDouble("Empty weight arm (m): ")
            };

            while (true)
            {
                Console.Write("Item name (ENTER to finish): ");
                var name = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(name))
                {
                    break;
                }

                calc.AddItem(new WeightItem
                {
                    Name = name,
                    WeightKg = ConsoleInput.ReadDouble("Weight (kg): "),
                    ArmMeters = ConsoleInput.ReadDouble("Arm (m): ")
                });
            }

            Console.WriteLine($"Total weight: {calc.TotalWeightKg():F1} kg");
            Console.WriteLine($"CG arm: {calc.CenterOfGravity():F3} m");
        }
    }
}
