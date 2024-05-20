namespace expenses_tracker_pb.Server.Exceptions.Authentication
{
    public class GoogleAuthenticationException : Exception
    {
        public GoogleAuthenticationException() : base("Two-Factor Authentication") { }
    }
}