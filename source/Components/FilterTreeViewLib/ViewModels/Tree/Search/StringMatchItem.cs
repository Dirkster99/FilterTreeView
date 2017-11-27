namespace FilterTreeViewLib.ViewModels.Tree.Search
{
    using FilterTreeViewLib.Interfaces;

    /// <summary>
    /// Implements a viewmodel that provides a string to display and can indicate
    /// a macthing range against another string via the <see cref="Range"/> property.
    /// </summary>
    public class StringMatchItem : Base.BaseViewModel
    {
        #region fields
        private ISelectionRange _Range;
        private string _DisplayString;
        #endregion fields

        #region ctors
        /// <summary>
        /// Parameterized class constructor.
        /// </summary>
        /// <param name="displayString"></param>
        public StringMatchItem(string displayString)
        {
            DisplayString = displayString;
        }

        /// <summary>
        /// Class constructor.
        /// </summary>
        public StringMatchItem()
        {
            _Range = new SelectionRange();
            _DisplayString = string.Empty;
        }
        #endregion ctors

        #region properties
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
                        _Range.End == value.End)
                        return;

                    _Range = value;
                    NotifyPropertyChanged(() => Range);
                }

                if (_Range == null && value != null ||
                    _Range != null && value == null)
                {
                    _Range = value;
                    NotifyPropertyChanged(() => Range);
                }
            }
        }

        /// <summary>
        /// Gets te string that should be displayed (with or without hightlighting)
        /// </summary>
        public string DisplayString
        {
            get
            {
                return _DisplayString;
            }

            private set
            {
                if (_DisplayString != value)
                {
                    _DisplayString = value;
                    NotifyPropertyChanged(() => DisplayString);
                }
            }
        }
        #endregion properties

        #region methods
        /// <summary>
        /// Evaluates the given string against the <see cref="DisplayString"/>
        /// property and sets the <see cref="Range"/> property to indicate the
        /// matched text range.
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        public bool MatchString(string searchString)
        {
            if (string.IsNullOrEmpty(DisplayString) == true &&
                string.IsNullOrEmpty(searchString) == true)
            {
                Range = new SelectionRange(0, 0);
                return true;
            }
            else
            {
                // There is no point in making this a match
                // if only one of the strings is not present
                if (string.IsNullOrEmpty(DisplayString) == true ||
                    string.IsNullOrEmpty(searchString) == true)
                {
                    Range = new SelectionRange(-1, -1);
                    return false;
                }
            }

            // Do we have a (sub)match or not ???
            int start;
            if ((start = DisplayString.IndexOf(searchString)) >= 0)
            {
                Range = new SelectionRange(start, start + searchString.Length);
                return true;
            }
            else
            {
                Range = new SelectionRange(start, -1);
                return false;
            }
        }
        #endregion methods
    }
}
