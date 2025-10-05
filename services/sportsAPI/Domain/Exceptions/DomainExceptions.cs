

namespace Domain.Exceptions
{
    internal class DomainExceptions : Exception
    {
        public DomainExceptions(string message) : base($"Domain Exception: \"{message}\" throws from Domain Layer")
        {
            
        }
    }
}
