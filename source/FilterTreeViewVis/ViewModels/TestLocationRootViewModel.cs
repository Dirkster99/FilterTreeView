namespace FilterTreeViewVis.ViewModels
{
    using FilterTreeViewLib.ViewModels.Base;
    using FilterTreeViewLib.ViewModelsSearch.SearchModels;
    using FilterTreeViewLib.ViewModelsSearch.SearchModels.Enums;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;

    /// <summary>
    /// Implements the root viewmodel of the tree structure to be presented in a treeview.
    /// (This implementation filters through the Visibility of a treeview item instead of
    ///  using BackUpNodes with Add/Remove)
    /// </summary>
    public class TestLocationRootViewModel : FilterTreeViewLib.ViewModels.Base.BaseViewModel
    {
        #region fields
        private const DispatcherPriority _ChildrenEditPrio = DispatcherPriority.DataBind;

        protected readonly ObservableCollection<TestLocationViewModel> _CountryRootItems = null;

        private ICommand _ExpandCommand;
        #endregion fields

        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        public TestLocationRootViewModel()
        {
            _CountryRootItems = new ObservableCollection<TestLocationViewModel>();
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// Gets all bindable rootitems of the displayed treeview
        /// </summary>
        public ObservableCollection<TestLocationViewModel> CountryRootItems
        {
            get
            {
                return _CountryRootItems;
            }
        }

        /// <summary>
        /// Gets the total count of (backup) rootitems in the tree collectiton.
        /// 
        /// This number can be larger than the nummber of items in the <see cref="CountryRootItems"/>
        /// collection if a filter is currently applied.
        /// </summary>
        public int CountryRootItemsCount
        {
            get
            {
                return CountryRootItems.Count();
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
                        var param = p as TestLocationViewModel;

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
        /// 
        /// </summary>
        /// <param name="zipContainerFile"></param>
        /// <param name="countryXMLFile"></param>
        /// <param name="regionsXMLFile"></param>
        /// <param name="citiesXMLFile"></param>
        public async Task LoadData(
             string zipContainerFile
           , string countryXMLFile
           ,string regionsXMLFile
           ,string citiesXMLFile)
        {
            var isoCountries = await BusinessLib.Database.LoadData(zipContainerFile
                                                                 , countryXMLFile
                                                                 , regionsXMLFile
                                                                 , citiesXMLFile);

            foreach (var item in isoCountries)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var vmItem = TestLocationViewModel.GetViewModelFromModel(item);

                    RootsAdd(vmItem);
                    //Root.RootsAdd(vmItem, false); // Make all items initially visible

                }, DispatcherPriority.ApplicationIdle);
            }
        }

        /// <summary>
        /// Implements the Async version of a PostOrder Search algorithm
        /// over a tree structure that can have n-root nodes.
        /// 
        /// PostOrder Algorithm Source:
        /// https://blogs.msdn.microsoft.com/daveremy/2010/03/16/non-recursive-post-order-depth-first-traversal-in-c/
        /// </summary>
        /// <param name="searchParams"></param>
        public async Task<int> DoSearchAsync(SearchParams searchParams, CancellationToken token)
        {
            return await Task.Run<int>(() =>
            {
                return DoSearch(searchParams, token);
            });
        }

        /// <summary>
        /// Implements the Async version of a PostOrder Search algorithm
        /// over a tree structure that can have n-root nodes.
        /// 
        /// PostOrder Algorithm Source:
        /// https://blogs.msdn.microsoft.com/daveremy/2010/03/16/non-recursive-post-order-depth-first-traversal-in-c/
        /// </summary>
        /// <param name="searchParams"></param>
        public int DoSearch(SearchParams searchParams, CancellationToken token)
        {
            ObservableCollection<TestLocationViewModel> root = _CountryRootItems;

            if (searchParams == null)
                searchParams = new SearchParams();

            searchParams.SearchStringTrim();
            searchParams.SearchStringToUpperCase();

            // Show all root items if string to search is empty
            if (searchParams.IsSearchStringEmpty == true ||
                searchParams.MinimalSearchStringLength >= searchParams.SearchString.Length)
            {
                foreach (var rootItem in CountryRootItems)
                {
                    if (token.IsCancellationRequested == true)
                        return 0;

                    rootItem.IsItemVisible = true;
                    //PreOrderTraversal(rootItem);  !!! Lazy Loading Children !!!
                    //rootItem.ChildrenClear(false);
                    rootItem.SetExpand(false);

                    //Application.Current.Dispatcher.Invoke(() => { root.Add(rootItem); }, _ChildrenEditPrio);
                }

                return 0;
            }

            int imatchCount = 0;

            // Go through all root items and process their children
            foreach (var rootItem in CountryRootItems)
            {
                if (token.IsCancellationRequested == true)
                    return 0;

                rootItem.Match = MatchType.NoMatch;

                // Match children of this root item
                var nodeMatchCount = MatchNodes(rootItem, searchParams);

                imatchCount += nodeMatchCount;

                // Match this root item and find correct match type between
                // parent and children below
                if (searchParams.MatchSearchString(rootItem.LocalName) == true)
                {
                    rootItem.Match = MatchType.NodeMatch;
                    imatchCount++;
                }

                if (nodeMatchCount > 0)
                {
                    if (rootItem.Match == MatchType.NodeMatch)
                        rootItem.Match = MatchType.Node_AND_SubNodeMatch;
                    else
                        rootItem.Match = MatchType.SubNodeMatch;
                }

                // Determine wether root item should visible and expanded or not
                if (rootItem.Match != MatchType.NoMatch)
                {
                    if ((rootItem.Match & MatchType.SubNodeMatch) != 0)
                        rootItem.SetExpand(true);
                    else
                        rootItem.SetExpand(false);

                    //Console.WriteLine("node: {0} match count: {1}", rootItem.LocalName, nodeMatchCount);
                    rootItem.IsItemVisible = true;
                }
                else
                    rootItem.IsItemVisible = false;
            }

            return imatchCount;
        }

        /// <summary>
        /// Add a rootitem into the root items collection.
        /// </summary>
        /// <param name="vmItem"></param>
        /// <param name="addBackupItemOnly"></param>
        protected void RootsAdd(TestLocationViewModel vmItem)
        {
          _CountryRootItems.Add(vmItem);
        }

        /// <summary>
        /// Implement a PostOrder matching algorithm with one root node
        /// and returns the number of matching children found.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="filterString"></param>
        /// <returns></returns>
        private Task<int> MatchNodesAsync(
             TestLocationViewModel root
           , SearchParams searchParams)
        {
            return Task.Run<int>(() => { return MatchNodes(root, searchParams); });
        }

        /// <summary>
        /// Implement a PostOrder matching algorithm with one root node
        /// and returns the number of matching children found.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="filterString"></param>
        /// <returns></returns>
        private int MatchNodes(
             TestLocationViewModel root
           , SearchParams searchParams)
        {
            var toVisit = new Stack<TestLocationViewModel>();
            var visitedAncestors = new Stack<TestLocationViewModel>();
            int MatchCount = 0;

            toVisit.Push(root);
            while (toVisit.Count > 0)
            {
                var node = toVisit.Peek();
                if (node.ChildrenCount > 0)
                {
                    if (PeekOrDefault(visitedAncestors) != node)
                    {
                        visitedAncestors.Push(node);
                        PushReverse(toVisit, node.Children);
                        continue;
                    }

                    visitedAncestors.Pop();
                }

                // Process Node and count matches (if any)
                int offset = -1;
                if ((node.Match = node.ProcessNodeMatch(searchParams, out offset)) == MatchType.NodeMatch)
                    MatchCount++;

                if (node.Match == MatchType.SubNodeMatch ||
                    node.Match == MatchType.Node_AND_SubNodeMatch)
                {
                    node.SetExpand(true);
                }
                else
                    node.SetExpand(false);

                toVisit.Pop();
            }

            return MatchCount;
        }

        /// <summary>
        /// Return the top element of stack or null if the Stack is empty.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private TestLocationViewModel PeekOrDefault(Stack<TestLocationViewModel> s)
        {
            return s.Count == 0 ? null : s.Peek();
        }

        /// <summary>
        /// Push all children of a given node in reverse order into the
        /// <seealso cref="Stack{T}"/> <paramref name="s"/>.
        /// 
        /// Use this to traverse the tree from left to right.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="list"></param>
        private void PushReverse(
              Stack<TestLocationViewModel> s
            , IEnumerable<TestLocationViewModel> list)
        {
            foreach (var l in list.ToArray().Reverse())
            {
                s.Push(l);
            }
        }
        #endregion methods
   }
}