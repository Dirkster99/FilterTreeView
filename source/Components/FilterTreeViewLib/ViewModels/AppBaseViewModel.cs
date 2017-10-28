namespace FilterTreeViewLib.ViewModels
{
    using System.Threading.Tasks;

    public abstract class AppBaseViewModel : Base.BaseViewModel
    {
        #region fields
        private bool _IsStringContainedSearchOption;
        private int _CountSearchMatches;
        private bool _IsProcessing;
        private bool _IsLoading;

        private string _StatusStringResult;
        private string _SearchString;
        #endregion fields

        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        public AppBaseViewModel()
        {
            _SearchString = "Washington";
            _IsProcessing = _IsLoading = false;
            _CountSearchMatches = 0;
            _IsStringContainedSearchOption = true;
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// Gets a property to determine if application is currently processing
        /// data (loading or searching for matches in the tree view) or not.
        /// </summary>
        public bool IsProcessing
        {
            get { return _IsProcessing; }
            protected set
            {
                if (_IsProcessing != value)
                {
                    _IsProcessing = value;
                    NotifyPropertyChanged(() => IsProcessing);
                }
            }
        }

        /// <summary>
        /// Gets a property to determine if application is currently processing
        /// data (loading or searching for matches in the tree view) or not.
        /// </summary>
        public bool IsLoading
        {
            get { return _IsLoading; }
            protected set
            {
                if (_IsLoading != value)
                {
                    _IsLoading = value;
                    NotifyPropertyChanged(() => IsLoading);
                }
            }
        }

        /// <summary>
        /// Gets the input string from search textbox control.
        /// </summary>
        public string SearchString
        {
            get { return _SearchString; }
            set
            {
                if (_SearchString != value)
                {
                    _SearchString = value;
                    NotifyPropertyChanged(() => SearchString);
                }
            }
        }

        /// <summary>
        /// Gets the search string that is synchronized with the results from the search algorithm.
        /// </summary>
        public string StatusStringResult
        {
            get { return _StatusStringResult; }
            protected set
            {
                if (_StatusStringResult != value)
                {
                    _StatusStringResult = value;
                    NotifyPropertyChanged(() => StatusStringResult);
                }
            }
        }

        /// <summary>
        /// Determines whether the search is looking for:
        /// - strings that are contained in a given string or
        /// - strings that match the searched string with all letters.
        /// </summary>
        public bool IsStringContainedSearchOption
        {
            get { return _IsStringContainedSearchOption; }
            set
            {
                if (_IsStringContainedSearchOption != value)
                {
                    _IsStringContainedSearchOption = value;
                    NotifyPropertyChanged(() => IsStringContainedSearchOption);
                }
            }
        }

        /// <summary>
        /// Gets the number of matches that are found durring
        /// a search in the tree view nodes for a given string.
        /// </summary>
        public int CountSearchMatches
        {
            get { return _CountSearchMatches; }
            protected set
            {
                if (_CountSearchMatches != value)
                {
                    _CountSearchMatches = value;
                    NotifyPropertyChanged(() => CountSearchMatches);
                }
            }
        }
        #endregion properties

        #region methods
        /// <summary>
        /// Loads the initial sample data from XML file into memory
        /// </summary>
        /// <returns></returns>
        public abstract Task LoadSampleDataAsync();
        #endregion methods
    }
}