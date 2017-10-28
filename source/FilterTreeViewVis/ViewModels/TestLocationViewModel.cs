namespace FilterTreeViewVis.ViewModels
{
    using BusinessLib.Models;
    using FilterTreeViewLib.Interfaces;
    using FilterTreeViewLib.ViewModelsSearch.SearchModels;
    using FilterTreeViewLib.ViewModelsSearch.SearchModels.Enums;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Threading;

    /// <summary>
    /// Implements an items viewmodel that should be bound to an items collection in a tree view.
    /// </summary>
    public class TestLocationViewModel : FilterTreeViewLib.ViewModels.Base.BaseViewModel, IHasDummyChild
    {
        #region fields
        private static DispatcherPriority _ChildrenEditPrio = DispatcherPriority.DataBind;

        private static readonly TestLocationViewModel DummyChild = new TestLocationViewModel();

        private bool _IsItemVisible;
        private bool _IsItemExpanded;

        private readonly ObservableCollection<TestLocationViewModel> _Children = null;
        private MatchType _Match;
        private string _LocalName;
        #endregion fields

        #region constructors
        /// <summary>
        /// Parameterized Class Constructor
        /// </summary>
        public TestLocationViewModel(
              BusinessLib.Models.MetaLocationModel locationModel
            , TestLocationViewModel parent
            )
            : this()
        {
            Parent = parent;

            _LocalName = locationModel.LocalName;
            ID = locationModel.ID;
            Latitude = locationModel.Geo_lat;
            Longitude = locationModel.Geo_lng;
            TypeOfLocation = locationModel.Type;

            ChildrenClear();  // Lazy Load Children !!!
        }

        /// <summary>
        /// Class Constructor
        /// </summary>
        protected TestLocationViewModel()
        {
            _IsItemVisible = true;
            _IsItemExpanded = false;

            _Children = new ObservableCollection<TestLocationViewModel>();

            _Match = MatchType.NoMatch;
            Parent = null;
        }
        #endregion constructors

        #region properties
        public IEnumerable<TestLocationViewModel> Children
        {
            get
            {
                return _Children;
            }
        }

        public TestLocationViewModel Parent { get; set; }

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

        public int ChildrenCount => _Children.Count;

        public virtual bool HasDummyChild
        {
            get
            {
                if (_Children != null)
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
        public static TestLocationViewModel GetViewModelFromModel(MetaLocationModel srcRoot)
        {
            if (srcRoot == null)
                return null;

            TestLocationViewModel dstRoot = new TestLocationViewModel(srcRoot, null);

            Queue<MetaLocationModel> srcQueue = new Queue<MetaLocationModel>();
            Queue<TestLocationViewModel> dstQueue = new Queue<TestLocationViewModel>();

            srcQueue.Enqueue(srcRoot);
            dstQueue.Enqueue(dstRoot);

            while (srcQueue.Count() > 0)
            {
                MetaLocationModel srcCurrent = srcQueue.Dequeue();
                TestLocationViewModel dstCurrent = dstQueue.Dequeue();

                ////Console.WriteLine(string.Format("{0,4} - {1}"
                ////                  , iLevel, current.GetPath()));

                foreach (var item in srcCurrent.Children)
                {
                    var dstVM = new TestLocationViewModel(item, dstCurrent);

                    dstCurrent.ChildrenAdd(dstVM);

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
        /// Re-load treeview items below item (eg. when item is expanded).
        /// </summary>
        /// <returns>Number of items found below the computer root item.</returns>
        public int LoadChildren()
        {
            if (HasDummyChild == true)
                ChildrenClear(false);

            if (_Children.Count() > 0)
            {
                foreach (var item in _Children)
                    item.IsItemVisible = true;
            }

            return _Children.Count;
        }

        public void ChildrenAdd(TestLocationViewModel child)
        {
            if (HasDummyChild == true)
            {
                Application.Current.Dispatcher.Invoke(() => { _Children.Clear(); }, _ChildrenEditPrio);
            }

            Application.Current.Dispatcher.Invoke(() => { _Children.Add(child); }, _ChildrenEditPrio);
        }

        public void ChildrenRemove(TestLocationViewModel child)
        {
            Application.Current.Dispatcher.Invoke(() => { _Children.Remove(child); }, _ChildrenEditPrio);
        }

        public void ChildrenClear(bool bAddDummyChild = true)
        {
            Application.Current.Dispatcher.Invoke(() => { _Children.Clear(); }, _ChildrenEditPrio);

            // Cities do not have children so we need no dummy child here
            if (bAddDummyChild == true && TypeOfLocation != LocationType.City)
            {
                Application.Current.Dispatcher.Invoke(() => { _Children.Add(DummyChild); }, _ChildrenEditPrio);
            }
        }

        /// <summary>
        /// Returns the string path either:
        /// 1) for the <paramref name="current"/> item or
        /// 2) for this item (if optional parameter <paramref name="current"/> is not set).
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public string GetStackPath(TestLocationViewModel current = null)
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
        public MatchType ProcessNodeMatch(SearchParams searchParams)
        {
            MatchType matchThisNode = MatchType.NoMatch;

            // Determine whether this node is a match or not
            if (searchParams.MatchSearchString(LocalName) == true)
                matchThisNode = MatchType.NodeMatch;

            //ChildrenClear(false);

            if (ChildrenCount > 0)
            {
                // Evaluate children by adding only thos children that contain no 'NoMatch'
                MatchType maxChildMatch = MatchType.NoMatch;
                foreach (var item in _Children)
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

                        item.IsItemVisible = true;
                        //ChildrenAdd(item);
                    }
                    else
                        item.IsItemVisible = false;
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
