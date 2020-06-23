using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    public class CarouselLayout : VirtualizingLayout, ILayout, IParallaxSource
    {
        private double _panLocation = 0;
        private double _panStartedX = 0;
        private double _previousTotalX = 0;

        private bool _isFirstMeasureOrLayoutDone = false;
        private bool _ignoreBringIntoView = false;

        private const string _scrollToItemAnimationName = "scrollToItemAnimationName";

        private List<View> _viewportChildren;

        private bool _ignoreInvalidation = false;
        private bool _isRecycleEnabled = true;
        private const double _viewportOverflowLenght = 0;

        /// <summary>
        /// Is debug text enabled
        /// </summary>
        public bool IsDebugEnabled { get; set; } = false;

        /// <summary>
        /// Event when current item index changed
        /// </summary>
        public event IndexChangedEvent CurrentItemIndexChanged;

        /// <summary>
        /// Event for pan changes
        /// </summary>
        public event PanChangedEvent PanChanged;

        /// <summary>
        /// Event when scroll is ended
        /// </summary>
        public event EventHandler ScrollEnded;

        #region BindingProperties

        /// <summary>
        /// Is user pan enabled
        /// </summary>
        public static readonly BindableProperty IsPanEnabledProperty =
            BindableProperty.Create("IsPanEnabled", typeof(bool), typeof(CarouselLayout), true);

        public bool IsPanEnabled
        {
            get { return (bool)GetValue(IsPanEnabledProperty); }
            set { SetValue(IsPanEnabledProperty, value); }
        }

        /// <summary>
        /// Is children flipped in the end
        /// </summary>
        public static readonly BindableProperty IsFlipEnabledProperty =
            BindableProperty.Create("IsFlipEnabled", typeof(bool), typeof(CarouselLayout), true);

        public bool IsFlipEnabled
        {
            get { return (bool)GetValue(IsFlipEnabledProperty); }
            set { SetValue(IsFlipEnabledProperty, value); }
        }

        /// <summary>
        /// Which item index is currently focused. Updated after panning and animations.
        /// </summary>
        public static readonly BindableProperty CurrentItemIndexProperty =
            BindableProperty.Create("CurrentItemIndex", typeof(int), typeof(CarouselLayout), 0, propertyChanged: OnCurrentItemIndexChanged);

        static void OnCurrentItemIndexChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as CarouselLayout).OnCurrentItemIndexChanged((int)newValue);
        }

        public int CurrentItemIndex
        {
            get { return (int)GetValue(CurrentItemIndexProperty); }
            set { SetValue(CurrentItemIndexProperty, value); }
        }

        /// <summary>
        /// How scroll is snapped. None = no snap, Mandatory = snap to item, MandatorySingle = snap to item but scroll only one item
        /// when swiped.
        /// </summary>
        public static readonly BindableProperty SnapPointsTypeProperty =
            BindableProperty.Create("SnapPointsType", typeof(SnapPointsTypes), typeof(CarouselLayout), SnapPointsTypes.None);

        public SnapPointsTypes SnapPointsType
        {
            get { return (SnapPointsTypes)GetValue(SnapPointsTypeProperty); }
            set { SetValue(SnapPointsTypeProperty, value); }
        }

        /// <summary>
        /// Where focused item is located
        /// </summary>
        public static readonly BindableProperty SnapPointsAlignmentProperty =
            BindableProperty.Create("SnapPointsAlignment", typeof(SnapPointsAlignments), typeof(CarouselLayout), SnapPointsAlignments.Start);

        public SnapPointsAlignments SnapPointsAlignment
        {
            get { return (SnapPointsAlignments)GetValue(SnapPointsAlignmentProperty); }
            set { SetValue(SnapPointsAlignmentProperty, value); }
        }

        /// <summary>
        /// Item scroll animation duration in milliseconds
        /// </summary>
        public static readonly BindableProperty ScrollToItemAnimationDurationProperty =
            BindableProperty.Create("ScrollToItemAnimationDuration", typeof(uint), typeof(CarouselLayout), (uint)250);

        public uint ScrollToItemAnimationDuration
        {
            get { return (uint)GetValue(ScrollToItemAnimationDurationProperty); }
            set { SetValue(ScrollToItemAnimationDurationProperty, value); }
        }

        /// <summary>
        /// Command to execute when current item is changed animation is finished
        /// </summary>
        public static readonly BindableProperty ScrollEndedCommandProperty =
            BindableProperty.Create("ScrollEndedCommand", typeof(ICommand), typeof(CarouselLayout), null);

        public ICommand ScrollEndedCommand
        {
            get { return (ICommand)GetValue(ScrollEndedCommandProperty); }
            set { SetValue(ScrollEndedCommandProperty, value); }
        }

        /// <summary>
        /// How much to make adjacent items partially visible by pixels
        /// </summary>
        public static readonly BindableProperty PeekAreaInsetsProperty =
            BindableProperty.Create("PeekAreaInsets", typeof(double), typeof(CarouselLayout), 0.0);

        public double PeekAreaInsets
        {
            get { return (double)GetValue(PeekAreaInsetsProperty); }
            set { SetValue(PeekAreaInsetsProperty, value); }
        }

        #endregion

        #region IParallaxSource

        /// <summary>
        /// Event when offset changes
        /// </summary>
        public event OffsetChangedEvent OffsetChanged;

        #endregion

        /// <summary>
        /// Is panning currently active
        /// </summary>
        public bool IsPanning { get; private set; }

        /// <summary>
        /// Is UI virtualization enabled
        /// </summary>
        public bool IsVirtualizationEnabled 
        { 
            get
            {
                if (ItemsGenerator == null || (ItemsGenerator.GeneratorHost is ItemsView itemsView && itemsView.IsVirtualizingEnabled == false))
                {
                    return false;
                }

                return ItemsGenerator != null;
            }
        }

        /// <summary>
        /// Is items recycled
        /// </summary>
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

        public bool IsSnapToItemNeeded
        {
            get
            {
                if (SnapPointsType == SnapPointsTypes.None)
                {
                    return false;
                }
                else
                {
                    return GetSnapToItemDelta(CurrentItemIndex) != 0;
                }
            }
        }

        public CarouselLayout()
        {
            _viewportChildren = new List<View>();

            IsClippedToBounds = true;
        }

        public void AddChild(View child, bool ignoreInvalidation = true)
        {
            bool original = _ignoreInvalidation;
            _ignoreInvalidation = ignoreInvalidation;
            Children.Add(child);
            _ignoreInvalidation = original;
        }

        public void InsertChild(int index, View child, bool ignoreInvalidation = true)
        {
            bool original = _ignoreInvalidation;
            _ignoreInvalidation = ignoreInvalidation;
            Children.Insert(index, child);
            _ignoreInvalidation = original;
        }

        public void RemoveChild(View child, bool ignoreInvalidation = true)
        {
            bool original = _ignoreInvalidation;
            _ignoreInvalidation = ignoreInvalidation;
            Children.Remove(child);
            _ignoreInvalidation = original;
        }

        /// <summary>
        /// Bring item into viewport by index
        /// </summary>
        /// <param name="index">Item index</param>
        /// <param name="location">Focused location</param>
        /// <param name="isAnimated">Is movement animated</param>
        public void BringIntoViewport(int index, SnapPointsAlignments location = SnapPointsAlignments.Start, bool isAnimated = false)
        {
            if (_isFirstMeasureOrLayoutDone == false || _ignoreBringIntoView == true)
            {
                return;
            }

            int actualChildrenCount = IsVirtualizationEnabled ? ItemsGenerator.TotalItemsCount : Children.Count;

            if (index >= actualChildrenCount || index < 0)
            {
                throw new Exception(string.Format("Could not bring child with index {0} to viewport. CarouselLayout has {1} children.", index, Children.Count - 1));
            }

            // Stop previous scroll animation
            this.AbortAnimation(_scrollToItemAnimationName);

            double panDelta = GetSnapToItemDelta(index);

            if (isAnimated)
            {
                _panStartedX = HorizontalOffset;
                _previousTotalX = HorizontalOffset;
                double newHorizontalOffset = HorizontalOffset + panDelta;

                // Create pan animation

                Animation scrollAnimation = new Animation(d =>
                {
                    UpdateChildren(Width, Height, d);

                }, HorizontalOffset, newHorizontalOffset);

                if (IsPanning)
                {
                    return;
                }

                // Do pan animation
                scrollAnimation.Commit(this, _scrollToItemAnimationName, 64, ScrollToItemAnimationDuration, Easing.CubicOut, finished: (s, isAborted) =>
                {
                    if (isAborted == false)
                    {
                        UpdateChildren(Width, Height, newHorizontalOffset);

                        _ignoreBringIntoView = true;
                        CurrentItemIndex = CalculateCurrentItemIndex();
                        _ignoreBringIntoView = false;

                        ScrollEndedCommand?.Execute(CurrentItemIndex);
                        ScrollEnded?.Invoke(this, new EventArgs());

                        UpdateViewportItemsAppearing();
                        RemoveVirtualizedChildren();
                    }
                });
            }
            else
            {
                UpdateChildren(Width, Height, panDelta);
            }
        }

        /// <summary>
        /// Get scroll delta to giving index
        /// </summary>
        private double GetSnapToItemDelta(int index)
        {
            int actualChildrenCount = IsVirtualizationEnabled ? ItemsGenerator.TotalItemsCount : Children.Count;
            View leftViewportChild = _viewportChildren.First();

            double leftViewportChildWidth = GetActualSize(leftViewportChild, Width, Height).Width;
            double panDelta = 0;

            if (IsFlipEnabled && IsVirtualizationEnabled == false)
            {
                // Initialize pan calculation delta values for pan distance to new index (to left and right)
                double rightPanDelta = (1 - _panLocation % 1) * leftViewportChildWidth;
                double leftPanDelta = (_panLocation % 1) * leftViewportChildWidth;

                // Get children between current location and new index if panned to right
                int iterator = (int)Math.Floor(_panLocation + 1);

                // If last child then start iterating from begining
                iterator = iterator > actualChildrenCount - 1 ? 0 : iterator;

                while (true)
                {
                    if (iterator == index)
                    {
                        break;
                    }

                    View child = GetChildByIndex(iterator);
                    rightPanDelta += GetActualSize(child, Width, Height).Width;

                    iterator++;
                    if (iterator > actualChildrenCount - 1)
                    {
                        iterator = 0;
                    }
                }

                // Get children between current location and new index if panned to left
                iterator = (int)Math.Floor(_panLocation);

                while (true)
                {
                    if (iterator == index)
                    {
                        break;
                    }

                    View child = GetChildByIndex(iterator);
                    leftPanDelta += GetActualSize(child, Width, Height).Width;

                    iterator--;
                    if (iterator < 0)
                    {
                        iterator = actualChildrenCount - 1;
                    }
                }

                // Get shortest panning distance for changes
                panDelta = (rightPanDelta <= leftPanDelta) ? -rightPanDelta : leftPanDelta;
            }
            else
            {
                Size currentItemSize = GetActualSize(GetChildByIndex(CurrentItemIndex), Width, Height);
                panDelta = (_panLocation % 1) * currentItemSize.Width;

                if (index < CurrentItemIndex)
                {
                    for (int i = index + 1; i < CurrentItemIndex; i++)
                    {
                        View child = GetChildByIndex(i);
                        panDelta += GetActualSize(child, Width, Height).Width;
                    }
                }
                else if (index > CurrentItemIndex)
                {
                    for (int i = CurrentItemIndex; i < index; i++)
                    {
                        View child = GetChildByIndex(i);
                        panDelta -= GetActualSize(child, Width, Height).Width;
                    }
                }
                else
                {
                    if (_panLocation > CurrentItemIndex)
                    {
                        panDelta = (_panLocation % 1) * GetActualSize(GetChildByIndex(CurrentItemIndex), Width, Height).Width;
                    }
                    else if (CurrentItemIndex > 0)
                    {
                        panDelta = -(1 - (_panLocation % 1)) * GetActualSize(GetChildByIndex(CurrentItemIndex - 1), Width, Height).Width;
                    }
                }
            }

            return panDelta;
        }

        /// <summary>
        /// Bring item into viewport by item
        /// </summary>
        /// <param name="child">Child to bring into view</param>
        /// <param name="location">Focused location</param>
        /// <param name="isAnimated">Is movement animated</param>
        public void BringIntoViewport(View child, SnapPointsAlignments location = SnapPointsAlignments.Start, bool isAnimated = false)
        {
            BringIntoViewport(Children.IndexOf(child), SnapPointsAlignment, isAnimated);
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
            UpdateChildren(Width, Height, HorizontalOffset);
        }

        /// <summary>
        /// Measure all children if measure needed
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            if (IsDebugEnabled) { Debug.WriteLine("CarouselLayout.OnMeasure: (" + widthConstraint + "," + heightConstraint + ")"); }

            if (_isFirstMeasureOrLayoutDone == false)
            {
                _panLocation = CurrentItemIndex;
                UpdateChildren(widthConstraint, heightConstraint, HorizontalOffset);
            }

            _isFirstMeasureOrLayoutDone = true;

            if (HorizontalOptions.Alignment == LayoutAlignment.Fill && 
                VerticalOptions.Alignment == LayoutAlignment.Fill &&
                double.IsInfinity(widthConstraint) == false &&
                double.IsInfinity(heightConstraint) == false)
            {
                Size size = new Size(widthConstraint, heightConstraint);
                return new SizeRequest(size, size);
            }
            else
            {
                return MeasureChildren(widthConstraint, heightConstraint);
            }
        }

        /// <summary>
        /// Layout all children into layout
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            if (IsDebugEnabled) { Debug.WriteLine("CarouselLayout.LayoutChildren: (" + width + "," + height + ")"); }

            if (_isFirstMeasureOrLayoutDone == false)
            {
                _panLocation = CurrentItemIndex;
            }
            _isFirstMeasureOrLayoutDone = true;
            UpdateChildren(width, height, HorizontalOffset - _panStartedX);
        }

        /// <summary>
        /// Do layout based on total panX
        /// </summary>
        /// <param name="width">Available width</param>
        /// <param name="height">Available height</param>
        /// <param name="totalX">Total pan X</param>
        public void UpdateChildren(double width, double height, double totalX)
        {
            int actualItemsCount = IsVirtualizationEnabled ? ItemsGenerator.TotalItemsCount : Children.Count;

            if (actualItemsCount == 0)
            {
                return;
            }

            // _panLocation = Math.Min(_panLocation, actualItemsCount);

            // Get new pan location based on pan delta and previous pan locatio
            (HorizontalOffset, _panLocation) = CalculateLocation(width, height, _panStartedX, _panLocation, _previousTotalX, totalX);

            // Save previous total pan
            _previousTotalX = totalX;

            // Is scrolling or panning 
            bool isScrolling = IsPanning || AnimationExtensions.AnimationIsRunning(this, _scrollToItemAnimationName);

            // Get focused child
            int focusedChildIndex = (int)Math.Floor(_panLocation);

            View focusedChild = GetChildByIndex(focusedChildIndex);
            Size focusedChildSize = GetActualSize(focusedChild, width, height);

            if (IsVirtualizationEnabled)
            {
                ItemsGenerator.SetRealized(focusedChildIndex);
            }

            if (focusedChild is ICarouselLayoutChild carouselChild)
            {
                carouselChild.OnAppeared(new CarouselAppearingArgs(isScrolling, focusedChildIndex));
            }

            // Layout focused child if needed
            Rectangle focusedChildLocation = new Rectangle(0, 0, focusedChildSize.Width, focusedChildSize.Height);
            if (focusedChild.Bounds != focusedChildLocation)
            {
                LayoutChildIntoBoundingRegion(focusedChild, focusedChildLocation);
            }

            if (actualItemsCount == 1)
            {
                _viewportChildren.Add(focusedChild);
                focusedChild.TranslationX = 0;
                return;
            }

            //
            // Set focused child TranslationX
            //

            if (SnapPointsAlignment == SnapPointsAlignments.Start)
            {
                focusedChild.TranslationX = -(_panLocation % 1) * focusedChildSize.Width;
            }
            else if (SnapPointsAlignment == SnapPointsAlignments.Center)
            {
                focusedChild.TranslationX = -(_panLocation % 1) * focusedChildSize.Width + ((width - focusedChildSize.Width) / 2);
            }
            else if (SnapPointsAlignment == SnapPointsAlignments.End)
            {
                focusedChild.TranslationX = -(_panLocation % 1) * focusedChildSize.Width + (width - focusedChildSize.Width);
            }

            // Bug on platform
            // if (Device.RuntimePlatform == Device.UWP && focusedChild.TranslationX.Equals(0)) { focusedChild.TranslationX = 0.001; }

            //
            // Layout other children based on focused child TranslationX
            //

            View leftVisibleChild = focusedChild;

            // Loop children on left side of focused child
            int index = focusedChildIndex - 1;

            // If flip is enabled and previous index is less than zero, then start left children from last child index
            if (index < 0 && IsFlipEnabled)
            {
                index = actualItemsCount - 1;
            }

            // Clear viewport children list
            _viewportChildren.Clear();
            List<int> virtualizedIndexList = new List<int>();
            List<int> realizedIndexList = new List<int>();

            if (IsFlipEnabled || index >= 0)
            {
                // Loop children on left
                while (true)
                {
                    // Get child and its size
                    View child = GetChildByIndex(index);
                    Size childSize = GetActualSize(child, width, height);

                    // Get child on right and its size. It has correct TranslationX based on focused child.
                    int nextChildIndex = index + 1 > actualItemsCount - 1 ? 0 : index + 1;
                    View nextChild = GetChildByIndex(nextChildIndex);

                    // Update child TranslationX based on child on right
                    child.TranslationX = nextChild.TranslationX - childSize.Width;

                    bool isChildOnViewport = child.TranslationX < width && child.TranslationX + childSize.Width > 0;
                    if (isChildOnViewport && child.Bounds.IsEmpty && child is ICarouselLayoutChild carouselChildOnleft)
                    {
                        carouselChildOnleft.OnAppeared(new CarouselAppearingArgs(isScrolling, index));
                    }

                    // All children are layouted to origin
                    Rectangle childLocation = new Rectangle(0, 0, childSize.Width, childSize.Height);
                    Rectangle childBounds = new Rectangle(child.Bounds.Left - child.Margin.Left, child.Bounds.Top - child.Margin.Top, child.Bounds.Right + child.Margin.Right, child.Bounds.Bottom + child.Margin.Bottom);
                    if (childBounds != childLocation && isChildOnViewport)
                    {
                        LayoutChildIntoBoundingRegion(child, childLocation);
                    }

                    // Bug on platform
                    // if (Device.RuntimePlatform == Device.UWP && child.TranslationX.Equals(0)) { child.TranslationX = 0.01; }

                    // Add to viewport children list
                    if (child.TranslationX + childSize.Width >= 0.01 && child.TranslationX < width)
                    {
                        _viewportChildren.Insert(0, child);
                    }

                    // If child is not visible anymore, then stop left children layouting
                    if (child.TranslationX + childSize.Width < -_viewportOverflowLenght)
                    {
                        realizedIndexList.Remove(index);
                        if (virtualizedIndexList.Contains(index) == false)
                        {
                            virtualizedIndexList.Add(index);
                        }
                        break;
                    }
                    else if (IsVirtualizationEnabled)
                    {
                        virtualizedIndexList.Remove(index);
                        if (realizedIndexList.Contains(index) == false)
                        {
                            realizedIndexList.Add(index);
                        }
                        ItemsGenerator.SetRealized(index);
                    }

                    // Update index
                    if (index == 0)
                    {
                        // If flip is enabled then continue from last child index
                        if (IsFlipEnabled)
                        {
                            if (IsVirtualizationEnabled)
                            {
                                index = ItemsGenerator.TotalItemsCount - 1;
                            }
                            else
                            {
                                index = Children.Count - 1;
                            }
                        }
                        // If not enabled, then update is done
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        index--;
                    }
 
                    // Stop translation update if child is not fully visible on left
                    if (nextChild.TranslationX <= 0)
                    {
                        leftVisibleChild = nextChild;
                        break;
                    }
                }
            }

            // Loop children on right side of focused child
            index = focusedChildIndex + 1;
            if (index > actualItemsCount - 1 && IsFlipEnabled)
            {
                index = 0;
            }

            // Add focused child to viewport children list
            _viewportChildren.Add(focusedChild);

            if (IsFlipEnabled || index <= actualItemsCount - 1)
            {
                // Loop children on right
                while (true)
                {
                    // Child and its size
                    View child = GetChildByIndex(index);

                    if (child == leftVisibleChild)
                    {
                        break;
                    }

                    Size childSize = GetActualSize(child, width, height);

                    // Child on left and its size
                    int previousChildIndex = index - 1 < 0 ? actualItemsCount - 1 : index - 1;
                    View previousChild = GetChildByIndex(previousChildIndex);
                    Size previousChildSize = GetActualSize(previousChild, width, height);

                    // Set child TranslationX base on child on left. It has correct translation based on focused item
                    child.TranslationX = previousChild.TranslationX + previousChildSize.Width;

                    bool isChildOnViewport = child.TranslationX < width && child.TranslationX + childSize.Width > 0;
                    if (isChildOnViewport && child.Bounds.IsEmpty && child is ICarouselLayoutChild carouselChildOnRight)
                    {
                        carouselChildOnRight.OnAppeared(new CarouselAppearingArgs(isScrolling, index));
                    }

                    // All children are layouted to origin
                    Rectangle childLocation = new Rectangle(0, 0, childSize.Width, childSize.Height);
                    Rectangle childBounds = new Rectangle(child.Bounds.Left - child.Margin.Left, child.Bounds.Top - child.Margin.Top, child.Bounds.Right + child.Margin.Right, child.Bounds.Bottom + child.Margin.Bottom);
                    if (childBounds != childLocation && isChildOnViewport)
                    {
                        LayoutChildIntoBoundingRegion(child, childLocation);
                    }

                    // Bug on platform
                    // if (Device.RuntimePlatform == Device.UWP && child.TranslationX.Equals(0)) { child.TranslationX = 0.01; }

                    // Add to viewport children list

                    if (child.TranslationX + childSize.Width >= 0.01 && child.TranslationX < width)
                    {
                        _viewportChildren.Insert(0, child);
                    }

                    // If child is not visible anymore, then stop left children layouting
                    if (child.TranslationX > width + _viewportOverflowLenght)
                    {
                        realizedIndexList.Remove(index);
                        if (virtualizedIndexList.Contains(index) == false)
                        {
                            virtualizedIndexList.Add(index);
                        }
                        break;
                    }
                    else if (IsVirtualizationEnabled)
                    {
                        virtualizedIndexList.Remove(index);
                        if (realizedIndexList.Contains(index) == false)
                        {
                            realizedIndexList.Add(index);
                        }
                    }

                    // Update loop index
                    if (index == actualItemsCount - 1)
                    {
                        if (IsFlipEnabled)
                        {
                            index = 0;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        index++;
                    }

                    // Stop if child is focused child or left visible child
                    if (index == focusedChildIndex)
                    {
                        break;
                    }
                }
            }

            // Virtualize all children which are not in viewport
            foreach (View child in Children.ToList())
            {
                if (_viewportChildren.Contains(child) == false)
                {
                    child.Opacity = 0;
                    child.InputTransparent = true;
                }
                else
                {
                    child.Opacity = 1;
                    child.InputTransparent = false;
                }
            }

            if (IsVirtualizationEnabled)
            {
                foreach (int virtualizedChildIndex in virtualizedIndexList)
                {
                    ItemsGenerator.SetVirtualized(virtualizedChildIndex);
                }
                foreach (int realizedChildIndex in realizedIndexList)
                {
                    ItemsGenerator.SetRealized(realizedChildIndex);
                }
            }

            // Raise events
            OffsetChanged?.Invoke(HorizontalOffset, VerticalOffset);
            PanChanged?.Invoke(this, new PanChangedArgs(_panLocation, CurrentItemIndex));
        }

        /// <summary>
        /// Return new pan location based on current and pan delta
        /// </summary>
        private (double horizontalOffset, double panLocation) CalculateLocation(
            double width, double height, double panStartedX, double currentPanLocation, double oldTotalX, double newTotalX)
        {
            int actualItemsCount = IsVirtualizationEnabled ? ItemsGenerator.TotalItemsCount : Children.Count;
            int currentChildIndex = Convert.ToInt32(Math.Floor(currentPanLocation));

            //
            // Calculate pan location
            //

            double totalPanDelta = -(oldTotalX - newTotalX);
            double newPanLocation = 0;

            View currentChild = GetChildByIndex(currentChildIndex);
            Size currentChildSize = GetActualSize(currentChild, width, height);
            newPanLocation = currentPanLocation - (totalPanDelta / currentChildSize.Width);

            if (totalPanDelta <= 0)
            {
                if (newPanLocation >= actualItemsCount)
                {
                    if (IsFlipEnabled)
                    {
                        newPanLocation = 0;
                    }
                    else
                    {
                        if (IsVirtualizationEnabled)
                        {
                            newPanLocation = actualItemsCount - 1;
                        }
                        else
                        {
                            newPanLocation = actualItemsCount - 1;
                        }
                    }
                }
            }
            else
            {
                if (newPanLocation < 0)
                {
                    if (IsFlipEnabled && Math.Abs(newPanLocation) > 0.001)
                    {
                         newPanLocation = actualItemsCount + newPanLocation;
                    }
                    else
                    {
                        newPanLocation = 0;
                    }
                }
            }

            double maxPanLocation = actualItemsCount;
            double maxHorizontalOffset = double.MaxValue;

            if (IsFlipEnabled == false && Width > 0)
            {
                View lastChild = null;
                if (IsVirtualizationEnabled)
                {
                    if (ItemsGenerator.HasItemViewGenerated(actualItemsCount - 1))
                    {
                        lastChild = ItemsGenerator.GetItemViewFromIndex(actualItemsCount - 1);
                    }
                }
                else
                {
                    lastChild = Children.Last();
                }

                if (lastChild != null && Children.Contains(lastChild))
                {
                    Size lastChildSize = GetActualSize(lastChild, width, height);

                    double xOffset = width;
                    bool allChildrenOnViewport = true;

                    for (int i = actualItemsCount - 1; i >= 0; i--)
                    {
                        View item = null;
                        if (IsVirtualizationEnabled)
                        {
                            item = ItemsGenerator.GetItemViewFromIndex(i);
                        }
                        else
                        {
                            item = Children.ElementAt(i);
                        }

                        Size itemSize = GetActualSize(item, width, height);
                        xOffset -= itemSize.Width;

                        if (SnapPointsAlignment != SnapPointsAlignments.Center && xOffset < 0)
                        {
                            double firstViewportItemPanLocation = -xOffset / itemSize.Width;
                            maxPanLocation = i + firstViewportItemPanLocation;
                            allChildrenOnViewport = false;
                            break;
                        }
                        else if (xOffset < (Width - lastChildSize.Width) / 2)
                        {
                            double firstViewportItemPanLocation = -(xOffset - Width + itemSize.Width) / itemSize.Width;
                            maxPanLocation = i + firstViewportItemPanLocation;
                            allChildrenOnViewport = false;
                            break;
                        }
                    }

                    if (allChildrenOnViewport == false)
                    {
                        newPanLocation = Math.Min(maxPanLocation, newPanLocation);
                    }
                    else
                    {
                        newPanLocation = 0;
                    }
                }
            }

            //
            // Calculate horizontal offset
            //

            double newHorizontalOffset = Math.Min(panStartedX + newTotalX, maxHorizontalOffset);

            if (newHorizontalOffset >= 6 * Math.Pow(10, 18) || newPanLocation < 0)
            {
            }

            // Return both
            return (newHorizontalOffset, newPanLocation);
        }

        /// <summary>
        /// Get child by index and add to layout if needed
        /// </summary>
        private View GetChildByIndex(int index)
        {
            try
            {
                View item = null;

                if (IsVirtualizationEnabled)
                {
                    _ignoreInvalidation = true;

                    item = ItemsGenerator.GenerateItemView(index);
                    if (Children.Contains(item) == false)
                    {
                        if (Children.Count > index)
                        {
                            Children.Insert(index, item);
                        }
                        else
                        {
                            Children.Add(item);
                        }
                    }

                    ItemsGenerator.SetRealized(index);

                    _ignoreInvalidation = false;
                }
                else
                {
                    item = Children.ElementAt(index);
                }

                return item;
            }
            catch (Exception x)
            {
                return null;
            }
        }

        /// <summary>
        /// Measure all children
        /// </summary>
        /// <returns>Children total size</returns>
        /// <param name="widthConstraint">Available width</param>
        /// <param name="heightConstraint">Available height</param>
        private SizeRequest MeasureChildren(double widthConstraint, double heightConstraint)
        {
            Size totalSize = new Size();

            foreach (View child in Children)
            {
                Size size = GetActualSize(child, widthConstraint, heightConstraint);
                totalSize.Height = Math.Max(totalSize.Height, size.Height);
                totalSize.Width += size.Width;
            }

            return new SizeRequest(totalSize, totalSize);
        }

        /// <summary>
        /// When panel is panned
        /// </summary>
        public void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (Children.Count == 0 || IsPanEnabled == false)
            {
                return;
            }
            
            if (e.StatusType == GestureStatus.Started)
            {
                IsPanning = true;
                _panStartedX = HorizontalOffset;
                _previousTotalX = 0;
                this.AbortAnimation(_scrollToItemAnimationName);

                if (HorizontalOffset >= 6 * Math.Pow(10, 18))
                {
                }
            }
            else if (e.StatusType == GestureStatus.Running)
            {
                UpdateChildren(Width, Height, e.TotalX);
            }
            else if (e.StatusType == GestureStatus.Canceled)
            {
                // Canceled when swiped. Do nothing. 
            }
            else if (e.StatusType == GestureStatus.Completed)
            {
                PanEnded();
            }
        }
        
        /// <summary>
        /// Called when pan ended with 'Completed" generator status.
        /// </summary>
        private void PanEnded()
        {
            IsPanning = false;

            if (SnapPointsType != SnapPointsTypes.None)
            {
                int originalFocusedIndex = CurrentItemIndex;
                CurrentItemIndex = CalculateCurrentItemIndex();
                if (originalFocusedIndex == CurrentItemIndex)
                {
                    BringIntoViewport(CurrentItemIndex, SnapPointsAlignment, true);
                }
            }
            else
            {
                CurrentItemIndex = (int)Math.Floor(_panLocation);
            }
        }

        /// <summary>
        /// Called from renderer when layout swiped
        /// </summary>
        public void OnSwiped(SwipeDirection direction, double velocity)
        {
            if (IsPanEnabled == false)
            {
                return;
            }

            IsPanning = false;
            this.AbortAnimation(_scrollToItemAnimationName);

            int actualChildrenCount = IsVirtualizationEnabled ? ItemsGenerator.TotalItemsCount : Children.Count;
            _panStartedX = HorizontalOffset;
            _previousTotalX = HorizontalOffset;

            double newHorizontalOffset = 0;
            double swipeLenght = 200 * velocity;

            if (SnapPointsType == SnapPointsTypes.MandatorySingle)
            {
                // Get focused child
                int focusedChildIndex = (int)Math.Floor(_panLocation);
                if (direction == SwipeDirection.Left)
                {
                    focusedChildIndex++;
                }

                if (IsFlipEnabled)
                {
                    if (focusedChildIndex > actualChildrenCount - 1)
                    {
                        focusedChildIndex = 0;
                    }
                    else if (focusedChildIndex < 0)
                    {
                        focusedChildIndex = actualChildrenCount - 1;
                    }
                }
                else 
                {
                    focusedChildIndex = Math.Min(actualChildrenCount - 1, focusedChildIndex);
                    focusedChildIndex = Math.Max (0, focusedChildIndex);
                }

                BringIntoViewport(focusedChildIndex, SnapPointsAlignment, true);

                return;
            }
            else if (SnapPointsType == SnapPointsTypes.Mandatory)
            {
                int currentChildIndex = (int)Math.Floor(_panLocation);

                Size currentItemSize = GetActualSize(GetChildByIndex(currentChildIndex), Width, Height);
                double currentItemRelativeX = (_panLocation % 1) * currentItemSize.Width;

                int actualItemsCount = IsVirtualizationEnabled ? ItemsGenerator.TotalItemsCount : Children.Count;

                if (direction == SwipeDirection.Left)
                {
                    double lenghtDelta = currentItemSize.Width - currentItemRelativeX;
                    newHorizontalOffset = HorizontalOffset - lenghtDelta;

                    for (int i = currentChildIndex + 1; i < actualItemsCount; i++)
                    {
                        if (IsVirtualizationEnabled)
                        {
                            double childWidth = GetActualSize(GetChildByIndex(i), Width, Height).Width;
                            lenghtDelta += childWidth;
                            newHorizontalOffset -= childWidth;
                        }
                        else
                        {
                            double childWidth = GetActualSize(Children.ElementAt(i), Width, Height).Width;
                            lenghtDelta += childWidth;
                            newHorizontalOffset -= childWidth;
                        }

                        if (lenghtDelta > swipeLenght)
                        {
                            break;
                        }

                        if (IsFlipEnabled && i + 1 >= actualItemsCount)
                        {
                            i = -1;
                        }
                    }
                }
                else
                {
                    double lenghtDelta = currentItemRelativeX;
                    newHorizontalOffset = HorizontalOffset + lenghtDelta;

                    for (int i = currentChildIndex - 1; i >= 0; i--)
                    {
                        if (IsVirtualizationEnabled)
                        {
                            double childWidth = GetActualSize(GetChildByIndex(i), Width, Height).Width;
                            lenghtDelta += childWidth;
                            newHorizontalOffset += childWidth;
                        }
                        else
                        {
                            double childWidth = GetActualSize(Children.ElementAt(i), Width, Height).Width;
                            lenghtDelta += childWidth;
                            newHorizontalOffset += childWidth;
                        }

                        if (lenghtDelta > swipeLenght)
                        {
                            break;
                        }

                        if (IsFlipEnabled && i - 1 < 0)
                        {
                            i = actualItemsCount;
                        }
                    }
                }
            }
            else
            {
                if (direction == SwipeDirection.Left)
                {
                    newHorizontalOffset = HorizontalOffset - swipeLenght;
                }
                else if (direction == SwipeDirection.Right)
                {
                    newHorizontalOffset = HorizontalOffset + swipeLenght;
                }
            }

            Animation scrollAnimation = new Animation(d =>
            {
                UpdateChildren(Width, Height, d);

            }, HorizontalOffset, newHorizontalOffset);
           
            // Do pan animation
            scrollAnimation.Commit(this, _scrollToItemAnimationName, 64, 1000, AnimationUtils.EaseOutQuint, finished: (s, isAborted) =>
            {
                if (isAborted == false)
                {
                    UpdateChildren(Width, Height, newHorizontalOffset);

                    _ignoreBringIntoView = true;
                    CurrentItemIndex = CalculateCurrentItemIndex();
                    _ignoreBringIntoView = false;

                    ScrollEndedCommand?.Execute(CurrentItemIndex);
                    ScrollEnded?.Invoke(this, new EventArgs());

                    UpdateViewportItemsAppearing();
                    RemoveVirtualizedChildren();
                }
            });
        }

        /// <summary>
        ///  Calculate current item index based on pan location
        /// </summary>
        private int CalculateCurrentItemIndex()
        {
            int index = (int)Math.Round(_panLocation, MidpointRounding.AwayFromZero);
            if (IsFlipEnabled && index > Children.Count - 1)
            {
                index = 0;
            }
            else if (IsFlipEnabled == false && index > Children.Count - 1)
            {
                index = Children.Count - 1;
            }

            return index;
        }
        
        /// <summary>
        /// Handle FocusedIndex changes
        /// </summary>
        private void OnCurrentItemIndexChanged(int newValue)
        {
            if (IsPanning == false && SnapPointsType != SnapPointsTypes.None)
            {
                BringIntoViewport(newValue, SnapPointsAlignment, true);
            }

            CurrentItemIndexChanged?.Invoke(this, newValue);
        }

        /// <summary>
        /// Get child actual size
        /// </summary>
        /// <param name="child"></param>
        private Size GetActualSize(View child, double availableWidth, double availableHeight)
        {
            double width = 0;
            double height = 0;

            if (child.WidthRequest >= 0)
            {
                width = child.WidthRequest;
            }
            else if (PeekAreaInsets > 0)
            {
                width = availableWidth - PeekAreaInsets;
            }
            else
            {
                width = availableWidth;
            }

            if (child.HeightRequest >= 0)
            {
                height = child.HeightRequest;
            }
            else if (double.IsNaN(availableHeight) == false && double.IsInfinity(availableHeight) == false)
            {
                height = availableHeight;
            }
            else
            {
                SizeRequest measuredSize = child.Measure(availableWidth, availableHeight, MeasureFlags.IncludeMargins);
                height = measuredSize.Request.Height;
            }

            return new Size(width, height);
        }

        /// <summary>
        /// Update appearing events for viewport children. Called when scroll is ended.
        /// </summary>
        private void UpdateViewportItemsAppearing()
        {
            foreach (View child in _viewportChildren)
            {
                if (child is ICarouselLayoutChild carouselChild)
                {
                    carouselChild.OnAppeared(new CarouselAppearingArgs(false, Children.IndexOf(child)));
                }
            }
        }

        private void RemoveVirtualizedChildren()
        {            
            if (IsVirtualizationEnabled && IsRecycleEnabled == false)
            {
                _ignoreInvalidation = true;

                // Virtualize all children which are not in viewport
                foreach (View child in Children.ToList())
                {
                    if (_viewportChildren.Contains(child) == false)
                    {
                        Children.Remove(child);
                    }
                }

                _ignoreInvalidation = false;
            }
        }
    }

    public class PanChangedArgs
    {
        /// <summary>
        /// Pan location: 0 -> last item index. Example: 1.5 then pan is middle of index 1 and 2 item.
        /// </summary>
        public double Location { get; private set; }

        public int FocusedIndex { get; private set; }

        public PanChangedArgs(double location, int focusedIndex)
        {
            Location = location;
            FocusedIndex = focusedIndex;
        }
    }

    public class CarouselAppearingArgs
    {
        public bool IsScrolling { get;  private set; }
        public int Index { get; private set; }

        public CarouselAppearingArgs(bool isScrolling, int index)
        {
            IsScrolling = isScrolling;
            Index = index;
        }
    }

    public interface ICarouselLayoutChild
    {
        void OnAppeared(CarouselAppearingArgs args);
        void OnDissapeared(); // TODO
    }

    /// <summary>
    /// Delegate for carouse pan changed event
    /// </summary>
    public delegate void PanChangedEvent(object sender, PanChangedArgs args);

    /// <summary>
    /// How carousel control is scrolled
    /// </summary>
    public enum SnapPointsTypes { None, Mandatory, MandatorySingle }

    /// <summary>
    /// How snapped item is aligned
    /// </summary>
    public enum SnapPointsAlignments { Start, Center, End }
}
