using Xamarin.Forms;

namespace XamKit
{
	public class FadeAndScaleAnimationGroup : NavigationAnimationGroup
	{
		public FadeAndScaleAnimationGroup()
        {
            IsDarkOverlayEnabled = false;
            IsShadowEnabled = false;

            uint duration = 400;
            Easing easing = Easing.CubicOut;

            In = new FadeAndScaleAnimation()
            {
                Easing = easing,
                Duration = duration,
                Direction = FadeAndScaleAnimation.FadeDirections.In
            };

            BackIn = new FadeAndScaleAnimation()
            {
                Easing = easing,
                Duration = duration,
                Direction = FadeAndScaleAnimation.FadeDirections.BackIn,
            };

            Out = new FadeAndScaleAnimation()
            {
                Easing = easing,
                Duration = duration,
                Direction = FadeAndScaleAnimation.FadeDirections.Out
            };

            BackOut = new FadeAndScaleAnimation()
            {
                Easing = easing,
                Duration = duration,
                Direction = FadeAndScaleAnimation.FadeDirections.BackOut
            };

            ModalIn = new PopAnimation()
            {
                OpacityEasing = easing,
                TranslationYEasing = easing,
                ScaleEasing = easing,
                Duration = duration,
                ScaleStart = 0.8,
                ScaleEnd = 1,
                OpacityStart = 0,
                OpacityEnd = 1,
            };

            ModalOut = new PopAnimation()
            {
                OpacityEasing = easing,
                TranslationYEasing = easing,
                ScaleEasing = easing,
                Duration = duration,
                ScaleStart = 1,
                ScaleEnd = 0.8,
                OpacityStart = 0,
                OpacityEnd = 1,
            };
        }
	}
}

