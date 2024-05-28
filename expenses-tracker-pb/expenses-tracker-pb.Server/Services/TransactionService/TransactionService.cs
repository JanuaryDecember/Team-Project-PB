using expenses_tracker_pb.Server.DTOs;
using expenses_tracker_pb.Server.Exceptions;
using expenses_tracker_pb.Server.Model;
using expenses_tracker_pb.Server.Services.UserService;
using Microsoft.EntityFrameworkCore;

namespace expenses_tracker_pb.Server.Services.TransactionService;

public class TransactionService(IUserService userService, EtDbContext etDbContext) : ITransactionService
{
    public async Task AddTransaction(TransactionAddRequest request)
    {
        var userId = userService.GetCurrentUser().Id;
        var wallet =
            await etDbContext.Wallets.Where(w => w.UserId.Equals(userId) && request.WalletId.Equals(request.WalletId))
                .FirstOrDefaultAsync();
        if (wallet != null)
            await etDbContext.Transactions.AddAsync(new Transaction()
            {
                WalletId = request.WalletId,
                Amount = request.Amount,
                Category = await etDbContext.Categories
                    .Where(c => (c.UserId == null || c.UserId.Equals(userId)) && c.Id.Equals(request.CategoryId))
                    .FirstOrDefaultAsync(),
                CategoryId = request.CategoryId,
                Date = request.Date,
                Description = request.Description,
                Title = request.Title,
                Type = request.Type,
                Wallet = wallet
            });
        else throw new WalletNotFoundException();
    }

    public Task<List<TransactionDto>> GetTransactions()
    {
        var userId = userService.GetCurrentUser().Id;
        var transactions = etDbContext.Transactions.Where(t => t.Wallet != null && t.Wallet.UserId.Equals(userId))
            .Select(t => t.ToDto())
            .ToListAsync();
        return transactions;
    }

    public Task<List<TransactionDto>> GetTransactions(long walletId, DateTime? startDate, DateTime? endDate,
        long? selectedCategory, decimal? minValue,
        decimal? maxValue, string? containsString, bool? caseSensitive)
    {
        var userId = userService.GetCurrentUser().Id;
        var transactions = etDbContext.Transactions.Where(t =>
                (t.Wallet != null && t.Wallet.UserId.Equals(userId) && t.WalletId.Equals(walletId)) &&
                (startDate == null || t.Date > startDate) && (endDate == null || t.Date < endDate) &&
                (selectedCategory == null || t.CategoryId.Equals(selectedCategory)) &&
                (minValue == null || t.Amount > minValue) && (maxValue == null || t.Amount < maxValue) &&
                (containsString == null || (caseSensitive == null
                    ? t.Title.Contains(containsString)
                    : t.Title.ToLower().Contains(containsString.ToLower()))))
            .Select(t => t.ToDto())
            .ToListAsync();
        return transactions;
    }

    public Task<TransactionDto> GetTransaction(long transactionId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateTransaction(TransactionUpdateRequest request)
    {
        throw new NotImplementedException();
    }

    public Task DeleteTransaction(long transactionId)
    {
        throw new NotImplementedException();
    }
}