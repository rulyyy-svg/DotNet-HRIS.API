using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HRIS.API.OpenApi;

/// <summary>
/// Adds a Bearer security requirement to every operation
/// except endpoints marked with AllowAnonymous.
/// </summary>
public sealed class AuthorizeOperationTransformer
    : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var allowsAnonymous = context.Description
            .ActionDescriptor
            .EndpointMetadata
            .OfType<IAllowAnonymous>()
            .Any();

        if (allowsAnonymous)
        {
            return Task.CompletedTask;
        }

        operation.Security ??= [];

        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecuritySchemeReference(
                        BearerSecuritySchemeTransformer.SchemeId,
                        context.Document)
                ] = []
            });

        return Task.CompletedTask;
    }
}