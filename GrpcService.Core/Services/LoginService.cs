using Grpc.Core;
using GrpcService.Protocol;
namespace GrpcService.Core.Services
{
    public class LoginService : Auth.AuthBase
    {
        private readonly string _username = "admin";
        private readonly string _password = "1234qwer";

        public override Task<AuthReply> Authentication(AuthRequest request, ServerCallContext context)
        {
            if (request.Login != _username || request.Password != _password) 
            {
                return Task.FromResult(new AuthReply
                {
                    Message = "Неверный логин или пароль",
                    IsTrue = false
                });
            }
            else return Task.FromResult(new AuthReply
            {
                Login = request.Login,
                Message = $"Успешный вход пользователя {request.Login}",
                IsTrue = true
            });
        }
    }
}
