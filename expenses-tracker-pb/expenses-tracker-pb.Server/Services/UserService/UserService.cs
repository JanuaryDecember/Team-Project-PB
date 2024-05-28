using expenses_tracker_pb.Server.Exceptions;
using Google.Authenticator;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using System.Security.Authentication;
using expenses_tracker_pb.Server.Model;

namespace expenses_tracker_pb.Server.Services.UserService
{
    public class UserService(UserManager<User> userManager, EmailSender emailSender, EtDbContext dbContext, IHttpContextAccessor httpContextAccessor) : IUserService
    {
        public async Task ChangePasswordWith2Fa(PasswordChangeRequest request)
        {
            var user = await FindUserByUsername(request.Username);
            TwoFactorAuthenticator TwoFacAuth = new();

            if (!TwoFacAuth.ValidateTwoFactorPIN(user.GoogleAuthKey, request.Code, TimeSpan.FromSeconds(15)))
            {
                throw new InvalidCredentialException("Code missmatched");
            }

            await ChangeUsersPassword(user, request.NewPassword);
        }

        public async Task ChangePasswordWithEmail(PasswordChangeRequest request)
        {
            var user = await FindUserByUsername(request.Username);
            if (request.Code != null)
            {
                string generatedCode = GenerateCodeForPasswordChange();

                ArgumentNullException.ThrowIfNull(user.Email);
                emailSender.sendPasswordRecoveryCode(user.Email, generatedCode);
                user.ResetPasswordCode = generatedCode;
                user.ResetPasswordCodeExpireTime = DateTime.Now.AddMinutes(1);
                await dbContext.SaveChangesAsync();
                throw new EmailConfirmationException();
            }

            if (user.ResetPasswordCode != request.Code)
                throw new InvalidCredentialException("Code missmatched");

            if (user.ResetPasswordCodeExpireTime <= DateTime.Now)
                throw new InvalidCredentialException("Code exipred");

            await ChangeUsersPassword(user, request.NewPassword);
        }

        public async Task<User> GetCurrentUser()
        {
            var user = (httpContextAccessor.HttpContext?.User) ?? throw new AuthenticationException("User not logged in");
            return await userManager.GetUserAsync(user) ?? throw new InvalidCredentialException("User not found");
        }

        public async Task<object> GetProfilePageData()
        {
            User user = await GetCurrentUser();
            IEnumerable<UserLoginInfo> logins = await userManager.GetLoginsAsync(user);
            return new { user, logins };
        }

        public bool IsUserLogged()
        {
            var user = httpContextAccessor.HttpContext?.User;
            var isAuthenticated = user?.Identity?.IsAuthenticated;
            return isAuthenticated != null && isAuthenticated.Equals(true);
        }

        public async Task<User> UpdateProfilePageData(UpdateUserRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            User user = await GetCurrentUser();

            var requestProperties = request.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var userProperties = user.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var requestProperty in requestProperties)
            {
                var value = requestProperty.GetValue(request);
                if (value != null)
                {
                    var userProperty = Array.Find(userProperties, prop => prop.Name.Equals(requestProperty.Name));
                    if (userProperty != null && userProperty.CanWrite)
                    {
                        userProperty.SetValue(user, value);
                    }
                }
            }

            IdentityResult result = await userManager.UpdateAsync(user);

            if (request.Password != null)
            {
                await userManager.RemovePasswordAsync(user);
                await userManager.AddPasswordAsync(user, request.Password);
            }
            return await GetCurrentUser();
        }

        public async Task ChangeProfilePicture(IFormFile file)
        {
            ArgumentNullException.ThrowIfNull(file);
            if (file.Length == 0)
            {
                throw new ArgumentException("Please upload a valid file");
            }

            if (!file.ContentType.Contains("image/jpeg") && !file.ContentType.Contains("image/png"))
            {
                throw new UnsupportedMediaTypeException();
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                throw new FileSizeException("Image is too big");
            }

            var user = await GetCurrentUser();

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            user.ProfilePicture = memoryStream.ToArray();
            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new Exception("Unable to update profile picture");
            }


        }

        private static string GenerateCodeForPasswordChange()
        {
            Random random = new();
            byte[] bytes = new byte[6];
            random.NextBytes(bytes);
            return Convert.ToBase64String(bytes)[..8];
        }

        private async Task ChangeUsersPassword(User user, string NewPassword)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, NewPassword);
            if (!result.Succeeded)
                throw new PasswordChangeException();
        }

        private async Task<User> FindUserByUsername(string Username)
        {
            return await userManager.FindByNameAsync(Username) ?? throw new InvalidCredentialException("User not found");
        }

    }
}

