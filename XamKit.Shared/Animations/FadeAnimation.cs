using System;

// Xamarin forms
using Xamarin.Forms;

namespace XamKit
{    
    public class FadeAnimation : IAnimation
	{
        public enum FadeDirections { In, Out }

        public FadeDirections FadeDirection { get; set; }

        public uint Duration { get; set; }

        public Easing Easing { get; set; }

		public double? StartOpacity { get; set; }

		public Animation Create(View target)
		{
			if (StartOpacity.HasValue)
			{
				target.Opacity = StartOpacity.Value;
			}

            Animation fadeAnimation = null;

            if (FadeDirection == FadeDirections.Out)
            {
				fadeAnimation = new Animation(d =>
				{
					target.Opacity = d;
				}, target.Opacity, 0, Easing);
			}
            else
            {
				fadeAnimation = new Animation(d =>
				{
					target.Opacity = d;
				}, target.Opacity, 1, Easing);
			}

			return fadeAnimation;
		}
	}
}
