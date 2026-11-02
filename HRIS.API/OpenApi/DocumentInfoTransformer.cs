using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HRIS.API.OpenApi;

/// <summary>
/// Customizes the OpenAPI document information.
/// </summary>
public sealed class DocumentInfoTransformer(
    IConfiguration configuration)
    : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "HRIS API",
            Version = "v1",
            Description =
                "Human Resource Information System: departments, positions, employees, and accounts.",

            Contact = new OpenApiContact
            {
                Name = "HRIS Platform Team",
                Email = "hris-support@company.id",
                Url = new Uri("https://intranet.company.id/hris")
            },

            License = new OpenApiLicense
            {
                Name = "Internal use only",
                Url = new Uri("https://intranet.company.id/hris/license")
            }
        };

        var servers = configuration
            .GetSection("OpenApi:Servers")
            .GetChildren()
            .Where(server =>
                !string.IsNullOrWhiteSpace(server["Url"]))
            .Select(server => new OpenApiServer
            {
                Url = server["Url"]!,
                Description = server["Description"]
            })
            .ToList();

        if (servers.Count > 0)
        {
            document.Servers = servers;
        }

        return Task.CompletedTask;
    }
}