namespace expenses_tracker_pb.Server.Exceptions.Authentication
{
    public class QuestionAuthenticationException : Exception
    {
        public QuestionAuthenticationException(string question) : base(question) { }
    }
}