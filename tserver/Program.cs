using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Read the static files root path from configuration
var staticFilesRootPath = builder.Configuration.GetValue<string>("StaticFiles:RootPath");

// Read the default files list from configuration
var defaultFiles = builder.Configuration.GetSection("StaticFiles:DefaultFiles").Get<List<string>>();

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

app.MapGet("/{**path}", async context =>
{
    var path = context.Request.Path.Value?.Trim('/') ?? string.Empty;
    var rootPath = Path.Combine(Directory.GetCurrentDirectory(), staticFilesRootPath, path);
    string? fileToServe = null;


    if (Directory.Exists(rootPath))
    {
        if (defaultFiles != null && defaultFiles.Count > 0)
        {
            // Check if any of the default files exist in the directory
            foreach (var defaultFile in defaultFiles)
            {
                var defaultFilePath = Path.Combine(rootPath, defaultFile);
                if (File.Exists(defaultFilePath))
                {
                    fileToServe = defaultFilePath;
                    break;
                }
            }
        }

        // If no index.html file is found, serve the first .html file in the root path
        if (fileToServe == null)
        {
            var firstHtmlFile = Directory.EnumerateFiles(rootPath, "*.html").FirstOrDefault();
            if (firstHtmlFile != null)
            {
                fileToServe = firstHtmlFile;
            }
        }
    }
    else if (File.Exists(rootPath)) // <-- Added this block to handle file requests
    {
        // If the path is a file, serve it directly
        fileToServe = rootPath;
    }

    if (fileToServe != null)
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(fileToServe);
    }
    else
    {
        context.Response.StatusCode = 204; // No Content
    }
});

app.Run();
