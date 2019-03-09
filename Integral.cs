using System;
using System.Threading.Tasks;

namespace MonteCarlo
{
    internal class Integral
    {
        private Random random_ = new Random(DateTime.Now.Millisecond);
        public double StartX { get; set; }
        public double EndX { get; set; }
        public double EndY { get; set; }
        public double StartY { get; set; }
        public int Accuracy { get; set; }
        private double value_;
        public double Value { get => value_; }
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

        public void Calculate()
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

        public void CalculateParalel()
        {
            int pointsIn = 0;
            Task[] taskArray = new Task[Accuracy];
            Object _lock = new Object();
            startTime_ = DateTime.Now;
            for (int i = 0; i < taskArray.Length; i++)
            {
                taskArray[i] = Task.Factory.StartNew((Object obj) =>
                {
                    int value = mainCalcFunc();
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

        private int mainCalcFunc() => funcIn(RandomPoint(), RandomPoint());

        private double CalculateValue(double pointsIn) => (pointsIn / (Accuracy * 1.0)) * ((EndX - StartX) * (EndY - StartY));

        public string CalculationTime() => $"{(endTime_ - startTime_)}";
    }
}