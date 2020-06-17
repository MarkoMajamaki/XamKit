using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace XamKit
{
    public class WrapLayout : Layout<View>
    {
        public enum Orientations { Horizontal, Vertical }

        private List<Rectangle> m_childrenLocations = null;
        private int m_actualColumnCount = 0;

        private int m_rowCount = 0;
        private List<View> m_hiddenChildren = null;

        /// <summary>
        /// Event when hidden children count changes. Children is hided if MaxRows is reached.
        /// </summary>
        public event EventHandler<CountChangedArgs> HiddenChildrenCountChanged;

        /// <summary>
        /// Event when column count changes
        /// </summary>
        public event EventHandler<CountChangedArgs> ColumnsCountChanged;

        /// <summary>
        /// Event when row count changes
        /// </summary>
        public event EventHandler<CountChangedArgs> RowCountChanged;

        #region Dependency properties

        /// <summary>
        /// Wrap diRectangleion
        /// </summary>
        public static readonly BindableProperty OrientationProperty =
            BindableProperty.Create("Orientation", typeof(Orientations), typeof(WrapLayout), Orientations.Horizontal);

        public Orientations Orientation
        {
            get { return (Orientations)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly BindableProperty ColumnWidthProperty =
            BindableProperty.Create("ColumnWidth", typeof(double?), typeof(WrapLayout), null);

        public double? ColumnWidth
        {
            get { return (double?)GetValue(ColumnWidthProperty); }
            set { SetValue(ColumnWidthProperty, value); }
        }

        public static readonly BindableProperty ColumnMaxWidthProperty =
            BindableProperty.Create("ColumnMaxWidth", typeof(double), typeof(WrapLayout), double.MaxValue);

        public double ColumnMaxWidth
        {
            get { return (double)GetValue(ColumnMaxWidthProperty); }
            set { SetValue(ColumnMaxWidthProperty, value); }
        }

        public static readonly BindableProperty ColumnMinWidthProperty =
            BindableProperty.Create("ColumnMinWidth", typeof(double), typeof(WrapLayout), 0.0);

        public double ColumnMinWidth
        {
            get { return (double)GetValue(ColumnMinWidthProperty); }
            set { SetValue(ColumnMinWidthProperty, value); }
        }

        public static readonly BindableProperty MaxColumnsProperty =
            BindableProperty.Create("MaxColumns", typeof(int), typeof(WrapLayout), int.MaxValue);

        public int MaxColumns
        {
            get { return (int)GetValue(MaxColumnsProperty); }
            set { SetValue(MaxColumnsProperty, value); }
        }

        public static readonly BindableProperty MaxRowsProperty =
            BindableProperty.Create("MaxRows", typeof(int?), typeof(WrapLayout), null);

        public int? MaxRows
        {
            get { return (int?)GetValue(MaxRowsProperty); }
            set { SetValue(MaxRowsProperty, value); }
        }

        public static readonly BindableProperty ColumnSpacingProperty =
            BindableProperty.Create("ColumnSpacing", typeof(double), typeof(WrapLayout), 0.0);

        public double ColumnSpacing
        {
            get { return (double)GetValue(ColumnSpacingProperty); }
            set { SetValue(ColumnSpacingProperty, value); }
        }

        public static readonly BindableProperty RowSpacingProperty =
            BindableProperty.Create("RowSpacing", typeof(double), typeof(WrapLayout), 0.0);

        public double RowSpacing
        {
            get { return (double)GetValue(RowSpacingProperty); }
            set { SetValue(RowSpacingProperty, value); }
        }

        /// <summary>
        /// Override every child margin if setted
        /// </summary>
        public static readonly BindableProperty ChildMarginProperty =
            BindableProperty.Create("ChildMargin", typeof(Thickness?), typeof(WrapLayout), null);

        public Thickness? ChildMargin
        {
            get { return (Thickness?)GetValue(ChildMarginProperty); }
            set { SetValue(ChildMarginProperty, value); }
        }

        /// <summary>
        /// Fixed columns count
        /// </summary>
        public static readonly BindableProperty ColumnsCountProperty =
            BindableProperty.Create("ColumnsCount", typeof(int?), typeof(WrapLayout), null);

        public int? ColumnsCount
        {
            get { return (int?)GetValue(ColumnsCountProperty); }
            set { SetValue(ColumnsCountProperty, value); }
        }
        /// <summary>
        /// Count of hidden children which is hided because MaxRows
        /// </summary>
        public static readonly BindableProperty HiddenChildrenCountProperty =
            BindableProperty.Create("HiddenChildrenCount", typeof(int), typeof(WrapLayout), 0);

        public int HiddenChildrenCount
        {
            get { return (int)GetValue(HiddenChildrenCountProperty); }
            protected set { SetValue(HiddenChildrenCountProperty, value); }
        }

        public static readonly BindableProperty RowCountProperty =
            BindableProperty.Create("RowCount", typeof(int), typeof(WrapLayout), 0);

        public int RowCount
        {
            get { return (int)GetValue(RowCountProperty); }
            protected set { SetValue(RowCountProperty, value); }
        }

        #endregion

        #region Attached properties

        /// <summary>
        /// Set or get column span for child
        /// </summary>
        public static readonly BindableProperty ColumnSpanProperty =
            BindableProperty.CreateAttached("ColumnSpan", typeof(int), typeof(WrapLayout), 0);

        public static int GetColumnSpan(BindableObject obj)
        {
            return (int)obj.GetValue(ColumnSpanProperty);
        }

        public static void SetColumnSpan(BindableObject obj, int value)
        {
            obj.SetValue(ColumnSpanProperty, value);
        }

        /// <summary>
        /// Is child in new row which is filled. Work on vertical and horizontal wrap orientations.
        /// </summary>
        public static readonly BindableProperty FillRowProperty =
            BindableProperty.CreateAttached("FillRow", typeof(bool), typeof(WrapLayout), false);

        public static bool GetFillRow(BindableObject obj)
        {
            return (bool)obj.GetValue(FillRowProperty);
        }

        public static void SetFillRow(BindableObject obj, bool value)
        {
            obj.SetValue(FillRowProperty, value);
        }

        /// <summary>
        /// Is last child of this row. Works only on horizontal wrap orientation.
        /// </summary>
        public static readonly BindableProperty IsRowEndingProperty =
            BindableProperty.CreateAttached("IsRowEnding", typeof(bool), typeof(WrapLayout), false);

        public static bool GetIsRowEnding(BindableObject obj)
        {
            return (bool)obj.GetValue(IsRowEndingProperty);
        }

        public static void SetIsRowEnding(BindableObject obj, bool value)
        {
            obj.SetValue(IsRowEndingProperty, value);
        }

        /// <summary>
        /// Is first child of the row. Works only on horizontal wrap orientation.
        /// </summary>
        public static readonly BindableProperty IsRowBeginingProperty =
            BindableProperty.CreateAttached("IsRowBegining", typeof(bool), typeof(WrapLayout), false);

        public static bool GetIsRowBegining(BindableObject obj)
        {
            return (bool)obj.GetValue(IsRowBeginingProperty);
        }

        public static void SetIsRowBegining(BindableObject obj, bool value)
        {
            obj.SetValue(IsRowBeginingProperty, value);
        }
        
        #endregion

        #region Properties

        /// <summary>
        /// Children which are hidden because MaxRows
        /// </summary>        
        public IReadOnlyList<View> HiddenChildren
        {
            get
            {
                return m_hiddenChildren.AsReadOnly();
            }
        }

        public bool HasColumns
        {
            get
            {
                if (ColumnWidth.HasValue || ColumnMinWidth > 0 || ColumnMaxWidth < double.MaxValue)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public int ActualColumnCount
        {
            get
            {
                return m_actualColumnCount;
            }
            private set
            {
                int oldColumnCount = m_actualColumnCount;
                m_actualColumnCount = value;

                if (value != oldColumnCount && ColumnsCountChanged != null)
                {
                    ColumnsCountChanged(this, new CountChangedArgs(oldColumnCount, m_actualColumnCount));
                }
            }
        }

        public double ActualColumnWidth { get; private set; }

        #endregion

        public WrapLayout()
        {
            m_childrenLocations = new List<Rectangle>();
            m_hiddenChildren = new List<View>();
        }

        /// <summary>
        /// Measure children total size
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            return MeasureChildren(widthConstraint, heightConstraint);
        }

        /// <summary>
        /// Arrange children
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            if (HorizontalOptions.Alignment == LayoutAlignment.Fill && 
                VerticalOptions.Alignment == LayoutAlignment.Fill &&
                double.IsNaN(width) == false && 
                double.IsNaN(height) == false &&
                double.IsInfinity(width) == false && 
                double.IsInfinity(height) == false)
            {
                MeasureChildren(width, height);
            }

            int i = 0;
            foreach (Rectangle location in m_childrenLocations)
            {
                if (i <= Children.Count() - 1)
                {
                    View child = Children[i];
                    LayoutChildIntoBoundingRegion(child, location);
                }

                i++;
            }

            int oldRowCount = RowCount;
            int oldHiddenChildrenCount = HiddenChildrenCount;

            RowCount = m_rowCount;
            HiddenChildrenCount = m_hiddenChildren.Count;

            if (HiddenChildrenCountChanged != null && oldHiddenChildrenCount != m_hiddenChildren.Count)
            {
                HiddenChildrenCountChanged(this, new CountChangedArgs(oldHiddenChildrenCount, m_hiddenChildren.Count));
            }
            if (RowCountChanged != null && oldRowCount != m_rowCount)
            {
                RowCountChanged(this, new CountChangedArgs(oldRowCount, m_rowCount));
            }
        }

        private SizeRequest MeasureChildren(double width, double height)
        {
            CalculateColumns(width);

            if (Orientation == Orientations.Vertical)
            {
                m_childrenLocations = GetVerticalWrappedChildrenLocations(width, height);
            }
            else
            {
                m_childrenLocations = GetHorizontalWrappedChildrenLocations(width, height);
            }

            Size size = new Size();

            foreach (Rectangle location in m_childrenLocations)
            {
                size.Width = Math.Max(size.Width, location.Right);
                size.Height = Math.Max(size.Height, location.Bottom);
            }

            return new SizeRequest(size, size);
        }

        #region Vertical

        /// <summary>
        /// Measure children and save locations
        /// </summary>
        /// <param name="width">Available width</param>
        /// <param name="height">Availale height</param>
        /// <returns>List of children locations</returns>
        private List<Rectangle> GetVerticalWrappedChildrenLocations(double width, double height)
        {
            List<Rectangle> locations = new List<Rectangle>();

            int groupChildrenCount = 0;
            int nextWholeRowChildIndex = GetNextWholeRowChild(0, out groupChildrenCount);

            int minChildrenInOneColumn = groupChildrenCount / ActualColumnCount;
            int childrenCountLeftToFirstColumns = groupChildrenCount - (minChildrenInOneColumn * ActualColumnCount);
            double yOffset = 0;

            int groupColumn = 0;
            int groupChildIndex = 0;
            int groupColumnIndex = 0;

            m_rowCount = 0;

            List<double> columnHeights = new List<double>();

            for (int i = 0; i < Children.Count; i++)
            {
                View child = Children[i] as View;

                // Override child margin if ChildMargin has value
                if (ChildMargin.HasValue)
                {
                    child.Margin = ChildMargin.Value;
                }

                Rectangle location = new Rectangle();

                double actualColumnSpacing = groupColumn > 0 ? ColumnSpacing : 0;

                if (i == nextWholeRowChildIndex)
                {
                    m_rowCount++;

                    if (columnHeights.Count > 0)
                    {
                        yOffset += columnHeights.Max();
                        columnHeights.Clear();
                    }

                    nextWholeRowChildIndex = GetNextWholeRowChild(i + 1, out groupChildrenCount);

                    minChildrenInOneColumn = groupChildrenCount / ActualColumnCount;
                    childrenCountLeftToFirstColumns = groupChildrenCount - (minChildrenInOneColumn * ActualColumnCount);

                    groupColumn = 0;
                    groupChildIndex = 0;
                    groupColumnIndex = 0;

                    m_rowCount += minChildrenInOneColumn + (childrenCountLeftToFirstColumns > 0 ? 1 : 0);

                    SizeRequest childSize = child.Measure(width, double.PositiveInfinity, MeasureFlags.IncludeMargins);
                    location = new Rectangle(0, yOffset, width, childSize.Request.Height);
                    yOffset += childSize.Request.Height + RowSpacing;
                }
                else
                {
                    if (child.IsVisible == false)
                    {
                        locations.Add(location);
                        continue;
                    }

                    double columnYOffset = columnHeights.Count > groupColumn ? columnHeights[groupColumn] : 0;

                    SizeRequest childSize = child.Measure(Math.Min(ActualColumnWidth, width), double.PositiveInfinity, MeasureFlags.IncludeMargins);
                    location = new Rectangle(groupColumn * (ActualColumnWidth + actualColumnSpacing), yOffset + columnYOffset, Math.Min(ActualColumnWidth, width), childSize.Request.Height);

                    if (columnHeights.Count <= groupColumn)
                    {
                        columnHeights.Add(location.Height + RowSpacing);
                        groupColumnIndex = 0;
                    }
                    else
                    {
                        columnHeights[groupColumn] = columnHeights[groupColumn] + location.Height + RowSpacing;
                        groupColumnIndex++;
                    }

                    if (groupColumnIndex >= (minChildrenInOneColumn - 1) + (childrenCountLeftToFirstColumns > 0 ? 1 : 0))
                    {
                        groupColumn++;
                        childrenCountLeftToFirstColumns--;
                    }

                    groupChildIndex++;
                }

                locations.Add(location);
            }

            return locations;
        }

        private int GetNextWholeRowChild(int startIndex, out int groupChildrenCount)
        {
            groupChildrenCount = 0;

            for (int i = startIndex; i < Children.Count; i++)
            {
                View child = Children[i];

                if (GetFillRow(child))
                {
                    return i;
                }

                if (child.IsVisible)
                {
                    groupChildrenCount++;
                }
            }

            return Children.Count;
        }

        #endregion

        #region Horizontal

        private List<Rectangle> GetHorizontalWrappedChildrenLocations(double width, double height)
        {
            List<Rectangle> locations = new List<Rectangle>();

            List<View> hiddenChildren = new List<View>();
            m_rowCount = Children.Count > 0 && GetFillRow(Children[0]) == false && GetIsRowBegining(Children[0]) == false ? 1 : 0;

            // If no columns
            if (ColumnWidth.HasValue == false && ColumnMinWidth == 0 && ColumnMaxWidth == double.MaxValue && ColumnsCount.HasValue == false)
            {
                double horizontalOffset = 0;
                double verticalOffset = 0;
                double currentRowHeight = 0;

                foreach (View child in Children)
                {
                    // Override child margin if ChildMargin has value
                    if (ChildMargin.HasValue)
                    {
                        child.Margin = ChildMargin.Value;
                    }

                    Rectangle location = new Rectangle();

                    bool hasMaxRows = MaxRows.HasValue && m_rowCount > MaxRows.Value;

                    // Dont measure collapsed or ignored children. If max row count is reached.
                    if (child.IsVisible == false || hasMaxRows)
                    {
                        locations.Add(location);

                        if (hasMaxRows)
                        {
                            hiddenChildren.Add(child);
                        }

                        continue;
                    }

                    // Measure child with all availale size
                    SizeRequest childSize = child.Measure(width, height, MeasureFlags.IncludeMargins);

                    bool isRowFilled = GetFillRow(child);
                    bool isRowBegining = GetIsRowBegining(child);
                    bool isRowEnding = GetIsRowEnding(child);

                    bool newRow = horizontalOffset + childSize.Request.Width > width;

                    // If there is no enought free columns on the right or child should fill whole row, go to new row
                    if (newRow || isRowFilled || isRowBegining)
                    {
                        verticalOffset += currentRowHeight;
                        m_rowCount++;
                        horizontalOffset = 0;
                        currentRowHeight = 0;

                        if (MaxRows.HasValue && m_rowCount > MaxRows.Value)
                        {
                            locations.Add(location);
                            hiddenChildren.Add(child);
                            continue;
                        }
                    }

                    if (isRowFilled)
                    {
                        location = new Rectangle(horizontalOffset, verticalOffset, width, childSize.Request.Height);
                    }
                    else if (isRowEnding)
                    {
                        location = new Rectangle(horizontalOffset, verticalOffset, width - horizontalOffset, childSize.Request.Height);
                    }
                    else
                    {
                        location = new Rectangle(horizontalOffset, verticalOffset, childSize.Request.Width, childSize.Request.Height);
                    }

                    locations.Add(location);

                    horizontalOffset += childSize.Request.Width + ColumnSpacing;

                    // Update current row height
                    if (currentRowHeight < childSize.Request.Height + RowSpacing)
                    {
                        currentRowHeight = childSize.Request.Height + RowSpacing;
                    }

                    if (isRowFilled || isRowEnding)
                    {
                        verticalOffset += currentRowHeight;
                        m_rowCount++;
                        horizontalOffset = 0;
                        currentRowHeight = 0;
                    }
                }
            }
            // If has columns
            else
            {
                double currentColumnIndex = 0;
                double currentRowHeight = 0;
                double verticalOffset = 0;

                foreach (View child in Children)
                {
                    // Override child margin if ChildMargin has value
                    if (ChildMargin.HasValue)
                    {
                        child.Margin = ChildMargin.Value;
                    }

                    Rectangle location = new Rectangle();

                    bool hasMaxRows = MaxRows.HasValue && m_rowCount > MaxRows.Value;

                    // Dont measure collapsed children
                    if (child.IsVisible == false || hasMaxRows)
                    {
                        locations.Add(location);

                        if (hasMaxRows)
                        {
                            hiddenChildren.Add(child);
                        }

                        continue;
                    }

                    bool isRowFilled = GetFillRow(child);
                    bool isRowBegining = GetIsRowBegining(child);
                    bool isRowEnding = GetIsRowEnding(child);
                    int columnSpan = GetColumnSpan(child);

                    bool newRow = currentColumnIndex + (columnSpan > 1 ? columnSpan : 1) > ActualColumnCount;

                    // If there is no enought free columns on the right or children is filled to whole row, go to new row
                    if (newRow || isRowFilled || isRowBegining)
                    {
                        verticalOffset += currentRowHeight;
                        m_rowCount++;
                        currentColumnIndex = 0;
                        currentRowHeight = 0;

                        if (MaxRows.HasValue && m_rowCount > MaxRows.Value)
                        {
                            locations.Add(location);
                            hiddenChildren.Add(child);
                            continue;
                        }
                    }

                    double childAvailableWidth = 0;

                    if (isRowFilled && double.IsInfinity(width) == false)
                    {
                        childAvailableWidth = width;
                    }
                    else
                    {
                        // Add column span to available width
                        childAvailableWidth = ActualColumnWidth * (columnSpan == 0 ? 1 : columnSpan);

                        if (currentColumnIndex + columnSpan >= ActualColumnCount)
                        {
                            childAvailableWidth += ColumnSpacing;
                        }
                        else
                        {
                            View nextVisibleChild = GetNextVisibleChild(child);
                            int widthInColumns = columnSpan > 1 ? columnSpan : 1;

                            if (nextVisibleChild != null)
                            {
                                // If not last child, then add available span between columns to child available width

                                int nextVisibleChildColumnSpan = GetColumnSpan(nextVisibleChild);
                                bool isNextChildFillRow = GetFillRow(nextVisibleChild);
                                bool isNExtChildRowBegining = GetIsRowBegining(nextVisibleChild);
                                int nextChildWidthInColumns = nextVisibleChildColumnSpan > 1 ? nextVisibleChildColumnSpan : 1;

                                // IS next child going to new row
                                if (isNextChildFillRow || currentColumnIndex + widthInColumns + nextChildWidthInColumns >= ActualColumnCount || isRowEnding || isNExtChildRowBegining)
                                {
                                    childAvailableWidth += ColumnSpacing * Math.Max(0, widthInColumns - 1);
                                }
                            }
                            else
                            {
                                // If last child and has span, then add all space between columns to child available width
                                childAvailableWidth += ColumnSpacing * Math.Max(0, widthInColumns - 1);
                            }
                        }
                    }

                    double horizontalOffset = currentColumnIndex * (ActualColumnWidth + ColumnSpacing);

                    // If row is ending, then take all available width (ignore column spanning)
                    if (isRowEnding)
                    {
                        childAvailableWidth = width - horizontalOffset;
                    }
                    else
                    {
                        // Prevent child to spanned out of the available width when column span is used and column has static width
                        childAvailableWidth = Math.Min(childAvailableWidth, width - horizontalOffset);
                    }

                    SizeRequest childSize = child.Measure(childAvailableWidth, double.PositiveInfinity, MeasureFlags.IncludeMargins);

                    // Remove from hidden children
                    if (m_hiddenChildren.Contains(child))
                    {
                        m_hiddenChildren.Remove(child);
                    }
                    
                    // Update current row height
                    if (currentRowHeight < childSize.Request.Height + RowSpacing)
                    {
                        currentRowHeight = childSize.Request.Height + RowSpacing;
                    }

                    locations.Add(new Rectangle(horizontalOffset, verticalOffset, childAvailableWidth, childSize.Request.Height));

                    // Increase column counter
                    currentColumnIndex += columnSpan > 1 ? columnSpan : 1;

                    if (isRowFilled || isRowEnding)
                    {
                        verticalOffset += currentRowHeight;
                        m_rowCount++;
                        currentColumnIndex = 0;
                        currentRowHeight = 0;
                    }
                }
            }

            m_hiddenChildren = hiddenChildren;

            if (MaxRows.HasValue)
            {
                m_rowCount = Math.Min(m_rowCount, MaxRows.Value);
            }

            return locations;
        }

        private View GetNextVisibleChild(View child)
        {
            for (int i = Children.IndexOf(child) + 1; i < Children.Count; i++)
            {
                View nextChild = Children[i] as View;

                if (nextChild.IsVisible)
                {
                    return nextChild;
                }
            }

            return null;
        }

        #endregion

        private void CalculateColumns(double availableWidth)
        {
            if (ColumnsCount.HasValue)
            {
                ActualColumnCount = ColumnsCount.Value;

                double columnWidth = ((availableWidth + ColumnSpacing) / ActualColumnCount) - ColumnSpacing;
                ActualColumnWidth = Math.Max(0, Math.Min(columnWidth, ColumnMaxWidth));
            }
            else if (ColumnWidth.HasValue == false)
            {
                int columnCount = 0;

                if (ColumnMinWidth > 0)
                {
                    columnCount = (int)Math.Floor((availableWidth + ColumnSpacing) / (Math.Max(1, ColumnMinWidth) + ColumnSpacing));
                }
                else if (ColumnMaxWidth < double.MaxValue)
                {
                    columnCount = (int)Math.Ceiling((availableWidth + ColumnSpacing) / (Math.Max(1, ColumnMaxWidth) + ColumnSpacing));
                }
                else if (ColumnMinWidth == 0 && ColumnMaxWidth == double.MaxValue)
                {
                    ActualColumnCount = -1;
                    ActualColumnWidth = -1;
                    return;
                }

                ActualColumnCount = Math.Max(1, Math.Min(columnCount, MaxColumns));
                double columnWidth = ((availableWidth + ColumnSpacing) / ActualColumnCount) - ColumnSpacing;
                ActualColumnWidth = Math.Max(0, Math.Min(columnWidth, ColumnMaxWidth));
            }
            else
            {
                ActualColumnCount = Math.Max(1, Math.Min(MaxColumns, (int)Math.Floor((availableWidth + ColumnSpacing) / (ColumnWidth.Value + ColumnSpacing))));
                ActualColumnWidth = ColumnWidth.Value;
            }
        }
    }

    /// <summary>
    /// Argumet for WrapLayout columns count changed event
    /// </summary>
    public class CountChangedArgs : EventArgs
    {
        public int OldCount { get; private set; }
        public int NewCount { get; private set; }

        public CountChangedArgs(int oldCount, int newCount)
        {
            OldCount = oldCount;
            NewCount = newCount;
        }
    }
}