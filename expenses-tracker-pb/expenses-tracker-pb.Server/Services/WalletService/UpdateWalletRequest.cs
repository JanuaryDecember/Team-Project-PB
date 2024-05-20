namespace expenses_tracker_pb.Server.Services.WalletService
{
    public class UpdateWalletRequest(string name, double? accountBalance)
    {
        public string Name { get; set; } = name;
        public double? AccountBalance { get; set;} = accountBalance;
    }
}
