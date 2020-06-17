using Xamarin.Forms;

namespace XamKit
{
	public class SlideAnimationGroup : NavigationAnimationGroup
	{
		public SlideAnimationGroup()
		{
            Easing easing = Easing.CubicOut;
            uint duration = 400;

            In = new SlideAnimation()
			{
                Easing = easing, 
                Duration = duration,
                Direction = SlideAnimation.SlideDirections.RightToCentre
			};

			BackIn = new SlideAnimation() 
			{ 
                Easing = easing, 
                Duration = duration,
                Direction = SlideAnimation.SlideDirections.LeftToCentre,
                Lenght = SlideAnimation.Lenghts.StartMiddle
			};

			Out = new SlideAnimation() 
			{
                Easing = easing, 
                Duration = duration,
                Direction = SlideAnimation.SlideDirections.CentreToLeft,
                Lenght = SlideAnimation.Lenghts.EndMiddle
            };

			BackOut = new SlideAnimation()
			{
                Easing = easing, 
                Duration = duration,
                Direction = SlideAnimation.SlideDirections.CentreToRight
            };

            ModalIn = new PopAnimation()
            {
                OpacityEasing = easing,
                TranslationYEasing = easing,
                ScaleEasing = easing,
                Duration = duration,
                OpacityInit = 0,
                OpacityStart = 0,
                OpacityEnd = 1,
                OpacityStartTime = 0,
                OpacityEndTime = 1,
                TranslationYInit = 100,
                TranslationYStart = 100,
                TranslationYEnd = 0,
                TranslationYStartTime = 0,
                TranslationYEndTime = 1,
            };

            ModalOut = new PopAnimation()
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
        }
	}
}

