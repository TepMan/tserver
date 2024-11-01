using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Read the static files configuration
var staticFileConfig = builder.Configuration.GetSection("StaticFiles").Get<StaticFileConfiguration>();

// Use the configured static files root path
if (staticFileConfig != null && !string.IsNullOrEmpty(staticFileConfig.RootPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), staticFileConfig.RootPath)),
        RequestPath = ""
    });
}
else
{
    // Handle the case where staticFilesRootPath is null or empty
    throw new InvalidOperationException("Static files root path is not configured.");
}

var staticFileService = new StaticFileService(staticFileConfig);

app.MapGet("/{**path}", async context =>
{
    await staticFileService.ServeFile(context);
});

app.Run();
