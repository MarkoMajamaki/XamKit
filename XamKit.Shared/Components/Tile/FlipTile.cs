using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    public class FlipTile : Layout<View>
    {
        private int _currentContentIndex = 0;
        private bool _isFlipAnimationRunning = false;

        private const string _flipInAnimationName = "flipInAnimationName";
		private const string C_flipOutAnimationName = "flipOutAnimationName";

		#region Properties

        /// <summary>
        /// Flip animation duration (ms)
        /// </summary>
		public static readonly BindableProperty FlipAnimationDurationProperty =
            BindableProperty.Create("FlipAnimationDuration", typeof(int), typeof(FlipTile), 1250);

        public int FlipAnimationDuration
        {
            get { return (int)GetValue(FlipAnimationDurationProperty); }
            set { SetValue(FlipAnimationDurationProperty, value); }
        }

        /// <summary>
        /// Animation hold duration (ms) 
        /// </summary>
		public static readonly BindableProperty HoldDurationProperty =
			BindableProperty.Create("HoldDuration", typeof(int), typeof(FlipTile), 5000);
        
		public int HoldDuration
		{
			get { return (int)GetValue(HoldDurationProperty); }
			set { SetValue(HoldDurationProperty, value); }
		}

		/// <summary>
		/// Is content change animation running
		/// </summary>
		public static readonly BindableProperty IsAnimationRunningProperty =
            BindableProperty.Create("IsAnimationRunning", typeof(bool), typeof(FlipTile), true, propertyChanged: OnIsAnimationRunning);

        private static void OnIsAnimationRunning(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as FlipTile).OnIsAnimationRunning((bool)newValue);
        }

        public bool IsAnimationRunning
		{
			get { return (bool)GetValue(IsAnimationRunningProperty); }
			set { SetValue(IsAnimationRunningProperty, value); }
		}

        #endregion

		public FlipTile()
        {
            OnIsAnimationRunning(IsAnimationRunning);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            Size s = new Size();
            s.Width = double.IsInfinity(widthConstraint) ? 0 : widthConstraint;
            s.Height = double.IsInfinity(heightConstraint) ? 0 : heightConstraint;
            return new SizeRequest(s, s);
        }

        /// <summary>
        /// 
        /// </summary>
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
            if (_isFlipAnimationRunning)
            {
                return;
            }

            for (int i = 0; i < Children.Count; i++)
            {
                View child = Children[i];

                if (i != _currentContentIndex)
                {
                    child.RotationX = -90;
                }

                LayoutChildIntoBoundingRegion(child, new Rectangle(0, 0, width, height));
            }
        }

        /// <summary>
        /// 
        /// </summary>
		private async void OnIsAnimationRunning(bool newValue)
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
                    Animation animationGroup = new Animation();
                    
                    Animation flipOutAnimation = new Animation();
					Animation flipInAnimation = new Animation();

					View flipOutView = Children[_currentContentIndex];
                    View flipInView = Children[_currentContentIndex + 1 >= Children.Count ? 0 : _currentContentIndex + 1];

                    // Set next index
                    _currentContentIndex = _currentContentIndex + 1 >= Children.Count ? 0 : _currentContentIndex + 1;

                    LayoutChildIntoBoundingRegion(flipInView, new Rectangle(0, 0, Width, Height));
                    flipInView.RotationX = -90;

                    new Animation(d => flipOutView.RotationX = d, 0, 90)
                        .Commit(this, C_flipOutAnimationName, 16, (uint)(FlipAnimationDuration / 4), Easing.Linear, (arg1, arg2) => 
                    {
						new Animation(d => flipInView.RotationX = d, -90, 0)
							.Commit(this, _flipInAnimationName, 16, (uint)(FlipAnimationDuration / 2), Easing.SpringOut);
					});					
				}
                else
                {
					break;
				}
            }
        }
	}
}
