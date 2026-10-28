using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TaskReportApi.Configuration;

public sealed class SwaggerBearerSecurityRequirementsOperationFilter : IOperationFilter, IDocumentFilter
{
    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        // Security references must resolve against the completed document to be serialized.
        document.SetReferenceHostDocument();
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorizeAttribute =
            context.MethodInfo.GetCustomAttributes<AuthorizeAttribute>(true).Any() ||
            context.MethodInfo.DeclaringType?.GetCustomAttributes<AuthorizeAttribute>(true).Any() == true;

        var hasAllowAnonymousAttribute =
            context.MethodInfo.GetCustomAttributes<AllowAnonymousAttribute>(true).Any() ||
            context.MethodInfo.DeclaringType?.GetCustomAttributes<AllowAnonymousAttribute>(true).Any() == true;

        if (!hasAuthorizeAttribute || hasAllowAnonymousAttribute)
        {
            return;
        }

        operation.Security ??= new List<OpenApiSecurityRequirement>();

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, null, null)
            ] = new List<string>()
        });
    }
}
