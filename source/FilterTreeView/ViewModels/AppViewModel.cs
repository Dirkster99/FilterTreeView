namespace FilterTreeView.ViewModels
{
    using FilterTreeView.SearchModels;
    using FilterTreeView.SearchModels.Enums;
    using FilterTreeView.ViewModels.Base;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;

    internal class AppViewModel : Base.BaseViewModel
    {
        #region fields
        private readonly ObservableCollection<MetaLocationViewModel> _CountryRootItems = null;
        private readonly IList<MetaLocationViewModel> _backUpCountryRoots = null;

        private bool _IsStringContainedSearchOption;
        private int _CountSearchMatches;
        private bool _IsProcessing;

        private ICommand _SearchCommand;
        private ICommand _ExpandCommand;

        private List<string> _Queue = new List<string>();
        private static SemaphoreSlim SlowStuffSemaphore = new SemaphoreSlim(1, 1);
        private string _StatusStringResult;
        private string _SearchString;
        #endregion fields

        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        public AppViewModel()
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
        /// Gets the input string from search textbox control.
        /// </summary>
        public string SearchString
        {
            get { return _SearchString; }
            private set
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
            private set
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
                    _SearchCommand = new RelayCommand<object>(async (p) =>
                    {
                        string findThis = p as string;

                        if (findThis == null)
                            return;

                        await SearchCommand_ExecutedAsync(findThis);
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
        public async Task LoadSampleDataAsync()
        {
            IsProcessing = true;
            StatusStringResult = "Loading Data... please wait.";
            try
            {
                var isoCountries = await BusinessLib.Database.LoadData
                    (@".\Resources\lokasyon.zip", "countries.xml", "regions.xml", "cities.xml");

                foreach (var item in isoCountries)
                {
                    await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                     {
                        //var vmItem = new MetaLocationViewModel(item, null);
                        var vmItem = MetaLocationViewModel.GetViewModelFromModel(item);

                         _backUpCountryRoots.Add(vmItem);
                        //_CountryRootItems.Add(vmItem);

                    }), DispatcherPriority.ContextIdle);
                }

                StatusStringResult = string.Format("Searching... '{0}'", SearchString);
                await SearchCommand_ExecutedAsync(SearchString);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task<int> SearchCommand_ExecutedAsync(string findThis)
        {
            _Queue.Add(findThis);

            //Make sure the task always processes the last input but is not started twice
            await SlowStuffSemaphore.WaitAsync();
            try
            {
                // There is more recent input to process so we ignore this one
                if (_Queue.Count > 1)
                {
                    _Queue.Remove(findThis);
                    return 0;
                }
                else
                {
                    Console.WriteLine("Queue Count: {0}", _Queue.Count);
                }

                // Setup search parameters
                SearchParams param = new SearchParams(findThis
                   , (IsStringContainedSearchOption == true ?
                      SearchMatch.StringIsContained : SearchMatch.StringIsMatched));

                try
                {
                    IsProcessing = true;

                    // Do the search and return number of results as int
                    CountSearchMatches = await Search.BackupNodeSearch.DoSearchAsync(
                                                                            _backUpCountryRoots
                                                                          , _CountryRootItems
                                                                          , param);

                    _Queue.Remove(findThis);

                    this.StatusStringResult = findThis;

                    return CountSearchMatches;
                }
                finally
                {
                    IsProcessing = false;
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            finally
            {
                SlowStuffSemaphore.Release();
            }

            return -1;
        }
        #endregion methods
    }
}