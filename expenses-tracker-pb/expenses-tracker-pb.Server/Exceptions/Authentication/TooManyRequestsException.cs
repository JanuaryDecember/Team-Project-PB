namespace expenses_tracker_pb.Server.Exceptions.Authentication
{
    public class TooManyRequestsException : Exception
    {
        public TooManyRequestsException() : base("Too many request, try later") { }
    }
}