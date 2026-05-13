using ApplePodcastTranscription.Interfaces;

namespace ApplePodcastTranscription.Services
{
    using System.IO;
    using System.Threading.Tasks;

    public class LocalPodcastFileManager : IPodcastFileManager
    {
        private readonly string _pathToDownloadedPodcasts =
            Path.Combine(Directory.GetCurrentDirectory(), "DownloadedPodcasts");

        public LocalPodcastFileManager()
        {
            if (!Directory.Exists(_pathToDownloadedPodcasts))
            {
                Directory.CreateDirectory(_pathToDownloadedPodcasts);
            }
        }
        private string GetFullPath(string storageKey) => Path.Combine(_pathToDownloadedPodcasts, storageKey);
        public Stream ReadAsFileStream(string storageKey)
        {
            var filePath = GetFullPath(storageKey);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file '{filePath}' does not exist.", filePath);
            }

            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true);
        }

        public async Task WriteStreamAsync(string storageKey, Stream audioStream)
        {
            var filePath = GetFullPath(storageKey);
            try
            {
                await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None,
                    8192, true);
                await audioStream.CopyToAsync(fileStream);
            }
            catch (Exception ex)
            {
                throw new IOException($"An error occurred while writing to the file '{filePath}'.", ex);
            }
        }

        public void DeleteFile(string storageKey)
        {
            var filePath = GetFullPath(storageKey);
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"An error occurred while deleting the file '{filePath}'.", ex);
            }
        }
    }
}
