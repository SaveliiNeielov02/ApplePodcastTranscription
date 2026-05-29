using ApplePodcastTranscription.Interfaces;
using ApplePodcastTranscription.Services;
using ApplePodcastTranscription.Services.Database;
using ApplePodcastTranscription.Services.Session;
using ApplePodcastTranscription.Services.Transcript;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Net;
namespace ApplePodcastTranscription
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplePodcastDbContext>(options =>
                options.UseSqlite(connectionString));
            services.AddScoped<ISessionRepository, PodcastSessionRepository>();
            services.AddScoped<IPodcastRecordRepository, PodcastRecordRepository>();

            services.AddTransient<ITranscriber, WhisperSmallTranscriber>();
            services.AddTransient<IAudioResampler, WhisperSmallAudioResampler>();

            services.AddTransient<IApplePodcastDownloader, DirectApplePodcastDownloader>();
            services.AddTransient<IPodcastFileManager, LocalPodcastFileManager>();
            services.AddSingleton<ITranscriptQueue, LocalTranscriptQueue>();
            services.AddTransient<SessionWorker>();

            services.AddSignalR();

            services.AddHttpClient("ApplePodcast")
                .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.All
                });
            services.AddScoped(sp =>
            {
                var navigationManager = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
                return new HttpClient
                {
                    BaseAddress = new Uri(navigationManager.BaseUri)
                };
            });
            services.AddTransient<PodcastUrlParser>();
            return services;
        }

        public static IServiceCollection AddCustomSerilog(this IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(Log.Logger, dispose: true);
            });

            return services;
        }
    }
}
