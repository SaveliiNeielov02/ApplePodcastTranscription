namespace ApplePodcastTranscription.Interfaces
{
    public interface IPodcastFileManager
    {
        public Task WriteStreamAsync(string storageKey, Stream audioStream);
        public Stream ReadAsFileStream(string storageKey);
        public void DeleteFile(string storageKey);
    }
}
