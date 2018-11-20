using System;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;

namespace readwritelock
{
    class Program
    {
        ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        volatile int value1 = 0;
        int Value1
        {
            get
            {
                rwLock.EnterReadLock();
                try
                {
                    WriteLine("get inner1");
                    Thread.Sleep(1000);
                    WriteLine("get inner2");
                    return value1;
                }
                finally
                {
                    rwLock.ExitReadLock();
                }
            }

            set
            {
                rwLock.EnterWriteLock();
                try
                {
                    Console.WriteLine("set inner1");
                    Thread.Sleep(3000);
                    Console.WriteLine("set inner2");
                    value1 = value;
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }
            }
        }
        static void Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            //program end 
            var t1 = Task.Run(() => { if(ReadKey(true).KeyChar == 'q') cts.Cancel(); });
            var t2 = Task.Run(() => MainProc(cts.Token));
            Task.WaitAll(t1, t2);
            cts.Dispose();
        }

        private static void MainProc(CancellationToken ctsToken)
        {
          //  while(true)
          //  {
                var p = new Program();

                var task1 = Task.Run(() => WriteLine(p.Value1));
                var task2 = Task.Run(() => WriteLine(p.Value1));
                Task.WhenAll(task1, task2).Wait();

                task1 = Task.Run(() =>
                {
                    Thread.Sleep(500);
                    WriteLine(p.Value1);
                });
                task2 = Task.Run(() =>
                {
                    p.Value1 = 10;
                });
                Task.WhenAll(task1, task2).Wait();

            //    if(ctsToken.IsCancellationRequested)
            //    {
            //        break;
            //    }
            //}
        }
    }
}
