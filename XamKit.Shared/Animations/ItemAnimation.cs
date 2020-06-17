using Xamarin.Forms;

namespace XamKit
{
    public class ItemAnimation : IAnimation
	{
		public double? OpacityInit            { get; set; } = null;
		public double? OpacityStart           { get; set; } = null;
		public double? OpacityEnd             { get; set; } = null;
		public double  OpacityStartTime       { get; set; } = 0;
		public double  OpacityEndTime         { get; set; } = 1;
		public Easing  OpacityAnimationEasing { get; set; } = Easing.Linear;

		public double? OffsetXInit            { get; set; } = null;
		public double? OffsetXStart           { get; set; } = null;
		public double? OffsetXEnd             { get; set; } = null;
		public double  OffsetXStartTime       { get; set; } = 0;
		public double  OffsetXEndTime         { get; set; } = 1;

		public double? OffsetYInit            { get; set; } = null;
		public double? OffsetYStart           { get; set; } = null;
		public double? OffsetYEnd             { get; set; } = null;
		public double  OffsetYStartTime       { get; set; } = 0;
		public double  OffsetYEndTime         { get; set; } = 1;

		public Easing  OffsetAnimationEasing  { get; set; } = Easing.Linear;

		public double? ScaleHeightInit        { get; set; } = null;
		public double? ScaleHeightStart       { get; set; } = null;
        public double? ScaleHeightEnd         { get; set; } = null;
		public double  ScaleHeightStartTime   { get; set; } = 0;
		public double  ScaleHeightEndTime     { get; set; } = 1;

		public double? ScaleWidthInit         { get; set; } = null;
		public double? ScaleWidthStart        { get; set; } = null;
        public double? ScaleWidthEnd          { get; set; } = null;
		public double  ScaleWidthStartTime    { get; set; } = 0;
		public double  ScaleWidthEndTime      { get; set; } = 1;

		public Easing ScaleAnimationEasing    { get; set; } = Easing.Linear;

        public uint Duration { get; set; }

        public Animation Create(View target)
		{
			Animation animationGroup = new Animation();

            if (OffsetXStart.HasValue || OffsetXEnd.HasValue)
			{
                double startX = target.TranslationX;
                if (OffsetXStart.HasValue)
                {
                    startX = OffsetXStart.Value;
				}
                else if (OffsetXInit.HasValue)
                {
					startX = OffsetXInit.Value;
				}

                double endX = 0;
				if (OffsetXEnd.HasValue)
                {
                    endX = OffsetXEnd.Value;
                }

				if (OffsetXInit.HasValue)
				{
					target.TranslationX = OffsetXInit.Value;
				}

				Animation a = new Animation(d =>
				{
					target.TranslationX = d;
				}, startX, endX, OffsetAnimationEasing);

                animationGroup.Add(OffsetXStartTime, OffsetXEndTime, a);
			}

            if (OffsetYStart.HasValue|| OffsetYEnd.HasValue)
			{
                double startY = target.TranslationY;
				if (OffsetYStart.HasValue && startY <= 0)
				{
					startY = OffsetYStart.Value;
				}
                else if (OffsetYInit.HasValue && startY <= 0)
                {
					startY = OffsetYInit.Value;
				}

                double endY = 0;
				if (OffsetYEnd.HasValue)
				{
					endY = OffsetYEnd.Value;
				}

				if (OffsetYInit.HasValue)
				{
					target.TranslationY = OffsetYInit.Value;
				}

				Animation a = new Animation(d =>
				{
					target.TranslationY = d;
				}, startY, endY, OffsetAnimationEasing);

                animationGroup.Add(OffsetYStartTime, OffsetYEndTime, a);
			}

            if (OpacityStart.HasValue || OpacityEnd.HasValue)
            {
                double opacityStart = target.Opacity;
                if (OpacityStart.HasValue)
				{
					opacityStart = OpacityStart.Value;
				}
                else if (OpacityInit.HasValue)
                {
					opacityStart = OpacityInit.Value;
				}

                double opacityEnd = 1;
                if (OpacityEnd.HasValue)
				{
					opacityEnd = OpacityEnd.Value;
				}
				
                if (OpacityInit.HasValue)
				{
					target.Opacity = OpacityInit.Value;
				}

				Animation a = new Animation(d =>
                {
                    target.Opacity = d;
                }, opacityStart, opacityEnd, OpacityAnimationEasing);
	
                animationGroup.Add(OpacityStartTime, OpacityEndTime, a);
			}

            Size measuredSize = Size.Zero;

            if (ScaleHeightStart.HasValue || ScaleHeightEnd.HasValue)
			{
                double targetHeight = target.HeightRequest;
				bool isAutoHeight = false;

				if (target.HeightRequest < 0)
				{
					isAutoHeight = true;

                    // Measure size
					View parent = target.Parent as View;
					measuredSize = target.Measure(parent.Width, parent.Height, MeasureFlags.IncludeMargins).Request;
                    				
					// Remove margin and padding
                    targetHeight = measuredSize.Height - target.Margin.Top - target.Margin.Bottom;
					if (target is Layout)
					{
						Layout l = target as Layout;
                        targetHeight -= l.Padding.Top + l.Padding.Bottom;
					}
				}

				// Calculate height when scale animation starts
                double startHeight = targetHeight;
				if (ScaleHeightStart.HasValue)
				{
                    startHeight = ScaleHeightStart.Value * targetHeight;
				}
                else if (ScaleHeightInit.HasValue)
                {
					startHeight = ScaleHeightInit.Value * targetHeight;
				}

				// Calculate height when scale animation ends
                double endHeight = targetHeight;
                if (ScaleHeightEnd.HasValue)
				{
                    endHeight = ScaleHeightEnd.Value * targetHeight;
				}

                if (ScaleHeightInit.HasValue)
				{
					target.HeightRequest = ScaleHeightInit.Value * targetHeight;
				}

				Animation a = new Animation(d =>
                {
                    target.HeightRequest = d;

                }, startHeight, endHeight, ScaleAnimationEasing, finished: () =>
                {
                    if (isAutoHeight)
                    {
                        target.HeightRequest = -1;
                    }
                });
                animationGroup.Add(ScaleHeightStartTime, ScaleHeightEndTime, a);

                // Vertical margin and padding scale

				Thickness originalMargin = target.Margin;
                if (ScaleHeightInit.HasValue)
                {
                    target.Margin = new Thickness(target.Margin.Left, target.Margin.Top * ScaleHeightInit.Value, target.Margin.Right, target.Margin.Bottom * ScaleHeightInit.Value);
                }

				Thickness originalPadding = new Thickness(0, 0, 0, 0);
				Layout targetLayout = target as Layout;
				if (targetLayout != null)
				{
					originalPadding = targetLayout.Padding;

					if (ScaleHeightInit.HasValue)
					{
                        targetLayout.Padding = new Thickness(targetLayout.Padding.Left, targetLayout.Padding.Top * ScaleHeightInit.Value, targetLayout.Padding.Right, targetLayout.Padding.Bottom * ScaleHeightInit.Value);
                    }
				}

                Animation marginAndPaddingVerticalScaleAnimation = new Animation(d =>
				{
                    target.Margin = new Thickness(target.Margin.Left, originalMargin.Top * d, target.Margin.Right, originalMargin.Bottom * d);

                    if (targetLayout != null)
					{
                        targetLayout.Padding = new Thickness(targetLayout.Padding.Left, originalPadding.Top * d, targetLayout.Padding.Right, originalPadding.Bottom * d);
					}

				}, ScaleHeightStart ?? 1, ScaleHeightEnd ?? 1, ScaleAnimationEasing);

				animationGroup.Add(ScaleHeightStartTime, ScaleHeightEndTime, marginAndPaddingVerticalScaleAnimation);
			}

			if (ScaleWidthStart.HasValue || ScaleWidthEnd.HasValue)
			{
				double targetWidth = target.WidthRequest;
				bool isAutoWidth = false;

				if (targetWidth < 0)
				{
					isAutoWidth = true;
				
                    // Measure if not measured
                    if (measuredSize == Size.Zero)
                    {
                        // Measure size
                        View parent = target.Parent as View;
                        measuredSize = target.Measure(parent.Width, parent.Height, MeasureFlags.IncludeMargins).Request;
                    }

                    // Remove margin and padding
                    targetWidth = measuredSize.Width - target.Margin.Left - target.Margin.Right;
                    if (target is Layout)
                    {
                        Layout l = target as Layout;
                        targetWidth -= l.Padding.Left + l.Padding.Right;
                    }
                }

                // Calculate width when scale animation starts
                double startWidth = target.Scale * targetWidth;
				if (ScaleWidthStart.HasValue)
				{
					startWidth = ScaleWidthStart.Value * targetWidth;
				}
                else if (ScaleWidthInit.HasValue)
                {
					startWidth = ScaleWidthInit.Value * targetWidth;
				}

				// Calculate width when scale animation ends
                double endWidth = targetWidth;
				if (ScaleWidthEnd.HasValue)
				{
					endWidth = ScaleWidthEnd.Value * targetWidth;
				}

				if (ScaleWidthInit.HasValue)
				{
					target.WidthRequest = ScaleWidthInit.Value * targetWidth;
				}

				Animation widthAnimation = new Animation(d =>
				{
					target.WidthRequest = d;

				}, startWidth, endWidth, ScaleAnimationEasing, finished: () =>
				{
					if (isAutoWidth)
					{
						target.WidthRequest = -1;
					}
				});
				animationGroup.Add(ScaleWidthStartTime, ScaleWidthEndTime, widthAnimation);

				// Horizontal margin and padding scale

				Thickness originalMargin = target.Margin;
                if (ScaleWidthInit.HasValue)
                {
                    target.Margin = new Thickness(target.Margin.Left * ScaleWidthInit.Value, target.Margin.Top, target.Margin.Right * ScaleWidthInit.Value, target.Margin.Bottom);
                }

                Thickness originalPadding = new Thickness(0, 0, 0, 0);

                Layout targetLayout = target as Layout;
				if (targetLayout != null)
				{
					originalPadding = targetLayout.Padding;
                    if (ScaleWidthInit.HasValue)
                    {
                        targetLayout.Padding = new Thickness(targetLayout.Padding.Left * ScaleWidthInit.Value, targetLayout.Padding.Top, targetLayout.Padding.Right * ScaleWidthInit.Value, targetLayout.Padding.Bottom);
                    }
				}

				Animation marginAndPaddingVerticalScaleAnimation = new Animation(d =>
				{
                    target.Margin = new Thickness(originalMargin.Left * d, target.Margin.Top, originalMargin.Right * d, target.Margin.Bottom);

					if (targetLayout != null)
					{
                        targetLayout.Padding = new Thickness(originalPadding.Left * d, targetLayout.Padding.Top, originalPadding.Right * d, targetLayout.Padding.Bottom);
					}

				}, ScaleWidthStart ?? 1, ScaleWidthEnd ?? 1, ScaleAnimationEasing);

				animationGroup.Add(ScaleWidthStartTime, ScaleWidthEndTime, marginAndPaddingVerticalScaleAnimation);
			}

			return animationGroup;
		}
	}
}
