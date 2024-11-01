var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Initialize the StaticFileService and read the configuration
var staticFileService = new StaticFileService(builder.Configuration);

// Use the configured static files root path
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = staticFileService.GetFileProvider(),
    RequestPath = "",
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
    }
});

app.MapGet("/{**path}", async context =>
{
    await staticFileService.ServeFile(context);
});

app.Run();
