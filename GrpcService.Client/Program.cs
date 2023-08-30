using System.Diagnostics;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcService.Client;
using GrpcService.Protocol;

namespace GrpcService.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            byte authCount = 0;
            string? login = "Elesy";
            int count = 0;
            Console.Write("Введите порт сервера:");
            string? port = Console.ReadLine();

            if (port != null)
            {
                // The port number must match the port of the gRPC server.
                using var channel = GrpcChannel.ForAddress($"https://localhost:{port}");
                var client = new Auth.AuthClient(channel);
                while (authCount < 4)
                {
                    authCount++;

                    AuthRequest authRequest = new AuthRequest();

                    Console.WriteLine("Login:");
                    authRequest.Login = Console.ReadLine();

                    Console.WriteLine("Password");
                    authRequest.Password = Console.ReadLine();

                    var auth = await client.AuthenticationAsync(authRequest);

                    Console.WriteLine(auth.Message);

                    if (!auth.IsTrue)
                    {
                        Console.WriteLine($"Осталось попыток {3 - authCount}");
                        if (authCount == 3)
                        {
                            Environment.Exit(0);
                        }
                    }
                    else
                    {
                        login = auth.Login;
                        break;
                    }
                }
                var clientMessage = new Greeter.GreeterClient(channel);
                using var call = clientMessage.SayMessageStream();

                var readTask = Task.Run(async () =>
                {
                    await foreach (var response in call.ResponseStream.ReadAllAsync())
                    {
                        Console.WriteLine($"{login}: Номер {response.Num} {response.Message}");
                    }
                });

                while (true)
                {
                    //await Task.Delay(1);
                    var result = $"message №{count++}";
                    Console.WriteLine($"Sent {result}");
                    if (string.IsNullOrWhiteSpace(result))
                    {
                        break;
                    }
                    await call.RequestStream.WriteAsync(new MessageRequest() { Name = result });
                }

                await call.RequestStream.CompleteAsync();
                await readTask;
            }
            //var reply = await client.SayHelloAsync(
            //                  new HelloRequest { Name = Console.ReadLine() });

            // Console.WriteLine("Greeting: " + reply.Message);

            //Console.ReadKey();
        }
    }
}
