using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;

namespace pipline
{
    class Program
    {
        static void Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var t1 = Task.Run(() =>
            {
                while(true)
                {
                    if(Console.ReadKey(true).KeyChar != 'q') continue;
                    cts.Cancel();
                    break;
                }
            });
            var t2 = Task.Run(() => { MainProc(cts.Token); });
            Task.WaitAll(t1, t2);
            cts.Dispose();
        }
        
        private static void MainProc(CancellationToken ctsToken)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[1];//ipv4 address
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 9000);
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            //service
            s.Bind(remoteEP);
            //client
            s.Connect(remoteEP);

            Task.Run(() => sendproc(s,remoteEP));
            var test = new Test();
            test.ProcessLinesAsync(s);
        }

        private static void sendproc(Socket s, IPEndPoint remoteEp)
        {
            //send message
            s.SendTo(new byte[] {1, 2, 3}, remoteEp);
        }
    }


    class Test
    {
        public async Task ProcessLinesAsync(Socket socket)
        {
            var pipe = new Pipe();
            Task writing = FillPipeAsync(socket, pipe.Writer);
            Task reading = ReadPipeAsync(pipe.Reader);
            await Task.WhenAll(reading, writing);
        }

        async Task FillPipeAsync(Socket socket, PipeWriter writer)
        {
            const int minimumBufferSize = 512;

            while(true)
            {
                Memory<byte> memory = writer.GetMemory(minimumBufferSize);
                try
                {
                    int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None);
                    if(bytesRead == 0)
                    {
                        break;
                    }
                    writer.Advance(bytesRead);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }

                FlushResult result = await writer.FlushAsync();

                if(result.IsCompleted)
                {
                    break;
                }
            }

            writer.Complete();
        }

        async Task ReadPipeAsync(PipeReader reader)
        {
            while(true)
            {
                ReadResult result = await reader.ReadAsync();

                ReadOnlySequence<byte> buffer = result.Buffer;
                SequencePosition? position = null;

                do
                {
                    position = buffer.PositionOf((byte)'\n');

                    if(position != null)
                    {
                        buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
                    }
                }
                while(position != null);

                reader.AdvanceTo(buffer.Start, buffer.End);

                //print read message
                foreach(var readOnlyMemory in buffer)
                {
                    foreach(var b in readOnlyMemory.ToArray())
                    {
                        Console.WriteLine(b);
                    }
                }

                if(result.IsCompleted)
                {
                    break;
                }
            }

            reader.Complete();
        }
    }
}
