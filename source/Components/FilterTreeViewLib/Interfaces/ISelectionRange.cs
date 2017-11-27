namespace FilterTreeViewLib.Interfaces
{
    using System;
    using System.Windows.Media;

    /// <summary>
    /// Defines a range that can be used to indicate
    /// the start and end of a text selection or any other kind of range.
    /// </summary>
    public interface ISelectionRange : ICloneable
    {
        /// <summary>
        /// Gets the start of the indicated range.
        /// </summary>
        int Start { get; }

        /// <summary>
        /// Gets the end of the indicated range.
        /// </summary>
        int End { get; }

        /// <summary>
        /// Gets a bool value to determine whether DarkSkin default
        /// value for <see cref="SelectionBackground"/> property should
        /// be applied or not.
        /// </summary>
        bool DarkSkin { get; }

        /// <summary>
        /// Gets the background color that is applied to the background brush,
        /// which should be applied when no match is indicated
        /// (this can be default(Color) in which case standard selection Brush
        /// is applied).
        /// 
        /// Note:
        /// Standard selection background color on light skin: 208, 247, 255
        /// Standard selection background color on dark  skin: 254, 252, 200
        /// </summary>
        Color SelectionBackground { get; }

        /// <summary>
        /// Gets the background color that is applied to the background brush.
        /// which should be applied when no match is indicated
        /// (this can be default(Color) in which case Transparent is applied).
        /// </summary>
        Color NormalBackground { get; }
    }
}
