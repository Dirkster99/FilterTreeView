namespace FilterTreeViewVis.ViewModels
{
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
    /// 
    /// Reactive Code Source:
    /// http://blog.petegoo.com/2011/11/22/building-an-auto-complete-control-with-reactive-extensions-rx/
    /// </summary>
    internal class AppViewModel : FilterTreeViewLib.ViewModels.AppBaseViewModel
    {
        #region fields
        protected readonly TestLocationRootViewModel _TestRoot;

        private Dictionary<string, CancellationTokenSource> _Queue = null;
        private static SemaphoreSlim SlowStuffSemaphore = null;
        private ICommand _SearchCommand;

        // Use this if you need logger output of the input value stream
        ////        private readonly ObservableCollection<string> logOutput = new ObservableCollection<string>();
        #endregion fields

        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        public AppViewModel()
            : base()
        {
            _TestRoot = new TestLocationRootViewModel();

            _Queue = new Dictionary<string, CancellationTokenSource>();
            SlowStuffSemaphore = new SemaphoreSlim(1, 1);
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// Gets the root viewmodel that can be bound to a treeview control.
        /// </summary>
        public TestLocationRootViewModel Root
        {
            get
            {
                return _TestRoot;
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
                        if (IsProcessing == true)
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
                await Root.LoadData(@".\Resources\lokasyon.zip"
                                 , "countries.xml", "regions.xml", "cities.xml");

                StatusStringResult = string.Format("Searching... '{0}'", SearchString);
                await DoSearchAsync(SearchString);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Async version to filter the display of nodes in a treeview
        /// with a filterstring (node is shown if filterstring is contained).
        /// </summary>
        /// <param name="findThis"></param>
        /// <returns></returns>
        private async Task<SearchResult> DoSearchAsync(string findThis)
        {
            return await Task.Run<SearchResult>(() => { return SearchCommand_ExecutedAsync(findThis); });
        }

        /// <summary>
        /// Filters the display of nodes in a treeview
        /// with a filterstring (node is shown if filterstring is contained).
        /// </summary>
        /// <param name="findThis"></param>
        /// <returns></returns>
        private async Task<SearchResult> SearchCommand_ExecutedAsync(string findThis)
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

            //Make sure the task always processes the last input but is not started twice
            await SlowStuffSemaphore.WaitAsync();
            try
            {
                IsProcessing = true;

                // There is more recent input to process so we ignore this one
                if (_Queue.Count > 1)
                {
                    _Queue.Remove(findThis);
                    return new SearchResult(param, 0);
                }

                // Do the search and return number of results as int
                CountSearchMatches = await Root.DoSearchAsync(param, tokenSource.Token);

                this.StatusStringResult = findThis;
                return new SearchResult(param, CountSearchMatches);
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            finally
            {
                _Queue.Remove(findThis);
                SlowStuffSemaphore.Release();
                IsProcessing = false;
            }

            return new SearchResult(param, 0);
        }
        #endregion methods
    }
}
