using Microsoft.AspNetCore.Identity;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;

    public AuthenticationService(UserManager<User> userManager, SignInManager<User> signInManager, ETDbContext dbContext, EmailSender emailSender)
    {
        _userManager = userManager;
    }

    public async Task<string> CredentialsCheck(UserModelForRegistration user)
    {
        if (user == null)
        {
            throw new ArgumentNullException("User not provided");
        }

        var existingUserByName = !string.IsNullOrEmpty(user.Username) ? await _userManager.FindByNameAsync(user.Username) : throw new Exception("Username is empty");
        if (existingUserByName != null)
        {
            return "Username already taken!";
        }

        var existingUserByEmail = !string.IsNullOrEmpty(user.Email) ? await _userManager.FindByEmailAsync(user.Email) : throw new Exception("Email is empty");
        if (existingUserByEmail != null)
        {
            return "Email already taken!";
        }

        return "";
    }

    public async Task<bool> register(UserModelForRegistration user)
    {
        string credentialsCheck = await CredentialsCheck(user);
        if (!string.IsNullOrEmpty(credentialsCheck))
            throw new Exception(credentialsCheck);

        var newUser = new User
        {
            UserName = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Wallets = new List<Wallet>(),
            EmailTwoFactorAuthenticationEnabled = false
        };

        var result = await _userManager.CreateAsync(newUser, user.Password);

        return result.Succeeded;
    }

}
