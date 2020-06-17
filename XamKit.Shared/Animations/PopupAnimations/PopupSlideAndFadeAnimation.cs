using System;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{        
    public class PopupSlideAndFadeAnimation : IAnimation
	{
        public double SlideStartOffset { get; set; }

		public double? OpacityInit { get; set; } = null;
		public double OpacityStart { get; set; } = 0;
		public double OpacityEnd { get; set; } = 1;
		public double OpacityStartTime { get; set; } = 0;
		public double OpacityEndTime { get; set; } = 1;

        public uint Duration { get; set; } = 300;

        public Easing Easing { get; set; }

        public Animation Create(View target)
		{
            PopupRootLayout rootLayout = (PopupRootLayout)target;

            // If full screen popup which is closing
            if (rootLayout.Popup.IsOpen == false && rootLayout.Popup.ActualPlacement == PopupPlacements.FullScreen)
            {
                return new Animation(d =>
                {
                    rootLayout.TranslationY = rootLayout.Height * d;
                }, 0, 1);
            }

            Animation animationGroup = new Animation();

            if (OpacityStartTime.Equals(OpacityEndTime) == false && OpacityStart != OpacityEnd && OpacityInit != OpacityEnd)
            {
                if (OpacityInit.HasValue)
                {
                    rootLayout.Opacity = OpacityInit.Value;
                }

                Animation fadeAnimation = new Animation(d =>
                {
                    rootLayout.Opacity = d;
                }, OpacityStart, OpacityEnd);

                animationGroup.Add(OpacityStartTime, OpacityEndTime, fadeAnimation);
            }

            double animatedValue = 0;

            if (SlideStartOffset != 0)
            {
                EventHandler sizeChanged = null;
                sizeChanged = (s, a) =>
                {
                    rootLayout.Content.TranslationY = CalculateTranslateY(animatedValue, rootLayout);
                };

                rootLayout.SizeChanged += sizeChanged;

                // Init TranslationY
                rootLayout.Content.TranslationY = CalculateTranslateY(0, rootLayout);

                Animation slideAnimation = new Animation(d =>
                {
                    animatedValue = d;
                    rootLayout.Content.TranslationY = CalculateTranslateY(d, rootLayout);
                }, 0, 1, AnimationUtils.EaseOutQuint, finished: () =>
                {
                    rootLayout.Content.SizeChanged -= sizeChanged;
                });

                animationGroup.Add(0, 1, slideAnimation);
            }

            return animationGroup;
        }

        private double CalculateTranslateY(double process, PopupRootLayout rootLayout)
        {
            if (Popup.IsTopPlacement(rootLayout.Popup.ActualPlacement))
            {
                return rootLayout.Height - ((rootLayout.Height - SlideStartOffset) * process) - SlideStartOffset;
            }
            else
            {
                return SlideStartOffset + ((rootLayout.Height - SlideStartOffset) * process) - rootLayout.Height;
            }
        }
    }
}
