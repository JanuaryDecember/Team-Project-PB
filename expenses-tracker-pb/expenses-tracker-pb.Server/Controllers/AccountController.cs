using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Google.Authenticator;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.UI.Services;

[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly ETDbContext _dbContext;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly EmailSender _emailSender;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ETDbContext dbContext, EmailSender emailSender)

    {
        _dbContext = dbContext;
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _signInManager.SignOutAsync();
        }
        catch (Exception exception)
        {
            return BadRequest("Unable to logout. " + exception.ToString());
        }
        return Ok("Sucsessfully logged out!");
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] Credentials cred)
    {
        var user = await _userManager.FindByNameAsync(cred.Login);

        if (user == null)
        {
            user = await _userManager.FindByEmailAsync(cred.Login);
            if (user == null)
                return Unauthorized("Invalid credentials");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, cred.Password, lockoutOnFailure: false);

        if (!signInResult.Succeeded)
        {
            return Unauthorized("Invalid credentials.");
        }

        // Two-Factor Authentication is needed
        if (user.TwoFactorEnabled && cred.AuthKey == null)
        {
            return StatusCode(202, "Two-Factor Authentication");
        }
        else if (user.TwoFactorEnabled && cred.AuthKey != null)
        {
            // Validate key
            TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
            bool isValid = TwoFacAuth.ValidateTwoFactorPIN(user.GoogleAuthKey, cred.AuthKey, TimeSpan.FromSeconds(15));

            if (!isValid)
            {
                return Unauthorized("Invalid credentials");
            }
        }

        // Generowanie tokena JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = GenerateRandomKey();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),

            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        await _signInManager.SignInAsync(user, isPersistent: false);

        // Zwr�� token JWT w odpowiedzi
        return Ok(new { Token = tokenString });
    }

    private string GenerateRandomKey()
    {
        const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        StringBuilder stringBuilder = new StringBuilder();
        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            byte[] uintBuffer = new byte[sizeof(uint)];

            for (int i = 0; i < 64; i++) // Generowanie klucza o d�ugo�ci 64 znak�w
            {
                rng.GetBytes(uintBuffer);
                uint num = BitConverter.ToUInt32(uintBuffer, 0);
                stringBuilder.Append(validChars[(int)(num % (uint)validChars.Length)]);
            }
        }

        return stringBuilder.ToString();
    }

    [HttpPost("register")]
    public async Task<IActionResult> AddUser([FromBody] UserModelForRegistration user)
    {
        var existingUser = await _userManager.FindByNameAsync(user.Username);
        if (existingUser != null)
        {
            return Conflict("Username already taken");
        }
        existingUser = await _userManager.FindByEmailAsync(user.Email);
        if (existingUser != null)
        {
            return Conflict("Email already in use");
        }
        var newUser = new User
        {
            UserName = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Wallets = new List<Wallet>()
        };

        var result = await _userManager.CreateAsync(newUser, user.Password);

        if (!result.Succeeded)
        {
            return BadRequest("Registration failed. " + result.ToString());
        }

        return Ok("Registartion succeded");
    }

    [HttpPost("changePassword")]
    public async Task<ActionResult> changePassword([FromBody] PasswordChange data)
    {
        // Checking if user exist
        var existingUser = await _userManager.FindByNameAsync(data.username);
        if (existingUser == null)
        {
            return BadRequest("Unsuccessful");
        }
        // Change password throw e-mail
        if (data.email == true)
        {
            if (existingUser.ResetPasswordCode != data.code || existingUser.ResetPasswordCodeExpireTime <= DateTime.Now)
            {
                return BadRequest("Unsuccessful");
            }
        }
        // Change password throw 2fa
        else if (data.fa == true && existingUser.TwoFactorEnabled)
        {
            TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
            bool isValid = TwoFacAuth.ValidateTwoFactorPIN(existingUser.GoogleAuthKey, data.code, TimeSpan.FromSeconds(15));

            if (!isValid)
            {
                return BadRequest("Unsuccessful");
            }
        }
        // Request body is incorrect
        else return BadRequest("Unsuccessful");

        // Setting new password for user
        var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
        var result = await _userManager.ResetPasswordAsync(existingUser, token, data.newPassword);

        if (result.Succeeded)
        {
            return Ok("Password have been changed");
        }
        else
        {
            return BadRequest("Unsuccessful");
        }
    }

    [HttpPost("passwordChangeEmail")]
    public async Task<ActionResult> passwordChangeEmail([FromBody] string username)
    {
        var existingUser = await _userManager.FindByNameAsync(username);
        if (existingUser != null)
        {
            Random random = new Random();
            byte[] bytes = new byte[6];
            random.NextBytes(bytes);
            string code = Convert.ToBase64String(bytes).Substring(0, 8);
            _emailSender.SendEmail(existingUser.Email, code);
            existingUser.ResetPasswordCode = code;
            existingUser.ResetPasswordCodeExpireTime = DateTime.Now.AddMinutes(1);
            await _dbContext.SaveChangesAsync();
            return Ok("Sent");
        }
        return NotFound("Not Found User");
    }

    [Authorize]
    [HttpGet("getWallets")]
    public async Task<JsonResult> getWallets()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return new JsonResult(Unauthorized("Session ended! Sign in again"));
        }
        var wallets = await _dbContext.Wallets.Where(w => w.UserId == userId).ToListAsync();
        return new JsonResult(wallets);
    }

    [Authorize]
    [HttpDelete("removeWallet/{walletId}")]
    public async Task<IActionResult> DeleteWallet(string walletId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized("Session ended! Sign in again");
        }


        var user = await _dbContext.Users
        .Include("Wallets")
        .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return Unauthorized("Unable to find user with this id");
        }

        var wallet = user.Wallets.FirstOrDefault(w => w.Id == long.Parse(walletId));

        if (wallet == null)
        {
            return NotFound("Wallet not found");
        }

        user.Wallets.Remove(wallet);

        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [Authorize]
    [HttpPut("updateWallet/{walletId}")]
    public async Task<IActionResult> updateWallet(string walletId, [FromBody] string name)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized("Session ended! Sign in again");
        }


        var user = await _dbContext.Users
        .Include("Wallets")
        .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return Unauthorized("Unable to find user with this id");
        }

        var wallet = user.Wallets.FirstOrDefault(w => w.Id == long.Parse(walletId));

        if (wallet == null)
        {
            return NotFound("Wallet not found");
        }

        wallet.Name = name;

        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [Authorize]
    [HttpPost("addWallet")]
    public async Task<IActionResult> AddWallet([FromBody] WalletRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized("Session ended! Sign in again");
        }

        var user = await _dbContext.Users
        .Include("Wallets")
        .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound("Unable to find user with this user id");
        }
        Wallet wallet = new()
        {
            User = user,
            UserId = userId,
            Name = request.name,
            IconId = 1,
            AccountBalance = 0,
            Incomes = new List<Income>(),
            Expenditures = new List<Expenditure>()
        };

        user.Wallets.Add(wallet);
        await _dbContext.SaveChangesAsync();
        return Ok("Wallet added succesfully");
    }

    private static byte[] ConvertSecretToBytes(string secret, bool secretIsBase32) =>
       secretIsBase32 ? Base32Encoding.ToBytes(secret) : Encoding.UTF8.GetBytes(secret);

    [Authorize]
    [HttpGet("GetTwoFactorStatus")]
    public async Task<IActionResult> GetTwoFactorStatus()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                return Ok(new { twoFactorEnabled = user.TwoFactorEnabled });
            }
            else
            {
                return NotFound("User not found");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error:" + ex.Message);
        }
    }
    [Authorize]
    [HttpGet("GetTwoFactorKey")]
    public async Task<IActionResult> GetTwoFactorKey()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                string GoogleAuthKey = "";
                string QrImageUrl = "";
                const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                // Generate 10 char secret key
                Random random = new Random();
                for (int i = 0; i < 10; i++)
                {
                    GoogleAuthKey += validChars[random.Next(0, validChars.Length)];
                }
                // Generate setupcode
                TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
                var setupInfo = TwoFacAuth.GenerateSetupCode("ExpensionTracker", user.UserName, ConvertSecretToBytes(GoogleAuthKey, false), 200);
                QrImageUrl = setupInfo.QrCodeSetupImageUrl;
                return Ok(new { authKey = GoogleAuthKey, barcodeImageUrl = QrImageUrl });
            }
            else
            {
                return NotFound("User not found");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error:" + ex.Message);
        }
    }
    [Authorize]
    [HttpPost("enableTwoFactor")]
    public async Task<IActionResult> enableTwoFactor()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                // Read json data from body
                string requestBody;
                using (var reader = new System.IO.StreamReader(Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                dynamic data = JObject.Parse(requestBody);
                string authKey = data.authKey;
                string enteredAuthKey = data.enteredAuthKey;
                // Validate key
                TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
                bool isValid = TwoFacAuth.ValidateTwoFactorPIN(authKey, enteredAuthKey, TimeSpan.FromSeconds(15));

                if (isValid)
                {
                    user.TwoFactorEnabled = true;
                    user.GoogleAuthKey = authKey;
                    await _dbContext.SaveChangesAsync();

                    return Ok(true);
                }

                return Ok(false);
            }
            else
            {
                return NotFound("User not found");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error:" + ex.Message);
        }
    }
    [Authorize]
    [HttpPost("disableTwoFactor")]
    public async Task<IActionResult> disableTwoFactor()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                // Read json data from body
                string requestBody;
                using (var reader = new System.IO.StreamReader(Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                dynamic data = JObject.Parse(requestBody);
                string enteredAuthKey = data.enteredAuthKey;
                // Validate key
                TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
                bool isValid = TwoFacAuth.ValidateTwoFactorPIN(user.GoogleAuthKey, enteredAuthKey, TimeSpan.FromSeconds(15));

                if (isValid)
                {
                    user.TwoFactorEnabled = false;
                    user.GoogleAuthKey = null;
                    await _dbContext.SaveChangesAsync();
                    return Ok(true);
                }

                return Ok(false);
            }
            else
            {
                return NotFound("User not found");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error:" + ex.Message);
        }
    }

    [HttpGet("GetProfilePageData")]
    [Authorize]
    public async Task<IActionResult> GetProfilePageData()
    {
        try
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new ArgumentNullException(nameof(userId));
            User user = await _userManager.FindByIdAsync(userId) ?? throw new Exception($"User {userId} not found in DB");
            IEnumerable<UserLoginInfo> logins = await _userManager.GetLoginsAsync(user);

            return new OkObjectResult(new { user, logins });
        }
        catch (Exception ex)
        {
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
    private string? GetCurrentUserId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userId;
    }


    [HttpPost("user")]
    public IActionResult IsUserLogged()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return StatusCode(401, "The user is not logged in!");
        }
        return Ok();
    }

    [HttpPost("UpdateProfilePageData")]
    [Authorize]
    public async Task<IActionResult> UpdateProfilePageData([FromBody] UserUpdateModel updatedUserData)
    {
        try
        {
            if (updatedUserData == null)
            {
                return BadRequest("Invalid data");
            }

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new ArgumentNullException(nameof(userId));
            User user = await _userManager.FindByIdAsync(userId) ?? throw new Exception($"User {userId} not found in DB");

            user.FirstName = updatedUserData.FirstName;
            user.LastName = updatedUserData.LastName;
            user.UserName = updatedUserData.UserName;
            user.Email = updatedUserData.Email;

            IdentityResult result = await _userManager.UpdateAsync(user);

            await _userManager.RemovePasswordAsync(user);
            await _userManager.AddPasswordAsync(user, updatedUserData.Password);

            if (result.Succeeded)
            {
                return new OkObjectResult(new { user });
            }
            else
            {
                return BadRequest("Failed to update user data");
            }
        }
        catch (Exception ex)
        {
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
    public class UserUpdateModel
    {
        public UserUpdateModel(string firstName, string lastName, string username, string email, string password)
        {
            FirstName = firstName;
            LastName = lastName;
            UserName = username;
            Email = email;
        }

        public UserUpdateModel()
        {

        }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
    }

    public class Credentials
    {
        public Credentials(string login, string password, string? authKey)
        {
            Login = login;
            Password = password;
            AuthKey = authKey;
        }

        public string Login { get; set; }
        public string Password { get; set; }
        public string? AuthKey { get; set; }
    }

    public class UserModelForRegistration
    {
        public UserModelForRegistration(string firstName, string lastName, string username, string email, string password)
        {
            FirstName = firstName;
            LastName = lastName;
            Username = username;
            Email = email;
            Password = password;
        }

        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}

public class WalletRequest
{
    public string name { get; set; }
}
