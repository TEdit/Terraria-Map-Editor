// This is a modified verion of the AutoGrid from WPF Contrib on codeproject
// http://wpfcontrib.codeplex.com/
// Licensed under Microsoft Permissive License (Ms-PL) v1.1
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
// WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.


using System.Windows;
using System.Windows.Controls;
using TEdit.UI.Xaml.Utility;

namespace TEdit.UI.Xaml
{
    /// <summary>
    /// Defines a flexible grid area that consists of columns and rows.
    /// Depending on the orientation, either the rows or the columns are auto-generated,
    /// and the children's position is set according to their index.
    /// </summary>
    public class AutoGrid : Grid
    {
        #region Fields

        /// <summary>
        /// A value of <c>true</c> forces children to be re-indexed at the next oportunity.
        /// </summary>
        private bool _shouldReindex = true;

        private int _rowOrColumnCount;

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Gets or sets a value indicating whether the children are automatically indexed.
        /// <remarks>
        /// The default is <c>true</c>.
        /// Note that if children are already indexed, setting this property to <c>false</c> will not remove their indices.
        /// </remarks>
        /// </summary>
        public bool IsAutoIndexing
        {
            get { return (bool)GetValue(IsAutoIndexingProperty); }
            set { SetValue(IsAutoIndexingProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="IsAutoIndexing"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsAutoIndexingProperty =
            DependencyProperty.Register("IsAutoIndexing", typeof(bool), typeof(AutoGrid), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the orientation.
        /// <remarks>The default is Vertical.</remarks>
        /// </summary>
        /// <value>The orientation.</value>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(AutoGrid), new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPropertyChanged));


        /// <summary>
        /// Gets or sets the child margin.
        /// </summary>
        /// <value>The child margin.</value>
        public Thickness? ChildMargin
        {
            get { return (Thickness?)GetValue(ChildMarginProperty); }
            set { SetValue(ChildMarginProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ChildMargin"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ChildMarginProperty =
            DependencyProperty.Register("ChildMargin", typeof(Thickness?), typeof(AutoGrid), new FrameworkPropertyMetadata((Thickness?)null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPropertyChanged));


        /// <summary>
        /// Gets or sets the child horizontal alignment.
        /// </summary>
        /// <value>The child horizontal alignment.</value>
        public HorizontalAlignment? ChildHorizontalAlignment
        {
            get { return (HorizontalAlignment?)GetValue(ChildHorizontalAlignmentProperty); }
            set { SetValue(ChildHorizontalAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ChildHorizontalAlignment"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ChildHorizontalAlignmentProperty =
            DependencyProperty.Register("ChildHorizontalAlignment", typeof(HorizontalAlignment?), typeof(AutoGrid), new FrameworkPropertyMetadata((HorizontalAlignment?)null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPropertyChanged));


        /// <summary>
        /// Gets or sets the child vertical alignment.
        /// </summary>
        /// <value>The child vertical alignment.</value>
        public VerticalAlignment? ChildVerticalAlignment
        {
            get { return (VerticalAlignment?)GetValue(ChildVerticalAlignmentProperty); }
            set { SetValue(ChildVerticalAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ChildVerticalAlignment"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ChildVerticalAlignmentProperty =
            DependencyProperty.Register("ChildVerticalAlignment", typeof(VerticalAlignment?), typeof(AutoGrid), new FrameworkPropertyMetadata((VerticalAlignment?)null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPropertyChanged));


        private static void OnPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((AutoGrid)o)._shouldReindex = true;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Measures the children of a <see cref="T:System.Windows.Controls.Grid"/> in anticipation of arranging them during the <see cref="M:ArrangeOverride"/> pass.
        /// </summary>
        /// <param name="constraint">Indicates an upper limit size that should not be exceeded.</param>
        /// <returns>
        /// 	<see cref="Size"/> that represents the required size to arrange child content.
        /// </returns>
        protected override Size MeasureOverride(Size constraint)
        {
            bool isVertical = Orientation == Orientation.Vertical;

            if (_shouldReindex || (IsAutoIndexing &&
                ((isVertical && _rowOrColumnCount != ColumnDefinitions.Count) ||
                (!isVertical && _rowOrColumnCount != RowDefinitions.Count))))
            {
                _shouldReindex = false;

                if (IsAutoIndexing)
                {
                    _rowOrColumnCount = (isVertical) ? ColumnDefinitions.Count : RowDefinitions.Count;
                    if (_rowOrColumnCount == 0) _rowOrColumnCount = 1;

                    int cellCount = 0;
                    foreach (UIElement child in Children)
                    {
                        cellCount += (isVertical) ? GetColumnSpan(child) : GetRowSpan(child);
                    }

                    //  Update the number of rows/columns
                    if (isVertical)
                    {
                        int newRowCount = ((cellCount - 1) / _rowOrColumnCount + 1);
                        while (RowDefinitions.Count < newRowCount)
                        {
                            RowDefinitions.Add(new RowDefinition());
                        }
                        if (RowDefinitions.Count > newRowCount)
                        {
                            RowDefinitions.RemoveRange(newRowCount, RowDefinitions.Count - newRowCount);
                        }
                    }
                    else // horizontal
                    {
                        int newColumnCount = ((cellCount - 1) / _rowOrColumnCount + 1);
                        while (ColumnDefinitions.Count < newColumnCount)
                        {
                            ColumnDefinitions.Add(new ColumnDefinition());
                        }
                        if (ColumnDefinitions.Count > newColumnCount)
                        {
                            ColumnDefinitions.RemoveRange(newColumnCount, ColumnDefinitions.Count - newColumnCount);
                        }
                    }
                }

                //  Update children indices
                int position = 0;
                foreach (UIElement child in Children)
                {
                    if (IsAutoIndexing)
                    {
                        if (isVertical)
                        {
                            SetRow(child, position / _rowOrColumnCount);
                            SetColumn(child, position % _rowOrColumnCount);
                            position += GetColumnSpan(child);
                        }
                        else
                        {
                            SetRow(child, position % _rowOrColumnCount);
                            SetColumn(child, position / _rowOrColumnCount);
                            position += GetRowSpan(child);
                        }
                    }

                    // Set margin and alignment
                    if (ChildMargin != null)
                    {
                        DependencyHelpers.SetIfDefault(child, MarginProperty, ChildMargin.Value);
                    }
                    if (ChildHorizontalAlignment != null)
                    {
                        DependencyHelpers.SetIfDefault(child, HorizontalAlignmentProperty, ChildHorizontalAlignment.Value);
                    }
                    if (ChildVerticalAlignment != null)
                    {
                        DependencyHelpers.SetIfDefault(child, VerticalAlignmentProperty, ChildVerticalAlignment.Value);
                    }
                }
            }

            return base.MeasureOverride(constraint);
        }

        /// <summary>
        /// Called when the visual children of a <see cref="System.Windows.Controls.Grid"/> element change.
        /// <remarks>Used to mark that the grid children have changed.</remarks>
        /// </summary>
        /// <param name="visualAdded">Identifies the visual child that's added.</param>
        /// <param name="visualRemoved">Identifies the visual child that's removed.</param>
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            _shouldReindex = true;

            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }

        #endregion
    }
}