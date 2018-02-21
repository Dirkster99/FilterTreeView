namespace FilterTreeView.ViewModels
{
    using FilterTreeView.Tasks;
    using FilterTreeViewLib.ViewModels;
    using FilterTreeViewLib.ViewModels.Base;
    using FilterTreeViewLib.ViewModelsSearch.SearchModels;
    using FilterTreeViewLib.ViewModelsSearch.SearchModels.Enums;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// Implements the application viewmodel object that manages all main commands
    /// and bindings visible in the main window.
    /// </summary>
    internal class AppViewModel : FilterTreeViewLib.ViewModels.AppBaseViewModel
    {
        #region fields
        private readonly OneTaskLimitedScheduler _myTaskScheduler;
        private readonly MetaLocationRootViewModel _Root;

        private ICommand _SearchCommand;
        #endregion fields

        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        public AppViewModel()
            : base()
        {
            _myTaskScheduler = new OneTaskLimitedScheduler();
            _Root = new MetaLocationRootViewModel();
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// Gets the root viewmodel that can be bound to a treeview control.
        /// </summary>
        public MetaLocationRootViewModel Root
        {
            get
            {
                return _Root;
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
                        if (Root.BackUpCountryRootsCount == 0 && IsProcessing == false)
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
            IsLoading = true;
            StatusStringResult = "Loading Data... please wait.";
            try
            {
                await Root.LoadData(@".\Resources\lokasyon.zip"
                                  , "countries.xml", "regions.xml", "cities.xml");

                IsLoading = false;
                StatusStringResult = string.Format("Searching... '{0}'", SearchString);

                await SearchCommand_ExecutedAsync(SearchString);
            }
            finally
            {
                IsLoading = false;
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Filters the display of nodes in a treeview
        /// with a filterstring (node is shown if filterstring is contained).
        /// </summary>
        protected async Task<int> SearchCommand_ExecutedAsync(string findThis)
        {
            // Provide Cancel method ...
            var tokenSource = new CancellationTokenSource();

            // Setup search parameters
            SearchParams param = new SearchParams(findThis
                , (IsStringContainedSearchOption == true ?
                    SearchMatch.StringIsContained : SearchMatch.StringIsMatched));

            // Make sure the task always processes the last input but is not started twice
            try
            {
                IsProcessing = true;

                // Do the search and return number of results as int
                // CountSearchMatches = await Root.DoSearchAsync(param, tokenSource.Token);
                CountSearchMatches = await Task.Factory.StartNew<int>(
                    () => Root.DoSearch(param, tokenSource.Token),
                    tokenSource.Token, TaskCreationOptions.None, _myTaskScheduler);

                this.StatusStringResult = findThis;
                return CountSearchMatches;
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            finally
            {
                IsProcessing = false;
            }

            return -1;
        }
        #endregion methods
    }
}
