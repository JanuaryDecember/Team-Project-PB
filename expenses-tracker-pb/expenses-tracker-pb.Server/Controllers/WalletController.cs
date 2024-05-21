using expenses_tracker_pb.Server.Services.WalletService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;

namespace expenses_tracker_pb.Server.Controllers
{
    [ApiController]
    [Route("api/user/wallet")]
    public class WalletController(IWalletService walletService) : ControllerBase
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
                if (e is AuthenticationException || e is InvalidCredentialException)
                {
                    return BadRequest(e.Message);
                }
                return StatusCode(500, e.Message);
            }
        }

        [Authorize]
        [HttpGet("{walletId}")]
        public async Task<IActionResult> GetWallet(string WalletId)
        {
            try
            {
                var wallets = await walletService.GetWallets();
                return Ok(wallets);
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
        [HttpPost]
        public async Task<IActionResult> AddWallet([FromBody] WalletRequest request)
        {
            try
            {
                await walletService.AddWallet(request);
                return Ok("Wallet added succesfully");
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
        [HttpDelete("{WalletId}")]
        public async Task<IActionResult> DeleteWallet(string WalletId)
        {
            try
            {
                await walletService.DeleteWallet(WalletId);
                return Ok("Wallet removed successfully");
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
        [HttpPut("{WalletId}")]
        public async Task<IActionResult> UpdateWallet(string WalletId, [FromBody] string Name)
        {
            try
            {
                await walletService.UpdateWallet(WalletId, Name);
                return Ok("Wallet updated successfully");
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
    }
}
