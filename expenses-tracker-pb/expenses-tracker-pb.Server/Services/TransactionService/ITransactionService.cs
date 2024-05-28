using expenses_tracker_pb.Server.DTOs;

namespace expenses_tracker_pb.Server.Services.TransactionService;

public interface ITransactionService
{
    public Task AddTransaction(TransactionAddRequest request);
    public Task<List<TransactionDto>> GetTransactions();

    public Task<List<TransactionDto>> GetTransactions(long walletId, DateTime? startDate, DateTime? endDate,
        long? selectedCategory, double? minValue, double? maxValue, string? containsString, bool? caseSensitive);

    public Task<TransactionDto> GetTransaction(long transactionId);
    public Task UpdateTransaction(TransactionUpdateRequest request);
    public Task DeleteTransaction(long transactionId);
}