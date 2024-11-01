using Microsoft.Extensions.FileProviders;

public class StaticFileService
{
    public StaticFileConfiguration Config { get; private set; }

    public StaticFileService(IConfiguration configuration)
    {
        Config = configuration.GetSection("StaticFiles").Get<StaticFileConfiguration>() ?? new StaticFileConfiguration();

        if (Config == null || string.IsNullOrEmpty(Config.RootPath))
        {
            throw new InvalidOperationException("Server is not configured properly! Please check the configuration.");
        }
    }

    public IFileProvider GetFileProvider()
    {
        return new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), Config.RootPath))
        {
            UsePollingFileWatcher = true
        };
    }

    public async Task ServeFile(HttpContext context)
    {
        var path = context.Request.Path.Value?.Trim('/') ?? string.Empty;
        var rootPath = Path.Combine(Directory.GetCurrentDirectory(), Config.RootPath, path);
        string? fileToServe = null;

        if (Directory.Exists(rootPath))
        {
            foreach (var defaultFile in Config.DefaultFiles)
            {
                var defaultFilePath = Path.Combine(rootPath, defaultFile);
                if (File.Exists(defaultFilePath))
                {
                    fileToServe = defaultFilePath;
                    break;
                }
            }

            if (fileToServe == null)
            {
                var firstHtmlFile = Directory.EnumerateFiles(rootPath, "*.html").FirstOrDefault();
                if (firstHtmlFile != null)
                {
                    fileToServe = firstHtmlFile;
                }
            }
        }
        else if (File.Exists(rootPath))
        {
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
    }
}