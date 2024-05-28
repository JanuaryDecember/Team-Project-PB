using expenses_tracker_pb.Server.DTOs;

namespace expenses_tracker_pb.Server.Services.WalletService
{
    public interface IWalletService
    {
        Task<ICollection<WalletDto>> GetWallets();
        Task<WalletDto> GetWallet(long walletId);
        Task DeleteWallet(long walletId);
        Task UpdateWallet(WalletUpdateRequest request);
        Task AddWallet(WalletAddRequest request);
    }
}