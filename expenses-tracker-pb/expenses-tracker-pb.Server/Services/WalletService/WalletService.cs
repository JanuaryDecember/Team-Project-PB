using expenses_tracker_pb.Server.DTOs;
using expenses_tracker_pb.Server.Exceptions;
using expenses_tracker_pb.Server.Model;
using expenses_tracker_pb.Server.Services.UserService;
using Microsoft.EntityFrameworkCore;

namespace expenses_tracker_pb.Server.Services.WalletService
{
    public class WalletService(EtDbContext dbContext, IUserService userService) : IWalletService
    {
        public async Task<ICollection<WalletDto>> GetWallets()
        {
            var user = await userService.GetCurrentUser();
            var wallets = await dbContext.Wallets.Where(w => w.UserId == user.Id).Select(w => w.ToDto()).ToListAsync();
            return wallets;
        }

        public async Task<WalletDto> GetWallet(long walletId)
        {
            var user = await userService.GetCurrentUser();
            var wallet = await dbContext.Wallets.Where(w => w.Id.Equals(walletId) && w.UserId.Equals(user.Id))
                .Select(w => w.ToDto()).FirstAsync();
            return wallet;
        }

        public async Task AddWallet(WalletAddRequest request)
        {
            var user = await userService.GetCurrentUser();

            Wallet wallet = new()
            {
                User = user,
                UserId = user.Id,
                Name = request.Name,
                IconId = request.IconId ?? 1,
                AccountBalance = request.AccountBalance ?? 0,
                Transactions = []
            };
            dbContext.Wallets.Add(wallet);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteWallet(long walletId)
        {
            var user = await userService.GetCurrentUser();

            var wallet =
                await dbContext.Wallets.Where(w => w.UserId.Equals(user.Id) && w.Id.Equals(walletId)).FirstAsync() ??
                throw new WalletNotFoundException();

            dbContext.Wallets.Remove(wallet);

            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateWallet(WalletUpdateRequest request)
        {
            var user = await userService.GetCurrentUser();
            var wallet =
                await dbContext.Wallets.Where(w => w.UserId.Equals(user.Id) && w.Id.Equals(request.Id)).FirstAsync() ??
                throw new WalletNotFoundException();
            wallet.Name = request.Name ?? wallet.Name;
            wallet.AccountBalance = request.AccountBalance ?? wallet.AccountBalance;
            dbContext.Wallets.Update(wallet);
            await dbContext.SaveChangesAsync();
        }
    }
}