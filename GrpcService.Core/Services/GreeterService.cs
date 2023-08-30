using Grpc.Core;
using GrpcService.Protocol;

namespace GrpcService.Core.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        private int _number = 0;

        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<MessageReply> SayMessage(MessageRequest request, ServerCallContext context)
        {
            return Task.FromResult(new MessageReply
            {
                Message = "Hello " + request.Name                
            });
        }

        public override async Task SayMessageStream(IAsyncStreamReader<MessageRequest> requestStream,
            IServerStreamWriter<MessageReply> responseStream, ServerCallContext context)
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                Console.WriteLine("Accepted message");
                _number++;
                await responseStream.WriteAsync(new MessageReply()
                {
                    Num = _number,
                    Message = "Accepted " + request.Name + " " + DateTime.UtcNow
                });                
            }
        }
    }
}