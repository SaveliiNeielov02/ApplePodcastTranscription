namespace ApplePodcastTranscription.Models.Exception
{
    public class DownloadException : System.Exception 
    {
        public DownloadException(string message, System.Exception innerException) : base(message, innerException) { }
    }
    public class DownloadTimeoutException : System.Exception
    {
        public DownloadTimeoutException(string message, System.Exception innerException) : base(message, innerException) { }
    }
}
