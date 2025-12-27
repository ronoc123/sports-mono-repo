namespace BuildingBlocks.Exceptions
{
    public class DomainException : Exception
    {
        public virtual string Code => ErrorCodes.DomainRule;

        public DomainException(string message)
            : base(message)
        {
        }
    }

}
