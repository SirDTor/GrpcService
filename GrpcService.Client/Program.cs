using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcService.Client;

namespace GrpcService.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {

            // The port number must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("https://localhost:7191");
            var client = new Greeter.GreeterClient(channel);

            using var call = client.SayHelloStream();

            var readTask = Task.Run(async () =>
            {
                await foreach (var response in call.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine(response.Message);
                }
            });

            while(true)
            {
                var result = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(result)) 
                {
                    break;
                }
                await call.RequestStream.WriteAsync(new HelloRequest() { Name = result });
            }

            await call.RequestStream.CompleteAsync();
            await readTask;

            //var reply = await client.SayHelloAsync(
            //                  new HelloRequest { Name = Console.ReadLine() });

            // Console.WriteLine("Greeting: " + reply.Message);

            //Console.ReadKey();
        }
    }
}