using System;
using PPLA.Project.UI.WeightBalance;

namespace PPLA.Project
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Lets get started :)");
            WeightBalanceConsoleUI.Run();

            Console.WriteLine();
            Console.WriteLine("Koniec. Naciśnij ENTER...");
            Console.ReadLine();
        }
    }
}
