using System;

namespace StreetviewJourney
{
    public class ZeroResultsException : Exception
    {
        public ZeroResultsException(string message) : base(message) { }
    }

    public class MetadataQueryException : Exception
    {
        public MetadataQueryException(string message) : base(message) { }
    }

    public class ThirdPartyPanoramaException : Exception
    {
        public ThirdPartyPanoramaException(string message) : base(message) { }
    }

    public class DontBillMeException : Exception
    {
        public DontBillMeException(string message) : base(message) { }
    }
}
