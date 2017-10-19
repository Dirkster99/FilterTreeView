namespace FilterTreeViewLib.ViewModelsSearch.SearchModels.Enums
{
    /// <summary>
    /// Determines whether a givem node has a match with regard to search parameters
    /// - search string and
    /// - additional search options (eg.: string contained, exact match)
    /// 
    /// This includes a reference to child items that might contain a
    /// match while the node itself may only be the parent of a child
    /// with an actual match.
    /// </summary>
    public enum MatchType
    {
        /// <summary>
        /// The node (and its children) contains no match.
        /// </summary>
        NoMatch = 0,

        /// <summary>
        /// The node was macth against the search parameters.
        /// </summary>
        NodeMatch = 1,

        /// <summary>
        /// A child node or children nodes of this node contain a match
        /// BUT this node does not contain a match.
        /// </summary>
        SubNodeMatch = 2,

        /// <summary>
        /// A child node or children nodes of this node contain a match
        /// AND this node does also contain a match.
        /// </summary>
        Node_AND_SubNodeMatch = 4
    }
}
