using System;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Net.Client;
namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5021");

            var calcClient = new Calc.CalcClient(channel);
            var addreply = await calcClient.AddAsync(
                new AddRequest { A = 2, B = 3 }
                );
            Console.WriteLine(addreply.C);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
