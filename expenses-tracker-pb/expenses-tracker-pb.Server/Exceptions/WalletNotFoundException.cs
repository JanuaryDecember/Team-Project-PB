namespace expenses_tracker_pb.Server.Exceptions
{
    public class WalletNotFoundException : Exception
    {
        public WalletNotFoundException() : base("Wallet not found") { }
    }
}
