
using BuildingBlocks.Exceptions;

namespace Domain.ValueObjects.ConcreteTypes
{
  public record RewardItemId
  {
    public Guid Value { get; set; }

    public RewardItemId(Guid value)
    {
      Value = value;
    }

    public static RewardItemId Of(Guid value)
    {
      ArgumentNullException.ThrowIfNull(value);
      if (value == Guid.Empty)
      {
        throw new DomainException("RewardItemId cannot be empty");
      }

      return new RewardItemId(value);
    }
    public bool HasValue => Value != Guid.Empty;
  }
}
