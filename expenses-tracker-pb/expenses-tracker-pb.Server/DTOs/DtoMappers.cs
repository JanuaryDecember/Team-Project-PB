using expenses_tracker_pb.Server.Model;

namespace expenses_tracker_pb.Server.DTOs;

public static class DtoMappers
{
    public static WalletDto ToDto(this Wallet wallet)
    {
        return new WalletDto(
            wallet.Id, wallet.Name, wallet.IconId, wallet.AccountBalance, wallet.UserId);
    }

    public static TransactionDto ToDto(this Transaction transaction)
    {
        return new TransactionDto(
            transaction.Id, transaction.Title, transaction.Amount, transaction.Date, transaction.WalletId,
            transaction.CategoryId, transaction.Description
        );
    }
}