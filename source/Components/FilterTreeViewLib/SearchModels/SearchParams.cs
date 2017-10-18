namespace FilterTreeViewLib.SearchModels
{
    /// <summary>
    /// Implements a search object that contains the search string,
    /// related options, as well as methods to determine whether a
    /// given string is a match against the string or not.
    /// </summary>
    public class SearchParams
    {
        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        public SearchParams(
              string searchString
            , Enums.SearchMatch match)
            : this()
        {
            OriginalSearchString =  SearchString = (searchString == null ? string.Empty : searchString);
            Match = match;
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        public SearchParams()
        {
            SearchString = string.Empty;
            Match = Enums.SearchMatch.StringIsContained;
            MinimalSearchStringLength = 1;
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// Gets the plain text string being searched or filtered.
        /// </summary>
        public string SearchString { get; private set; }

        /// <summary>
        /// Gets the plain text ORIGINAL string being searched or filtered.
        /// This string is the string that was set by the constructor and
        /// CANNOT be changed later on.
        /// </summary>
        public string OriginalSearchString { get; private set; }

        /// <summary>
        /// Gets the string being searched or filtered.
        /// </summary>
        public Enums.SearchMatch Match { get; private set; }

        /// <summary>
        /// Gets whether search string contains actual content or not.
        /// </summary>
        public bool IsSearchStringEmpty
        {
            get
            {
                return string.IsNullOrEmpty(SearchString);
            }
        }

        /// <summary>
        /// Gets the minimal search string length required.
        /// Any string shorter than this will not be searched at all.
        /// </summary>
        public int MinimalSearchStringLength { get; set; }
        #endregion properties

        #region methods
        /// <summary>
        /// Determines if a given string is considered a match in comparison
        /// to the search string and its options or not.
        /// </summary>
        /// <param name="stringToFind"></param>
        /// <returns>true if <paramref name="stringToFind"/>is a match, otherwise false</returns>
        public bool MatchSearchString(string stringToFind)
        {
            stringToFind = (stringToFind == null ? string.Empty : stringToFind);

            stringToFind = stringToFind.ToUpper();

            switch (Match)
            {
                case Enums.SearchMatch.StringIsContained:
                    return stringToFind.Contains(SearchString);

                case Enums.SearchMatch.StringIsMatched:
                    return SearchString == stringToFind;

                default:
                    throw new System.ArgumentOutOfRangeException(
                        string.Format("Internal Error: Search option '{0}' not implemented.", Match));
            }
        }

        /// <summary>
        /// Can be called to trim the search string before matching takes place.
        /// </summary>
        public void SearchStringTrim()
        {
            SearchString = SearchString.Trim();
        }

        /// <summary>
        /// Can be called to convert the search string
        /// to upper case before matching takes place.
        /// </summary>
        public void SearchStringToUpperCase()
        {
            SearchString = SearchString.ToUpper();
        }
        #endregion methods
    }
}
