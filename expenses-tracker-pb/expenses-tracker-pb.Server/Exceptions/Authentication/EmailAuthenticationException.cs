namespace expenses_tracker_pb.Server.Exceptions.Authentication
{
    public class EmailAuthenticationException : Exception
    {
        public EmailAuthenticationException() : base("Email Authentication") { }
    }
}