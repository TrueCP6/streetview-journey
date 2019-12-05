using System;

namespace StreetviewJourney
{
    /// <summary>
    /// Thrown when no matching panoramas could be found in the determined radius
    /// </summary>
    public class ZeroResultsException : Exception
    {
        public ZeroResultsException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when a Streetview Static API metadata query returns an unknown error
    /// </summary>
    public class MetadataQueryException : Exception
    {
        public MetadataQueryException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when a method can not use a third party panorama
    /// </summary>
    public class ThirdPartyPanoramaException : Exception
    {
        public ThirdPartyPanoramaException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown as a precaution when Setup.DontBillMe is true to stop unintentional billing
    /// </summary>
    public class DontBillMeException : Exception
    {
        public DontBillMeException(string message) : base(message) { }
    }
}
