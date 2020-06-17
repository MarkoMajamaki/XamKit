using System;
using System.Collections.Generic;
using System.Linq;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
	public enum ExpandingDirections { Left, Top, Right, Bottom }

	/// <summary>
	/// Layout which children are all layouted like in StackLayout and ExpandingContent is expanded to ExpandingDirection
	/// </summary>
	public class ExpanderLayout : StackLayout
	{
		private bool m_ignoreInvalidate = false;

		private Size m_contentSize = new Size();
		private Size m_expandingContentSize = new Size();

		private double m_expandingHeight = 0;
		private const string c_expandAnimationName = "expandAnimation";
		private const string c_collapseAnimationName = "collapseAnimation";

        public delegate void IsEpandedChangedEvent(object sender, bool isExpanded);

        /// <summary>
        /// Event when IsExpanded changes
        /// </summary>
        public event IsEpandedChangedEvent IsExpandedChanged;

		#region Properties

		/// <summary>
		/// Expanding content
		/// </summary>
		public static readonly BindableProperty ExpandingContentProperty =
			BindableProperty.Create("ExpandingContent", typeof(View), typeof(ExpanderLayout), null, propertyChanged: OnExpandingContentChanged);

		static void OnExpandingContentChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as ExpanderLayout).OnExpandingContentChanged(oldValue as View, newValue as View);
		}

		public View ExpandingContent
		{
			get { return (View)GetValue(ExpandingContentProperty); }
			set { SetValue(ExpandingContentProperty, value); }
		}

		/// <summary>
		/// Expanding content template
		/// </summary>
		public static readonly BindableProperty ExpandingContentTemplateProperty =
			BindableProperty.Create("ExpandingContentTemplate", typeof(DataTemplate), typeof(ExpanderLayout), null, propertyChanged: OnExpandingContentTemplateChanged);

		static void OnExpandingContentTemplateChanged(BindableObject bindable, object oldValue, object newValue)
		{
			ExpanderLayout expander = bindable as ExpanderLayout;

			if (newValue != null)
			{
				expander.ExpandingContent = (newValue as DataTemplate).CreateContent() as View;
			}
			else
			{
				expander.ExpandingContent = null;
			}
		}

		public DataTemplate ExpandingContentTemplate
		{
			get { return (DataTemplate)GetValue(ExpandingContentTemplateProperty); }
			set { SetValue(ExpandingContentTemplateProperty, value); }
		}

		/// <summary>
		/// Which direction ExpandingContent is expanded relative to layout childrens
		/// </summary>
		public static readonly BindableProperty ExpandingDirectionProperty =
			BindableProperty.Create("ExpandingDirection", typeof(ExpandingDirections), typeof(ExpanderLayout), ExpandingDirections.Bottom);

		public ExpandingDirections ExpandingDirection
		{
			get { return (ExpandingDirections)GetValue(ExpandingDirectionProperty); }
			set { SetValue(ExpandingDirectionProperty, value); }
		}

		/// <summary>
		/// Is ExpandingContent expanded
		/// </summary>
		public static readonly BindableProperty IsExpandedProperty =
			BindableProperty.Create("IsExpanded", typeof(bool), typeof(ExpanderLayout), false, propertyChanged: OnIsExpandedChanged);


		private static void OnIsExpandedChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as ExpanderLayout).OnIsExpandedChanged((bool)oldValue, (bool)newValue);
		}

		public bool IsExpanded
		{
			get { return (bool)GetValue(IsExpandedProperty); }
			set { SetValue(IsExpandedProperty, value); }
		}

		/// <summary>
		/// Expanding animation duration in milliseconds
		/// </summary>
		public static readonly BindableProperty AnimationDurationProperty =
			BindableProperty.Create("AnimationDuration", typeof(uint), typeof(ExpanderLayout), (uint)200);

		public uint AnimationDuration
		{
			get { return (uint)GetValue(AnimationDurationProperty); }
			set { SetValue(AnimationDurationProperty, value); }
		}

		/// <summary>
		/// Expanding animation easing function
		/// </summary>
		public static readonly BindableProperty EasingFunctionProperty =
			BindableProperty.Create("EasingFunction", typeof(Easing), typeof(ExpanderLayout), Easing.SinOut);

		public Easing EasingFunction
		{
			get { return (Easing)GetValue(EasingFunctionProperty); }
			set { SetValue(EasingFunctionProperty, value); }
		}

		/// <summary>
		/// Is animation running
		/// </summary>
		public static readonly BindableProperty IsAnimationRunningProperty =
			BindableProperty.Create("IsAnimationRunning", typeof(bool), typeof(ExpanderLayout), false);

		public bool IsAnimationRunning
		{
			get { return (bool)GetValue(IsAnimationRunningProperty); }
			private set { SetValue(IsAnimationRunningProperty, value); }
		}

		/// <summary>
		/// Max expanding lenght
		/// </summary>
		public static readonly BindableProperty MaxExpandLenghtProperty =
            BindableProperty.Create("MaxExpandLenght", typeof(double), typeof(ExpanderLayout), double.MaxValue);

		public double MaxExpandLenght
		{
			get { return (double)GetValue(MaxExpandLenghtProperty); }
			set { SetValue(MaxExpandLenghtProperty, value); }
		}

		#endregion

		public ExpanderLayout()
		{
			IsClippedToBounds = true;
		}

		/// <summary>
		/// Layout children
		/// </summary>
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			double xOffset = x;
			double yOffset = y;

			if (ExpandingContent != null && ExpandingContent.IsVisible && IsExpanded)
			{
				switch (ExpandingDirection)
				{
					case ExpandingDirections.Left:
						LayoutChildIntoBoundingRegion(ExpandingContent, new Rectangle(0, 0, m_expandingContentSize.Width, m_expandingContentSize.Height));
						xOffset = m_expandingContentSize.Width;
						break;
					case ExpandingDirections.Top:
						double actualWidth = m_expandingContentSize.Width;
						if (ExpandingContent.HorizontalOptions.Alignment == LayoutAlignment.Fill)
						{
							actualWidth = width;
						}
						LayoutChildIntoBoundingRegion(ExpandingContent, new Rectangle(0, 0, actualWidth, m_expandingContentSize.Height));
						yOffset = m_expandingContentSize.Height;
						break;
				}
			}
			
			base.LayoutChildren(xOffset, yOffset, width, m_contentSize.Height);

			if (ExpandingContent != null && ExpandingContent.IsVisible && IsExpanded)
			{
				switch (ExpandingDirection)
				{
					case ExpandingDirections.Right:
						LayoutChildIntoBoundingRegion(ExpandingContent, new Rectangle(m_contentSize.Width, 0, m_expandingContentSize.Width, m_expandingContentSize.Height));
						break;
					case ExpandingDirections.Bottom:
						double actualWidth = m_expandingContentSize.Width;
						if (ExpandingContent.HorizontalOptions.Alignment == LayoutAlignment.Fill)
						{
							actualWidth = width;
						}
						LayoutChildIntoBoundingRegion(ExpandingContent, new Rectangle(0, m_contentSize.Height, actualWidth, m_expandingContentSize.Height));
						break;
				}
			}
		}

		/// <summary>
		/// Measure view size
		/// </summary>
		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			SizeRequest s = new SizeRequest();

			if (IsAnimationRunning)
			{
				s.Minimum = new Size(m_contentSize.Width, m_contentSize.Height + m_expandingHeight);
				s.Request = new Size(m_contentSize.Width, m_contentSize.Height + m_expandingHeight);
			}
			else
			{
				s = base.OnMeasure(widthConstraint, heightConstraint);
				m_contentSize = s.Request;
				if (ExpandingContent != null && ExpandingContent.IsVisible && IsExpanded)
				{
                    m_expandingContentSize = ExpandingContent.Measure(widthConstraint, heightConstraint, MeasureFlags.IncludeMargins).Request;

					if (MaxExpandLenght < double.MaxValue)
					{
						m_expandingContentSize.Height = Math.Min(MaxExpandLenght, m_expandingContentSize.Height);
					}

					switch (ExpandingDirection)
					{
						case ExpandingDirections.Left:
						case ExpandingDirections.Right:
							s.Minimum = new Size(s.Minimum.Width + m_expandingContentSize.Width, s.Minimum.Height);
							s.Request = new Size(s.Request.Width + m_expandingContentSize.Width, s.Request.Height);
							break;
						case ExpandingDirections.Top:
						case ExpandingDirections.Bottom:
							s.Minimum = new Size(s.Minimum.Width, s.Minimum.Height + m_expandingContentSize.Height);
							s.Request = new Size(s.Request.Width, s.Request.Height + m_expandingContentSize.Height);
							break;
					}
				}
			}

			return s;
		}

		/// <summary>
		/// Should layout invalidate when child is added.
		/// </summary>
		protected override bool ShouldInvalidateOnChildAdded(View child)
		{
			return base.ShouldInvalidateOnChildAdded(child) && !m_ignoreInvalidate;
		}

		/// <summary>
		/// Should layout invalidate when child is removed.
		/// </summary>
		protected override bool ShouldInvalidateOnChildRemoved(View child)
		{
			return base.ShouldInvalidateOnChildRemoved(child) && !m_ignoreInvalidate;
		}

		/// <summary>
		/// Called when IsExpanded property value changes
		/// </summary>
		private void OnIsExpandedChanged(bool oldValue, bool newValue)
		{
			IsAnimationRunning = true;

			if (newValue)
			{
				switch (ExpandingDirection)
				{
					case ExpandingDirections.Left:
					case ExpandingDirections.Right:
						// TODO
						break;
					case ExpandingDirections.Top:
					case ExpandingDirections.Bottom:

						this.AbortAnimation(c_collapseAnimationName);

                        m_expandingContentSize = ExpandingContent.Measure(Width, double.PositiveInfinity, MeasureFlags.IncludeMargins).Request;

                        if (MaxExpandLenght < double.MaxValue)
                        {
                            m_expandingContentSize.Height = Math.Min(MaxExpandLenght, m_expandingContentSize.Height);
                        }

						new Animation(d =>
						{
							m_expandingHeight = d;
							InvalidateMeasure();

						}, 0, m_expandingContentSize.Height, Easing.Linear)
						.Commit(this, c_expandAnimationName, 16, AnimationDuration, EasingFunction, (arg1, arg2) =>
						{
                            if (IsExpanded)
                            {
								IsAnimationRunning = false;
							}
						});
						break;
				}
			}
			else
			{				 
				switch (ExpandingDirection)
				{
					case ExpandingDirections.Left:
					case ExpandingDirections.Right:
						// TODO
						break;
					case ExpandingDirections.Top:
					case ExpandingDirections.Bottom:

                        this.AbortAnimation(c_expandAnimationName);

						new Animation(d =>
						{
							m_expandingHeight = d;
							InvalidateMeasure();

						}, m_expandingContentSize.Height, 0, Easing.Linear)
						.Commit(this, c_collapseAnimationName, 16, AnimationDuration, EasingFunction, (arg1, arg2) =>
						{
                            if (IsExpanded == false)
                            {
                                IsAnimationRunning = false;
                            }
						});
						break;
				}
			}

            if (IsExpandedChanged != null)
            {
                IsExpandedChanged(this, IsExpanded);
            }
		}

		/// <summary>
		/// Called when ExpandingContent changes. Add new view to layout and remove old
		/// </summary>
		private void OnExpandingContentChanged(View oldView, View newView)
		{
			m_ignoreInvalidate = true;

			if (oldView != null && Children.Contains(oldView))
			{
				Children.Remove(oldView);
			}

			if (newView != null)
			{
				Children.Add(newView);
			}

			m_ignoreInvalidate = false;
		}
	}
}
