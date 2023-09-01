using GrpcService.Core.Interceptors;
using GrpcService.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

var serviceProvider = builder.Services.BuildServiceProvider();

var logger = serviceProvider.GetService<ILogger<LoggingInterceptor>>();

// Add services to the container.
builder.Services.AddGrpc(configureOptions:options =>
{
    options.Interceptors.Add(typeof(LoggingInterceptor), logger);
});
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GetNodeInformationService>();
app.MapGrpcService<LoginService>();
app.MapGrpcService<GreeterService>();
app.MapGet("/", () => "Test login service");
app.Run();
