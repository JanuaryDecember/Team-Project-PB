using expenses_tracker_pb.Server.Exceptions;
using expenses_tracker_pb.Server.Services.UserService;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;

namespace expenses_tracker_pb.Server.Services.WalletService
{
    public class WalletService(ETDbContext dbContext, IUserService userService) : IWalletService
    {
        public async Task<ICollection<Wallet>> GetWallets()
        {
            var user = await userService.GetCurrentUser();
            var wallets = await dbContext.Wallets.Where(w => w.UserId == user.Id).ToListAsync();
            return wallets;
        }

        public async Task<Wallet> getWallet(string WalletId)
        {
            var user = await userService.GetCurrentUser();
            var wallet = await dbContext.Wallets.Where(w => w.UserId.Equals(user.Id) && w.Id.Equals(WalletId)).FirstAsync();
            return wallet;
        }

        public async Task AddWallet(UpdateWalletRequest request)
        {
            var user = await userService.GetCurrentUser();
            
            Wallet wallet = new()
            {
                User = user,
                UserId = user.Id,
                Name = request.Name,
                IconId = 1,
                AccountBalance = request.AccountBalance != null ? (double)request.AccountBalance : 0,
                Incomes = [],
                Expenditures = []
            };
            dbContext.Wallets.Add(wallet);

            await dbContext.SaveChangesAsync();

        }

        public async Task DeleteWallet(string WalletId)
        {
            var user = await userService.GetCurrentUser();

            var wallet = dbContext.Wallets.Where(w => w.Id.Equals(WalletId) && w.UserId.Equals(user.Id)).First() ?? throw new WalletNotFoundException();
            
            dbContext.Wallets.Remove(wallet);

            await dbContext.SaveChangesAsync();
        }

        public Task UpdateWallet(string WalletId, string Name)
        {
            throw new NotImplementedException();
        }
    }
}
