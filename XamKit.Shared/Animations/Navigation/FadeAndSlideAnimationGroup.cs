using System;
using Xamarin.Forms;

namespace XamKit
{
    public class FadeAndSlideAnimationGroup : NavigationAnimationGroup
    {
        public FadeAndSlideAnimationGroup()
        {
            IsDarkOverlayEnabled = false;
            IsShadowEnabled = false;

            uint duration = 400;
            Easing easing = Easing.CubicOut;

            In = new FadeAndSlideAnimation()
            {
                Easing = easing,
                Duration = duration,
                FadeStartTime = 0,
                FadeEndTime = 0.3,
                Direction = FadeAndSlideAnimation.FadeDirections.In
            };

            BackIn = new FadeAndSlideAnimation()
            {
                Easing = easing,
                Duration = duration,
                IsFadeEnabled = false,
                Direction = FadeAndSlideAnimation.FadeDirections.BackIn,
            };

            Out = new FadeAndSlideAnimation()
            {
                Easing = easing,
                Duration = duration,
                IsFadeEnabled = false,
                Direction = FadeAndSlideAnimation.FadeDirections.Out
            };

            BackOut = new FadeAndSlideAnimation()
            {
                Easing = easing,
                Duration = duration,
                FadeStartTime = 0,
                FadeEndTime = 1,
                Direction = FadeAndSlideAnimation.FadeDirections.BackOut
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
