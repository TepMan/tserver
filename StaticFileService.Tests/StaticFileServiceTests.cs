using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Xunit;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
        File.WriteAllText(Path.Combine(_testRootPath, "testfile.html"), "<html><body>Test File</body></html>");
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

    [Fact]
    public async Task ServeFile_ShouldServeSpecificFile()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/testfile.html";

        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        // Act
        await _staticFileService.ServeFile(context);

        // Assert
        responseStream.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(responseStream);
        var responseBody = await reader.ReadToEndAsync();

        Assert.Equal("text/html", context.Response.ContentType);
        Assert.Contains("Test File", responseBody);
    }

    [Fact]
    public void StaticFileService_ShouldThrowException_WhenConfigurationIsInvalid()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"StaticFiles:RootPath", ""}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => new StaticFileService(configuration));
        Assert.Equal("Server is not configured properly! Please check the configuration.", exception.Message);
    }

    [Fact]
    public async Task ServeFile_ShouldServeFirstHtmlFile_WhenNoDefaultFileExists()
    {
        // Arrange
        var testSubDir = Path.Combine(_testRootPath, "subdir");
        if (!Directory.Exists(testSubDir))
        {
            Directory.CreateDirectory(testSubDir);
        }

        File.WriteAllText(Path.Combine(testSubDir, "first.html"), "<html><body>First HTML File</body></html>");

        var inMemorySettings = new Dictionary<string, string?>
        {
            {"StaticFiles:RootPath", _testRootPath},
            {"StaticFiles:DefaultFiles:0", "nonexistent.html"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var staticFileService = new StaticFileService(configuration);

        var context = new DefaultHttpContext();
        context.Request.Path = "/subdir";

        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        // Act
        await staticFileService.ServeFile(context);

        // Assert
        responseStream.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(responseStream);
        var responseBody = await reader.ReadToEndAsync();

        Assert.Equal("text/html", context.Response.ContentType);
        Assert.Contains("First HTML File", responseBody);
    }
}