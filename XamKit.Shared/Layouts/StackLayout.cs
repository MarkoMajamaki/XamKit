﻿using System.Collections.Generic;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
	/// <summary>
	/// Orientations what Common.StackLayout can have
	/// </summary>
	public enum StackOrientations { Horizontal, Vertical, Depth }

    /// <summary>
    /// Layout where all children are stacked over each other
    /// </summary>
    public class StackLayout : Layout<View>
    {
        // Children size chache. Size is NOT included Spacing.
        private Dictionary<View, SizeCache> m_childrenSizes = null;

        #region Properties

        /// <summary>
        /// Stack orientations
        /// </summary>
        public static readonly BindableProperty OrientationProperty =
            BindableProperty.Create("Orientation", typeof(StackOrientations), typeof(StackLayout), StackOrientations.Vertical);

        public StackOrientations Orientation
        {
            get { return (StackOrientations)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Child margin
        /// </summary>
        public static readonly BindableProperty SpacingProperty =
            BindableProperty.Create("Spacing", typeof(double), typeof(StackLayout), 0.0);

        public double Spacing
        {
            get { return (double)GetValue(SpacingProperty); }
            set { SetValue(SpacingProperty, value); }
        }

        #endregion

        public StackLayout()
        {
            m_childrenSizes = new Dictionary<View, SizeCache>();
        }

        /// <summary>
        /// Measure all children
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            return MeasureChildren(widthConstraint, heightConstraint);
        }

        /// <summary>
        /// Layout all children
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            MeasureChildren(width, height);

            double xOffset = 0;
            double yOffset = 0;
            int index = 0;

            Size availableSize = new Size(width, height);

            foreach (View child in Children)
            {
                SizeCache sizeCache = null;
                if (m_childrenSizes.TryGetValue(child, out sizeCache) == false)
                {
                    sizeCache = new SizeCache(child);
                    m_childrenSizes.Add(child, sizeCache);
                }

                if (Orientation == StackOrientations.Depth)
                {
                    Rectangle location = new Rectangle(Spacing, Spacing, width - Spacing * 2, height - Spacing * 2);
                    if (location != child.Bounds)
                    {
                        LayoutChildIntoBoundingRegion(child, location);
                    }
                }
                else if (Orientation == StackOrientations.Horizontal)
                {
                    SizeRequest s = sizeCache.GetSize(new Size(double.PositiveInfinity, height));

                    Rectangle location = new Rectangle(xOffset, 0, s.Request.Width, height);
                    if (child.Bounds != location)
                    {
                        LayoutChildIntoBoundingRegion(child, location);
                    }

                    xOffset += s.Request.Width + Spacing;
                }
                else
                {
                    if (child is ItemContainer && (child as ItemContainer).IsSeparatorVisible)
                    {
                        yOffset--;
                    }

                    SizeRequest s = sizeCache.GetSize(new Size(width, double.PositiveInfinity));

                    Rectangle location = new Rectangle(0, yOffset, width, s.Request.Height);
                    if (child.Bounds != location)
                    {
                        LayoutChildIntoBoundingRegion(child, location);
                    }

                    yOffset += s.Request.Height + Spacing;
                }

                index++;
            }
        }

        /// <summary>
        /// Cached results must be cleared whenever the measurement of the layout is invalidated.
        /// </summary>
        protected override void InvalidateMeasure()
        {
            m_childrenSizes.Clear();
            base.InvalidateMeasure();
        }

        protected override void InvalidateLayout()
        {
            m_childrenSizes.Clear();
            base.InvalidateLayout();
        }

        /// <summary>
        /// Cached results must be cleared whenever the measurement of any layout child is invalidated.
        /// </summary>
        protected override void OnChildMeasureInvalidated()
        {
            m_childrenSizes.Clear();
            base.OnChildMeasureInvalidated();
        }

        protected override void OnChildRemoved(Element child)
        {
            if (child is View view && m_childrenSizes.ContainsKey(view))
            {
                m_childrenSizes.Remove(view);
            }

            base.OnChildRemoved(child);
        }

        /// <summary>
        /// Measure children sizes
        /// </summary>
        private SizeRequest MeasureChildren(double width, double height)
        {
            SizeRequest size = new SizeRequest();

            foreach (View child in Children)
            {
                if (child.IsVisible == false)
                {
                    continue;
                }

                SizeRequest childSize = new SizeRequest();

                SizeCache sizeCache = null;
                if (m_childrenSizes.TryGetValue(child, out sizeCache) == false)
                {
                    sizeCache = new SizeCache(child);
                    m_childrenSizes.Add(child, sizeCache);
                }

                if (Orientation == StackOrientations.Depth)
                {
                    if (double.IsNaN(width) == false && double.IsNaN(height) == false && double.IsInfinity(width) == false && double.IsInfinity(height) == false)
                    {
                        Size s = new Size(width - Spacing * 2, height - Spacing * 2);
                        childSize = new SizeRequest(s, s);
                    }
                    else
                    {
                        childSize = sizeCache.GetSize(new Size(width, double.PositiveInfinity));

                        if (size.Request.Width < childSize.Request.Width)
                        {
                            size.Request = new Size(childSize.Request.Width, size.Request.Height);
                        }
                        if (size.Request.Height < childSize.Request.Height)
                        {
                            size.Request = new Size(size.Request.Width, childSize.Request.Height);
                        }

                        if (size.Minimum.Width < childSize.Minimum.Width)
                        {
                            size.Minimum = new Size(childSize.Minimum.Width, size.Minimum.Height);
                        }
                        if (size.Minimum.Height < childSize.Minimum.Height)
                        {
                            size.Minimum = new Size(size.Minimum.Width, childSize.Minimum.Height);
                        }
                    }
                }
                else if (Orientation == StackOrientations.Horizontal)
                {
                    childSize = sizeCache.GetSize(new Size(double.PositiveInfinity, height));

                    // Update total size width
                    size.Request = new Size(size.Request.Width + childSize.Request.Width + Spacing, size.Request.Height);
                    size.Minimum = new Size(size.Minimum.Width + childSize.Minimum.Width + Spacing, size.Minimum.Height);

                    // Update total size height
                    double childReqHeight = childSize.Request.Height;
                    double childMinHeight = childSize.Minimum.Height;

                    if (size.Request.Height < childReqHeight)
                    {
                        size.Request = new Size(size.Request.Width, childReqHeight);
                    }

                    if (size.Minimum.Height < childMinHeight)
                    {
                        size.Minimum = new Size(size.Minimum.Width, childReqHeight);
                    }
                }
                else
                {
                    childSize = sizeCache.GetSize(new Size(width, double.PositiveInfinity));

                    // Update total size height
                    size.Request = new Size(size.Request.Width, size.Request.Height + childSize.Request.Height + Spacing);
                    size.Minimum = new Size(size.Minimum.Width, size.Minimum.Height + childSize.Minimum.Height + Spacing);

                    // Update total size width
                    double childReqWidth = childSize.Request.Width;
                    double childMinWidth = childSize.Minimum.Width;

                    if (size.Request.Width < childReqWidth)
                    {
                        size.Request = new Size(childReqWidth, size.Request.Height);
                    }
                    if (size.Minimum.Width < childMinWidth)
                    {
                        size.Minimum = new Size(childMinWidth, size.Minimum.Height);
                    }
                }
            }

            return size;
        }

        #region SizeCache helper

        private class SizeCache
        {
            private Dictionary<Size, SizeRequest> m_sizeCache = null;

            public View View { get; private set; }

            public SizeCache(View view)
            {
                m_sizeCache = new Dictionary<Size, SizeRequest>();
                View = view;
            }

            public SizeRequest GetSize(Size availableSize)
            {
                SizeRequest s = new SizeRequest();

                if (m_sizeCache.TryGetValue(availableSize, out s) == false)
                {
                    if (View.WidthRequest >= 0 && View.HeightRequest >= 0)
                    {
                        s = new SizeRequest(new Size(View.WidthRequest + View.Margin.HorizontalThickness, View.HeightRequest + View.Margin.VerticalThickness), new Size(View.WidthRequest + View.Margin.HorizontalThickness, View.HeightRequest + View.Margin.VerticalThickness));
                    }
                    else
                    {
                        s = View.Measure(availableSize.Width, availableSize.Height, MeasureFlags.IncludeMargins);
                        // System.Diagnostics.Debug.WriteLine("Actual measure: " + View.ToString());
                    }

                    m_sizeCache.Add(availableSize, s);
                }

                return s;
            }
        }

        #endregion
    }
}
