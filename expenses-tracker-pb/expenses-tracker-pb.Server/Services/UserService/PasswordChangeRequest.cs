namespace expenses_tracker_pb.Server.Services.UserService
{
    public class PasswordChangeRequest(string username, string newPassword, string? code)
    {
        public string Username { get; set; } = username;
        public string NewPassword { get; set; } = newPassword;
        public string? Code { get; set; } = code;
    }
}