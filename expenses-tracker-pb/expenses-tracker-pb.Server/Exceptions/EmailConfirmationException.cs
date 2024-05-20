namespace expenses_tracker_pb.Server.Exceptions
{
    public class EmailConfirmationException : Exception
    {
        public EmailConfirmationException() : base("Email code verification needed") { }
    }
}
