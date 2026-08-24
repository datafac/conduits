using Grpc.Core;
using ProtoBuf.Grpc.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XGrpcServer;
using XGrpcShared;

namespace XGprcServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var calcServer = new CalculatorServer(10042);
            try
            {
                Console.WriteLine("Server running... press any key");
                Console.ReadKey();
            }
            finally
            {
                await calcServer.DisposeAsync().ConfigureAwait(false);
            }

        }
    }
}
