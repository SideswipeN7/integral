using System;
using System.Threading;
using System.Threading.Tasks;

namespace MonteCarlo
{
    internal enum ComputeType
    {
        SingleThread,
        TaskAsync,
        TaskLock,
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

        //////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////  Random  ///////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////
        private double RandomPointX() => StartX + new Random(DateTime.Now.Millisecond).NextDouble() * (EndX - StartX);

        private double RandomPointY() => StartY + new Random(DateTime.Now.Millisecond).NextDouble() * (EndY - StartY);

        private async Task<double> AsyncRandomPointX()
        {
            double rand = await Task.FromResult(new Random(DateTime.Now.Millisecond).NextDouble());
            return StartX + rand * (EndX - StartX);
        }

        private async Task<double> AsyncRandomPointY()
        {
            double rand = await Task.FromResult(new Random(DateTime.Now.Millisecond).NextDouble());
            return StartY + rand * (EndY - StartY);
        }

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

                case ComputeType.TaskLock:
                    StartTaskLock();
                    break;

                case ComputeType.TaskAsync:
                    StartTaskAsync();
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
        private void StartTaskAsync()
        {
            var data = GetPararellCalculationData();
            int iterations = data.Item2;
            int size = data.Item1;
            Task[] taskArray = new Task[size];
            startTime_ = DateTime.Now;
            for (int i = 0; i < size; ++i)
            {
                taskArray[i] = CalculateTaskAsync(iterations).ContinueWith(x =>
                    {
                        pointsIn_ += x.Result;
                    });
            }
            Task.WaitAll(taskArray);
            value_ = CalculateValue(pointsIn_);
            endTime_ = DateTime.Now;
        }

        private void StartTaskLock()
        {
            var data = GetPararellCalculationData();
            int iterations = data.Item2;
            int size = data.Item1;
            Task[] taskArray = new Task[size];
            object _lock = new object();
            startTime_ = DateTime.Now;
            for (int i = 0; i < size; ++i)
            {
                taskArray[i] = CalculateTaskLock(iterations, _lock);
            }
            Task.WaitAll(taskArray);
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
                threadArray[i] = new Thread(() =>
                {
                    CalculateMultiThread(iterations);
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
            for (int i = 0; i < Accuracy; ++i)
            {
                pointsIn_ += MainCalcFunc();
            }
            value_ = CalculateValue(pointsIn_);
            endTime_ = DateTime.Now;
        }

        private async Task<int> CalculateTaskAsync(int iterations)
        {
            int value = 0;
            for (int i = 0; i < iterations; ++i)
            {
                value += await AsyncMainCalcFunc();
            }
            return value;
        }

        private async Task CalculateTaskLock(int size, object lockObj)
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

        private void CalculateMultiThread(int iterations)
        {
            int value = 0;
            for (int i = 0; i <= iterations; ++i)
            {
                value += MainCalcFunc();
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

        private async Task<int> AsyncMainCalcFunc()
        {
            double x = await AsyncRandomPointX();
            double y = await AsyncRandomPointY();
            return FuncIn(x, y);
        }

        private float CalculateValue(int pointsIn) => (pointsIn / (float)Accuracy) * ((EndX - StartX) * (EndY - StartY));

    }
}