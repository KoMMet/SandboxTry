using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace blockingcollection
{
    class Program
    {
        static void Main()
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
            log.Dispose();
            cts.Dispose();
            Console.ReadKey();
        }
    }

    class Log : IDisposable

    {
        readonly BlockingCollection<string> BC = new BlockingCollection<string>(100);
        private readonly CancellationTokenSource cts;

        public Log(CancellationTokenSource cts)
        {
            this.cts = cts;
        }

        public void write()
        {
            int i = 0;
            while (i < 1000)
            {
                //Console.ReadKey(); //debug
                var msg = i.ToString();
                try
                {
                    BC.Add(msg);
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("add end");
                    break;
                }
                i++;
                Console.WriteLine("add " + msg);
                if (cts.IsCancellationRequested)
                {
                    BC.CompleteAdding();
                    break;
                }
            }
            BC.CompleteAdding();
        }

        readonly List<string> output = new List<string>(10000);

        public void get()
        {
            while (true)
            {
                //BC.GetConsumingEnumerable(cts.Token);

                try
                {
                    string msg = BC.Take();
                    Console.WriteLine("take " + msg);
                    output.Add(msg);
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("take end");
                    break;
                }

                if (BC.IsCompleted)
                {
                    break;
                }
            }
        }

        public void Dispose()
        {
            BC?.Dispose();
            output.ForEach(Console.WriteLine);
        }
    }
}
