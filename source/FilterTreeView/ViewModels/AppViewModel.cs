namespace FilterTreeView.ViewModels
{
    using FilterTreeViewLib.ViewModels;
    using FilterTreeViewLib.ViewModels.Base;
    using FilterTreeViewLib.ViewModelsSearch.SearchModels;
    using FilterTreeViewLib.ViewModelsSearch.SearchModels.Enums;
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        private readonly MetaLocationRootViewModel _Root;

        private ICommand _SearchCommand;

        private Dictionary<string, CancellationTokenSource> _Queue = null;
        private static SemaphoreSlim SlowStuffSemaphore = null;
        #endregion fields

        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        public AppViewModel()
            : base()
        {
            _Root = new MetaLocationRootViewModel();

            _Queue = new Dictionary<string, CancellationTokenSource>();
            SlowStuffSemaphore = new SemaphoreSlim(1, 1);
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
            // Cancel current task(s) if there is any...
            var queueList = _Queue.Values.ToList();

            for (int i = 0; i < queueList.Count; i++)
                queueList[i].Cancel();

            var tokenSource = new CancellationTokenSource();
            _Queue.Add(findThis, tokenSource);

            // Setup search parameters
            SearchParams param = new SearchParams(findThis
                , (IsStringContainedSearchOption == true ?
                    SearchMatch.StringIsContained : SearchMatch.StringIsMatched));

            // Make sure the task always processes the last input but is not started twice
            await SlowStuffSemaphore.WaitAsync();
            try
            {
                IsProcessing = true;

                // There is more recent input to process so we ignore this one
                if (_Queue.Count > 1)
                {
                    _Queue.Remove(findThis);
                    return 0;
                }

                // Do the search and return number of results as int
                CountSearchMatches = await Root.DoSearchAsync(param, tokenSource.Token);

                this.StatusStringResult = findThis;
                return CountSearchMatches;
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            finally
            {
                _Queue.Remove(findThis);
                IsProcessing = false;
                SlowStuffSemaphore.Release();
            }

            return -1;
        }
        #endregion methods
    }
}
