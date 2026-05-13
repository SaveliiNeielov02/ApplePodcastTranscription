namespace ApplePodcastTranscription.Interfaces
{
    public interface IPodcastFileIo
    {
        public Task WriteStreamAsync(string fileName, Stream audioStream);
        public Stream ReadAsFileStream(string fileName);
        public string GetFilePath(string fileName, string audioFormat);
    }
}
