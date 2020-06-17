using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace XamKit
{
    public class PopupScaleAnimation : IAnimation
    {
        public uint Duration { get; set; } = 200;

        public double OpacityStart { get; set; }
        public double OpacityEnd { get; set; }

        public double ScaleStart { get; set; }
        public double ScaleEnd { get; set; }

        public double ContentSlideDistance { get; set; } = 50;

        public Easing Easing { get; set; } = Easing.CubicOut;

        public Animation Create(View target)
        {
            PopupRootLayout rootLayout = (PopupRootLayout)target;
            Popup popup = rootLayout.Popup;

            rootLayout.Scale = ScaleStart;
            rootLayout.Opacity = OpacityStart;

            Animation animationGroup = new Animation();

            animationGroup.Add(0, 1, new Animation(d =>
            {
                rootLayout.Scale = ScaleStart + (ScaleEnd - ScaleStart) * d;
                rootLayout.Opacity = OpacityStart + (OpacityEnd - OpacityStart) * d;

                if (Popup.IsTopPlacement(popup.ActualPlacement) && Popup.IsRightPlacement(popup.ActualPlacement))
                {
                    rootLayout.TranslationX = ((rootLayout.Width * d) / 2) * (1 - d);
                    rootLayout.TranslationY = ((rootLayout.Height * d) / 2) * (1 - d);
                }
                else if (Popup.IsTopPlacement(popup.ActualPlacement) && Popup.IsRightPlacement(popup.ActualPlacement) == false)
                {
                    rootLayout.TranslationX = -((rootLayout.Width * d) / 2) * (1 - d);
                    rootLayout.TranslationY = ((rootLayout.Height * d) / 2) * (1 - d);
                }
                else if (Popup.IsTopPlacement(popup.ActualPlacement) == false && Popup.IsRightPlacement(popup.ActualPlacement))
                {
                    rootLayout.TranslationX = ((rootLayout.Width * d) / 2) * (1 - d);
                    rootLayout.TranslationY = -((rootLayout.Height * d) / 2) * (1 - d);
                }
                else if (Popup.IsTopPlacement(popup.ActualPlacement) == false && Popup.IsRightPlacement(popup.ActualPlacement) == false)
                {
                    rootLayout.TranslationX = -((rootLayout.Width * d) / 2) * (1 - d);
                    rootLayout.TranslationY = -((rootLayout.Height * d) / 2) * (1 - d);
                }
            }, 0, 1, Easing));

            animationGroup.Add(0, 1, new Animation(d =>
            {
                if (Popup.IsTopPlacement(popup.ActualPlacement) && Popup.IsRightPlacement(popup.ActualPlacement))
                {
                }
                else if (Popup.IsTopPlacement(popup.ActualPlacement) && Popup.IsRightPlacement(popup.ActualPlacement) == false)
                {
                }
                else if (Popup.IsTopPlacement(popup.ActualPlacement) == false && Popup.IsRightPlacement(popup.ActualPlacement))
                {
                }
                else if (Popup.IsTopPlacement(popup.ActualPlacement) == false && Popup.IsRightPlacement(popup.ActualPlacement) == false)
                {
                    rootLayout.Content.TranslationY = -ContentSlideDistance * (1 - d);
                }
            }, 0, 1, Easing.CubicOut));

            return animationGroup;
        }
    }
}
