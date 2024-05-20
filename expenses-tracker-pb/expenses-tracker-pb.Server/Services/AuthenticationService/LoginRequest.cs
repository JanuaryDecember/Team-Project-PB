namespace expenses_tracker_pb.Server.Services.AuthenticationService
{
    public class LoginRequest(string login, string password, string? authKey, string? emailAuthorizationCode, string? securityQuestionAnswer)
    {
        public string Login { get; set; } = login;
        public string Password { get; set; } = password;
        public string? AuthKey { get; set; } = authKey;
        public string? EmailAuthorizationCode { get; set; } = emailAuthorizationCode;
        public string? SecurityQuestionAnswer { get; set; } = securityQuestionAnswer;
    }
}