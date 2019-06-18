using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonteCarlo
{
    internal enum ComputeType
    {
        SingleThread,
        ParallelFor,
        Task,
        MultiThread,
    }

    internal class Integral
    {
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
        private Random random_;

        //////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////  Random  ///////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////
        private double RandomPointX() => RandomPointX(random_);

        private double RandomPointY() => RandomPointY(random_);

        private double RandomPointX(Random random) => StartX + random.NextDouble() * (EndX - StartX);

        private double RandomPointY(Random random) => StartY + random.NextDouble() * (EndY - StartY);

        //////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////  Support Functions  ///////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////

        private Tuple<int, int> GetPararellCalculationData()
        {
            double processors = Environment.ProcessorCount;
            int tries = (int)Math.Round(Accuracy / processors);
            Console.WriteLine($"Rdzenie: {processors}");
            Console.WriteLine($"Próby na rdzeń: {tries}");
            return Tuple.Create((int)processors, tries);
        }

        private string CalculationTime() => $"{(endTime_ - startTime_)}";

        private void ShowData()
        {
            Console.WriteLine($"Dokładność: {Accuracy}");
            Console.WriteLine($"Przedział X: {StartX} <-> {EndX}");
            Console.WriteLine($"Przedział Y: {StartY} <-> {EndY}");
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////  Compute  ///////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////
        public void Compute(ComputeType type)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine($"{type}");
            ShowData();
            pointsIn_ = 0;
            value_ = 0;
            switch (type)
            {
                case ComputeType.SingleThread:
                    Calculate();
                    break;

                case ComputeType.Task:
                    StartTask();
                    break;

                case ComputeType.ParallelFor://TPL library
                    StartParallelFor();
                    break;
                case ComputeType.MultiThread:
                    StartMultiThread();
                    break;
            }
            Console.WriteLine($"Wartość: {Value}");
            Console.WriteLine($"Czas obliczeń: {CalculationTime()}");
            Console.WriteLine("==================================================");
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////  Start  ///////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////
        private void StartParallelFor()
        {
            startTime_ = DateTime.Now;
            CalculateParallelFor();
            value_ = CalculateValue(pointsIn_);
            endTime_ = DateTime.Now;
        }

        private void StartTask()
        {
            var data = GetPararellCalculationData();
            int iterations = data.Item2;
            int size = data.Item1;
            Task[] taskArray = new Task[size];
            int[] results = new int[size];
            object _lock = new object();
            startTime_ = DateTime.Now;
            for (int i = 0; i < size; ++i)
            {
                int index = i;
                taskArray[i] = Task.Run(() => CalculateTask(index, iterations, results));
            }
            Task.WaitAll(taskArray);
            pointsIn_ = results.Sum();
            value_ = CalculateValue(pointsIn_);
            endTime_ = DateTime.Now;
        }

        private void StartMultiThread()
        {
            var data = GetPararellCalculationData();
            int iterations = data.Item2;
            int size = data.Item1;
            startTime_ = DateTime.Now;
            Thread[] threadArray = new Thread[size];

            for (int i = 0; i < size; ++i)
            {
                Random random = new Random(DateTime.Now.Millisecond);
                threadArray[i] = new Thread(() =>
                {
                    CalculateMultiThread(iterations, random);
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

        //////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////  Calculate  /////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////
        private void Calculate()
        {
            pointsIn_ = 0;
            startTime_ = DateTime.Now;
            random_ = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < Accuracy; ++i)
            {
                pointsIn_ += MainCalcFunc();
            }
            value_ = CalculateValue(pointsIn_);
            endTime_ = DateTime.Now;
        }

        private void CalculateParallelFor()
        {
            object _lock = new object();
            Parallel.For(0, Accuracy, i =>
            {
                double x, y;
                lock (_lock)
                {
                    x = RandomPointX();
                    y = RandomPointY();
                }
                pointsIn_ += FuncIn(x, y);
            });
        }

        private void CalculateTask(int index, int iterations, int[] results)
        {
            Random random = new Random(DateTime.Now.Millisecond);
            int value = 0;
            for (int i = 0; i < iterations; ++i)
            {
                value += MainCalcFunc(random);
            }
            results[index] = value;
        }

        private void CalculateMultiThread(int iterations, Random random)
        {
            int value = 0;
            for (int i = 0; i <= iterations; ++i)
            {
                value += MainCalcFunc(random);
            }
            pointsIn_ += value;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////  Main Function  ////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////
        private int FuncIn(double x, double y)
        {
            if ((y > 0) && (y <= func(x)))
            {
                return 1;
            }
            else if ((y > 0) && (y <= func(x)))
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        private int MainCalcFunc() => FuncIn(RandomPointX(), RandomPointY());

        private int MainCalcFunc(Random random) => FuncIn(RandomPointX(random), RandomPointY(random));

        private float CalculateValue(int pointsIn) => (pointsIn / (float)Accuracy) * ((EndX - StartX) * (EndY - StartY));

    }
}