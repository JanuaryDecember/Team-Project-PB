using expenses_tracker_pb.Server.Enums;

namespace expenses_tracker_pb.Server.Services.TransactionService;

public class TransactionUpdateRequest(
    long id,
    string title,
    decimal amount,
    DateTime date,
    long walletId,
    long categoryId,
    string? description)
{
    public long Id { get; set; } = id;
    public string? Title { get; set; } = title;
    public string? Description { get; set; } = description;
    public decimal? Amount { get; set; } = amount;
    public DateTime? Date { get; set; } = date;
    public long? WalletId { get; set; } = walletId;
    public long? CategoryId { get; set; } = categoryId;
}

public class TransactionAddRequest(
    string title,
    decimal amount,
    DateTime date,
    long walletId,
    long categoryId,
    TransactionTypeEnum type,
    string? description)
{
    public string Title { get; set; } = title;
    public string? Description { get; set; } = description;
    public decimal Amount { get; set; } = amount;
    public DateTime Date { get; set; } = date;
    public long WalletId { get; set; } = walletId;
    public long CategoryId { get; set; } = categoryId;
    public TransactionTypeEnum Type { get; set; } = type;
}