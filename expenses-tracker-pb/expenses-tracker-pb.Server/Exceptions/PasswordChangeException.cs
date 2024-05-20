namespace expenses_tracker_pb.Server.Exceptions
{
    public class PasswordChangeException : Exception
    {
        public PasswordChangeException() : base("Unable to change password") { }
    }
}
