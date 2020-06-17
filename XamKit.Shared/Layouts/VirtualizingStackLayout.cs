using System;
using System.Collections.Generic;
using System.Linq;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    public class VirtualizingStackLayout : VirtualizingLayout
    {
        private List<View> _visibleItems = null;
        private Dictionary<View, Rectangle> _locations = null;

        private double _firstVisibleItemOffset = 0;
        private int _firstVisibleItemIndex = 0;

        private View _currentStickyElement = null;
        private View _nextStickyElement = null;

        private bool _ignoreInvalidation = false;
        private Size _availableSize = new Size();
        private bool _isRecycleEnabled = true;
        private const double _viewportOverflowLenght = 0;

        #region Sticky headers

        /// <summary>
        /// Is child sticky
        /// <summary>
        public static readonly BindableProperty IsStickyProperty =
            BindableProperty.CreateAttached("IsSticky", typeof(bool), typeof(VirtualizingStackLayout), false);

        public static bool GetIsSticky(BindableObject view)
        {
            return (bool)view.GetValue(IsStickyProperty);
        }

        public static void SetIsSticky(BindableObject view, bool value)
        {
            view.SetValue(IsStickyProperty, value);
        }

        #endregion

        public IReadOnlyList<View> ViewportChildren
        {
            get
            {
                return _visibleItems;
            }
        }

        public bool IsRecycleEnabled 
        {
            get
            {
                return _isRecycleEnabled;
            }
            set
            {
                _isRecycleEnabled = value;
                ItemsGenerator.IsRecycleEnabled = value;
            }
        }

        public VirtualizingStackLayout()
        {
            _visibleItems = new List<View>();
            _locations = new Dictionary<View, Rectangle>();

            CompressedLayout.SetIsHeadless(this, true);
        }

        #region Ignore invalidation
        
        protected override void InvalidateLayout()
        {
            if (_ignoreInvalidation == false)
            {
                base.InvalidateLayout();
            }
        }

        protected override void InvalidateMeasure()
        {
            if (_ignoreInvalidation == false)
            {
                base.InvalidateMeasure();
            }
        }

        protected override void OnChildMeasureInvalidated()
        {
            if (_ignoreInvalidation == false)
            {
                base.OnChildMeasureInvalidated();
            }
        }

        protected override bool ShouldInvalidateOnChildAdded(View child)
        {
            return base.ShouldInvalidateOnChildAdded(child) && _ignoreInvalidation == false;
        }

        protected override bool ShouldInvalidateOnChildRemoved(View child)
        {
            return base.ShouldInvalidateOnChildRemoved(child) && _ignoreInvalidation == false;
        }

        #endregion

        public override void Initialize()
        {
            UpdateVisibleItems(Width, 0);
        }

        #region Measure / Layout

        /// <summary>
        /// Measure realized items
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            _availableSize = new Size(widthConstraint, heightConstraint);

            UpdateVisibleItems(widthConstraint, 0);

            Size totalSize = new Size(widthConstraint, EstimateExtentHeight());

            return new SizeRequest(totalSize, totalSize);
        }

        /// <summary>
        /// Arrange realized items
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            _availableSize = new Size(width, height);

            if (_locations.Count == 0)
            {
                UpdateVisibleItems(width, 0);
            }

            double currentStickyHeaderY = 0;

            // Arrange only viewport visible items
            foreach (View item in _visibleItems)
            {
                Rectangle location = _locations[item];

                LayoutChildIntoBoundingRegion(item, location);

                if (_currentStickyElement == item)
                {
                    currentStickyHeaderY = location.Y;
                }
            }

            if (_currentStickyElement != null && currentStickyHeaderY < VerticalOffset)
            {
                double currentStickyElementHeight = _locations[_currentStickyElement].Height;

                if (_nextStickyElement != null && _visibleItems.Contains(_nextStickyElement))
                {
                    Rectangle nextStickyElementLocation = _locations[_nextStickyElement];

                    if (nextStickyElementLocation.Y < VerticalOffset + currentStickyElementHeight)
                    {
                        LayoutChildIntoBoundingRegion(_currentStickyElement, new Rectangle(0, nextStickyElementLocation.Y - currentStickyElementHeight, width, currentStickyElementHeight));
                    }
                    else
                    {
                        LayoutChildIntoBoundingRegion(_currentStickyElement, new Rectangle(0, VerticalOffset, width, currentStickyElementHeight));
                    }
                }
                else
                {
                    LayoutChildIntoBoundingRegion(_currentStickyElement, new Rectangle(0, VerticalOffset, width, currentStickyElementHeight));
                }
            }
        }

        #endregion

        /// <summary>
        /// Host scroll viewer vertical scroll offset changed
        /// </summary>
        /// <param name="newOffset">New offset</param>
        protected override void OnVerticalScrollChanged(double oldOffset, double newOffset)
        {
            UpdateVisibleItems(_availableSize.Width, oldOffset - newOffset);
        }

        /// <summary>
        /// Update visible items based on items measured or estimated size and scroll info.
        /// </summary>
        private void UpdateVisibleItems(double width, double scrollDelta)
        {
            _ignoreInvalidation = true;
            bool doMeasureInvalidation = false;

            double yOffset = _firstVisibleItemOffset;
            double firstVisibleItemHeight = 0;
            _visibleItems.Clear();

            _currentStickyElement = null;
            _nextStickyElement = null;

            if (scrollDelta <= 0)
            {
                for (int i = _firstVisibleItemIndex; i < ItemsGenerator.TotalItemsCount; i++)
                {
                    (View itemContainer, Rectangle childLocation) = GetItemContainerLocationByindex(width, i, yOffset, ref doMeasureInvalidation);

                    // Set current sticky element
                    if (GetIsSticky(itemContainer))
                    {
                        _currentStickyElement = itemContainer;
                    }

                    _firstVisibleItemIndex = i;
                    yOffset += childLocation.Height;

                    if (yOffset >= VerticalOffset - _viewportOverflowLenght)
                    {
                        _visibleItems.Add(itemContainer);

                        if (Children.Contains(itemContainer) == false)
                        {
                            Children.Add(itemContainer);
                            doMeasureInvalidation = true;
                        }

                        ItemsGenerator.SetRealized(i);
                        LayoutChild(itemContainer, childLocation);

                        firstVisibleItemHeight = childLocation.Height;
                        break;
                    }
                    // Remove container from children if not visible
                    else if (Children.Contains(itemContainer) && itemContainer != _currentStickyElement)
                    {
                        if (IsRecycleEnabled == false)
                        {
                            Children.Remove(itemContainer);
                        }

                        ItemsGenerator.SetVirtualized(i);
                    }

                    _firstVisibleItemOffset = yOffset;
                }
            }
            else
            {
                for (int i = _firstVisibleItemIndex; i >= 0; i--)
                {
                    (View itemContainer, Rectangle childLocation) = GetItemContainerLocationByindex(width, i, yOffset, ref doMeasureInvalidation);

                    // Set current sticky element
                    if (GetIsSticky(itemContainer))
                    {
                        _currentStickyElement = itemContainer;
                    }

                    _firstVisibleItemIndex = i;
                    yOffset -= childLocation.Height;

                    if (yOffset + childLocation.Height <= VerticalOffset - _viewportOverflowLenght || i == 0)
                    {
                        _visibleItems.Add(itemContainer);

                        if (Children.Contains(itemContainer) == false)
                        {
                            Children.Add(itemContainer);
                            doMeasureInvalidation = true;
                        }

                        ItemsGenerator.SetRealized(i);
                        LayoutChild(itemContainer, childLocation);

                        firstVisibleItemHeight = childLocation.Height;
                        break;
                    }
                    // Remove container from children if not visible
                    else if (Children.Contains(itemContainer) && itemContainer != _currentStickyElement)
                    {
                        if (IsRecycleEnabled == false)
                        {
                            Children.Remove(itemContainer);
                        }

                        ItemsGenerator.SetVirtualized(i);
                    }

                    _firstVisibleItemOffset = yOffset;
                }

                _firstVisibleItemOffset = Math.Max(0, _firstVisibleItemOffset);
            }

            int lastVisibleItemIndex = _firstVisibleItemIndex;
            yOffset = _firstVisibleItemOffset + firstVisibleItemHeight;

            // Set items visible until viewport is ended
            for (int i = _firstVisibleItemIndex + 1; i < ItemsGenerator.TotalItemsCount; i++)
            {
                if (yOffset >= VerticalOffset + ViewportSize.Height + _viewportOverflowLenght)
                {
                    break;
                }

                (View itemContainer, Rectangle childLocation) = GetItemContainerLocationByindex(width, i, yOffset, ref doMeasureInvalidation);

                // Get next sticky elements
                if (_nextStickyElement == null && _currentStickyElement != null && itemContainer != _currentStickyElement && GetIsSticky(itemContainer))
                {
                    _nextStickyElement = itemContainer;
                }

                LayoutChild(itemContainer, childLocation);

                _visibleItems.Add(itemContainer);
                lastVisibleItemIndex = i;

                yOffset += childLocation.Height;

                ItemsGenerator.SetRealized(i);
            }

            // Remove items after viewport
            for (int i = lastVisibleItemIndex + 1; i < ItemsGenerator.TotalItemsCount; i++)
            {
                if (ItemsGenerator.HasItemViewGenerated(i))
                {
                    View itemContainer = ItemsGenerator.GetItemViewFromIndex(i);

                    if (Children.Contains(itemContainer) && IsRecycleEnabled == false)
                    {
                        Children.Remove(itemContainer);
                    }

                    ItemsGenerator.SetVirtualized(i);
                }
                else
                {
                    break;
                }
            }

            if (_currentStickyElement != null)
            {
                RaiseChild(_currentStickyElement);
            }

            _ignoreInvalidation = false;

            if (doMeasureInvalidation && scrollDelta.Equals(0))
            {
                InvalidateMeasure();
                InvalidateLayout();
            }
        }

        /// <summary>
        /// Layout child if new location
        /// </summary>
        private void LayoutChild(View child, Rectangle location)
        {
            if (child.Bounds != location)
            {
                LayoutChildIntoBoundingRegion(child, location);
            }
        }

        private (View itemContainer, Rectangle childLocation) GetItemContainerLocationByindex(double width, int index, double yOffset, ref bool doMeasureInvalidation)
        {
            View itemContainer = null;

            if (ItemsGenerator.HasItemViewGenerated(index))
            {
                itemContainer = ItemsGenerator.GetItemViewFromIndex(index);
            }
            else
            {
                itemContainer = ItemsGenerator.GenerateItemView(index);
            }

            // Add child to layout because then style will activate
            if (Children.Contains(itemContainer) == false)
            {
                Children.Add(itemContainer);
                doMeasureInvalidation = true;
            }

            Rectangle childLocation = new Rectangle();
            if (IsRecycleEnabled)
            {
                SizeRequest itemSize = itemContainer.Measure(width, double.PositiveInfinity, MeasureFlags.IncludeMargins);
                childLocation = new Rectangle(0, yOffset, width, itemSize.Request.Height);

                if (_locations.ContainsKey(itemContainer))
                {
                    _locations[itemContainer] = childLocation;
                }
                else
                {
                    _locations.Add(itemContainer, childLocation);
                }
            }
            else if (_locations.TryGetValue(itemContainer, out childLocation) == false)
            {
                SizeRequest itemSize = itemContainer.Measure(width, double.PositiveInfinity, MeasureFlags.IncludeMargins);
                childLocation = new Rectangle(0, yOffset, width, itemSize.Request.Height);
                _locations.Add(itemContainer, childLocation);
            }
            else
            {
                childLocation = new Rectangle(0, yOffset, width, _locations[itemContainer].Height);
                _locations[itemContainer] = childLocation;
            }

            return (itemContainer, childLocation);
        }

        /// <summary>
        /// Estimate whole extent height based on realized children average height and children count
        /// </summary>
        private double EstimateExtentHeight()
        {
            if (ItemsGenerator.TotalItemsCount == 0)
            {
                return 0;
            }

            double totalHeight = _locations.Sum(x => x.Value.Height);

            // Estimate one child height based on generated items height
            double oneChildEstimateHeight = totalHeight / _locations.Count;

            // Calculate not generated items based on estimation
            double notGeneratedChildrenHeightEstimation = (ItemsGenerator.TotalItemsCount - _locations.Count) * oneChildEstimateHeight;

            // Estimate height based on generated items absolute height and not generated items estimation total height
            return totalHeight + notGeneratedChildrenHeightEstimation;
        }
    }
}
