using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Models.Exception;
using ApplePodcastTranscription.Models.PodcastDto;
using Newtonsoft.Json.Linq;

namespace ApplePodcastTranscription.Services.Session
{
    // This class is responsible for downloading podcast episodes directly from Apple Podcasts using their web API.
    // There are other sources to download apple podcasts, but this implementation is more reliable and faster than using third-party services.
    public class DirectApplePodcastDownloader : IApplePodcastDownloader
    {
        private const string ApplePodcastBaseUrl = "https://amp-api.podcasts.apple.com/v1/catalog/de/podcast-episodes/%podcastId%?include=channel%2Cpodcast&include%5Bpodcasts%5D=episodes%2Cpodcast-seasons%2Ctrailers&include%5Bpodcast-seasons%5D=episodes&fields=artistName%2Cartwork%2CassetUrl%2CcontentRating%2Cdescription%2CdurationInMilliseconds%2CepisodeNumber%2Cguid%2CisExplicit%2Ckind%2CmediaKind%2Cname%2Coffers%2CreleaseDateTime%2Cseason%2CseasonNumber%2CstoreUrl%2Csummary%2Ctitle%2Curl&with=entitlements%2ChlsVideo&l=de-DE";
        private const string AudioFormat = "wave";
        
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPodcastFileManager _podcastFileIo;
        private readonly ILogger<DirectApplePodcastDownloader> _logger;
        private readonly string _appleBearerToken; // Need to be added as environment variable AppleBearerToken, can be obtained from Apple Podcasts web app network requests (must be periodically updated)

        public DirectApplePodcastDownloader(IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<DirectApplePodcastDownloader> logger,
            IPodcastFileManager podcastFileIo)
        {
            _httpClientFactory = httpClientFactory;
            _podcastFileIo = podcastFileIo;
            _logger = logger;
            _appleBearerToken = configuration.GetValue<string?>("AppleBearer") 
                ?? throw new ArgumentNullException("No value for env. variable AppleBearer");
        }

        public async Task<AudioPodcastDto> DownloadPodcastEpisodeAsync(AssetPodcastDto podcastData, string fileName, CancellationToken downloadCt)
        {
            using var client = _httpClientFactory.CreateClient("ApplePodcast");
            client.DefaultRequestHeaders.TryAddWithoutValidation("accept", $"audio/{AudioFormat};q=0.9,application/ogg;q=0.7,video/*;q=0.6,*/*;q=0.5");

            var fullFileName = $"{fileName}.{AudioFormat}";
            try
            {
                var response = await client.GetAsync(podcastData.AssetUrl, HttpCompletionOption.ResponseHeadersRead, downloadCt);
                response.EnsureSuccessStatusCode();
                var downloadedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                await using var contentStream = await response.Content.ReadAsStreamAsync(downloadCt);

                await _podcastFileIo.WriteStreamAsync(fullFileName, contentStream);

                return new AudioPodcastDto
                {
                    ExternalId = podcastData.ExternalId,
                    Title = podcastData.Title,
                    ArtistName = podcastData.ArtistName,
                    IconUrl = podcastData.IconUrl,
                    DownloadedAtInUnixTimeSeconds = downloadedTime,
                    StorageKey = fullFileName
                };
            }
            catch (IOException ex) 
            {
                DeleteFileAfterUnsuccessfulDownload(fullFileName);
                throw new DownloadException("An error occurred while saving the podcast episode locally", ex);
            }
            catch (OperationCanceledException ex)
            {
                DeleteFileAfterUnsuccessfulDownload(fullFileName);
                throw new DownloadTimeoutException("Download timed out", ex);
            }
            catch (Exception ex)
            {
                DeleteFileAfterUnsuccessfulDownload(fullFileName);
                throw new DownloadException("An error occurred while downloading the podcast episode", ex);
            }

        }
        private void DeleteFileAfterUnsuccessfulDownload(string fileName)
        {
            try
            {
                _podcastFileIo.DeleteFile(fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the file after an unsuccessful download.");
            }
        }
        public async Task<AssetPodcastDto> GetPodcastData(string podcastId) 
        {
            var url = ApplePodcastBaseUrl.Replace("%podcastId%", podcastId);
            using var client = _httpClientFactory.CreateClient("ApplePodcast");

            client.DefaultRequestHeaders.TryAddWithoutValidation("authorization", _appleBearerToken);
            client.DefaultRequestHeaders.TryAddWithoutValidation("origin", $"https://podcasts.apple.com");

            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                var json = JObject.Parse(content);
                var podcastData = GetPodcastData(json);

                return podcastData;
            }
            catch (Exception ex)
            {
                throw new DownloadException("An error occurred while fetching podcast data", ex);
            }
        }
        private AssetPodcastDto GetPodcastData(JObject podcastJson) 
        {
            // Can be extended
            return new AssetPodcastDto
            {
                ExternalId = podcastJson.SelectToken("data[0].id")?.ToString() ?? throw new InvalidOperationException("External ID not found"),
                Title = podcastJson.SelectToken("data[0].attributes.name")?.ToString(),
                ArtistName = podcastJson.SelectToken("data[0].attributes.artistName")?.ToString(),
                IconUrl = podcastJson.SelectToken("data[0].attributes.artwork.url")?.ToString(),
                AssetUrl = podcastJson.SelectToken("data[0].attributes.assetUrl")?.ToString() ?? throw new InvalidOperationException("Asset URL not found"),
            };
        }
    }
}
