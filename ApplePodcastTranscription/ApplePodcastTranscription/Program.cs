using ApplePodcastTranscription;
using ApplePodcastTranscription.Client.Pages;
using ApplePodcastTranscription.Components;
using ApplePodcastTranscription.Services.Database;
using ApplePodcastTranscription.Services.Hub;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddCustomSerilog()
    .AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddControllers();
builder.Configuration.AddEnvironmentVariables();

builder.Host.UseDefaultServiceProvider((_, options) =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});


var app = builder.Build();

// Make sure the database is created and the directory for the SQLite database exists
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplePodcastDbContext>();

    if (dbContext.Database.IsSqlite())
    {
        var connectionString = dbContext.Database.GetConnectionString();
        var builderDb = new SqliteConnectionStringBuilder(connectionString);
        var dir = Path.GetDirectoryName(builderDb.DataSource);

        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    dbContext.Database.Migrate();
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapControllers();

app.MapHub<SessionHub>("/session-hub");

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ApplePodcastTranscription.Client._Imports).Assembly);

app.Run();
