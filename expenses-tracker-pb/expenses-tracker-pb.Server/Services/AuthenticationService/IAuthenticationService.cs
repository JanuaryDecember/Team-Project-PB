namespace expenses_tracker_pb.Server.Services.AuthenticationService
{
    public interface IAuthenticationService
    {
        Task Register(RegisterRequest request);
        Task Login(LoginRequest request);
        Task Logout();
    }
}