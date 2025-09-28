using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.Sockets;
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
      // unwrap common EF wrapper so we see the provider exception
      if (ex is DbUpdateException dbe && dbe.InnerException is not null)
        ex = dbe.InnerException;

      return ex switch
      {
        // ----- existing cases (unchanged) -----
        ValidationException da => (400, new ServiceResponse<object>
        {
          Success = false,
          Message = da.ValidationResult?.ErrorMessage ?? da.Message,
          ErrorCode = ErrorCodes.Validation,
          TraceId = traceId
        }),

        EntityNotFoundException nf => (404, new ServiceResponse<object>
        {
          Success = false,
          Message = nf.Message,
          ErrorCode = ErrorCodes.NotFound,
          TraceId = traceId
        }),

        DbUpdateConcurrencyException => (409, new ServiceResponse<object>
        {
          Success = false,
          Message = "A concurrency conflict occurred. Please reload and retry.",
          ErrorCode = ErrorCodes.Concurrency,
          TraceId = traceId
        }),

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

        // ----- NEW: database/service infrastructure -----

        // SQL Server unavailable / transient provider error
        SqlException => (503, new ServiceResponse<object>
        {
          Success = false,
          Message = "Database is unavailable. Please try again.",
          ErrorCode = ErrorCodes.ServiceUnavailable,
          TraceId = traceId
        }),

        // Downstream HTTP call failed (service down/5xx/connection failure)
        HttpRequestException => (502, new ServiceResponse<object>
        {
          Success = false,
          Message = "Downstream service error.",
          ErrorCode = ErrorCodes.DownstreamError,
          TraceId = traceId,
          Details = ex.Message
        }),

        // Network connectivity issue
        SocketException => (503, new ServiceResponse<object>
        {
          Success = false,
          Message = "Network connectivity issue.",
          ErrorCode = ErrorCodes.NetworkError,
          TraceId = traceId
        }),

        // Timeouts (server-side or canceled operations)
        TimeoutException or TaskCanceledException => (504, new ServiceResponse<object>
        {
          Success = false,
          Message = "Operation timed out.",
          ErrorCode = ErrorCodes.Timeout,
          TraceId = traceId
        }),

        // ----- fallback -----
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
