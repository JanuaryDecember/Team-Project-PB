using System.Security.Authentication;
using expenses_tracker_pb.Server.Services.WalletService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace expenses_tracker_pb.Server.Controllers;

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
            return e is AuthenticationException or InvalidCredentialException
                ? BadRequest(e.Message)
                : StatusCode(500, e.Message);
        }
    }

    [Authorize]
    [HttpGet("{walletId:long}")]
    public async Task<IActionResult> GetWallet(long walletId)
    {
        try
        {
            var wallet = await walletService.GetWallet(walletId);
            return Ok(wallet);
        }
        catch (Exception e)
        {
            return e is AuthenticationException or InvalidCredentialException
                ? BadRequest(e.Message)
                : StatusCode(500, e.Message);
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddWallet([FromBody] WalletAddRequest request)
    {
        try
        {
            await walletService.AddWallet(request);
            return Ok("Wallet added successfully");
        }
        catch (Exception e)
        {
            return e is AuthenticationException or InvalidCredentialException
                ? BadRequest(e.Message)
                : StatusCode(500, e.Message);
        }
    }

    [Authorize]
    [HttpDelete("{walletId:long}")]
    public async Task<IActionResult> DeleteWallet(long walletId)
    {
        try
        {
            await walletService.DeleteWallet(walletId);
            return Ok("Wallet removed successfully");
        }
        catch (Exception e)
        {
            return e is AuthenticationException or InvalidCredentialException
                ? BadRequest(e.Message)
                : StatusCode(500, e.Message);
        }
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> UpdateWallet([FromBody] WalletUpdateRequest request)
    {
        try
        {
            await walletService.UpdateWallet(request);
            return Ok("Wallet updated successfully");
        }
        catch (Exception e)
        {
            return e is AuthenticationException or InvalidCredentialException
                ? BadRequest(e.Message)
                : StatusCode(500, e.Message);
        }
    }
}