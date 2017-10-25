namespace FilterTreeViewRx.ViewModels
{
    using FilterTreeViewLib.ViewModelsSearch.SearchModels;
    using FilterTreeViewLib.ViewModelsSearch.SearchModels.Enums;
    using FilterTreeViewRx.Commands;
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;

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
        private ReactiveRelayCommand textBoxEnterCommand;

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
            // Setup the command for the enter key on the textbox
            textBoxEnterCommand = new ReactiveRelayCommand(obj => { });

            // Listen to all property change events on SearchText
            var searchTextChanged = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                ev => PropertyChanged += ev,
                ev => PropertyChanged -= ev
                )
                .Where(ev => ev.EventArgs.PropertyName == "SearchString");


            // Transform the event stream into a stream of strings (the input values)
            var input = searchTextChanged
                .Where(ev => SearchString == null || SearchString.Length < 4)
                .Throttle(TimeSpan.FromSeconds(3))
                .Merge(searchTextChanged
                    .Where(ev => SearchString != null && SearchString.Length >= 4)
                    .Throttle(TimeSpan.FromMilliseconds(400)))
                .Select(args => SearchString)
                .Merge(textBoxEnterCommand.Executed.Select(e => SearchString))
                .DistinctUntilChanged();                      // Don't requery if value has not changed

            // Log all events in the event stream to the Log viewer
///            input.ObserveOn(Application.Current.Dispatcher)
///                .Subscribe(e => LogOutput.Insert(0,
///                    string.Format("Text Changed. Current Value - {0}", e)));


            // Setup an Observer for the search operation
            var search = Observable.ToAsync<string, SearchResult>(DoSearch);

            // Chain the input event stream and the search stream
            // cancelling searches when input is received
            var results = from searchTerm in input
                          from result in search(searchTerm).TakeUntil(input)
                          select result;

            // Log the search result and add the results to the results collection
            results.ObserveOn(Application.Current.Dispatcher)
            .Subscribe(result =>
            {
                StatusStringResult = "...";
                CountSearchMatches = 0;
////                LogOutput.Insert(0, string.Format("Search for '{0}' returned '{1}' items", result.SearchTerm, result.Results));

                StatusStringResult = result.OriginalSearchString;
                CountSearchMatches = result.Results;
            }
            );
        }
        #endregion constructors

        /// <summary>
        /// Gets a command that is executed when the user hits enter in the bound textbox.
        /// </summary>
        public ReactiveRelayCommand TextBoxEnterCommand
        {
            get { return textBoxEnterCommand; }
            protected set { textBoxEnterCommand = value; }
        }

        /// <summary>
        /// Gets an observable collection of strings that contain a log
        /// of the input stream and the number of results matched per string.
        /// </summary>
////        public ObservableCollection<string> LogOutput
////        {
////            get { return logOutput; }
////        }

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
            return await Task.Run<SearchResult>(() => { return DoSearch(findThis); });
        }

        /// <summary>
        /// Filters the display of nodes in a treeview
        /// with a filterstring (node is shown if filterstring is contained).
        /// </summary>
        /// <param name="findThis"></param>
        /// <returns></returns>
        private SearchResult DoSearch(string findThis)
        {
            // Setup search parameters
            SearchParams param = new SearchParams(findThis
               , (IsStringContainedSearchOption == true ?
                  SearchMatch.StringIsContained : SearchMatch.StringIsMatched));

            try
            {
                IsProcessing = true;

                // Do the search and return number of results as int
                CountSearchMatches = Root.DoSearch(param);

                return new SearchResult(param, CountSearchMatches);
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            finally
            {
                IsProcessing = false;
            }

            return new SearchResult(param, 0);
        }
        #endregion methods
    }
}
