using System;

namespace MonteCarlo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            int accuracy = 900000000;
            Integral integral = new Integral()
            {
                StartX = -10,
                EndX = 10,
                StartY = -10,
                EndY = 10,
                Accuracy = accuracy,
                func = (x) => (Math.Pow(x,12) + Math.Pow(x, -4/5) * x + 3)
            };


            Console.WriteLine("==================================================");
            Console.WriteLine("Początek obliczeń");
            integral.Compute(ComputeType.SingleThread);
            integral.Compute(ComputeType.ParallelLock);
            integral.Compute(ComputeType.ParallelAsync);
            Console.WriteLine("Koniec obliczeń");
            Console.WriteLine("==================================================");


            Console.ReadKey();
        }
    }
}