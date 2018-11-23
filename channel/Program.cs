using System;
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
            var t1 = Task.Run( async () => await test.Consumer(cts));
            var t2 = Task.Run( async () => await test.Producer(cts));
            var t3 = Task.Run( async () => await test.Producer(cts));
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
        private readonly Channel<string> collection;

        public Test(Channel<string> collection)
        {
            this.collection = collection;
        }

        public async Task Producer(CancellationTokenSource cts)
        {
            for (var i = 0; i < 100; i++)
            {
                //Console.ReadKey(); //debug
                await collection.Writer.WriteAsync(i.ToString()).ConfigureAwait(false);
                Console.WriteLine("item write: " + i);

                if(cts.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        public async Task Consumer(CancellationTokenSource cts)
        {
            while (true)
            {
                try
                {
                    while(await collection.Reader.WaitToReadAsync(cts.Token).ConfigureAwait(false))
                    {
                        while(collection.Reader.TryRead(out var item))
                        {
                            Console.WriteLine("item read: " + item);
                        }
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine($"unexpected exception:{e}");
                    break;
                }
            }

        }
    }
}