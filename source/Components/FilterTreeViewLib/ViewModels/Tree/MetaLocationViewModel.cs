namespace FilterTreeViewLib.ViewModels
{
    using BusinessLib.Models;
    using FilterTreeViewLib.Interfaces;
    using FilterTreeViewLib.ViewModels.Tree.Search;
    using FilterTreeViewLib.ViewModelsSearch.SearchModels;
    using FilterTreeViewLib.ViewModelsSearch.SearchModels.Enums;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Threading;

    /// <summary>
    /// Implements an items viewmodel that should be bound to an items collection in a tree view.
    /// </summary>
    public class MetaLocationViewModel : Base.BaseViewModel, IHasDummyChild
    {
        #region fields
        private static DispatcherPriority _ChildrenEditPrio = DispatcherPriority.Render;

        private static readonly MetaLocationViewModel DummyChild = new MetaLocationViewModel();

        private bool _IsItemExpanded;
        private List<MetaLocationViewModel> _BackUpNodes = null;

        private readonly ObservableCollection<MetaLocationViewModel> _Children = null;
        private MatchType _Match;
        private string _LocalName;
        private ISelectionRange _Range = null;
        #endregion fields

        #region constructors
        /// <summary>
        /// Parameterized Class Constructor
        /// </summary>
        public MetaLocationViewModel(
              BusinessLib.Models.MetaLocationModel locationModel
            , MetaLocationViewModel parent
            )
            : this()
        {
            Parent = parent;

            _LocalName = locationModel.LocalName;
            ID = locationModel.ID;
            Latitude = locationModel.Geo_lat;
            Longitude = locationModel.Geo_lng;
            TypeOfLocation = locationModel.Type;

            ChildrenClear(false);  // Lazy Load Children !!!
        }

        /// <summary>
        /// Class Constructor
        /// </summary>
        protected MetaLocationViewModel()
        {
            ////            _IsItemVisible = true;
            _IsItemExpanded = false;

            _Children = new ObservableCollection<MetaLocationViewModel>();
            _BackUpNodes = new List<MetaLocationViewModel>();

            _Match = MatchType.NoMatch;
            Parent = null;
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// Gets the <seealso cref="MetaLocationModel"/> Parent (if any) of this item.
        /// </summary>
        public MetaLocationViewModel Parent { get; private set; }

        /// <summary>
        /// Gets the <see cref="MatchType"/> for this item. This field
        /// determines the classification of a match towards a current
        /// search criteria.
        /// </summary>
        public MatchType Match
        {
            get { return _Match; }

            private set
            {
                if (_Match != value)
                {
                    _Match = value;
                    NotifyPropertyChanged(() => Match);
                }
            }
        }

        /// <summary>
        /// Gets the range property that indicates the text range
        /// to be considered a match against another (search) string.
        /// </summary>
        public ISelectionRange Range
        {
            get
            {
                return _Range;
            }

            private set
            {
                if (_Range != null && value != null)
                {
                    // Nothing changed - so we change nothing here :-)
                    if (_Range.Start == value.Start &&
                        _Range.End == value.End &&
                        _Range.SelectionBackground == value.SelectionBackground &&
                        _Range.SelectionBackground == value.SelectionBackground)
                        return;

                    _Range = (ISelectionRange)value.Clone();
                    NotifyPropertyChanged(() => Range);
                }

                if (_Range == null && value != null ||
                    _Range != null && value == null)
                {
                    _Range = (ISelectionRange)value.Clone();
                    NotifyPropertyChanged(() => Range);
                }
            }
        }

        /// <summary>
        /// Gets/sets whether this item is expanded in
        /// the tree view (items under this item are visible), or not.
        /// </summary>
        public bool IsItemExpanded
        {
            get { return _IsItemExpanded; }
            set
            {
                if (_IsItemExpanded != value)
                {
                    _IsItemExpanded = value;
                    NotifyPropertyChanged(() => IsItemExpanded);
                }
            }
        }

        /// <summary>
        /// Gets the name of this item (name of country, region, or city)
        /// </summary>
        public string LocalName
        {
            get
            {
                return _LocalName;
            }

            private set
            {
                if (_LocalName != value)
                {
                    _LocalName = value;
                    NotifyPropertyChanged(() => LocalName);
                }
            }
        }

        /// <summary>
        /// Gets the technical ID (for data reference) of this item.
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// Gets the Geographical Latitude (if any) of this item.
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        /// Gets the Geographical Longitude (if any) of this item.
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        /// Gets the type of location of this object.
        /// </summary>
        public LocationType TypeOfLocation { get; }

        /// <summary>
        /// Gets the COMPLETE collection of child items under this item (complete sub-tree).
        /// This collection is always complete, maintained, and always available list of
        /// items under this item.
        /// 
        /// The <see cref="Children"/> collection contains child items (sub-tree) matched
        /// to the current search criteria (if any).
        /// </summary>
        public IEnumerable<MetaLocationViewModel> BackUpNodes
        {
            get
            {
                return _BackUpNodes;
            }
        }

        /// <summary>
        /// Gets all child items (nodes) under this item. This collection (sub-tree)
        /// may hold no items, if this item is filtered and none of the items in the
        /// <see cref="Children"/> collection matched the current search criteria.
        /// 
        /// The <see cref="BackUpNodes"/> collection contains the complete, maintained,
        /// and always available list of items under this item.
        /// </summary>
        public IEnumerable<MetaLocationViewModel> Children
        {
            get
            {
                return _Children;
            }
        }

        /// <summary>
        /// Returns the total number of child items (nodes) under this item.
        /// </summary>
        public int ChildrenCount => _BackUpNodes.Count;

        /// <summary>
        /// Determines whether this item has a dummy child below or not.
        /// </summary>
        public virtual bool HasDummyChild
        {
            get
            {
                if (this.Children != null)
                {
                    if (this._Children.Count == 1)
                    {
                        if (this._Children[0] == DummyChild)
                            return true;
                    }
                }

                return false;
            }
        }
        #endregion properties

        #region methods
        /// <summary>
        /// Returns the string path either:
        /// 1) for the <paramref name="current"/> item or
        /// 2) for this item (if optional parameter <paramref name="current"/> is not set).
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public string GetStackPath(MetaLocationViewModel current = null)
        {
            if (current == null)
                current = this;

            string result = string.Empty;

            // Traverse the list of parents backwards and
            // add each child to the path
            while (current != null)
            {
                result = "/" + LocalName + result;

                current = current.Parent;
            }

            return result;
        }

        /// <summary>
        /// The TREELIB (see FilterTreeViewLib (Project) > Properties > Conditional Compilation Symbol)
        /// switch determines whether the TreeLib LevelOrder traversal function is used or not.
        /// The point for training is here:
        /// 
        /// Both functions are equivalent but using the TreeLib traversal function should simplify
        /// the problem of efficiently traversing any tree in the given order.
        /// </summary>
#if TREELIB
        ///<summary>
        /// Convert a Model into a ViewModel using
        /// a LevelOrderTraversal Algorithm via TreeLib library.
        ///</summary>
        internal static MetaLocationViewModel GetViewModelFromModel(MetaLocationModel srcRoot)
        {
            if (srcRoot == null)
                return null;

            var srcItems = TreeLib.BreadthFirst.Traverse.LevelOrder(srcRoot, i => i.Children);
            var dstIdItems = new Dictionary<int, MetaLocationViewModel>();
            MetaLocationViewModel dstRoot = null;

            foreach (var node in srcItems.Select(i => i.Node))
            {
                if (node.Parent == null)
                {
                    dstRoot = new MetaLocationViewModel(node, null);
                    dstIdItems.Add(dstRoot.ID, dstRoot);
                }
                else
                {
                    MetaLocationViewModel vmParentItem;     // Find parent ViewModel for Model
                    dstIdItems.TryGetValue(node.Parent.ID, out vmParentItem);

                    var dstNode = new MetaLocationViewModel(node, vmParentItem);
                    vmParentItem.ChildrenAdd(dstNode);     // Insert converted ViewModel below ViewModel parent
                    dstIdItems.Add(dstNode.ID, dstNode);
                }
            }

            dstIdItems.Clear(); // Destroy temp ID look-up structure

            return dstRoot;
        }
#else
        /// <summary>
        /// Convert a Model into a ViewModel using
        /// a LevelOrderTraversal Algorithm
        /// </summary>
        /// <param name="srcRoot"></param>
        internal static MetaLocationViewModel GetViewModelFromModel(MetaLocationModel srcRoot)
        {
            if (srcRoot == null)
                return null;

            MetaLocationViewModel dstRoot = new MetaLocationViewModel(srcRoot, null);

            Queue<MetaLocationModel> srcQueue = new Queue<MetaLocationModel>();
            Queue<MetaLocationViewModel> dstQueue = new Queue<MetaLocationViewModel>();

            srcQueue.Enqueue(srcRoot);
            dstQueue.Enqueue(dstRoot);

            while (srcQueue.Count() > 0)
            {
                MetaLocationModel srcCurrent = srcQueue.Dequeue();
                MetaLocationViewModel dstCurrent = dstQueue.Dequeue();

                ////Console.WriteLine(string.Format("{0,4} - {1}"
                ////                  , iLevel, current.GetPath()));

                foreach (var item in srcCurrent.Children)
                {
                    var dstVM = new MetaLocationViewModel(item, dstCurrent);

                    dstCurrent.ChildrenAddBackupNodes(dstVM);

                    srcQueue.Enqueue(item);
                    dstQueue.Enqueue(dstVM);
                }
            }

            return dstRoot;
        }
#endif

        /// <summary>
        /// Re-load treeview items below the root item.
        /// </summary>
        /// <returns>Number of items found below the root item.</returns>
        internal int LoadChildren()
        {
            ChildrenClear(false, false); // Clear collection of children

            if (_BackUpNodes.Count() > 0)
            {
                foreach (var item in _BackUpNodes)
                    this.ChildrenAdd(item, false);
            }

            return _Children.Count;
        }

        internal void ChildrenClear(bool bClearBackup = true
                                , bool bAddDummyChild = true)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { _Children.Clear(); }, _ChildrenEditPrio);

                // Cities do not have children so we need no dummy child here
                if (bAddDummyChild == true && TypeOfLocation != LocationType.City)
                {
                    Application.Current.Dispatcher.Invoke(() => { _Children.Add(DummyChild); }, _ChildrenEditPrio);
                }

                if (bClearBackup == true)
                    _BackUpNodes.Clear();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Determines the <seealso cref="MatchType"/> of a node by evaluating the
        /// given search string parameter and the matchtype of its children.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="filterString"></param>
        internal MatchType ProcessNodeMatch(SearchParams searchParams, out int MatchStart)
        {
            MatchStart = -1;
            MatchType matchThisNode = MatchType.NoMatch;

            // Determine whether this node is a match or not
            if ((MatchStart = searchParams.MatchSearchString(LocalName)) >= 0)
                matchThisNode = MatchType.NodeMatch;

            ChildrenClear(false);

            if (ChildrenCount > 0)
            {
                // Evaluate children by adding only thos children that contain no 'NoMatch'
                MatchType maxChildMatch = MatchType.NoMatch;
                foreach (var item in BackUpNodes)
                {
                    if (item.Match != MatchType.NoMatch)
                    {
                        // Expand this item if it (or one of its children) contains a match
                        if (item.Match == MatchType.SubNodeMatch ||
                            item.Match == MatchType.Node_AND_SubNodeMatch)
                        {
                            item.SetExpand(true);
                        }
                        else
                            item.SetExpand(false);

                        if (maxChildMatch < item.Match)
                            maxChildMatch = item.Match;

                        ChildrenAdd(item, false);
                    }
                }

                if (matchThisNode == MatchType.NoMatch && maxChildMatch != MatchType.NoMatch)
                    matchThisNode = MatchType.SubNodeMatch;

                if (matchThisNode == MatchType.NodeMatch && maxChildMatch != MatchType.NoMatch)
                    matchThisNode = MatchType.Node_AND_SubNodeMatch;
            }

            return matchThisNode;
        }

        /// <summary>
        /// Sets the tree view item to the corresponding expanded state
        /// as indicated in the <seealso cref="IsExpanded"/> property.
        /// </summary>
        /// <param name="isExpanded"></param>
        internal void SetExpand(bool isExpanded)
        {
            IsItemExpanded = isExpanded;
        }

        /// <summary>
        /// Sets the type of match detmerined for this item against a certain
        /// match criteria.
        /// 
        /// We use this method instead of a setter to make this accessible for
        /// the root viewmodel but next to invisible for everyone else...
        /// </summary>
        /// <param name="match"></param>
        /// <param name="matchStart"></param>
        internal MatchType SetMatch(MatchType match,
                                    int matchStart = -1,
                                    int matchEnd = -1)
        {
            this.Match = match;
            this.Range = new SelectionRange(matchStart, matchEnd);

            return match;
        }

        /// <summary>
        /// Add a child item including a reference to a backupnode
        /// (add to backupnode is determined by <paramref name="bAddBackup"/>).
        /// </summary>
        /// <param name="child"></param>
        /// <param name="bAddBackup"></param>
        private void ChildrenAdd(MetaLocationViewModel child, bool bAddBackup = true)
        {
            try
            {
                if (HasDummyChild == true)
                {
                    Application.Current.Dispatcher.Invoke(() => { _Children.Clear(); }, _ChildrenEditPrio);
                }

                Application.Current.Dispatcher.Invoke(() => { _Children.Add(child); }, _ChildrenEditPrio);

                if (bAddBackup == true)
                    _BackUpNodes.Add(child);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Add a child node to a backupnode only.
        /// </summary>
        /// <param name="child"></param>
        private void ChildrenAddBackupNodes(MetaLocationViewModel child)
        {
            _BackUpNodes.Add(child);
        }

        private void ChildrenRemove(MetaLocationViewModel child, bool bRemoveBackup = true)
        {
            Application.Current.Dispatcher.Invoke(() => { _Children.Remove(child); }, _ChildrenEditPrio);

            if (bRemoveBackup == true)
                _BackUpNodes.Remove(child);
        }
#endregion methods
    }
}
