namespace FilterTreeView.ViewModels
{
    using FilterTreeViewLib.SearchModels;
    using FilterTreeViewLib.SearchModels.Enums;
    using FilterTreeViewLib.ViewModels;
    using FilterTreeViewLib.ViewModels.Base;
    using FilterTreeViewLib.ViewModels.Search;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;

    /// <summary>
    /// Implements the application viewmodel object that manages all main commands
    /// and bindings visible in the main window.
    /// </summary>
    internal class AppViewModel : FilterTreeViewLib.ViewModels.AppBaseViewModel
    {
        #region fields
        private ICommand _SearchCommand;

        private List<string> _Queue = null;
        private static SemaphoreSlim SlowStuffSemaphore = null;
        #endregion fields

        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        public AppViewModel()
            : base()
        {
            _Queue = new List<string>();
            SlowStuffSemaphore = new SemaphoreSlim(1, 1);
        }
        #endregion constructors

        #region properties
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
        #endregion properties

        #region methods
        /// <summary>
        /// Loads the initial sample data from XML file into memory
        /// </summary>
        /// <returns></returns>
        public override async Task LoadSampleDataAsync()
        {
            IsProcessing = true;
            StatusStringResult = "Loading Data... please wait.";
            try
            {
                var isoCountries = await BusinessLib.Database.LoadData
                    (@".\Resources\lokasyon.zip", "countries.xml", "regions.xml", "cities.xml");

                foreach (var item in isoCountries)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        //var vmItem = new MetaLocationViewModel(item, null);
                        var vmItem = MetaLocationViewModel.GetViewModelFromModel(item);

                        _backUpCountryRoots.Add(vmItem);
                        //_CountryRootItems.Add(vmItem);

                    }, DispatcherPriority.ApplicationIdle);
                }

                StatusStringResult = string.Format("Searching... '{0}'", SearchString);
                await SearchCommand_ExecutedAsync(SearchString);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Filters the display of nodes in a treeview
        /// with a filterstring (node is shown if filterstring is contained).
        /// </summary>
        protected async Task<int> SearchCommand_ExecutedAsync(string findThis)
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
                    CountSearchMatches = await BackupNodeSearch.DoSearchAsync(_backUpCountryRoots
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
