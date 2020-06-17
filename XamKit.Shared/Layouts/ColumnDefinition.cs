using System;

// Xamarin forms
using Xamarin.Forms;

namespace XamKit
{
	/// <summary>
	/// One column definition. Width priority: 1. Width is absolute 2. MinWidth 3. MaxWidth 4. Widt is *. Auto width is not supported!
	/// </summary>
	public class ColumnDefinition : BindableObject
	{		
		#region Properties

		public static readonly BindableProperty WidthProperty =
			BindableProperty.Create("Width", typeof(GridLength), typeof(ColumnDefinition), GridLength.Star);

		public GridLength Width
		{
			get { return (GridLength)GetValue(WidthProperty); }
			set { SetValue(WidthProperty, value); }
		}
		/*
		public static readonly BindableProperty MaxWidthProperty =
			BindableProperty.Create("MaxWidth", typeof(double), typeof(ColumnDefinition), double.MaxValue);

		public double MaxWidth
		{
			get { return (double)GetValue(MaxWidthProperty); }
			set { SetValue(MaxWidthProperty, value); }
		}

		public static readonly BindableProperty MinWidthProperty =
			BindableProperty.Create("MinWidth", typeof(double), typeof(ColumnDefinition), 0.0);

		public double MinWidth
		{
			get { return (double)GetValue(MinWidthProperty); }
			set { SetValue(MinWidthProperty, value); }
		}
		*/
		public static readonly BindableProperty ActualWidthProperty =
			BindableProperty.Create("ActualWidth", typeof(double), typeof(ColumnDefinition), 0.0);

		public double ActualWidth
		{
			get { return (double)GetValue(ActualWidthProperty); }
			internal set { SetValue(ActualWidthProperty, value); }
		}

		#endregion
	}
}
