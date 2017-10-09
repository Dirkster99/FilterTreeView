namespace FilterTreeView.ViewModels
{
    using FilterTreeView.SearchModels;
    using FilterTreeView.SearchModels.Enums;
    using FilterTreeView.ViewModels.Base;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;

    internal class AppViewModel : Base.BaseViewModel
    {
        #region fields
        private ObservableCollection<MetaLocationViewModel> _CountyRootItems = new ObservableCollection<MetaLocationViewModel>();
        private readonly IList<MetaLocationViewModel> _backUpCountryRoots = new List<MetaLocationViewModel>();

        private ICommand _SearchCommand;

        private bool _IsStringContainedSearchOption;
        private int _CountSearchMatches;
        private bool _IsProcessing;
        private string _SearchString;

        private ICommand _ExpandCommand;
        #endregion fields

        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        public AppViewModel()
        {
            _IsProcessing = false;
            _SearchString = "Washington";
            _CountSearchMatches = 0;
            _IsStringContainedSearchOption = true;
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
                return _CountyRootItems;
            }
        }

        /// <summary>
        /// Gets a property to determine if application is currently processing
        /// data (loading or searching for matches in the tree view) or not.
        /// </summary>
        public bool IsProcessing
        {
            get { return _IsProcessing; }
            private set
            {
                if (_IsProcessing != value)
                {
                    _IsProcessing = value;
                    NotifyPropertyChanged(() => IsProcessing);
                }
            }
        }

        /// <summary>
        /// Gets a search string to match in the tree view.
        /// </summary>
        public string SearchString
        {
            get { return _SearchString; }
            private set
            {
                if (_SearchString != value)
                {
                    _SearchString = value;
                    NotifyPropertyChanged(() => IsProcessing);
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
            private set
            {
                if (_CountSearchMatches != value)
                {
                    _CountSearchMatches = value;
                    NotifyPropertyChanged(() => CountSearchMatches);
                }
            }
        }

        /// <summary>
        /// Gets a command that filters the display of nodes in a treeview
        /// with a filterstring (node is shown if filterstring is contained).
        /// </summary>
        public ICommand SearchCommand
        {
            get
            {
                if (_SearchCommand == null)
                {
                    _SearchCommand = new RelayCommand<object>((p) =>
                    {
                        string findThis = p as string;

                        if (findThis == null)
                            return;

                        SearchCommand_ExecutedAsync(findThis);
                    },
                    (p =>
                    {
                        if (_backUpCountryRoots.Count == 0 && IsProcessing == false)
                            return false;

                        return true;
                    })
                    );
                }

                return _SearchCommand;
            }
        }


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
        public async Task LoadSampleDataAsync()
        {
            IsProcessing = true;

            try
            {
                var isoCountries = await BusinessLib.Database.LoadData
                    (@".\Resources\lokasyon.zip"
                    , "countries.xml", "regions.xml", "cities.xml");

                foreach (var item in isoCountries)
                {
                    var vmItem = new MetaLocationViewModel(item, null);
                    _backUpCountryRoots.Add(vmItem);

                    // Add with low UI priority to make adding a smooth experience
                    _CountyRootItems.Add(vmItem);
                }

                return;
            }
            finally
            {
                IsProcessing = false;
                SearchCommand_ExecutedAsync(SearchString);
            }
        }

        private int SearchCommand_ExecutedAsync(string findThis)
        {
            // Setup search parameters
            SearchParams param = new SearchParams(findThis
               , (IsStringContainedSearchOption == true ?
                  SearchMatch.StringIsContained : SearchMatch.StringIsMatched));

            try
            {
                IsProcessing = true;

                // Do the search and return number of results as int
                CountSearchMatches = Search.BackupNodeSearch.SearchPostOrderTraversal(
                        _backUpCountryRoots
                      , _CountyRootItems
                      , param);

                return CountSearchMatches;
            }
            finally
            {
                IsProcessing = false;
            }
        }
        #endregion methods
    }
}