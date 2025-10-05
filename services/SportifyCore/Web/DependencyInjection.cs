using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json;

namespace Web
{
  public static class DependencyInjection
  {
    /// <summary>
    /// Registers Sportify web plumbing:
    /// - JsonSerializerOptions (camelCase)
    /// - ExceptionHandlingMiddleware
    /// - IStartupFilter to auto-insert middleware
    /// - ApiBehavior to return ServiceResponse on model-binding 400s
    /// </summary>
    public static IServiceCollection AddSportifyWeb(this IServiceCollection services)
    {
      // JSON options used by the middleware
      services.TryAddSingleton(new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      });

      // Middleware itself (IMiddleware style)
      services.TryAddTransient<Web.ExceptionHandlingMiddleware>();

      return services;
    }
  }
}
