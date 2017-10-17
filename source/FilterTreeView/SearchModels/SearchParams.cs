namespace FilterTreeView.SearchModels
{
    internal class SearchParams
    {
        #region constructors
        public SearchParams(
              string searchString
            , Enums.SearchMatch match)
            : this()
        {
            SearchString = (searchString == null ? string.Empty : searchString);
            Match = match;
        }

        public SearchParams()
        {
            SearchString = string.Empty;
            Match = Enums.SearchMatch.StringIsContained;
            MinimalSearchStringLength = 2;
        }
        #endregion constructors

        #region properties
        public string SearchString { get; private set; }

        public Enums.SearchMatch Match { get; private set; }

        public bool IsSearchStringEmpty
        {
            get
            {
                return string.IsNullOrEmpty(SearchString);
            }
        }

        public int MinimalSearchStringLength { get; }
        #endregion properties

        #region methods
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

        public void SearchStringTrim()
        {
            SearchString = SearchString.Trim();
        }

        public void SearchStringToUpperCase()
        {
            SearchString = SearchString.ToUpper();
        }
        #endregion methods
    }
}
