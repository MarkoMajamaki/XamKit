using System;
using System.Threading.Tasks;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    public class CarouselTile : CarouselView
    {
		#region Properties

		public static readonly BindableProperty HoldDurationProperty =
			BindableProperty.Create("HoldDuration", typeof(int), typeof(CarouselTile), 5000);

		public int HoldDuration
		{
			get { return (int)GetValue(HoldDurationProperty); }
			set { SetValue(HoldDurationProperty, value); }
		}

        /// <summary>
        /// Is content change animation running
        /// </summary>
        public static readonly BindableProperty IsAnimationRunningProperty =
            BindableProperty.Create("IsAnimationRunning", typeof(bool), typeof(CarouselTile), false, propertyChanged: OnIsAnimationRunning);

		private static void OnIsAnimationRunning(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as CarouselTile).OnIsAnimationRunning((bool)oldValue, (bool)newValue);
		}

		public bool IsAnimationRunning
        {
            get { return (bool)GetValue(IsAnimationRunningProperty); }
            set { SetValue(IsAnimationRunningProperty, value); }
        }

		#endregion

		public CarouselTile()
        {
        }

		/// <summary>
		/// 
		/// </summary>
		private async void OnIsAnimationRunning(bool oldValue, bool newValue)
		{
			if (newValue)
			{
				await StartFlipAnimation();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private async Task StartFlipAnimation()
		{
			while (true && IsVisible)
			{
				await Task.Delay(HoldDuration);

				if (IsAnimationRunning && VisualTreeHelper.GetParent<NavigationPage>(this) != null)
				{
					// Set next index
					CurrentItemIndex = CurrentItemIndex + 1 >= Children.Count ? 0 : CurrentItemIndex + 1;
                }
				else
				{
					break;
				}
			}
		}
	}
}
