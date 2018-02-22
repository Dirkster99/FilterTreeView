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
    internal class AppViewModel : FilterTreeViewLib.ViewModels.AppBaseViewModel, IDisposable
    {
        #region fields
        private readonly OneTaskProcessor _procesor;

        private readonly MetaLocationRootViewModel _Root;

        private ICommand _SearchCommand;
        private bool _Disposed;
        #endregion fields

        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        public AppViewModel()
            : base()
        {
            _procesor = new OneTaskProcessor();
            _Root = new MetaLocationRootViewModel();
            _Disposed = false;
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
        /// Implements the <see cref="IDisposable"/> interface.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

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
            // Setup search parameters
            SearchParams param = new SearchParams(findThis
                , (IsStringContainedSearchOption == true ?
                    SearchMatch.StringIsContained : SearchMatch.StringIsMatched));

            // Make sure the task always processes the last input but is not started twice
            try
            {
                IsProcessing = true;

                var tokenSource = new CancellationTokenSource();
                Func<int> a = new Func<int> (() => Root.DoSearch(param, tokenSource.Token));
                var t = await _procesor.ExecuteOneTask(a, tokenSource);

                this.StatusStringResult = findThis;
                CountSearchMatches = t;

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


        /// <summary>
        /// The bulk of the clean-up code is implemented here.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed == false)
            {
                if (disposing == true)
                {
                    _procesor.Dispose();
                }

                _Disposed = true;
            }
        }
        #endregion methods
    }
}
