namespace FilterTreeViewLib.ViewModels
{
    using BusinessLib.Models;
    using FilterTreeViewLib.SearchModels;
    using FilterTreeViewLib.SearchModels.Enums;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Threading;

    public class MetaLocationViewModel : Base.BaseViewModel
    {
        #region fields
        private static DispatcherPriority _ChildrenEditPrio = DispatcherPriority.DataBind;

        private static readonly MetaLocationViewModel DummyChild = new MetaLocationViewModel();

        private bool _IsItemVisible;
        private bool _IsItemExpanded;
        private List<MetaLocationViewModel> _BackUpNodes = null;

        private readonly ObservableCollection<MetaLocationViewModel> _Children = null;
        private MatchType _Match;
        private string _LocalName;
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
            _IsItemVisible = true;
            _IsItemExpanded = false;

            _Children = new ObservableCollection<MetaLocationViewModel>();
            _BackUpNodes = new List<MetaLocationViewModel>();

            _Match = MatchType.NoMatch;
            Parent = null;
        }
        #endregion constructors

        #region properties
        public MetaLocationViewModel Parent { get; set; }

        public MatchType Match
        {
            get { return _Match; }
            set
            {
                if (_Match != value)
                {
                    _Match = value;
                    NotifyPropertyChanged(() => Match);
                }
            }
        }

        public bool IsItemVisible
        {
            get { return _IsItemVisible; }
            set
            {
                if (_IsItemVisible != value)
                {
                    _IsItemVisible = value;
                    NotifyPropertyChanged(() => IsItemVisible);
                }
            }
        }

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

        public string LocalName
        {
            get
            {
                return _LocalName;
            }
            set
            {
                if (_LocalName != value)
                {
                    _LocalName = value;
                    NotifyPropertyChanged(() => LocalName);
                }
            }
        }

        public int ID { get; }

        public double Latitude { get; }

        public double Longitude { get; }

        /// <summary>
        /// Gets the type of location of this object.
        /// </summary>
        public LocationType TypeOfLocation { get; }

        public IEnumerable<MetaLocationViewModel> BackUpNodes
        {
            get
            {
                return _BackUpNodes;
            }
        }

        public IEnumerable<MetaLocationViewModel> Children
        {
            get
            {
                return _Children;
            }
        }

        public int ChildrenCount => _BackUpNodes.Count;

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
        /// Convert a Model into a ViewModel using
        /// a LevelOrderTraversal Algorithm
        /// </summary>
        /// <param name="srcRoot"></param>
        public static MetaLocationViewModel GetViewModelFromModel(MetaLocationModel srcRoot)
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

        /// <summary>
        /// Sets the tree view item to the corresponding expanded state
        /// as indicated in the <seealso cref="IsExpanded"/> property.
        /// </summary>
        /// <param name="isExpanded"></param>
        public void SetExpand(bool isExpanded)
        {
            IsItemExpanded = isExpanded;
        }

        /// <summary>
        /// Re-load treeview items below the computer root item.
        /// </summary>
        /// <returns>Number of items found below the computer root item.</returns>
        public int LoadChildren()
        {
            ChildrenClear(false, false); // Clear collection of children

            if (_BackUpNodes.Count() > 0)
            {
                foreach (var item in _BackUpNodes)
                    this.ChildrenAdd(item, false);
            }

            return _Children.Count;
        }

        public void ChildrenAdd(MetaLocationViewModel child, bool bAddBackup = true)
        {
            if (HasDummyChild == true)
            {
                Application.Current.Dispatcher.Invoke(() => { _Children.Clear(); }, _ChildrenEditPrio);
            }

            Application.Current.Dispatcher.Invoke(() => { _Children.Add(child); }, _ChildrenEditPrio);

            if (bAddBackup == true)
                _BackUpNodes.Add(child);
        }

        public void ChildrenAddBackupNodes(MetaLocationViewModel child)
        {
            _BackUpNodes.Add(child);
        }

        public void ChildrenRemove(MetaLocationViewModel child, bool bRemoveBackup = true)
        {
            Application.Current.Dispatcher.Invoke(() => { _Children.Remove(child); }, _ChildrenEditPrio);

            if (bRemoveBackup == true)
                _BackUpNodes.Remove(child);
        }

        public void ChildrenClear(bool bClearBackup = true
                                , bool bAddDummyChild = true)
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
        /// Determines the <seealso cref="MatchType"/> of a node by evaluating the
        /// given search string parameter and the matchtype of its children.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="filterString"></param>
        internal MatchType ProcessNodeMatch(SearchParams searchParams)
        {
            MatchType matchThisNode = MatchType.NoMatch;

            // Determine whether this node is a match or not
            if (searchParams.MatchSearchString(LocalName) == true)
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
        #endregion methods
    }
}
