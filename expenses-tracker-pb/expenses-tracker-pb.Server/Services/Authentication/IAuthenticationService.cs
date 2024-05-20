public interface IAuthenticationService
{
    Task<bool> register(UserModelForRegistration user);
}
