using ApplePodcastTranscription;
using ApplePodcastTranscription.Client.Pages;
using ApplePodcastTranscription.Components;

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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ApplePodcastTranscription.Client._Imports).Assembly);

app.Run();
