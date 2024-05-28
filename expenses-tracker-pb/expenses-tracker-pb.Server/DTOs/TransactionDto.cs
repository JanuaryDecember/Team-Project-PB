namespace expenses_tracker_pb.Server.DTOs;

public record TransactionDto(
    long Id,
    string Title,
    decimal Amount,
    DateTime Date,
    long WalletId,
    long CategoryId,
    string? Description);