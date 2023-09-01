using Grpc.Core;
using Grpc.Core.Interceptors;

namespace GrpcService.Core.Interceptors
{
    public class LoggingInterceptor : Interceptor
    {
        private ILogger<LoggingInterceptor> _logger;

        public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
        {
            _logger = logger;
        }

        public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, 
            IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            _logger.Log(LogLevel.Information, message: $"Start - {context.Method}");

            await base.DuplexStreamingServerHandler(requestStream, responseStream, context, continuation);

            _logger.Log(LogLevel.Information, message: $"End - {context.Method}");
        }

        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, 
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            _logger.Log(LogLevel.Critical, message: $"Connected client with rights: admin");

            return base.UnaryServerHandler(request, context, continuation);
        }
    }
}
