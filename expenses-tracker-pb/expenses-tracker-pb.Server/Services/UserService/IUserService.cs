namespace expenses_tracker_pb.Server.Services.UserService
{
    public interface IUserService
    {
        bool IsUserLogged();
        Task ChangePasswordWith2Fa(PasswordChangeRequest request);
        Task ChangePasswordWithEmail(PasswordChangeRequest request);
        Task<User> UpdateProfilePageData(UpdateUserRequest request);
        Task<object> GetProfilePageData();
        Task<User> GetCurrentUser();
        Task ChangeProfilePicture(IFormFile file);
    }
}