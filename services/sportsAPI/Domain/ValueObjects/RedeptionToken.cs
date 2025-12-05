using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects
{
  public sealed record RedemptionToken(
      RewardItemId RewardItemId,
      OrganizationId OrgId,
      UserId RedeemingUser,
      long Amount,
      string RedemptionId);

}
