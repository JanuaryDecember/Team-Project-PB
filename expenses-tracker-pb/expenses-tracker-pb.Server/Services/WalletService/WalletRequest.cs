namespace expenses_tracker_pb.Server.Services.WalletService
{
    public class WalletRequest(string name, double? accountBalance)
    {
        public string Name { get; set; } = name;
        public double? AccountBalance { get; set; } = accountBalance;
    }
}
