using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadSafeCollection
{
    class Program
    {
        static void Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Log log = new Log(cts);
            var t1 = Task.Run(() => { log.write(); });
            var t4 = Task.Run(() => { log.write(); });
            var t2 = Task.Run(() => { log.get(); });
            var t3 = Task.Run(() =>
            {
                if (Console.ReadKey().KeyChar == 'q')
                {
                    cts.Cancel();
                }
            });
            Task.WaitAll(t1, t2, t3, t4);
            cts.Dispose();
            Console.ReadKey();
        }
    }

    class Log
    {
        BlockingCollection<string> BC = new BlockingCollection<string>();
        private CancellationTokenSource cts;

        public Log(CancellationTokenSource cts)
        {
            this.cts = cts;
        }

        public void write()
        {
            for (int i = 0; i < 100; i++)
            {
                var msg = i.ToString();
                if (BC.TryAdd(msg))
                {
                    Console.WriteLine("add suc " + msg);
                }
                else
                {
                    Console.WriteLine("add fail " + msg);
                }
            }
        }

        public void get()
        {
            while (true)
            {
                if (BC.TryTake(out string msg))
                {
                    Console.WriteLine("take suc " + msg);
                }
                else
                {
                    Console.WriteLine("take fail " + msg);
                }

                if (cts.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}
