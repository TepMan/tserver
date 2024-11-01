using Microsoft.Extensions.Configuration;
using Xunit;
using System.Collections.Generic;

public class StaticFileConfigurationTests
{
    [Fact]
    public void Configuration_ShouldBindStaticFileConfiguration()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"StaticFiles:RootPath", "wwwroot"},
            {"StaticFiles:DefaultFiles:0", "index.html"},
            {"StaticFiles:DefaultFiles:1", "default.html"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        var staticFileConfig = new StaticFileConfiguration();
        configuration.GetSection("StaticFiles").Bind(staticFileConfig);

        // Assert
        Assert.Equal("wwwroot", staticFileConfig.RootPath);
        Assert.Contains("index.html", staticFileConfig.DefaultFiles);
        Assert.Contains("default.html", staticFileConfig.DefaultFiles);
    }

    [Fact]
    public void Configuration_ShouldHandleMissingDefaultFiles()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"StaticFiles:RootPath", "wwwroot"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        var staticFileConfig = new StaticFileConfiguration();
        configuration.GetSection("StaticFiles").Bind(staticFileConfig);

        // Assert
        Assert.Equal("wwwroot", staticFileConfig.RootPath);
        Assert.Empty(staticFileConfig.DefaultFiles);
    }
}