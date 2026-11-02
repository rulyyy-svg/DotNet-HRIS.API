using HRIS.API.OpenApi;

namespace HRIS.API.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddHrisOpenApi(
        this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            // Document information
            options.AddDocumentTransformer<DocumentInfoTransformer>();

            // JWT Bearer security scheme
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();

            // Authorization requirement for operations
            options.AddOperationTransformer<AuthorizeOperationTransformer>();
        });

        return services;
    }

    public static WebApplication UseHrisOpenApi(
        this WebApplication app)
    {
        var enabled =
            app.Environment.IsDevelopment() ||
            app.Configuration.GetValue<bool>("Swagger:Enabled");

        if (!enabled)
        {
            return app;
        }

        // OpenAPI JSON endpoint
        app.MapOpenApi().AllowAnonymous();

        // Swagger UI
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(
                "/openapi/v1.json",
                "HRIS API");

            options.RoutePrefix = "swagger";
            options.DocumentTitle = "HRIS API";

            options.DisplayRequestDuration();
            options.EnablePersistAuthorization();

            options.DocExpansion(
                Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        });

        return app;
    }
}