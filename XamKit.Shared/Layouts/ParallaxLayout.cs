using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace XamKit
{
    /// <summary>
    /// Parallax source offset changed event
    /// </summary>
    public delegate void OffsetChangedEvent(double xOffset, double yOffset);

    /// <summary>
    /// Interface which parallax source must implement
    /// </summary>
    public interface IParallaxSource
    {
        double VerticalOffset { get; }
        double HorizontalOffset { get; }
        event OffsetChangedEvent OffsetChanged;
    }

    /// <summary>
    /// Parallax layout
    /// </summary>
    public class ParallaxLayout : Layout<View>
    {
        // Child cache
        private List<ChildInfo> m_childrenSizes = null;
		
        // Layout available size
        private Size m_availableSize = new Size();

        // Parallax source element
        private IParallaxSource m_parallaxSourceView = null;

        /// <summary>
        /// Helper for child info
        /// </summary>
        private class ChildInfo
        {
            public View Child { get; set; }
			public Size Size { get; set; }

            public bool IsParallaxEnabled { get; set; }

            public double VerticalShiftRatio { get; set; }
			public double VerticalSourceEndOffset { get; set; }
			public double VerticalSourceStartOffset { get; set; }

			public double HorizontalShiftRatio { get; set; }
			public double HorizontalSourceEndOffset { get; set; }
			public double HorizontalSourceStartOffset { get; set; }

			public ChildInfo(View child)
            {
                Child = child;
            }
        }

        #region Attached properties

        /// <summary>
        /// Is parallax motion enabled for a child
        /// </summary>
        public static readonly BindableProperty IsParallaxEnabledProperty =
		   BindableProperty.CreateAttached("IsParallaxEnabled", typeof(bool), typeof(ParallaxLayout), true);

		public static bool GetIsParallaxEnabled(BindableObject view)
		{
			return (bool)view.GetValue(IsParallaxEnabledProperty);
		}

		public static void SetIsParallaxEnabled(BindableObject view, bool value)
		{
			view.SetValue(IsParallaxEnabledProperty, value);
		}

		#endregion

		#region Attached properties - Horizontal

		/// <summary>
		/// A value of the enumeration that determines how the horizontal source offset and child are interpreted.
		/// </summary>
		public static readonly BindableProperty HorizontalShiftRatioProperty =
            BindableProperty.CreateAttached("HorizontalShiftRatio", typeof(double), typeof(ParallaxLayout), 1.0);

		public static double GetHorizontalShiftRatio(BindableObject view)
		{
			return (double)view.GetValue(HorizontalShiftRatioProperty);
		}

		public static void SetHorizontalShiftRatio(BindableObject view, double value)
		{
			view.SetValue(HorizontalShiftRatioProperty, value);
		}

		/// <summary>
		/// The horizontal scroll offset at which parallax motion starts. The default is 0.
		/// </summary>
		public static readonly BindableProperty HorizontalSourceStartOffsetProperty =
			BindableProperty.CreateAttached("HorizontalSourceStartOffset", typeof(double), typeof(ParallaxLayout), 0.0);

		public static double GetHorizontalSourceStartOffset(BindableObject view)
		{
			return (double)view.GetValue(HorizontalSourceStartOffsetProperty);
		}

		public static void SetHorizontalSourceStartOffset(BindableObject view, double value)
		{
			view.SetValue(HorizontalSourceStartOffsetProperty, value);
		}

		/// <summary>
		/// The horizontal scroll offset at which parallax motion ends. The default is 0.
		/// </summary>
		public static readonly BindableProperty HorizontalSourceEndOffsetProperty =
			BindableProperty.CreateAttached("HorizontalSourceEndOffset", typeof(double), typeof(ParallaxLayout), 100.0);

		public static double GetHorizontalSourceEndOffset(BindableObject view)
		{
			return (double)view.GetValue(HorizontalSourceEndOffsetProperty);
		}

		public static void SetHorizontalSourceEndOffset(BindableObject view, double value)
		{
			view.SetValue(HorizontalSourceEndOffsetProperty, value);
		}

		#endregion

		#region Attached properties - Vertical

		/// <summary>
		/// A value of the enumeration that determines how the vertical source offset and child are interpreted.
		/// </summary>
		public static readonly BindableProperty VerticalShiftRatioProperty =
			BindableProperty.CreateAttached("VerticalShiftRatio", typeof(double), typeof(ParallaxLayout), 1.0);

		public static double GetVerticalShiftRatio(BindableObject view)
		{
			return (double)view.GetValue(VerticalShiftRatioProperty);
		}

		public static void SetVerticalShiftRatio(BindableObject view, double value)
		{
			view.SetValue(VerticalShiftRatioProperty, value);
		}

		/// <summary>
		/// The vertical scroll offset at which parallax motion starts. The default is 0.
		/// </summary>
		public static readonly BindableProperty VerticalSourceStartOffsetProperty =
			BindableProperty.CreateAttached("VerticalSourceStartOffset", typeof(double), typeof(ParallaxLayout), 0.0);

		public static double GetVerticalSourceStartOffset(BindableObject view)
		{
			return (double)view.GetValue(VerticalSourceStartOffsetProperty);
		}

		public static void SetVerticalSourceStartOffset(BindableObject view, double value)
		{
			view.SetValue(VerticalSourceStartOffsetProperty, value);
		}

		/// <summary>
		/// The vertical scroll offset at which parallax motion ends. The default is 0.
		/// </summary>
		public static readonly BindableProperty VerticalSourceEndOffsetProperty =
			BindableProperty.CreateAttached("VerticalSourceEndOffset", typeof(double), typeof(ParallaxLayout), 100.0);

		public static double GetVerticalSourceEndOffset(BindableObject view)
		{
			return (double)view.GetValue(VerticalSourceEndOffsetProperty);
		}

		public static void SetVerticalSourceEndOffset(BindableObject view, double value)
		{
			view.SetValue(VerticalSourceEndOffsetProperty, value);
		}

		#endregion

		#region Bindable properties

		/// <summary>
		/// Source vertical offset
		/// </summary>
		public static readonly BindableProperty SourceProperty =
			BindableProperty.Create("Source", typeof(View), typeof(ParallaxLayout), propertyChanged: OnSourceChanged);

		private static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as ParallaxLayout).OnSourceChanged(oldValue as View, newValue as View);
		}

        public View Source
		{
			get { return (View)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

        #endregion

        public ParallaxLayout()
        {
			m_childrenSizes = new List<ChildInfo>();
		}

        /// <summary>
        /// Reset motion
        /// </summary>
        public void Reset()
        {
			foreach (ChildInfo childInfo in m_childrenSizes)
			{
				childInfo.Child.TranslationX = 0;
				childInfo.Child.TranslationY = 0;
			}
        }

		/// <summary>
		/// Find IParallaxSource child using recursion
		/// </summary>
		public static IParallaxSource FindParallaxSourceRecursive(View view)
		{
			if (view is IParallaxSource)
			{
				return view as IParallaxSource;
			}
			else if (view is ILayoutController)
			{
				ILayoutController layout = view as ILayoutController;
				foreach (View child in layout.Children)
				{
					IParallaxSource source = FindParallaxSourceRecursive(child);

					if (source != null)
					{
						return source;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Measure all children
		/// </summary>
		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            m_availableSize = new Size(widthConstraint, heightConstraint);
            Size s = MeasureChildren(widthConstraint, heightConstraint);
            return new SizeRequest(s, s);
        }

        /// <summary>
        /// Layout all children above each other
        /// </summary>
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            m_availableSize = new Size(width, height);

            foreach (ChildInfo childInfo in m_childrenSizes)
            {
				LayoutChildIntoBoundingRegion(childInfo.Child, new Rectangle(0, 0, width, height));
			}
        }
		
        /// <summary>
        /// Measure all childre above each other
        /// </summary>
        private Size MeasureChildren(double width, double height)
		{
			m_childrenSizes.Clear();

			Size s = new Size();

			foreach (View child in Children)
			{
                Size childSize = child.Measure(width, height, MeasureFlags.IncludeMargins).Request;

                ChildInfo info = new ChildInfo(child);
                info.Size = childSize;
				info.IsParallaxEnabled = GetIsParallaxEnabled(child);

				info.HorizontalShiftRatio = GetHorizontalShiftRatio(child);
				info.HorizontalSourceStartOffset = GetHorizontalSourceStartOffset(child);
                info.HorizontalSourceEndOffset = GetHorizontalSourceEndOffset(child);

				info.VerticalShiftRatio = GetVerticalShiftRatio(child);
				info.VerticalSourceStartOffset = GetVerticalSourceStartOffset(child);
				info.VerticalSourceEndOffset = GetVerticalSourceEndOffset(child);

				m_childrenSizes.Add(info);

                if (s.Width < childSize.Width)
				{
					s.Width = childSize.Width;
				}

				if (s.Height < childSize.Height)
				{
					s.Height = childSize.Height;
				}
			}

			return s;
		}

		/// <summary>
		/// Called when source changes. If the source element is not a implementing IParallaxSource, the XAML tree is walked 
		/// starting at the source element to find an embedded IParallaxSource element.
		/// </summary>
		private void OnSourceChanged(View oldSource, View newSource)
        {
            if (m_parallaxSourceView != null)
            {
                m_parallaxSourceView.OffsetChanged -= OnSourceOffsetChanged;
            }

            if (newSource is IParallaxSource)
            {
                m_parallaxSourceView = newSource as IParallaxSource;
				m_parallaxSourceView.OffsetChanged += OnSourceOffsetChanged;
			}
            else
            {
                m_parallaxSourceView = FindParallaxSourceRecursive(newSource);

                if (m_parallaxSourceView != null)
                {
					m_parallaxSourceView.OffsetChanged += OnSourceOffsetChanged;
				}
			}
        }

        /// <summary>
        /// Do layout when source offsets chanes
        /// </summary>
        private void OnSourceOffsetChanged(double xOffset, double yOffset)
        {
            UpdateLayout(Width, Height);
        }

        /// <summary>
        /// Update children based on motion
        /// </summary>
        private void UpdateLayout(double width, double height)
        {
            foreach (ChildInfo childInfo in m_childrenSizes)
            {
                if (childInfo.IsParallaxEnabled && m_parallaxSourceView != null)
                {
                    double x = childInfo.Child.TranslationX;
                    if (childInfo.HorizontalSourceStartOffset <= m_parallaxSourceView.HorizontalOffset &&
                        childInfo.HorizontalSourceEndOffset >= m_parallaxSourceView.HorizontalOffset)
                    {
						x = m_parallaxSourceView.HorizontalOffset * childInfo.HorizontalShiftRatio;
					}

                    double y = childInfo.Child.TranslationY;
                    if (childInfo.VerticalSourceStartOffset <= m_parallaxSourceView.VerticalOffset &&
                        childInfo.VerticalSourceEndOffset >= m_parallaxSourceView.VerticalOffset)
                    {
						y = m_parallaxSourceView.VerticalOffset * childInfo.VerticalShiftRatio;
					}

					childInfo.Child.TranslationX = x;
					childInfo.Child.TranslationY = y;
				}
                else
                {
                    childInfo.Child.TranslationX = 0;
					childInfo.Child.TranslationY = 0;
				}
            }
        }
    }
}
