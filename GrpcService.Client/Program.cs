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
            //Console.Write("Введите порт сервера:");
            string? port = "1";//Console.ReadLine();

            if (port != null)
            {
                // The port number must match the port of the gRPC server.
                //using var channel = GrpcChannel.ForAddress($"https://localhost:{port}");

                using var channel = GrpcChannel.ForAddress($"https://localhost:9070");
                var client = new Auth.AuthClient(channel);
                while (authCount < 4)
                {
                    authCount++;

                    AuthRequest authRequest = new AuthRequest();

                    Console.Write("Login: ");
                    authRequest.Login = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(login)) continue;

                    Console.Write("Password: ");
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

                var connectOPCUA = new GetNode.GetNodeClient(channel);
                var callConnect = await connectOPCUA.ConnectToOPCServerAsync(new NodeRequest());
                Console.WriteLine(callConnect.IsConnect.ToString());

                var getNodeInfo = connectOPCUA.GetNodeInformation(new NodeRequest());
                Console.WriteLine($"{getNodeInfo.NodeName} : {getNodeInfo.NodeValue}");

                var getNodeInfoStream = connectOPCUA.GetNodeInformationClientStream();

                var readNodeTask = Task.Run(async () =>
                {
                    await foreach (var response in getNodeInfoStream.ResponseStream.ReadAllAsync())
                    {
                        Console.WriteLine($"{login}: Node : {response.NodeName} Value : {response.NodeValue}");
                    }
                });
                await readNodeTask;
                await Task.Delay(5000);
                ConsoleKeyInfo cki = new ConsoleKeyInfo();
                var GetNodeValueStream = connectOPCUA.GetNodeInformationServerStream(new NodeRequest());
                // получаем поток сервера
                var responseStream = GetNodeValueStream.ResponseStream;
                // с помощью итераторов извлекаем каждое сообщение из потока
                while (await responseStream.MoveNext())
                {
                    NodeReply response = responseStream.Current;
                    if (cki.Key == ConsoleKey.Z)
                    {
                        break;
                    }
                    Console.WriteLine($"{login}: Node : {response.NodeName} Value : {response.NodeValue}");
                    if (response.NodeName.Contains("14"))
                    {
                        Console.WriteLine("________________________________");
                    }
                }

                //ConsoleKeyInfo cki = new ConsoleKeyInfo();
                int i = 0;
                while (i!=10)
                {
                    if (cki.Key == ConsoleKey.Z)
                    {
                        break;
                    }
                    await Task.Delay(2000);
                    await getNodeInfoStream.RequestStream.WriteAsync(new NodeRequest());
                    i++;
                }
                await getNodeInfoStream.RequestStream.CompleteAsync();
                await readNodeTask;

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
                    await Task.Delay(10000);
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
