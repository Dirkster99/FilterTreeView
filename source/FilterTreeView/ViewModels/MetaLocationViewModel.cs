namespace FilterTreeView.ViewModels
{
    using BusinessLib.Models;
    using FilterTreeView.SearchModels;
    using FilterTreeView.SearchModels.Enums;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    internal class MetaLocationViewModel : Base.BaseViewModel
    {
        #region fields
        private static readonly MetaLocationViewModel DummyChild = new MetaLocationViewModel();

        private readonly BusinessLib.Models.MetaLocationModel _LocationModel = null;
        private bool _IsItemVisible;
        private bool _IsItemExpanded;
        private List<MetaLocationViewModel> _BackUpNodes = null;

        private readonly ObservableCollection<MetaLocationViewModel> _Children = null;
        private MatchType _Match;
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

            // Traverse down the model tree to create viewmodels with
            // the same parent - child relationship structure
            _BackUpNodes = new List<MetaLocationViewModel>(
                            (from child in locationModel.Children
                             select new MetaLocationViewModel(child, this))
                            .ToList());

            _LocationModel = locationModel;
        }

        /// <summary>
        /// Class Constructor
        /// </summary>
        public MetaLocationViewModel()
        {
            _IsItemVisible = true;
            _IsItemExpanded = false;

            _Children = new ObservableCollection<MetaLocationViewModel>();
            _BackUpNodes = new List<MetaLocationViewModel>();

            _Match = MatchType.NoMatch;
            Parent = null;

            ChildrenClear(false);  // Lazy Load Children !!!
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
                if (_LocationModel == null)
                    return string.Empty;

                return _LocationModel.LocalName;
            }
            set
            {
                if (_LocationModel == null)
                    return;

                if (_LocationModel.LocalName != value)
                {
                    _LocationModel.LocalName = value;
                    NotifyPropertyChanged(() => LocalName);
                }
            }
        }

        public int ID
        {
            get
            {
                if (_LocationModel == null)
                    return -1;

                return _LocationModel.ID;
            }
        }

        public double Latitude
        {
            get
            {
                if (_LocationModel == null)
                    return -1;

                return _LocationModel.Geo_lat;
            }
        }

        public double Longitude
        {
            get
            {
                if (_LocationModel == null)
                    return -1;

                return _LocationModel.Geo_lat;
            }
        }

        /// <summary>
        /// Gets the type of location of this object.
        /// </summary>
        public LocationType TypeOfLocation
        {
            get
            {
                if (_LocationModel == null)
                    return LocationType.Unknown;

                return _LocationModel.Type;
            }
        }

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
        #endregion properties

        #region methods
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
                _Children.Clear();

            _Children.Add(child);

            if (bAddBackup == true)
                _BackUpNodes.Add(child);
        }

        public void ChildrenRemove(MetaLocationViewModel child, bool bRemoveBackup = true)
        {
            _Children.Remove(child);

            if (bRemoveBackup == true)
                _BackUpNodes.Remove(child);
        }

        public void ChildrenClear(bool bClearBackup = true
                                , bool bAddDummyChild = true)
        {
            _Children.Clear();

            if (bAddDummyChild == true)
                _Children.Add(DummyChild);

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
            if (_LocationModel != null)
            {
                while (current != null)
                {
                    result = "/" + _LocationModel.LocalName + result;

                    current = current.Parent;
                }
            }
            else
            {
                result = "???";

                if (this.Equals(MetaLocationViewModel.DummyChild) == true)
                    result = "DummyChild";
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
