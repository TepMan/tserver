using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

public class StaticFileServiceTests
{
    private readonly StaticFileService _staticFileService;
    private readonly string _testRootPath;

    public StaticFileServiceTests()
    {
        _testRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        var inMemorySettings = new Dictionary<string, string?>
        {
            {"StaticFiles:RootPath", _testRootPath},
            {"StaticFiles:DefaultFiles:0", "index.html"},
            {"StaticFiles:DefaultFiles:1", "default.html"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _staticFileService = new StaticFileService(configuration);

        SetupTestFiles();
    }

    private void SetupTestFiles()
    {
        if (!Directory.Exists(_testRootPath))
        {
            Directory.CreateDirectory(_testRootPath);
        }

        File.WriteAllText(Path.Combine(_testRootPath, "index.html"), "<html><body>Index</body></html>");
        File.WriteAllText(Path.Combine(_testRootPath, "default.html"), "<html><body>Default</body></html>");
    }

    [Fact]
    public void GetFileProvider_ShouldReturnPhysicalFileProvider()
    {
        // Act
        var fileProvider = _staticFileService.GetFileProvider();

        // Assert
        Assert.NotNull(fileProvider);
        Assert.IsType<PhysicalFileProvider>(fileProvider);
    }

    [Fact]
    public async Task ServeFile_ShouldServeDefaultFile()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/";

        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        // Act
        await _staticFileService.ServeFile(context);

        // Assert
        responseStream.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(responseStream);
        var responseBody = await reader.ReadToEndAsync();

        Assert.Equal("text/html", context.Response.ContentType);
        Assert.NotEmpty(responseBody);
    }

    [Fact]
    public async Task ServeFile_ShouldReturnNoContent_WhenDirectoryIsEmpty()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/empty";

        // Act
        await _staticFileService.ServeFile(context);

        // Assert
        Assert.Equal(204, context.Response.StatusCode);
    }
}