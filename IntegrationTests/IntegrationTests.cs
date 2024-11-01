using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System.IO;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();

        SetupTestFiles();
    }

    private void SetupTestFiles()
    {
        var testRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var testSubDir = Path.Combine(testRootPath, "subdir");

        if (!Directory.Exists(testSubDir))
        {
            Directory.CreateDirectory(testSubDir);
        }

        File.WriteAllText(Path.Combine(testSubDir, "first.html"), "<html><body>First HTML File</body></html>");
    }

    [Fact]
    public async Task GetRoot_ReturnsIndexHtml()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("<h1>Index Page</h1>", content);
    }

    [Fact]
    public async Task GetSubdir_ReturnsFirstHtmlFile()
    {
        // Act
        var response = await _client.GetAsync("/subdir");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("<html><body>First HTML File</body></html>", content);
    }

    [Fact]
    public async Task GetNonExistentFile_ReturnsNoContent()
    {
        // Act
        var response = await _client.GetAsync("/nonexistent");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }
}