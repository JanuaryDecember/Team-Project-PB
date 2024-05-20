using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using expenses_tracker_pb.Server.Services.AuthenticationService;
using System.Security.Authentication;
using expenses_tracker_pb.Server.Exceptions.Authentication;
using Google.Authenticator;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace expenses_tracker_pb.Server.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController(IAuthenticationService authenticationService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                await authenticationService.Register(request);
                return StatusCode(201, "Account created successfully");
            }
            catch (Exception e)
            {
                if (e is InvalidCredentialException)
                    return BadRequest(e.Message);
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                await authenticationService.Login(request);
                return Ok("Logged in successfully");
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    GoogleAuthenticationException e => StatusCode(202, e.Message),
                    EmailAuthenticationException e => StatusCode(202, e.Message),
                    QuestionAuthenticationException e => StatusCode(203, e.Message),
                    _ => StatusCode(500, exception.Message),
                };
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await authenticationService.Logout();
                return Ok("Logged out successfully");
            }
            catch (Exception exception)
            {
                return BadRequest("Unable to logout. " + exception.Message);
            }
        }

        [Authorize]
        [HttpPost("enableSecurityQuestionAuthentication")]
        public async Task<IActionResult> enableSecurityQuestionAuthentication()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await dbContext.Users.Include(x => x.SecurityQuestion).FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                if (user.SecurityQuestion == null)
                {
                    string requestBody;
                    using (var reader = new StreamReader(Request.Body))
                    {
                        requestBody = await reader.ReadToEndAsync();
                    }
                    dynamic data = JObject.Parse(requestBody);

                    string securityQuestion = data.securityQuestion;
                    string securityQuestionAnswer = data.securityQuestionAnswer;

                    if (!await dbContext.SecurityQuestions.AnyAsync(x => x.Question == securityQuestion))
                    {
                        return Unauthorized(new { message = "Security question with this id does not exist" });
                    }

                    user.SecurityQuestionAnswer = securityQuestionAnswer;
                    user.SecurityQuestion = dbContext.SecurityQuestions.FirstOrDefault(x => x.Question == securityQuestion);
                    dbContext.Update(user);
                    dbContext.SaveChanges();

                    return Ok(new { message = "Verification enabled" });

                }
                else
                {
                    return Ok(new { message = "Verification is already enabled" });
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, "Error:" + e.Message);
            }
        }

        [Authorize]
        [HttpPost("disableSecurityQuestionAuthentication")]
        public async Task<IActionResult> disableSecurityQuestionAuthentication()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await dbContext.Users.Include(x => x.SecurityQuestion).FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                if (user.SecurityQuestion != null)
                {
                    string requestBody;

                    using (var reader = new StreamReader(Request.Body))
                    {
                        requestBody = await reader.ReadToEndAsync();
                    }
                    dynamic data = JObject.Parse(requestBody);

                    string securityQuestionAnswer = data.securityQuestionAnswer;


                    if (user.SecurityQuestionAnswer != securityQuestionAnswer)
                    {
                        return Unauthorized(new { message = "Answer is not correct" });
                    }

                    user.SecurityQuestionAnswer = null;
                    user.SecurityQuestion = null;
                    dbContext.Update(user);
                    dbContext.SaveChanges();

                    return Ok(new { message = "Verification disable" });

                }
                else
                {
                    return Ok(new { message = "Verification is already disabled" });
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, "Error:" + e.Message);
            }
        }

        [HttpGet("getSecurityQuestions")]
        public async Task<JsonResult> getSecurityQuestions()
        {
            try
            {
                var questions = await dbContext.SecurityQuestions.ToListAsync();

                return new JsonResult(questions);
            }
            catch (Exception ex)
            {
                return new JsonResult(Unauthorized("Error:" + ex.Message));
            }
        }

        [Authorize]
        [HttpGet("getSecurityQuestionsStatus")]
        public async Task<IActionResult> getSecurityQuestionsStatus()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await dbContext.Users.Include(x => x.SecurityQuestion).FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null)
                {
                    var status = user.SecurityQuestion != null ? true : false;

                    return Ok(new { securityQuestionStatus = status });
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
        [HttpGet("getUserSecurityQuestion")]
        public async Task<IActionResult> getUserSecurityQuestion()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await dbContext.Users.Include(x => x.SecurityQuestion).FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null)
                {
                    if (user.SecurityQuestion != null)
                    {
                        return Ok(new { securityQuestion = user.SecurityQuestion.Question });
                    }
                    else
                    {
                        return NotFound("Question not found");
                    }
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
        // Email authentication
        [Authorize]
        [HttpPost("disableEmailAuthentication")]
        public async Task<IActionResult> disableEmailAuthentication()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null)
                {
                    if (user.EmailTwoFactorAuthenticationEnabled == true)
                    {
                        string requestBody;
                        using (var reader = new StreamReader(Request.Body))
                        {
                            requestBody = await reader.ReadToEndAsync();
                        }
                        dynamic data = JObject.Parse(requestBody);
                        string emailAuthenticationCode = data.emailAuthenticationCode;


                        if (user.EmailTwoFactorAuthenticationCode != emailAuthenticationCode ||
                            user.EmailTwoFactorAuthenticationExpiryTime < DateTime.Now)
                        {
                            return Unauthorized(new { message = "Code is not valid or expired" });
                        }

                        user.EmailTwoFactorAuthenticationEnabled = false;
                        dbContext.Update(user);
                        dbContext.SaveChanges();

                        return Ok(new { message = "Verification disabled" });

                    }
                    else
                    {
                        return Ok(new { message = "Verification is already disabled" });
                    }
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
        [HttpPost("enableEmailAuthentication")]
        public async Task<IActionResult> enableEmailAuthentication()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null)
                {
                    if (user.EmailTwoFactorAuthenticationEnabled == true)
                    {

                        return Ok(new { message = "Verification is already active" });
                    }
                    else
                    {
                        user.EmailTwoFactorAuthenticationEnabled = true;
                        dbContext.Update(user);
                        dbContext.SaveChanges();

                        return Ok(new { message = "Verification activated" });
                    }
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
        [HttpGet("getEmailAuthenticationStatus")]
        public async Task<IActionResult> getEmailAuthentication()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null)
                {
                    return Ok(new { EmailAuthentication = user.EmailTwoFactorAuthenticationEnabled });
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
        [HttpPost("sendEmailAuthenticationCode")]
        public async Task<IActionResult> SendEmailAuthenticationCode()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null)
                {
                    if (user.EmailTwoFactorAuthenticationEnabled == true)
                    {
                        // Check if user already sent request recently
                        TimeSpan roznica = DateTime.Now - user.LastEmailTwoFactorAuthenticationCodeSent.GetValueOrDefault();
                        if (roznica.TotalMinutes <= 1)
                        {

                            return Ok(new { message = "Too many request, try later" });
                        }
                        // Generate code
                        string code = "";
                        const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

                        Random random = new Random();
                        for (int i = 0; i < 10; i++)
                        {
                            code += validChars[random.Next(0, validChars.Length)];
                        }
                        // Save code and expiry time
                        user.EmailTwoFactorAuthenticationCode = code;
                        user.EmailTwoFactorAuthenticationExpiryTime = DateTime.Now.AddMinutes(5);
                        user.LastEmailTwoFactorAuthenticationCodeSent = DateTime.Now;

                        // Save data to database
                        dbContext.Update(user);
                        dbContext.SaveChanges();

                        //Send email
                        emailSender.SendTwoFactorAuthenticationCode(user.Email, user.EmailTwoFactorAuthenticationCode);
                        return Ok(new { message = "Email with your code have been sent" });
                    }

                    return Ok(new { message = "Verification is not enable" });
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

        // -------------------------------------------------------------
        [Authorize]
        [HttpGet("GetTwoFactorStatus")]
        public async Task<IActionResult> GetTwoFactorStatus()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

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
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null)
                {
                    string GoogleAuthKey = "";
                    string QrImageUrl = "";
                    const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                    Random random = new Random();
                    for (int i = 0; i < 10; i++)
                    {
                        GoogleAuthKey += validChars[random.Next(0, validChars.Length)];
                    }
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
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null)
                {
                    string requestBody;
                    using (var reader = new StreamReader(Request.Body))
                    {
                        requestBody = await reader.ReadToEndAsync();
                    }
                    dynamic data = JObject.Parse(requestBody);
                    string authKey = data.authKey;
                    string enteredAuthKey = data.enteredAuthKey;
                    TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
                    bool isValid = TwoFacAuth.ValidateTwoFactorPIN(authKey, enteredAuthKey, TimeSpan.FromSeconds(15));

                    if (isValid)
                    {
                        user.TwoFactorEnabled = true;
                        user.GoogleAuthKey = authKey;
                        await dbContext.SaveChangesAsync();

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
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null)
                {
                    string requestBody;
                    using (var reader = new StreamReader(Request.Body))
                    {
                        requestBody = await reader.ReadToEndAsync();
                    }
                    dynamic data = JObject.Parse(requestBody);
                    string enteredAuthKey = data.enteredAuthKey;
                    TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
                    bool isValid = TwoFacAuth.ValidateTwoFactorPIN(user.GoogleAuthKey, enteredAuthKey, TimeSpan.FromSeconds(15));

                    if (isValid)
                    {
                        user.TwoFactorEnabled = false;
                        user.GoogleAuthKey = null;
                        await dbContext.SaveChangesAsync();
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
    }
}