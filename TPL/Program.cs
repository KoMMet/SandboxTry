using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace TPL
{
    class Program
    {
        static void Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            //program end 
            var t1 = Task.Run(() =>
            {
                if(Console.ReadKey(true).KeyChar == 'q') cts.Cancel();
            });
            var t2 = Task.Run(() => { MainProc(cts.Token); });
            Task.WaitAll(t1, t2);
            cts.Dispose();
        }

        private static void MainProc(CancellationToken ctsToken) =>
                //main logic
                Enumerable.Range(1, 10).AsParallel().Select(x => x * 10).ForAll(Console.WriteLine);
    }
}
