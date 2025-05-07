using System;
using System.Threading;

namespace ThreadMinSharp
{
    class Program
    {
        private static readonly int dim = 10000000;
        private static readonly int threadNum = 10;
        private readonly Thread[] thread = new Thread[threadNum];

        private readonly int[] arr = new int[dim];
        private int globalMin = int.MaxValue;
        private int globalMinIndex = -1;

        private int threadCount = 0;
        private readonly object lockerForMin = new object();
        private readonly object lockerForCount = new object();

        static void Main(string[] args)
        {
            Program main = new Program();
            main.InitArr();

            main.ParallelMinSearch();

            Console.WriteLine($"Min value: {main.globalMin} at index: {main.globalMinIndex}");
            Console.ReadKey();
        }

        private void InitArr()
        {
            Random rnd = new Random();
            for (int i = 0; i < dim; i++)
            {
                arr[i] = rnd.Next(100000);
            }

            int negativeIndex = new Random().Next(dim);
            arr[negativeIndex] = -rnd.Next(1, 100);
        }

        private void ParallelMinSearch()
        {
            int chunkSize = dim / threadNum;

            for (int i = 0; i < threadNum; i++)
            {
                int start = i * chunkSize;
                int end = (i == threadNum - 1) ? dim : start + chunkSize;

                thread[i] = new Thread(StarterThread);
                thread[i].Start(new Bound(start, end));
            }

           
            lock (lockerForCount)
            {
                while (threadCount < threadNum)
                {
                    Monitor.Wait(lockerForCount);
                }
            }
        }

        private void StarterThread(object? param)
        {
            if (param is Bound bound)
            {
                int localMin = int.MaxValue;
                int localIndex = -1;

                for (int i = bound.StartIndex; i < bound.FinishIndex; i++)
                {
                    if (arr[i] < localMin)
                    {
                        localMin = arr[i];
                        localIndex = i;
                    }
                }

                Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] " +
                                  $"Checked range [{bound.StartIndex}, {bound.FinishIndex}) " +
                                  $"Local min = {localMin} at index {localIndex}");

                lock (lockerForMin)
                {
                    if (localMin < globalMin)
                    {
                        globalMin = localMin;
                        globalMinIndex = localIndex;
                    }
                }

                IncrementThreadCount();
            }
        }


        private void IncrementThreadCount()
        {
            lock (lockerForCount)
            {
                threadCount++;
                Monitor.Pulse(lockerForCount);
            }
        }

        class Bound
        {
            public int StartIndex { get; }
            public int FinishIndex { get; }

            public Bound(int start, int finish)
            {
                StartIndex = start;
                FinishIndex = finish;
            }
        }
    }
}
