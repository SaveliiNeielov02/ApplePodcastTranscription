namespace ApplePodcastTranscription.Models.Exception
{
    public class TranscribingException : System.Exception
    {
        public TranscribingException(string message, System.Exception innerException) : base(message, innerException) { }
    }
}
