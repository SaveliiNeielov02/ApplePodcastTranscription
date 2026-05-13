using ApplePodcastTranscription.Interfaces;

namespace ApplePodcastTranscription.Services
{
    using System.IO;
    using System.Threading.Tasks;

    public class LocalPodcastFileIo : IPodcastFileIo
    {
        private readonly string _pathToDownloadedPodcasts = Path.Combine(Directory.GetCurrentDirectory(), "DownloadedPodcasts");

        public LocalPodcastFileIo()
        {
            if (!Directory.Exists(_pathToDownloadedPodcasts))
            {
                Directory.CreateDirectory(_pathToDownloadedPodcasts);
            }
        }

        public Stream ReadAsFileStream(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file '{filePath}' does not exist.", filePath);
            }

            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true);
        }

        public async Task WriteStreamAsync(string filePath, Stream audioStream)
        {
            try
            {
                await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
                await audioStream.CopyToAsync(fileStream);
            }
            catch (Exception ex)
            {
                throw new IOException($"An error occurred while writing to the file '{filePath}'.", ex);
            }
        }
        public string GetFilePath(string fileName, string audioFormat)
        {
            return Path.Combine(_pathToDownloadedPodcasts, $"{fileName}.{audioFormat}");
        }
    }
}
