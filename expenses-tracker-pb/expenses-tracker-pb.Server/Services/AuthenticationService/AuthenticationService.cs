using expenses_tracker_pb.Server.Exceptions.Authentication;
using Google.Authenticator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;
using System.Text;


namespace expenses_tracker_pb.Server.Services.AuthenticationService
{
    public class AuthenticationService(UserManager<User> userManager, SignInManager<User> signInManager, ETDbContext dbContext, EmailSender emailSender) : IAuthenticationService
    {
        public async Task Register(RegisterRequest request)
        {
            await CredentialsAvailabilityCheck(request);

            var newUser = new User
            {
                UserName = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Wallets = [],
                EmailTwoFactorAuthenticationEnabled = false
            };

            await userManager.CreateAsync(newUser, request.Password);
        }

        public async Task Login(LoginRequest request)
        {
            var user = await userManager.Users
                   .Include(u => u.SecurityQuestion)
                   .FirstOrDefaultAsync(u => request.Login.Contains('@') ? u.Email == request.Login : u.UserName == request.Login) ??
                   throw new InvalidCredentialException("Invalid credentials");

            await CheckTwoFactorAuthentication(user, request);

            var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

            if (!signInResult.Succeeded)
            {
                throw new InvalidCredentialException("Invalid credentials");
            }

            await signInManager.SignInAsync(user, isPersistent: false);
        }

        public async Task Logout()
        {
            await signInManager.SignOutAsync();
        }

        private async Task CredentialsAvailabilityCheck(RegisterRequest request)
        {
            var existingUser = await userManager.FindByNameAsync(request.Username);
            if (existingUser != null)
            {
                throw new InvalidCredentialException("Username already taken");
            }
            existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidCredentialException("Email already in use");
            }
        }

        private async Task CheckTwoFactorAuthentication(User user, LoginRequest request)
        {
            ArgumentNullException.ThrowIfNull(user);

            if (user.TwoFactorEnabled)
                AuthenticateWithGoogle(user, request);

            if (user.EmailTwoFactorAuthenticationEnabled)
                await AuthenticateWithEmail(user, request);

            if (user.SecurityQuestionAnswer != null)
                AuthenticateWithQuestion(user, request);

        }

        private static void AuthenticateWithGoogle(User user, LoginRequest request)
        {
            if (request.AuthKey == null)
                throw new GoogleAuthenticationException();
            else
            {
                TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
                bool isValid = TwoFacAuth.ValidateTwoFactorPIN(user.GoogleAuthKey, request.AuthKey, TimeSpan.FromSeconds(15));
                if (!isValid)
                    throw new InvalidCredentialException("Google authenticator code missmatch");
            }
        }

        private async Task AuthenticateWithEmail(User user, LoginRequest request)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            if (request.EmailAuthorizationCode == null)
            {
                TimeSpan diff = DateTime.Now - user.LastEmailTwoFactorAuthenticationCodeSent.GetValueOrDefault();
                if (diff.TotalMinutes <= 1)
                {
                    throw new TooManyRequestsException();
                }

                string code = "";

                Random random = new Random();
                for (int i = 0; i < 10; i++)
                {
                    code += validChars[random.Next(0, validChars.Length)];
                }

                user.EmailTwoFactorAuthenticationCode = code;
                user.EmailTwoFactorAuthenticationExpiryTime = DateTime.Now.AddMinutes(5);
                user.LastEmailTwoFactorAuthenticationCodeSent = DateTime.Now;

                dbContext.Update(user);
                await dbContext.SaveChangesAsync();

                if (!string.IsNullOrEmpty(user.Email))
                    emailSender.SendTwoFactorAuthenticationCode(user.Email, user.EmailTwoFactorAuthenticationCode);

                throw new EmailAuthenticationException();
            }
            else
            {
                if (!(user.EmailTwoFactorAuthenticationCode == request.EmailAuthorizationCode
                    && user.EmailTwoFactorAuthenticationExpiryTime.GetValueOrDefault().CompareTo(DateTime.Now) > 0))
                    throw new InvalidCredentialException("Email code missmatch");
            }
        }

        private static void AuthenticateWithQuestion(User user, LoginRequest request)
        {
            if (request.SecurityQuestionAnswer == null)
            {
                ArgumentNullException.ThrowIfNull(user.SecurityQuestion);
                throw new QuestionAuthenticationException(user.SecurityQuestion.Question);
            }
            else
            {
                if (user.SecurityQuestionAnswer != request.SecurityQuestionAnswer)
                    throw new InvalidCredentialException("Security answer missmatch");
            }
        }

        private static byte[] ConvertSecretToBytes(string secret, bool secretIsBase32) =>
           secretIsBase32 ? Base32Encoding.ToBytes(secret) : Encoding.UTF8.GetBytes(secret);

    }
}