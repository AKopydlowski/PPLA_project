using PPLA_project.Core.WeightBalance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPLA.Project.UI.WeightBalance
{
    public static class WeightBalanceConsoleUI
    {
        public static void Run()
        {
            var calc = new WeightBalanceCalculator();

            Console.Write("Empty weight (kg): ");
            calc.EmptyWeightKg = ReadDouble();

            Console.Write("Empty weight arm (m): ");
            calc.EmptyWeightArm = ReadDouble();

            while (true)
            {
                Console.Write("Item name (ENTER to finish): ");
                var name = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(name))
                    break;

                Console.Write("Weight (kg): ");
                var w = ReadDouble();

                Console.Write("Arm (m): ");
                var a = ReadDouble();

                calc.AddItem(new WeightItem
                {
                    Name = name,
                    WeightKg = w,
                    ArmMeters = a
                });
            }

            Console.WriteLine($"Total weight: {calc.TotalWeightKg():F1} kg");
            Console.WriteLine($"CG arm: {calc.CenterOfGravity():F3} m");
        }

        private static double ReadDouble()
        {
            double v;
            while (!double.TryParse(Console.ReadLine(), out v))
            {
                Console.Write("Invalid value, try again: ");
            }
            return v;
        }

    }
}