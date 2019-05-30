using System;
using System.Threading;
using System.Threading.Tasks;

namespace MonteCarlo
{
    internal enum ComputeType
    {
        SingleThread,
        ParallelAsync,
        ParallelLock,
        MultiThread,
    }

    internal class Integral
    {
        private Random random_ = new Random(DateTime.Now.Millisecond);
        public float StartX { get; set; }
        public float EndX { get; set; }
        public float EndY { get; set; }
        public float StartY { get; set; }
        public int Accuracy { get; set; }
        public float Value { get => value_; }

        private float value_;
        private int pointsIn_;
        public Func<double, double> func { get; set; }
        private DateTime startTime_;
        private DateTime endTime_;

        private double RandomPointX() => StartX + random_.NextDouble() * (EndX - StartX);

        private double RandomPointY() => StartY + random_.NextDouble() * (EndY - StartY);
        private double SecureRandomX(object _lock)
        {
            lock (_lock)
            {
                double rand = random_.NextDouble();
                return StartX + rand * (EndX - StartX);
            }
        }
        private double SecureRandomY(object _lock)
        {
            lock (_lock)
            {
                double rand = random_.NextDouble();
                return StartY + rand * (EndY - StartY);
            }
        }
        private async Task<double> AsyncRandomPointX()
        {
            double rand = await Task.FromResult(random_.NextDouble());
            return StartX + rand * (EndX - StartX);
        }

        private async Task<double> AsyncRandomPointY()
        {
            double rand = await Task.FromResult(random_.NextDouble());
            return StartY + rand * (EndY - StartY);
        }

        private int FuncIn(double x, double y)
        {
            if ((y > 0) && (y <= func(x)))
                return 1;
            else if ((y > 0) && (y <= func(x)))
                return -1;
            return 0;
        }

        private int MainCalcFunc() => FuncIn(RandomPointX(), RandomPointY());

        private async Task<int> AsyncMainCalcFunc()
        {
            double x = await AsyncRandomPointX();
            double y = await AsyncRandomPointY();
            return FuncIn(x, y);
        }
        private int SercureMainCalcFunc(object _lock)
        {
            double x = SecureRandomX(_lock);
            double y = SecureRandomY(_lock);
            return FuncIn(x, y);
        }

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
                    StartParallelLock();
                    break;

                case ComputeType.ParallelAsync:
                    StartParallelAsync();
                    break;
                case ComputeType.MultiThread:
                    StartMultiThread();
                    break;
            }
            Console.WriteLine($"Wartość: {value_}");
            Console.WriteLine($"Czas obliczeń: {CalculationTime()}");
            Console.WriteLine("==================================================");
        }

        private void Calculate()
        {
            int pointsIn = 0;
            startTime_ = DateTime.Now;
            for (int i = 0; i < Accuracy; i++)
            {
                pointsIn += MainCalcFunc();
            }
            value_ = CalculateValue(pointsIn);
            endTime_ = DateTime.Now;
        }
       
        private void StartMultiThread()
        {
            var data = GetPararellCalculationData();
            int iterations = data.Item2;
            int size = data.Item1;
            startTime_ = DateTime.Now;
            Thread[] threadArray = new Thread[size];
            pointsIn_ = 0;
            object o = new object();
            for (int i = 0; i < size; ++i)
            {
                threadArray[i] = new Thread(() =>
                {
                    MultiCalculateAsync(iterations, o);
                });
            }
            foreach (var thread in threadArray)
            {
                thread.Start();
            }
            foreach (var thread in threadArray)
            {
                thread.Join();

            }
            value_ = CalculateValue(pointsIn_);
            endTime_ = DateTime.Now;

        }
        private void MultiCalculateAsync(int iterations, object _lock)
        {
            int value = 0;
            for (int k = 0; k <= iterations; ++k)
            {
                value += SercureMainCalcFunc(_lock);
            }
            lock (_lock)
            {
                pointsIn_ += value;
            }

        }
       
       
        private void StartParallelLock()
        {
            int pointsIn = 0;
            var data = GetPararellCalculationData();
            int iterations = data.Item2;
            int size = data.Item1;
            Task[] taskArray = new Task[size];
            object _lock = new object();
            pointsIn_ = 0;
            startTime_ = DateTime.Now;
            for (int i = 0; i < size; ++i)
            {
                taskArray[i] = CalculateParallelLock(iterations, _lock);
            }
            Task.WaitAll(taskArray);
            value_ = CalculateValue(pointsIn_);
            endTime_ = DateTime.Now;
        }

        private void StartParallelAsync()
        {
            int pointsIn = 0;
            var data = GetPararellCalculationData();
            int iterations = data.Item2;
            int size = data.Item1;
            Task[] taskArray = new Task[size];
            startTime_ = DateTime.Now;
            for (int i = 0; i < size; i++)
            {
                taskArray[i] = CalculateParallelAsync(iterations).ContinueWith(x =>
                    {
                        pointsIn += x.Result;
                    });
            }

            Task.WaitAll(taskArray);
            value_ = CalculateValue(pointsIn);
            endTime_ = DateTime.Now;
        }

        private async Task<int> CalculateParallelAsync(int size)
        {
            int value = 0;
            for (int k = 0; k <= size; ++k)
            {
                value += await AsyncMainCalcFunc();
            }
            return value;
        }

        private async Task CalculateParallelLock(int size, object lockObj)
        {
            int value = 0;
            for (int k = 0; k <= size; ++k)
            {
                value += await AsyncMainCalcFunc();
            }
            lock (lockObj)
            {
                pointsIn_ += value;
            }
        }
    }
}