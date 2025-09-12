using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PointsCode
{
  public sealed class PointsCode : Aggregate<PointCodeId>
  {

    internal PointsCode() { }

    public OrganizationId OrganizationId { get; private set; } = null!;
    public string Code { get; private set; } = string.Empty; 
    public int Points { get; private set; }
    public UserId? AssignedUserId { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public PointsCodeStatus Status { get; private set; } = PointsCodeStatus.Issued;
    public DateTime? RedeemedAt { get; private set; }

    public static PointsCode Issue(
        PointCodeId id,
        OrganizationId orgId,
        string code,
        int points,
        DateTime? expiresAt = null)
    {
      if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
      if (points <= 0) throw new ArgumentException("Points must be positive.", nameof(points));

      return new PointsCode
      {
        Id = id,
        OrganizationId = orgId,
        Code = code,
        Points = points,
        ExpiresAt = expiresAt,
        CreatedAt = DateTime.UtcNow
      };
    }

    public void AssignToUser(UserId userId)
    {
      if (Status is PointsCodeStatus.Redeemed or PointsCodeStatus.Voided or PointsCodeStatus.Expired)
        throw new InvalidOperationException("Cannot assign a code that is not active.");

      AssignedUserId = userId;
      if (Status == PointsCodeStatus.Issued) Status = PointsCodeStatus.Assigned;
    }

    public void Redeem(UserId userId, OrganizationId requestingOrg, DateTime? now = null)
    {
      if (requestingOrg != OrganizationId)
        throw new InvalidOperationException("Code can only be redeemed at the issuing organization.");

      if (Status == PointsCodeStatus.Redeemed) throw new InvalidOperationException("Code already redeemed.");
      if (Status is PointsCodeStatus.Voided or PointsCodeStatus.Expired)
        throw new InvalidOperationException("Code is not redeemable.");

      var clock = now ?? DateTime.UtcNow;
      if (ExpiresAt is not null && clock > ExpiresAt.Value)
      {
        Status = PointsCodeStatus.Expired;
        throw new InvalidOperationException("Code has expired.");
      }

      Status = PointsCodeStatus.Redeemed;
      RedeemedAt = clock;

      // If it wasn't explicitly assigned earlier, tie it to the redeemer.
      AssignedUserId ??= userId;
    }



    public void Void(string reason)
    {
      if (Status == PointsCodeStatus.Redeemed)
        throw new InvalidOperationException("Cannot void a redeemed code.");

      if (Status == PointsCodeStatus.Voided) return;
      Status = PointsCodeStatus.Voided;
      // `reason` can be logged/audited by the application layer if needed.
    }
  }

  public enum PointsCodeStatus { Issued, Assigned, Redeemed, Expired, Voided }
}
