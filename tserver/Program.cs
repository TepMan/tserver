using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Read the static files root path from configuration
var staticFilesRootPath = builder.Configuration.GetValue<string>("StaticFiles:RootPath");

// Use the configured static files root path
if (!string.IsNullOrEmpty(staticFilesRootPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), staticFilesRootPath)),
        RequestPath = ""
    });
}
else
{
    // Handle the case where staticFilesRootPath is null or empty
    throw new InvalidOperationException("Static files root path is not configured.");
}

app.MapGet("/", async context => 
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync(Path.Combine(staticFilesRootPath, "index.html"));
});

app.Run();
