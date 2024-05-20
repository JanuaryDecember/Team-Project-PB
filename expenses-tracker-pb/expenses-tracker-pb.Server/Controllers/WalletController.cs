using expenses_tracker_pb.Server.Services.UserService;
using expenses_tracker_pb.Server.Services.WalletService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;
using System.Security.Claims;

namespace expenses_tracker_pb.Server.Controllers
{
    [ApiController]
    [Route("api/user/wallet")]
    public class WalletController(UserManager<User> userManager, ETDbContext dbContext, EmailSender emailSender, IWalletService walletService) : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetWallets()
        {
            try
            {
                var wallets = await walletService.GetWallets();
                return Ok(wallets);
            }
            catch (Exception e)
            {
                if(e is AuthenticationException || e is InvalidCredentialException)
                {
                    return BadRequest(e.Message);
                }
                return StatusCode(500, e.Message);
            }
        }

        [Authorize]
        [HttpGet("/{walletId}")]
        public async Task<IActionResult> GetWallet(string WalletId)
        {
           
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddWallet([FromBody] UpdateWalletRequest request)
        {
            try
            {
                await walletService.AddWallet(request);
                return Ok("Added wallet succesfully");
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
        [HttpDelete("removeWallet/{walletId}")]
        public async Task<IActionResult> DeleteWallet(string walletId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Session ended! Sign in again");
            }


            var user = await dbContext.Users
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

            await dbContext.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpPut("updateWallet/{walletId}")]
        public async Task<IActionResult> UpdateWallet(string walletId, [FromBody] string name)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("Session ended! Sign in again");
            }


            var user = await dbContext.Users
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

            await dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
