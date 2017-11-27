namespace FilterTreeViewLib.Behaviors
{
    using FilterTreeViewLib.Interfaces;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System;

    /// <summary>
    /// Implements a <see cref="TextBlock"/> behavior to highlight text as specified by
    /// a bound property that adheres to the <see cref="ISelectionRange"/> interface.
    /// </summary>
    public static class HighlightTextBlockBehavior
    {
        #region fields
        /// <summary>
        /// Back Store of Attachable Dependency Property that indicates the range
        /// of text that should be highlighting (if any).
        /// </summary>
        private static readonly DependencyProperty RangeProperty =
            DependencyProperty.RegisterAttached("Range",
                typeof(ISelectionRange),
                typeof(HighlightTextBlockBehavior), new PropertyMetadata(null, OnRangeChanged));
        #endregion fields

        #region methods
        /// <summary>
        /// Gets the current values of the Range dependency property.
        /// </summary>
        public static ISelectionRange GetRange(DependencyObject obj)
        {
            return (ISelectionRange)obj.GetValue(RangeProperty);
        }

        /// <summary>
        /// Gets the current values of the Range dependency property.
        /// </summary>
        public static void SetRange(DependencyObject obj, ISelectionRange value)
        {
            obj.SetValue(RangeProperty, value);
        }

        /// <summary>
        /// Method executes whenever the Range dependency property valua has changed
        /// (in the bound viewmodel).
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBlock txtblock = d as TextBlock;

            if (txtblock == null)
                return;

            var range = GetRange(d);  // Get the bound Range value to do highlighting

            // Standard background is transparent
            SolidColorBrush normalBackGround = new SolidColorBrush(Color.FromArgb(00, 00, 00, 00));
            if (range != null)
            {
                if (range.NormalBackground != default(Color))
                    normalBackGround = new SolidColorBrush(range.NormalBackground);
            }

            // Reset Highlighting - this must be done anyways since
            // multiple selection runs will overlay each other
            var txtrange = new TextRange(txtblock.ContentStart, txtblock.ContentEnd);
            txtrange.ApplyPropertyValue(TextElement.BackgroundProperty, normalBackGround);

            if (range == null)
                return;

            if (range.Start < 0 || range.End < 0) // Nothing to highlight here :-(
                return;

            try
            {
                // Standard selection background color on dark  skin: 254, 252, 200
                // Standard selection background color on light skin: 208, 247, 255
                Color selColor = (range.DarkSkin ? Color.FromArgb(255, 254, 252, 200) :
                                                   Color.FromArgb(255, 208, 247, 255));

                Brush selectionBackground = new SolidColorBrush(selColor);
                if (range != null)
                {
                    if (range.SelectionBackground != default(Color))
                        selectionBackground = new SolidColorBrush(range.SelectionBackground);
                }

                TextRange txtrangel = new TextRange(
                        txtblock.ContentStart.GetPositionAtOffset(range.Start + 1)
                      , txtblock.ContentStart.GetPositionAtOffset(range.End + 1));

                txtrangel.ApplyPropertyValue(TextElement.BackgroundProperty, selectionBackground);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                Console.WriteLine(exc.StackTrace);
            }
        }
        #endregion methods
    }
}
