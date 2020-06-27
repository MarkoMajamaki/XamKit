using System;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
	/// <summary>
	/// Interface which TabView header must implement to make possible change tabs
	/// </summary>
	public interface ITabBar
	{
		TabView Source { get; set; }
	}

    /// <summary>
    /// Tab view with header
    /// </summary>
	public class TabView : CarouselView
	{
		#region Properties

		/// <summary>
		/// Tab header view. Must implement ITabHeader interface.
		/// </summary>
		public static readonly BindableProperty HeaderViewTemplateProperty =
			BindableProperty.Create("HeaderViewTemplate", typeof(DataTemplate), typeof(TabView), null);

		public DataTemplate HeaderViewTemplate
		{
			get { return (DataTemplate)GetValue(HeaderViewTemplateProperty); }
			set { SetValue(HeaderViewTemplateProperty, value); }
		}

		/// <summary>
		/// Actual header view. Must implement ITabHeader interface.
		/// </summary>
		public static readonly BindableProperty HeaderViewProperty =
			BindableProperty.Create("HeaderView", typeof(View), typeof(TabView), null);

		public View HeaderView
		{
			get { return (View)GetValue(HeaderViewProperty); }
			set { SetValue(HeaderViewProperty, value); }
		}

		/// <summary>
		/// How much next and previous item is shown on left and right
		/// </summary>
		public static readonly BindableProperty PeekAreaInsetsProperty =
			BindableProperty.Create("PeekAreaInsets", typeof(double), typeof(TabView), 0.0);

        public double PeekAreaInsets
		{
			get { return (double)GetValue(PeekAreaInsetsProperty); }
			set { SetValue(PeekAreaInsetsProperty, value); }
		}

		#endregion

		#region Hidden properties

		public new SnapPointsTypes SnapPointsType
		{
			get
			{
				return base.SnapPointsType;
			}
			private set
			{
				base.SnapPointsType = value;
			}
		}

		#endregion

		public TabView() : base()
		{
			SnapPointsType = SnapPointsTypes.MandatorySingle;
		}

        /// <summary>
        /// Called when ItemsLayout is changed
        /// </summary>
        protected override void OnItemsLayoutChanged(Layout<View> oldLayout, Layout<View> newLayout)
        {
            base.OnItemsLayoutChanged(oldLayout, newLayout);

            if (newLayout is CarouselLayout layout)
            {
				Binding bind = new Binding(TabView.PeekAreaInsetsProperty.PropertyName);
                bind.Source = this;
                bind.Mode = BindingMode.TwoWay;
                layout.SetBinding(CarouselLayout.PeekAreaInsetsProperty, bind);
            }
        }
	}
}
