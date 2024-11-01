public class StaticFileService
{
    private readonly StaticFileConfiguration _config;

    public StaticFileService(StaticFileConfiguration config)
    {
        _config = config;
    }

    public async Task ServeFile(HttpContext context)
    {
        var path = context.Request.Path.Value?.Trim('/') ?? string.Empty;
        var rootPath = Path.Combine(Directory.GetCurrentDirectory(), _config.RootPath, path);
        string? fileToServe = null;

        if (Directory.Exists(rootPath))
        {
            foreach (var defaultFile in _config.DefaultFiles)
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