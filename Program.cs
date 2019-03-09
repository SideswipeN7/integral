using System;

namespace MonteCarlo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            int accuracy = 5000;
            Integral integral = new Integral()
            {
                StartX = -10,
                EndX = 10,
                StartY = -10,
                EndY = 10,
                Accuracy = accuracy,
                func = (x) => (Math.Pow(x,12) + Math.Pow(x, -4/5) * x + 3)
            };

            Console.WriteLine("Początek obliczeń");
            Console.WriteLine("==================================================");

            Console.WriteLine("Liczenie całki");
            Console.WriteLine("Jeden wątek");
            integral.Calculate();
            Console.WriteLine($"Wartość: {integral.Value}");
            Console.WriteLine($"Czas obliczeń: {integral.CalculationTime()}");

            Console.WriteLine("==================================================");

            Console.WriteLine($"{accuracy} wątków");
            integral.CalculateParalel();
            Console.WriteLine($"Wartość: {integral.Value}");
            Console.WriteLine($"Czas obliczeń: {integral.CalculationTime()}");

            Console.WriteLine("==================================================");
            Console.WriteLine("Koniec obliczeń");
            Console.ReadKey();
        }
    }
}