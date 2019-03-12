using System;
using System.Threading.Tasks;

namespace MonteCarlo
{
    internal enum ComputeType
    {
        SingleThread,
        ParallelArray,
        ParallelLock,
        ParallelFor,
    }

    internal class Integral
    {
        private Random random_ = new Random(DateTime.Now.Millisecond);
        public float StartX { get; set; }
        public float EndX { get; set; }
        public float EndY { get; set; }
        public float StartY { get; set; }
        public int Accuracy { get; set; }
        private float value_;
        public float Value { get => value_; }
        public Func<double, double> func { get; set; }
        private DateTime startTime_;
        private DateTime endTime_;

        private double RandomPoint() => StartX + random_.NextDouble() * (EndX - StartX);

        private int funcIn(double x, double y)
        {
            if ((y > 0) && (y <= func(x)))
                return 1;
            else if ((y > 0) && (y <= func(x)))
                return -1;
            return 0;
        }

        private int mainCalcFunc() => funcIn(RandomPoint(), RandomPoint());

        private float CalculateValue(int pointsIn) => (pointsIn / (float)Accuracy) * ((EndX - StartX) * (EndY - StartY));

        public string CalculationTime() => $"{(endTime_ - startTime_)}";

        private Tuple<int, int> GetPararellCalculationData()
        {
            int processors = Environment.ProcessorCount;
            int tries = Accuracy / processors;
            Console.WriteLine($"Rdzenie: {processors}");
            Console.WriteLine($"Próby na rdzeń: {tries}");
            return Tuple.Create(processors, tries);
        }

        private void Calculate()
        {
            int pointsIn = 0;
            startTime_ = DateTime.Now;
            for (int i = 0; i < Accuracy; i++)
            {
                pointsIn += mainCalcFunc();
            }
            value_ = CalculateValue(pointsIn);
            endTime_ = DateTime.Now;
        }

        public void Compute()
        {
            Compute(ComputeType.SingleThread);
        }

        public void Compute(ComputeType type)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine($"{type}");
            switch (type)
            {
                case ComputeType.SingleThread:
                    Calculate();
                    break;

                case ComputeType.ParallelLock:
                    CalculateParallelLock();
                    break;

                case ComputeType.ParallelArray:
                    StartParallelArray();
                    break;

                case ComputeType.ParallelFor:
                    CalculateParallelFor();
                    break;
            }
            Console.WriteLine($"Wartość: {Value}");
            Console.WriteLine($"Czas obliczeń: {CalculationTime()}");
            Console.WriteLine("==================================================");
        }

        private void CalculateParallelLock()
        {
            int pointsIn = 0;
            var data = GetPararellCalculationData();
            Task[] taskArray = new Task[data.Item1];
            int[] values = new int[data.Item1];
            object _lock = new object();
            startTime_ = DateTime.Now;
            for (int i = 0; i < taskArray.Length; ++i)
            {
                taskArray[i] = Task.Factory.StartNew((object obj) =>
                {
                    int value = 0;
                    for (int k = 0; k <= data.Item2; ++k)
                    {
                        value += mainCalcFunc();
                    }
                    lock (obj)
                    {
                        pointsIn += value;
                    }
                }, _lock);
            }
            Task.WaitAll(taskArray);
            value_ = CalculateValue(pointsIn);
            endTime_ = DateTime.Now;
        }

        private void StartParallelArray()
        {
            int pointsIn = 0;
            var data = GetPararellCalculationData();
            int iterations = data.Item2;
            int size = data.Item1;
            Task[] taskArray = new Task[size];
            int[] values = new int[size];
            startTime_ = DateTime.Now;
            for (int i = 0; i < size; i++)
            {
                taskArray[i] = Task.Factory.StartNew(async (x) =>
                {
                    values[(int)x] = await CalculateParallelArray(iterations);
                }, i);
            }
            
            Task.WaitAll(taskArray);
            foreach (int v in values)
            {
                pointsIn += v;
            }
            value_ = CalculateValue(pointsIn);
            endTime_ = DateTime.Now;
        }

        private void CalculateParallelFor()
        {
            int pointsIn = 0;
            startTime_ = DateTime.Now;
            Parallel.For(0, Accuracy, x =>
             {
                 pointsIn += mainCalcFunc();
             });
            value_ = CalculateValue(pointsIn);
            endTime_ = DateTime.Now;
        }

        private async Task<int> CalculateParallelArray(int size)
        {
            int value = 0;
            for (int k = 0; k <= size; ++k)
            {
                value +=  mainCalcFunc();
            }
            return value;
        }
    }
}