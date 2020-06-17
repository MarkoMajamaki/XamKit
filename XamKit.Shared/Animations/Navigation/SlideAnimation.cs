using System;
using Xamarin.Forms;

namespace XamKit
{
    public class SlideAnimation : IAnimation
	{
        public enum SlideDirections
        {
            UpToCentre,
            DownToCentre,
            LeftToCentre,
            RightToCentre,
            CentreToDown,
            CentreToUp,
            CentreToLeft,
            CentreToRight
        }

        public enum Lenghts 
        { 
            Default, 
            StartMiddle,
            EndMiddle 
        }

        /// <summary>
        /// Slide direction to and from centre
        /// </summary>
        public SlideDirections Direction { get; set; } = SlideDirections.RightToCentre;

        /// <summary>
        /// Is slide started / ended on middle or outside of target / parent bounds
        /// </summary>
		public Lenghts Lenght { get; set; }

        /// <summary>
        /// Animation duration
        /// </summary>
        public uint Duration { get; set; }

        /// <summary>
        /// Animation easing function
        /// </summary>
        /// <value>The animation easing.</value>
        public Easing Easing { get; set; }

        /// <summary>
        /// Create animation
        /// </summary>
		public Animation Create(View target)
        {
            Animation offsetAnimation = null;

            View referenceElement = target.Parent as View;

            if (Direction == SlideDirections.LeftToCentre)
            {
                // Init before animation starts
                if (Lenght == Lenghts.StartMiddle)
                {
                    target.TranslationX = -(referenceElement.Width / 2);
                }
                else
                {
                    target.TranslationX = -referenceElement.Width;
                }

                offsetAnimation = new Animation(d =>
                {
                    double x = 0;

                    if (Lenght == Lenghts.StartMiddle)
                    {
                        x = -(referenceElement.Width / 2) * d;
                    }
                    else
                    {
                        x = -referenceElement.Width * d;
                    }

                    target.TranslationX = x;

                }, 1, 0, Easing);
            }
            else if (Direction == SlideDirections.RightToCentre)
            {
                // Init before animation starts
                if (Lenght == Lenghts.StartMiddle)
                {
                    target.TranslationX = (referenceElement.Width / 2);
                }
                else
                {
                    target.TranslationX = referenceElement.Width;
                }

                offsetAnimation = new Animation(d =>
                {
                    double x = 0;

                    if (Lenght == Lenghts.StartMiddle)
                    {
                        x = (referenceElement.Width / 2) * d;
                    }
                    else
                    {
                        x = referenceElement.Width * d;
                    }

                    target.TranslationX = x;

                }, 1, 0, Easing);
            }
            else if (Direction == SlideDirections.UpToCentre)
            {
                // Init before animation starts
                if (Lenght == Lenghts.StartMiddle)
                {
                    target.TranslationY = -(referenceElement.Height / 2);
                }
                else
                {
                    target.TranslationY = -referenceElement.Height;
                }

                offsetAnimation = new Animation(d =>
                {
                    double y = 0;

                    if (Lenght == Lenghts.StartMiddle)
                    {
                        y = -(referenceElement.Height / 2) * d;
                    }
                    else
                    {
                        y = -referenceElement.Height * d;
                    }

                    target.TranslationY = y;

                }, 1, 0, Easing);
            }
            else if (Direction == SlideDirections.DownToCentre)
            {
                // Init before animation starts
                if (Lenght == Lenghts.StartMiddle)
                {
                    target.TranslationY = (referenceElement.Height / 2);
                }
                else
                {
                    target.TranslationY = referenceElement.Height;
                }

                offsetAnimation = new Animation(d =>
                {
                    double y = 0;

                    if (Lenght == Lenghts.StartMiddle)
                    {
                        y = (referenceElement.Height / 2) * d;
                    }
                    else
                    {
                        y = referenceElement.Height * d;
                    }

                    target.TranslationY = y;

                }, 1, 0, Easing);
            }
            else if (Direction == SlideDirections.CentreToDown)
            {
                // Init before animation starts
                target.TranslationY = 0;

                offsetAnimation = new Animation(d =>
                {
                    double y = 0;

                    if (Lenght == Lenghts.StartMiddle)
                    {
                        y = (referenceElement.Height / 2) * d;
                    }
                    else
                    {
                        y = referenceElement.Height * d;
                    }

                    target.TranslationY = y;

                }, 0, 1, Easing);
            }
            else if (Direction == SlideDirections.CentreToLeft)
            {
                // Init before animation starts
                target.TranslationX = 0;

                offsetAnimation = new Animation(d =>
                {
                    double x = 0;

                    if (Lenght == Lenghts.EndMiddle)
                    {
                        x = -(referenceElement.Width / 2) * d;
                    }
                    else
                    {
                        x = -referenceElement.Width * d;
                    }

                    target.TranslationX = x;

                }, 0, 1, Easing);
            }
            else if (Direction == SlideDirections.CentreToRight)
            {
                // Init before animation starts
                target.TranslationX = 0;

                offsetAnimation = new Animation(d =>
                {
                    double x = 0;

                    if (Lenght == Lenghts.EndMiddle)
                    {
                        x = (referenceElement.Width / 2) * d;
                    }
                    else
                    {
                        x = referenceElement.Width * d;
                    }

                    target.TranslationX = x;

                }, 0, 1, Easing);
            }
            else
            {
                throw new NotImplementedException();
            }

            return offsetAnimation;
        }
	}
}

