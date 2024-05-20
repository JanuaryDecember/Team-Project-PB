using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using expenses_tracker_pb.Server.Services.UserService;
using System.Security.Authentication;
using expenses_tracker_pb.Server.Exceptions.Authentication;

namespace expenses_tracker_pb.Server.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpPost("isUserLoggedIn")]
        public IActionResult IsUserLogged()
        {
            try
            {
                return userService.IsUserLogged() ? Ok("User is logged in") : Unauthorized("User is not logged in");
            }
            catch (Exception e)
            {
                if (e is AuthenticationException || e is InvalidCredentialException)
                {
                    return BadRequest(e.Message);
                }
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("changePasswordWith2Fa")]
        public async Task<ActionResult> ChangePasswordWith2Fa([FromBody] PasswordChangeRequest request)
        {
            try
            {
                await userService.ChangePasswordWith2Fa(request);
                return Ok("Password changed successfully!");
            }
            catch (Exception e)
            {
                if (e is AuthenticationException || e is InvalidCredentialException)
                {
                    return BadRequest(e.Message);
                }
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("changePasswordWithEmail")]
        public async Task<ActionResult> ChangePasswordWithEmail([FromBody] PasswordChangeRequest request)
        {
            try
            {
                await userService.ChangePasswordWithEmail(request);
                return Ok("Password changed successfully!");
            }
            catch (Exception e)
            {
                if (e is EmailAuthenticationException)
                    return StatusCode(202, e.Message);
                if (e is AuthenticationException || e is InvalidCredentialException)
                {
                    return BadRequest(e.Message);
                }
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("profilePageData")]
        [Authorize]
        public async Task<IActionResult> GetProfilePageData()
        {
            try
            {
                var data = await userService.GetProfilePageData();
                return Ok(data);
            }
            catch (Exception e)
            {
                if (e is AuthenticationException || e is InvalidCredentialException)
                {
                    return BadRequest(e.Message);
                }
                return StatusCode(500, e.Message);
            }
        }

        [Authorize]
        [HttpPut("profilePageData")]
        public async Task<IActionResult> UpdateProfilePageData([FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await userService.UpdateProfilePageData(request);
                return Ok(new { user });
            }
            catch(Exception e)
            {
                if (e is AuthenticationException || e is InvalidCredentialException)
                {
                    return BadRequest(e.Message);
                }
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("profilePicture")]
        public async Task<IActionResult> ChangeProfilePicture(IFormFile file)
        {
            try
            {
                await userService.ChangeProfilePicture(file);
                return Ok("Profile picture changed successfully");
            }
            catch(Exception e)
            {
                if (e is AuthenticationException || e is InvalidCredentialException)
                {
                    return BadRequest(e.Message);
                }
                return StatusCode(500, e.Message);
            }
        }
    }
}