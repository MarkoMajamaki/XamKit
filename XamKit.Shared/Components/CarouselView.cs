using System;
using System.Windows.Input;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
	public class CarouselView : ItemsView
	{
		private CarouselLayout m_layout = null;

		/// <summary>
		/// Event when current item index changed
		/// </summary>
		public event IndexChangedEvent CurrentItemIndexChanged;

		/// <summary>
		/// Event when carouse panning changes
		/// </summary>
		public event PanChangedEvent PanChanged;

		/// <summary>
		/// Event when scroll is ended
		/// </summary>
		public event EventHandler ScrollEnded;

		/// <summary>
		/// Is debug text enabled
		/// </summary>
		public bool IsDebugEnabled { get; set; } = false;

		#region Properties

		/// <summary>
		/// Is children flipped in the end
		/// </summary>
		public static readonly BindableProperty IsFlipEnabledProperty =
			BindableProperty.Create("IsFlipEnabled", typeof(bool), typeof(CarouselView), true);

		public bool IsFlipEnabled
		{
			get { return (bool)GetValue(IsFlipEnabledProperty); }
			set { SetValue(IsFlipEnabledProperty, value); }
		}

        /// <summary>
        /// Is user pan enabled
        /// </summary>
        public static readonly BindableProperty IsPanEnabledProperty =
            BindableProperty.Create("IsPanEnabled", typeof(bool), typeof(CarouselView), true);

        public bool IsPanEnabled
        {
            get { return (bool)GetValue(IsPanEnabledProperty); }
            set { SetValue(IsPanEnabledProperty, value); }
        }

        /// <summary>
        /// Scroll items or pixels
        /// </summary>
        public static readonly BindableProperty SnapPointsTypeProperty =
			BindableProperty.Create("SnapPointsType", typeof(SnapPointsTypes), typeof(CarouselView), SnapPointsTypes.None);

		public SnapPointsTypes SnapPointsType
		{
			get { return (SnapPointsTypes)GetValue(SnapPointsTypeProperty); }
			set { SetValue(SnapPointsTypeProperty, value); }
		}

		/// <summary>
		/// Selected tabitem
		/// </summary>
		public static readonly BindableProperty CurrentItemIndexProperty =
			BindableProperty.Create("CurrentItemIndex", typeof(int), typeof(CarouselView), 0);

		public int CurrentItemIndex
		{
			get { return (int)GetValue(CurrentItemIndexProperty); }
			set { SetValue(CurrentItemIndexProperty, value); }
		}

		/// <summary>
		/// How focused item is located
		/// </summary>
		public static readonly BindableProperty SnapPointsAlignmentProperty =
			BindableProperty.Create("SnapPointsAlignment", typeof(SnapPointsAlignments), typeof(CarouselView), SnapPointsAlignments.Start);

		public SnapPointsAlignments SnapPointsAlignment
		{
			get { return (SnapPointsAlignments)GetValue(SnapPointsAlignmentProperty); }
			set { SetValue(SnapPointsAlignmentProperty, value); }
		}

        /// <summary>
        /// Command to execute when current item is changed animation is finished
        /// </summary>
        public static readonly BindableProperty ScrollEndedCommandProperty =
            BindableProperty.Create("ScrollEndedCommand", typeof(ICommand), typeof(CarouselView), null);

        public ICommand ScrollEndedCommand
        {
            get { return (ICommand)GetValue(ScrollEndedCommandProperty); }
            set { SetValue(ScrollEndedCommandProperty, value); }
        }

        #endregion

        /// <summary>
        /// Bring item to viewport by index
        /// </summary>
        /// <param name="index">Item index to show</param>
        /// <param name="isAnimated">Is movement animated</param>
		public void BringIntoViewport(int index, bool isAnimated = true)
		{
			if (m_layout != null)
			{
				m_layout.BringIntoViewport(index, SnapPointsAlignment, isAnimated);
			}
		}

        /// <summary>
        /// Called when ItemsLayout is changed
        /// </summary>
        protected override void OnItemsLayoutChanged(Layout<View> oldLayout, Layout<View> newLayout)
        {
            base.OnItemsLayoutChanged(oldLayout, newLayout);

            if (oldLayout != null && oldLayout is CarouselLayout)
            {
                CarouselLayout layout = oldLayout as CarouselLayout;
                layout.PanChanged -= OnPanChanged;
                layout.CurrentItemIndexChanged -= OnCurrentItemIndexChanged;
            }

            if (newLayout != null && newLayout is CarouselLayout)
            {
                m_layout = newLayout as CarouselLayout;
                InitializeLayout(m_layout);
            }
            else
            {
                m_layout = null;
            }
        }

		/// <summary>
		/// Set layout bindings and other default values
		/// </summary>
		private void InitializeLayout(CarouselLayout layout)
		{
			layout.HorizontalOptions = LayoutOptions.Fill;
			layout.VerticalOptions = LayoutOptions.Fill;
			layout.PanChanged += OnPanChanged;
            layout.CurrentItemIndexChanged += OnCurrentItemIndexChanged;
			layout.ScrollEnded += OnScrollEnded;
			layout.IsDebugEnabled = IsDebugEnabled;

			Binding bind = new Binding(CarouselView.IsFlipEnabledProperty.PropertyName);
			bind.Source = this;
			layout.SetBinding(CarouselLayout.IsFlipEnabledProperty, bind);

            bind = new Binding(CarouselView.IsPanEnabledProperty.PropertyName);
            bind.Source = this;
            layout.SetBinding(CarouselLayout.IsPanEnabledProperty, bind);

            bind = new Binding(CarouselView.SnapPointsTypeProperty.PropertyName);
			bind.Source = this;
			layout.SetBinding(CarouselLayout.SnapPointsTypeProperty, bind);

			bind = new Binding(CarouselView.CurrentItemIndexProperty.PropertyName);
			bind.Source = this;
			bind.Mode = BindingMode.TwoWay;
			layout.SetBinding(CarouselLayout.CurrentItemIndexProperty, bind);

			bind = new Binding(CarouselView.SnapPointsAlignmentProperty.PropertyName);
			bind.Source = this;
			bind.Mode = BindingMode.TwoWay;
			layout.SetBinding(CarouselLayout.SnapPointsAlignmentProperty, bind);

            bind = new Binding(CarouselView.ScrollEndedCommandProperty.PropertyName);
            bind.Source = this;
            bind.Mode = BindingMode.TwoWay;
            layout.SetBinding(CarouselLayout.ScrollEndedCommandProperty, bind);
        }

		/// <summary>
		/// Event when CarouselLayout pan changed
		/// </summary>
		protected void OnPanChanged(object sender, PanChangedArgs args)
		{
			// Continue event raise
            PanChanged?.Invoke(this, args);
		}

        /// <summary>
        /// Event to raise when layout focused index changed
        /// </summary>
		private void OnCurrentItemIndexChanged(object sender, int index)
		{
			CurrentItemIndexChanged?.Invoke(this, index);
		}

		private void OnScrollEnded(object sender, EventArgs e)
		{
			ScrollEnded?.Invoke(this, e);
		}
	}
}
