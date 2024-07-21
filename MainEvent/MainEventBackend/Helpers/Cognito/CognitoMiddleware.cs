using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace MainEvent.Helpers.Cognito;

/// <summary>
/// Middleware for handling Cognito authentication.
/// </summary>
public class CognitoMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HttpContext for the current request.</param>
    /// <param name="cognitoService">The ICognitoService instance for handling Cognito operations.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context, ICognitoService cognitoService)
    {
        // Do not run our middleware on Anonymous routes
        if (context.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() != null)
        {
            await next(context);
            return;
        }

        var email = context.User.FindFirst(ClaimTypes.Email)?.Value;
        if (context.User.Identity is { IsAuthenticated: true } && email != null)
            cognitoService.Set(new CognitoUser(email));
        else
        {
            // Return unauthorized error
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Unauthorized: Email not found");
        }

        await next(context);
    }
}