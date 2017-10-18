namespace FilterTreeViewLib.SearchModels
{
    /// <summary>
    /// Models the results of a search in terms of the actual string and options
    /// being searched and the results found.
    /// </summary>
    public class SearchResult
    {
        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="searchParams"></param>
        /// <param name="results"></param>
        public SearchResult(SearchParams searchParams, int results)
            : this()
        {
            Options = searchParams;
            Results = results;
        }

        /// <summary>
        /// Hidden Class constructor
        /// </summary>
        protected SearchResult()
        {
        }
        #endregion constructors

        /// <summary>
        /// Gets the searchterm that was usedt o find the shown results.
        /// </summary>
        public string SearchTerm
        {
            get
            {
                if (Options != null)
                    return Options.SearchString;

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the ORIGONAL searchterm that was used to find the shown results.
        /// </summary>
        public string OriginalSearchString
        {
            get
            {
                if (Options != null)
                    return Options.OriginalSearchString;

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the search parameters and the search term for this result.
        /// </summary>
        public SearchParams Options { get; protected set; }

        /// <summary>
        /// Gets the number of matches found.
        /// </summary>
        public int Results { get; protected set; }
    }
}
