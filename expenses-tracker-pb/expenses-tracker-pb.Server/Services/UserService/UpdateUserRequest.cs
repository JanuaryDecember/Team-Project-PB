namespace expenses_tracker_pb.Server.Services.UserService
{
    public class UpdateUserRequest(string? firstName, string? lastName, string? username, string? email, string? password)
    {
        public string? FirstName { get; set; } = firstName;
        public string? LastName { get; set; } = lastName;
        public string? UserName { get; set; } = username;
        public string? Email { get; set; } = email;
        public string? Password { get; set; } = password;
    }
}
