

namespace Domain.ValueObjects
{
    public record Address
    {
        public string Street { get; }
        public string City { get; }
        public string State { get; }
        public string ZipCode { get; }
        public string Country { get; }

        protected Address() { }

        private Address(string street, string city, string state, string zipCode, string country)
        {

            Street = street;
            City = city;
            State = state;
            ZipCode = zipCode;
            Country = country;
        }

        public static Address Of(string street, string city, string state, string zipCode, string country)
        {
            if (string.IsNullOrWhiteSpace(street)) throw new ArgumentException("Street is required.");
            if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("City is required.");
            if (string.IsNullOrWhiteSpace(state)) throw new ArgumentException("State is required.");
            if (string.IsNullOrWhiteSpace(zipCode)) throw new ArgumentException("ZipCode is required.");
            if (string.IsNullOrWhiteSpace(country)) throw new ArgumentException("Country is required.");
            return new Address(street, city, state, zipCode, country);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Street, City, State, ZipCode, Country);
        }

        public override string ToString()
        {
            return $"{Street}, {City}, {State}, {ZipCode}, {Country}";
        }
    }
}
