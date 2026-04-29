using ApplePodcastTranscription.Models;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices.JavaScript;

namespace ApplePodcastTranscription.Services
{
    public class ApplePodcastDownloader
    {
        private const string ApplePodcastBaseUrl = "https://amp-api.podcasts.apple.com/v1/catalog/de/podcast-episodes/%podcastId%?include=channel%2Cpodcast&include%5Bpodcasts%5D=episodes%2Cpodcast-seasons%2Ctrailers&include%5Bpodcast-seasons%5D=episodes&fields=artistName%2Cartwork%2CassetUrl%2CcontentRating%2Cdescription%2CdurationInMilliseconds%2CepisodeNumber%2Cguid%2CisExplicit%2Ckind%2CmediaKind%2Cname%2Coffers%2CreleaseDateTime%2Cseason%2CseasonNumber%2CstoreUrl%2Csummary%2Ctitle%2Curl&with=entitlements%2ChlsVideo&l=%lang%";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private readonly string _appleBearerToken;

        public ApplePodcastDownloader(IHttpClientFactory httpClientFactory, ILogger logger, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _appleBearerToken = configuration.GetValue<string?>("AppleBearer") 
                ?? throw new ArgumentNullException("No value for env. variable AppleBearerToken");
        }

        public async Task DownloadPodcastEpisodeAsync(string podcastId, string language = "de-DE")
        {
            var assetUrl = await GetAssetUrl(podcastId, language);

            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("accept", "audio/webm,audio/ogg,audio/wav,audio/*;q=0.9,application/ogg;q=0.7,video/*;q=0.6,*/*;q=0.5");
            
            try
            {
                var response = await client.GetAsync(assetUrl);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsByteArrayAsync();
                if (!Directory.Exists(Constants.PathToDownloadedPodcasts)) 
                {
                    Directory.CreateDirectory(Constants.PathToDownloadedPodcasts);
                }
                var tempFilePath = $"{Constants.PathToDownloadedPodcasts}/{podcastId}.mp3";
                await File.WriteAllBytesAsync(tempFilePath, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading podcast episode with ID {PodcastId} from URL {AssetUrl}", podcastId, assetUrl);
                throw;
            }

        }
        private async Task<string> GetAssetUrl(string podcastId, string language = "de-DE") 
        {
            var url = ApplePodcastBaseUrl.Replace("%podcastId%", podcastId).Replace("%lang%", language);
            using var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.TryAddWithoutValidation("authorization", _appleBearerToken);
            client.DefaultRequestHeaders.TryAddWithoutValidation("origin", $"https://podcasts.apple.com");

            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                var json = JObject.Parse(content);
                var assetUrl = json["data"]?.FirstOrDefault()?["attributes"]?["assetUrl"]?.ToString() 
                    ?? throw new InvalidOperationException("Asset URL not found");

                return assetUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obtaining podcast download URL for episode with ID {PodcastId}", podcastId);
                throw;
            }
        }
    }
}
