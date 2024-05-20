namespace expenses_tracker_pb.Server.Exceptions
{
    public class FileSizeException : Exception
    {
        public FileSizeException() : base("File is too big") { }
        public FileSizeException(string message) : base(message) { }
    }
}
