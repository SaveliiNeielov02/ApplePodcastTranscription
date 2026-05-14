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
        private readonly string _appleBearerToken; // Need to be added as environment variable AppleBearerToken, can be obtained from Apple Podcasts web app network requests (must be periodically updated)

        public DirectApplePodcastDownloader(IHttpClientFactory httpClientFactory, IConfiguration configuration, IPodcastFileManager podcastFileIo)
        {
            _httpClientFactory = httpClientFactory;
            _podcastFileIo = podcastFileIo;
            _appleBearerToken = configuration.GetValue<string?>("AppleBearer") 
                ?? throw new ArgumentNullException("No value for env. variable AppleBearerToken");
        }

        public async Task<AudioPodcastDto> DownloadPodcastEpisodeAsync(string podcastId, string fileName, CancellationToken downloadCt)
        {
            var podcastData = await GetPodcastData(podcastId);

            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("accept", $"audio/{AudioFormat};q=0.9,application/ogg;q=0.7,video/*;q=0.6,*/*;q=0.5");

            try
            {
                var response = await client.GetAsync(podcastData.AssetUrl, HttpCompletionOption.ResponseHeadersRead, downloadCt);
                response.EnsureSuccessStatusCode();
                var downloadedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                await using var contentStream = await response.Content.ReadAsStreamAsync(downloadCt);

                var fullName = $"{fileName}.{AudioFormat}";
                await _podcastFileIo.WriteStreamAsync(fullName, contentStream);

                return new AudioPodcastDto
                {
                    ExternalId = podcastData.ExternalId,
                    Title = podcastData.Title,
                    ArtistName = podcastData.ArtistName,
                    IconUrl = podcastData.IconUrl,
                    DownloadedAtInUnixTimeSeconds = downloadedTime,
                    StorageKey = fullName
                };
            }
            catch (IOException ex) 
            {
                throw new DownloadException("An error occurred while saving the podcast episode locally", ex);
            }
            catch (OperationCanceledException ex)
            {
                throw new DownloadTimeoutException("Download timed out", ex);
            }
            catch (Exception ex)
            {
                throw new DownloadException("An error occurred while downloading the podcast episode", ex);
            }

        }
        private async Task<AssetPodcastDto> GetPodcastData(string podcastId) 
        {
            var url = ApplePodcastBaseUrl.Replace("%podcastId%", podcastId);
            using var client = _httpClientFactory.CreateClient();

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
