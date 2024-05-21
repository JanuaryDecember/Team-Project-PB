namespace expenses_tracker_pb.Server.Services.WalletService
{
    public interface IWalletService
    {
        Task<ICollection<Wallet>> GetWallets();
        Task DeleteWallet(string WalletId);
        Task UpdateWallet(string WalletId, string Name);
        Task AddWallet(WalletRequest request);
    }
}
