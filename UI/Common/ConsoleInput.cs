using System;
using System.Globalization;

namespace PPLA.Project.UI.Common
{
    public static class ConsoleInput
    {
        public static double ReadDouble(string prompt)
        {
            Console.Write(prompt);
            while (true)
            {
                var raw = Console.ReadLine();
                if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var invariant))
                {
                    return invariant;
                }

                if (double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out var current))
                {
                    return current;
                }

                Console.Write("Invalid value, try again: ");
            }
        }

        public static int ReadInt(string prompt)
        {
            Console.Write(prompt);
            int value;
            while (!int.TryParse(Console.ReadLine(), out value))
            {
                Console.Write("Invalid value, try again: ");
            }

            return value;
        }
    }
}
