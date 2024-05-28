using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using expenses_tracker_api.Model;

namespace expenses_tracker_pb.Server.Model;

public class Wallet
{
    public long Id { get; set; }
    [MaxLength(255)] public string Name { get; set; } = string.Empty;
    public long IconId { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal AccountBalance { get; set; }
    [MaxLength(255)] public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = new User();
    public ICollection<Transaction> Transactions { get; set; } = [];
    public ICollection<Obligation> Obligations { get; set; } = [];
    public ICollection<Budget> Budgets { get; set; } = [];
}