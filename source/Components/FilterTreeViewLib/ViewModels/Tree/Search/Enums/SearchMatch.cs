namespace FilterTreeViewLib.ViewModelsSearch.SearchModels.Enums
{
    /// <summary>
    /// Determines the type of match that should be implemented to
    /// match a node to a search string during a search.
    /// </summary>
    public enum SearchMatch
    {
        /// <summary>
        /// The string searched is contained somewhere in within a nodes string.
        /// </summary>
        StringIsContained = 0,

        /// <summary>
        /// The string searched is an exact match within a nodes string
        /// (length of strings is identical and ALL letters are present
        /// in the given order - but case folding may still be applied).
        /// </summary>
        StringIsMatched = 1
    }
}
