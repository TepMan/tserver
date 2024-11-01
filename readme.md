# tserver Web Server

This project is a simple web server built with ASP.NET Core. It serves static files from a configurable root directory and supports custom default files.

## Features

- Serves static files from a configurable root directory.
- Supports custom default files (e.g., `index.html`, `default.html`).
- Automatically serves the first HTML file found in a directory if no default file is present.
- Returns a "No Content" response if the directory is empty.

## Configuration

The server configuration is managed through the `appsettings.json` file. Below are the configuration options:

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "StaticFiles": {
    "RootPath": "wwwroot",
    "DefaultFiles": [
      "index.html",
      "default.html"
    ]
  }
}
   ```

- **StaticFiles:RootPath**: The root directory from which static files are served.
- **StaticFiles:DefaultFiles**: A list of default files to look for in each directory.

## Usage

1. Clone the repository.
2. Configure the `appsettings.json` file as needed.
3. Build and run the project using the following commands:

   ```sh
   dotnet build
   dotnet run
   ```

4. Access the server at `http://localhost:{port}`.

## Example

To serve files from the `wwwroot` directory and use `index.html` or `default.html` as default files, configure the `appsettings.json` as shown above.

When you navigate to `http://localhost:{port}/sub/`, the server will:
- Serve `default.html` if it exists in the `sub` directory.
- Serve the first HTML file found in the `sub` directory if no default file is present.
- Return a "No Content" response if the `sub` directory is empty.

## License

This project is licensed under the MIT License.
