namespace BusinessLib.Models
{
    /// <summary>
    /// Determines the type of location in a collection entry.
    /// </summary>
    public enum LocationType
    {
        Unknown = 1000,

        /// <summary>
        /// This location refers to a country on our little world called Earth.
        /// </summary>
        Country = 100,

        /// <summary>
        /// This location refers to a region that is part of a country.
        /// </summary>
        Region = 10,

        /// <summary>
        /// This location refers to a city (e.g. Berlin) located within
        /// a region.
        /// </summary>
        City = 0
    }
}
