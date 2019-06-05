using System;

namespace MonteCarlo
{
    internal class Program
    {
        private static void Main()
        {
            int accuracy = 9000000;
            Integral integral = new Integral()
            {
                StartX = -10,
                EndX = 10,
                StartY = -10,
                EndY = 10,
                Accuracy = accuracy,
                func = (x) => (Math.Pow(x, 12) + Math.Pow(x, -4 / 5) * x + 3)
            };


            Console.WriteLine("==================================================");
            Console.WriteLine("Początek obliczeń");
            integral.Compute(ComputeType.SingleThread);
            integral.Compute(ComputeType.TaskLock);
            integral.Compute(ComputeType.TaskAsync);
            integral.Compute(ComputeType.MultiThread);
            Console.WriteLine("Koniec obliczeń");
            Console.WriteLine("==================================================");


            Console.ReadKey();
        }
    }
}