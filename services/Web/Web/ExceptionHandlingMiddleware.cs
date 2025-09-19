using Contracts.Contracts;
using BuildingBlocks.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;

namespace Web.Web
{
  // Domain exceptions you throw from aggregates/handlers (examples)

  public sealed class ExceptionHandlingMiddleware : IMiddleware
  {
    private readonly JsonSerializerOptions _json;

    public ExceptionHandlingMiddleware(JsonSerializerOptions jsonOptions) => _json = jsonOptions;

    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
      try
      {
        await next(ctx);
      }
      catch (Exception ex)
      {
        var traceId = Activity.Current?.Id ?? ctx.TraceIdentifier;
        var (status, body) = MapException(ex, traceId);

        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(body, _json));
      }
    }

    private static (int Status, ServiceResponse<object> Body) MapException(Exception ex, string traceId)
    {
      return ex switch
      {
        // FluentValidation (from pipeline behavior)
        ValidationException da => (400, new ServiceResponse<object>
        {
          Success = false,
          Message = da.ValidationResult?.ErrorMessage ?? da.Message,
          ErrorCode = ErrorCodes.Validation,
          TraceId = traceId,
        }),

        // Not found
        EntityNotFoundException nf => (404, new ServiceResponse<object>
        {
          Success = false,
          Message = nf.Message,
          ErrorCode = ErrorCodes.NotFound,
          TraceId = traceId
        }),

        // Optimistic concurrency from EF Core
        DbUpdateConcurrencyException => (409, new ServiceResponse<object>
        {
          Success = false,
          Message = "A concurrency conflict occurred. Please reload and retry.",
          ErrorCode = ErrorCodes.Concurrency,
          TraceId = traceId
        }),

        // Domain rule violations (thrown by aggregates)
        DomainException de => (422, new ServiceResponse<object>
        {
          Success = false,
          Message = de.Message,
          ErrorCode = de.Code,
          TraceId = traceId
        }),

        UnauthorizedAccessException => (401, new ServiceResponse<object>
        {
          Success = false,
          Message = "Authentication is required.",
          ErrorCode = ErrorCodes.Unauthorized,
          TraceId = traceId
        }),

        FormatException or JsonException => (400, new ServiceResponse<object>
        {
          Success = false,
          Message = "Malformed input.",
          ErrorCode = ErrorCodes.BadRequest,
          TraceId = traceId,
          Details = ex.Message
        }),

        _ => (500, new ServiceResponse<object>
        {
          Success = false,
          Message = "An unexpected error occurred.",
          ErrorCode = ErrorCodes.Unknown,
          TraceId = traceId,
          Details = ex.Message
        })
      };
    }
  }
}
