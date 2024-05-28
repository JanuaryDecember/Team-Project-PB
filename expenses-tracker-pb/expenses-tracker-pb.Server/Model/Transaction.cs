using System.ComponentModel.DataAnnotations.Schema;
using expenses_tracker_pb.Server.Enums;

namespace expenses_tracker_pb.Server.Model;

public class Transaction
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    [ForeignKey("WalletId")] public long WalletId { get; set; }
    public Wallet? Wallet { get; set; }
    [ForeignKey("CategoryId")] public long CategoryId { get; set; }
    public Category? Category { get; set; }
    public TransactionTypeEnum Type { get; set; }
}