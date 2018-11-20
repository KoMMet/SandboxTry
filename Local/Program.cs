using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Local
{
    class Program
    {
        static void Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            //program end 
            var t1 = Task.Run(() => { if(Console.ReadKey(true).KeyChar == 'q') cts.Cancel(); });
            var t2 = Task.Run(() => { MainProc(cts.Token); });
            Task.WaitAll(t1, t2);
            cts.Dispose();
        }
        
        private static void MainProc(CancellationToken ctsToken)
        {
            var threadLocal = new ThreadLocal<int>(() => Thread.CurrentThread.ManagedThreadId, true);
            Action action = () => Console.WriteLine(threadLocal.Value);
            Parallel.Invoke(action, action, action, action);

            var values = threadLocal.Values.Select(x => x.ToString()).Aggregate((x, y) => $"{x}, {y}");
            Console.WriteLine(values);
        }
    }
}
