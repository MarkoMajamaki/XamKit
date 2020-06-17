using System;
using Xamarin.Forms;

namespace XamKit
{
	[ContentProperty("Content")]
	public class TabItem : Layout<View>, IContent, ICarouselLayoutChild
	{
		public enum ContentCreateEvents { Default, Appeared, ScrollEnded }

		private View _actualContentElement;

		/// <summary>
		/// Is debugging text enabled
		/// </summary>
		public bool IsDebugEnabled { get; set; } = true;

		private bool _isInvalidationIgnored = false;

		/// <summary>
		/// Event when content is created
		/// </summary>
		public event EventHandler ContentCreated;

		#region Properties

		/// <summary>
		/// Tab content
		/// </summary>
		public static readonly BindableProperty ContentProperty =
			BindableProperty.Create("Content", typeof(object), typeof(TabItem), null, propertyChanged: OnContentChanged);

		private static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as TabItem).OnContentChanged(oldValue, newValue);
		}

		public object Content
		{
			get { return (object)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		/// <summary>
		/// Tab content template
		/// </summary>
		public static readonly BindableProperty ContentTemplateProperty =
			BindableProperty.Create("ContentTemplate", typeof(DataTemplate), typeof(TabItem), null, propertyChanged: OnContentTemplateChanged);

		private static void OnContentTemplateChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as TabItem).OnContentTemplateChanged(oldValue as DataTemplate, newValue as DataTemplate);
		}

		public DataTemplate ContentTemplate
		{
			get { return (DataTemplate)GetValue(ContentTemplateProperty); }
			set { SetValue(ContentTemplateProperty, value); }
		}

		/// <summary>
		/// Tab header text
		/// </summary>
		public static readonly BindableProperty HeaderProperty =
			BindableProperty.Create("Header", typeof(object), typeof(TabItem), null);

		public object Header
		{
			get { return (object)GetValue(HeaderProperty); }
			set { SetValue(HeaderProperty, value); }
		}

		/// <summary>
		/// Tab header button template
		/// </summary>
		public static readonly BindableProperty HeaderTemplateProperty =
			BindableProperty.Create("HeaderTemplate", typeof(DataTemplate), typeof(TabItem), null);

		public DataTemplate HeaderTemplate
		{
			get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
			set { SetValue(HeaderTemplateProperty, value); }
		}

		#endregion

		#region Content create

		/// <summary>
		/// Create content when appeared first time
		/// </summary>
		public static readonly BindableProperty ContentCreateEventProperty =
			BindableProperty.Create("ContentCreateEvent", typeof(ContentCreateEvents), typeof(TabItem), ContentCreateEvents.Default);

		public ContentCreateEvents ContentCreateEvent
		{
			get { return (ContentCreateEvents)GetValue(ContentCreateEventProperty); }
			set { SetValue(ContentCreateEventProperty, value); }
		}

		/// <summary>
		/// Animation for content adding. Used only if ContentCreateEvent is ScrollEnded.
		/// </summary>
		public static readonly BindableProperty ContentAnimationProperty =
			BindableProperty.Create("ContentAnimation", typeof(IAnimation), typeof(TabItem), null);

		public IAnimation ContentAnimation
		{
			get { return (IAnimation)GetValue(ContentAnimationProperty); }
			set { SetValue(ContentAnimationProperty, value); }
		}

		#endregion

		public TabItem()
		{
		}

		#region ICarouselLayoutChild

		/// <summary>
		/// Called when TabItem is appeared on TabView's viewport
		/// </summary>
		public void OnAppeared(CarouselAppearingArgs args)
		{
			if (ContentTemplate != null && 
				Content is View == false && 
				_actualContentElement == null && 
				(ContentCreateEvent == ContentCreateEvents.Appeared || (ContentCreateEvent == ContentCreateEvents.ScrollEnded && args.IsScrolling == false)))
			{
				_actualContentElement = ContentTemplate.CreateContent() as View;

				if (Content != null)
				{
					_actualContentElement.BindingContext = Content;
				}

				if (ContentCreateEvent == ContentCreateEvents.ScrollEnded && ContentAnimation != null)
				{
					Animation animation = ContentAnimation.Create(_actualContentElement);

					EventHandler eventHandler = null;
					eventHandler = (s, e) =>
					{
						_actualContentElement.SizeChanged -= eventHandler;

						Device.BeginInvokeOnMainThread(() =>
						{
							animation.Commit(this, "ContentCreateAnimationName", 64, (uint)ContentAnimation.Duration);
						});
					};

					_actualContentElement.SizeChanged += eventHandler;
				}

				Children.Add(_actualContentElement);

				ContentCreated?.Invoke(this, new EventArgs());
			}
		}

		/// <summary>
		/// Called when TabItem is dissapeared on TabView's viewport
		/// </summary>
		public void OnDissapeared()
		{
			return;
		}

        #endregion

        #region Measure / Layout

        /// <summary>
        /// Measure actual content size
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			if (IsDebugEnabled) { System.Diagnostics.Debug.WriteLine("TabItem.OnMeasure: " + Header); }

			if (_actualContentElement != null)
			{
				return _actualContentElement.Measure(widthConstraint, heightConstraint, MeasureFlags.IncludeMargins);
			}
			else
			{
				return base.OnMeasure(widthConstraint, heightConstraint);
			}
		}

		/// <summary>
		/// Layout actual content
		/// </summary>
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			if (IsDebugEnabled) { System.Diagnostics.Debug.WriteLine("TabItem.LayoutChildren: " + Header); }

			if (_actualContentElement != null)
			{
				Rectangle newLocation = new Rectangle(x, y, width, height);

				Rectangle actualBounds = new Rectangle(
					_actualContentElement.Bounds.Left - _actualContentElement.Margin.Left,
					_actualContentElement.Bounds.Top - _actualContentElement.Margin.Top,
					_actualContentElement.Bounds.Right + _actualContentElement.Margin.Right,
					_actualContentElement.Bounds.Bottom + _actualContentElement.Margin.Bottom);

				if (_actualContentElement != null && actualBounds != newLocation)
				{
					LayoutChildIntoBoundingRegion(_actualContentElement, newLocation);
				}
			}
		}

        #endregion

        #region Measure invalidation

        /// <summary>
        /// Should invalidate measure and arrange when child is added
        /// </summary>
        protected override bool ShouldInvalidateOnChildAdded(View child)
		{
			return base.ShouldInvalidateOnChildAdded(child) && !_isInvalidationIgnored;
		}

		/// <summary>
		/// Should invalidate measure and arrange when child is removed
		/// </summary>
		protected override bool ShouldInvalidateOnChildRemoved(View child)
		{
			return base.ShouldInvalidateOnChildRemoved(child) && !_isInvalidationIgnored;
		}

		protected override void OnChildMeasureInvalidated()
		{
			if (_isInvalidationIgnored == false)
			{
				base.OnChildMeasureInvalidated();
			}
		}

		protected override void InvalidateLayout()
		{
			if (_isInvalidationIgnored == false)
			{
				base.InvalidateLayout();
			}
		}

		protected override void InvalidateMeasure()
		{
			if (_isInvalidationIgnored == false)
			{
				base.InvalidateMeasure();
			}
		}

		#endregion

		/// <summary>
		/// Add content to children collection
		/// </summary>
		private void OnContentChanged(object oldContent, object newContent)
		{
			if (newContent is View newContentView)
			{
				if (_actualContentElement != null && Children.Contains(_actualContentElement))
				{
					Children.Remove(_actualContentElement);
				}

				_actualContentElement = newContentView;

				if (_actualContentElement != null && Children.Contains(_actualContentElement) == false)
				{
					Children.Add(_actualContentElement);
				}

				ContentCreated?.Invoke(this, new EventArgs());
			}
			else if (oldContent is View oldContentView)
			{
				if (_actualContentElement != null && Children.Contains(_actualContentElement))
				{
					Children.Remove(_actualContentElement);
				}

				_actualContentElement = null;
			}
			else if (_actualContentElement != null)
			{
				_actualContentElement.BindingContext = newContent;
			}
		}

		/// <summary>
		/// Create content with content template
		/// </summary>
		private void OnContentTemplateChanged(DataTemplate oldContentTemplate, DataTemplate newContentTemplate)
		{
			if (ContentCreateEvent != ContentCreateEvents.Default)
			{
				return;
			}

			if (Content == null || Content is View == false)
			{
				if (_actualContentElement != null && Children.Contains(_actualContentElement))
				{
					Children.Remove(_actualContentElement);
					_actualContentElement = null;
				}

				if (newContentTemplate != null)
				{
					_actualContentElement = newContentTemplate.CreateContent() as View;

					if (Content != null)
					{
						_actualContentElement.BindingContext = Content;
					}

					Children.Add(_actualContentElement);

					ContentCreated?.Invoke(this, new EventArgs());
				}
			}
			else
			{
				// Do nothing with template
			}
		}
	}
}
