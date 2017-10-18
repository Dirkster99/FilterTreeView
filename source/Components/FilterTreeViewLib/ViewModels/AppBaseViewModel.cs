namespace FilterTreeViewLib.ViewModels
{
    using FilterTreeViewLib.ViewModels.Base;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public abstract class AppBaseViewModel : Base.BaseViewModel
    {
        #region fields
        protected readonly ObservableCollection<MetaLocationViewModel> _CountryRootItems = null;
        protected readonly IList<MetaLocationViewModel> _backUpCountryRoots = null;

        private bool _IsStringContainedSearchOption;
        private int _CountSearchMatches;
        private bool _IsProcessing;

        private ICommand _ExpandCommand;

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
            _IsProcessing = false;
            _CountSearchMatches = 0;
            _IsStringContainedSearchOption = true;

            _CountryRootItems = new ObservableCollection<MetaLocationViewModel>();
            _backUpCountryRoots = new List<MetaLocationViewModel>(400);
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// Gets all bindable rootitems of the displayed treeview
        /// </summary>
        public ObservableCollection<MetaLocationViewModel> CountryRootItems
        {
            get
            {
                return _CountryRootItems;
            }
        }

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

        /// <summary>
        /// Gets a command that expands the item given in the command parameter.
        /// </summary>
        public ICommand ExpandCommand
        {
            get
            {
                if (_ExpandCommand == null)
                {
                    _ExpandCommand = new RelayCommand<object>((p) =>
                    {
                        var param = p as MetaLocationViewModel;

                        if (param == null)
                            return;

                        param.LoadChildren();
                    });
                }

                return _ExpandCommand;
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