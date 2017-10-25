namespace FilterTreeViewLib.ViewModels
{
    using FilterTreeViewLib.ViewModels.Base;
    using FilterTreeViewLib.ViewModelsSearch.SearchModels;
    using FilterTreeViewLib.ViewModelsSearch.SearchModels.Enums;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;

    /// <summary>
    /// Implements the root viewmodel of the tree structure to be presented in a treeview.
    /// </summary>
    public class MetaLocationRootViewModel : Base.BaseViewModel
    {
        #region fields
        private const DispatcherPriority _ChildrenEditPrio = DispatcherPriority.DataBind;

        protected readonly ObservableCollection<MetaLocationViewModel> _CountryRootItems = null;
        protected readonly IList<MetaLocationViewModel> _BackUpCountryRoots = null;

        private ICommand _ExpandCommand;
        #endregion fields

        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        public MetaLocationRootViewModel()
        {
            _CountryRootItems = new ObservableCollection<MetaLocationViewModel>();
            _BackUpCountryRoots = new List<MetaLocationViewModel>(400);
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
        /// Gets the total count of (backup) rootitems in the tree collectiton.
        /// 
        /// This number can be larger than the nummber of items in the <see cref="CountryRootItems"/>
        /// collection if a filter is currently applied.
        /// </summary>
        public int BackUpCountryRootsCount
        {
            get
            {
                return _BackUpCountryRoots.Count();
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
                    var vmItem = MetaLocationViewModel.GetViewModelFromModel(item);

                    RootsAdd(vmItem, true);
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
        public async Task<int> DoSearchAsync(SearchParams searchParams)
        {
            return await Task.Run<int>(() =>
            {
                return DoSearch(searchParams);
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
        public int DoSearch(SearchParams searchParams)
        {
            IList<MetaLocationViewModel> backUpRoots = _BackUpCountryRoots;
            ObservableCollection<MetaLocationViewModel> root = _CountryRootItems;

            if (searchParams == null)
                searchParams = new SearchParams();

            searchParams.SearchStringTrim();
            searchParams.SearchStringToUpperCase();

            Application.Current.Dispatcher.Invoke(() => { root.Clear(); }, _ChildrenEditPrio);

            // Show all root items if string to search is empty
            if (searchParams.IsSearchStringEmpty == true ||
                searchParams.MinimalSearchStringLength >= searchParams.SearchString.Length)
            {
                foreach (var rootItem in backUpRoots)
                {
                    //PreOrderTraversal(rootItem);  !!! Lazy Loading Children !!!
                    rootItem.ChildrenClear(false);
                    rootItem.SetExpand(false);

                    Application.Current.Dispatcher.Invoke(() => { root.Add(rootItem); }, _ChildrenEditPrio);
                }

                return 0;
            }

            int imatchCount = 0;

            // Go through all root items and process their children
            foreach (var rootItem in backUpRoots)
            {
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
                    Application.Current.Dispatcher.Invoke(() => { root.Add(rootItem); }, _ChildrenEditPrio);
                }
            }

            return imatchCount;
        }

        /// <summary>
        /// Add a rootitem into the root items collection -
        /// always adds into the backup root items collection and
        /// only adds into the observable collection
        /// if <paramref name="addBackupItemOnly"/> is false.
        /// </summary>
        /// <param name="vmItem"></param>
        /// <param name="addBackupItemOnly"></param>
        protected void RootsAdd(MetaLocationViewModel vmItem
                            , bool addBackupItemOnly)
        {
            _BackUpCountryRoots.Add(vmItem);

            if (addBackupItemOnly == false)
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
             MetaLocationViewModel root
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
             MetaLocationViewModel root
           , SearchParams searchParams)
        {
            var toVisit = new Stack<MetaLocationViewModel>();
            var visitedAncestors = new Stack<MetaLocationViewModel>();
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
                        PushReverse(toVisit, node.BackUpNodes);
                        continue;
                    }

                    visitedAncestors.Pop();
                }

                // Process Node and count matches (if any)
                if ((node.Match = node.ProcessNodeMatch(searchParams)) == MatchType.NodeMatch)
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
        private MetaLocationViewModel PeekOrDefault(Stack<MetaLocationViewModel> s)
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
              Stack<MetaLocationViewModel> s
            , IEnumerable<MetaLocationViewModel> list)
        {
            foreach (var l in list.ToArray().Reverse())
            {
                s.Push(l);
            }
        }
        /***
                /// <summary>
                /// Traverse the tree from root to child and add child nodes from underneath
                /// each parent into the bound collection. Children are not really added but
                /// transferred from the BackUpNodes collection into the Children collection.
                /// 
                /// http://urosv.blogspot.de/2011/04/iterative-binary-tree-traversal.html
                /// Preorder is the simplest iterative tree traversal.
                /// 
                /// We start by pushing the tree root to the stack.Then, until the stack is empty,
                /// we repeat the following routine:
                /// - pop the next node and then push its children to the stack.
                /// </summary>
                /// <param name="solutionRoot"></param>
                private static void PreOrderTraversal(MetaLocationViewModel solutionRoot)
                {
                    Stack<MetaLocationViewModel> stack = new Stack<MetaLocationViewModel>();

                    if (solutionRoot != null)
                        stack.Push(solutionRoot);

                    while (stack.Count() > 0)
                    {
                        MetaLocationViewModel current = stack.Pop();

                        //System.Console.WriteLine(string.Format("{0}", current.GetStackPath()));
                        current.ChildrenClear(false);

                        foreach (var item in current.BackUpNodes)
                        {
                            current.ChildrenAdd(item, false);
                            stack.Push(item);
                        }
                    }
                }
        ***/
        #endregion methods
    }
}