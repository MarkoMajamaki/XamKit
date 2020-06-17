using Xamarin.Forms;

namespace XamKit
{
    public class PopAnimationGroup : NavigationAnimationGroup
    {
        public PopAnimationGroup()
        {
            IsDarkOverlayEnabled = false;
            IsShadowEnabled = false;

            uint duration = 400;
            Easing easing = Easing.CubicOut;

            In = new PopAnimation()
            {
                OpacityEasing = easing,
                TranslationYEasing = easing,
                ScaleEasing = easing,
                Duration = duration,
                OpacityInit = 0,
                OpacityStart = 0,
                OpacityEnd = 1,
                OpacityStartTime = 0.5,
                OpacityEndTime = 1,
                TranslationYInit = 100,
                TranslationYStart = 100,
                TranslationYEnd = 0,
                TranslationYStartTime = 0.5,
                TranslationYEndTime = 1,
            };

            BackIn = new PopAnimation()
            {
                OpacityEasing = easing,
                TranslationYEasing = easing,
                ScaleEasing = easing,
                Duration = duration,
                OpacityInit = 0,
                OpacityStart = 0,
                OpacityEnd = 1,
                OpacityStartTime = 0.5,
                OpacityEndTime = 1,
                ScaleInit = 0.95,
                ScaleStart = 0.95,
                ScaleEnd = 1,
                ScaleStartTime = 0.5,
                ScaleEndTime = 1,
            };

            Out = new PopAnimation()
            {
                OpacityEasing = easing,
                TranslationYEasing = easing,
                ScaleEasing = easing,
                Duration = duration,
                OpacityInit = 1,
                OpacityStart = 1,
                OpacityEnd = 0,
                OpacityStartTime = 0,
                OpacityEndTime = 0.5,
            };

            BackOut = new PopAnimation()
            {
                OpacityEasing = easing,
                TranslationYEasing = easing,
                ScaleEasing = easing,
                Duration = duration,
                OpacityInit = 1,
                OpacityStart = 1,
                OpacityEnd = 0,
                OpacityStartTime = 0,
                OpacityEndTime = 0.5,
            };

            ModalIn = new PopAnimation()
            {
                ScaleEasing = easing,
                Duration = 200,
                ScaleStart = 0.9,
                ScaleEnd = 1,
            };

            ModalOut = new PopAnimation()
            {
                OpacityEasing = easing,
                Duration = 200,
                OpacityEnd = 0,
            };
        }
    }
}
