namespace expenses_tracker_pb.Server.Services.WalletService;

public class WalletAddRequest(string name, long iconId, decimal? accountBalance)
{
    public string Name { get; set; } = name;
    public long? IconId { get; set; } = iconId;
    public decimal? AccountBalance { get; set; } = accountBalance;
}

public class WalletUpdateRequest(long id, string? name, decimal? accountBalance, long? iconId)
{
    public long Id { get; set; } = id;
    public string? Name { get; set; } = name;
    public decimal? AccountBalance { get; set; } = accountBalance;
    public long? IconId { get; set; } = iconId;
}