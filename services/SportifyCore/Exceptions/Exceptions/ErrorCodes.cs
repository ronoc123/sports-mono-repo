namespace BuildingBlocks.Exceptions
{
    public static class ErrorCodes
    {
        public const string NotFound = "NotFound";
        public const string Validation = "Validation";
        public const string Concurrency = "Concurrency";
        public const string Conflict = "Conflict";
        public const string Forbidden = "Forbidden";
        public const string Unauthorized = "Unauthorized";
        public const string DomainRule = "DomainRule";
        public const string BadRequest = "BadRequest";
        public const string Unknown = "Unknown";
        public const string ServiceUnavailable = "ServiceUnavailable";
        public const string DownstreamError = "DownstreamError";
        public const string NetworkError = "NetworkError";
        public const string Timeout = "Timeout";
        public const string ClientCanceled = "ClientCanceled";
    }
}
