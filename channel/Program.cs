using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace channel
{
    class Program
    {
        static void Main(string[] args)
        {
            var collection = Channel.CreateUnbounded<string>();
            CancellationTokenSource cts = new CancellationTokenSource();

            var test = new Test(collection);
            var t1 = Task.Run(() => test.Consumer(cts));
            var t2 = Task.Run(() => test.Producer(cts));
            var t3 = Task.Run(() => test.Producer(cts));
            var t4 = Task.Run(() =>
            {
                if (Console.ReadKey().KeyChar == 'q')
                {
                    cts.Cancel();
                }
            });
            Task.WaitAll(t1, t2, t3, t4);
            Console.ReadKey();
        }
    }

    class Test
    {
        private readonly Channel<string> _collection;

        public Test(Channel<string> collection)
        {
            _collection = collection;
        }

        public async Task Producer(CancellationTokenSource cts)
        {
            for (var i = 0; i < 1000; i++)
            {
                //Console.ReadKey(); //debug
                await _collection.Writer.WriteAsync(i.ToString(), cts.Token);
                Console.WriteLine("item write: " + i);

                if (cts.IsCancellationRequested)
                {
                    //_collection.Writer.Complete();
                    break;
                }
            }
        }

        public async Task Consumer(CancellationTokenSource cts)
        {
            while (true)
            {
                while (_collection.Reader.TryRead(out var item))
                {
                    Console.WriteLine("item read: " + item);
                }

                if (cts.IsCancellationRequested)
                {
                    break;
                }
            }

        }
    }
}