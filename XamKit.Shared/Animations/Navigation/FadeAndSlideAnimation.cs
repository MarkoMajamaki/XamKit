using System;
using Xamarin.Forms;

namespace XamKit
{
    public class FadeAndSlideAnimation : IAnimation
    {
        public enum FadeDirections { In, Out, BackIn, BackOut }

        public FadeDirections Direction { get; set; }

        public double SlideOffset { get; set; } = 100;

        public uint Duration { get; set; }

        public Easing Easing { get; set; }

        public double FadeEndTime { get; set; } = 1;
        public double FadeStartTime { get; set; } = 0;
        public bool IsFadeEnabled { get; set; } = true;

        public Animation Create(View target)
        {
            Animation group = new Animation();
            
            if (Direction == FadeDirections.In)
            {
                if (IsFadeEnabled)
                {
                    target.Opacity = 0;

                    Animation fade = new Animation(d =>
                    {
                        target.Opacity = d;
                    }, 0, 1, Easing);

                    group.Add(FadeStartTime, FadeEndTime, fade);
                }

                Animation slide = new Animation(d =>
                {
                    target.TranslationY = d;
                }, SlideOffset, 0, Easing);

                group.Add(0, 1, slide);
            }
            else if (Direction == FadeDirections.BackIn)
            {
                if (IsFadeEnabled)
                {
                    target.Opacity = 0;

                    Animation fade = new Animation(d =>
                    {
                        target.Opacity = d;
                    }, 0, 1, Easing);

                    group.Add(FadeStartTime, FadeEndTime, fade);
                }

                Animation slide = new Animation(d =>
                {
                    target.TranslationY = d;
                }, -SlideOffset, 0, Easing);

                group.Add(0, 1, slide);
            }
            else if (Direction == FadeDirections.BackOut)
            {
                if (IsFadeEnabled)
                {
                    Animation fade = new Animation(d =>
                    {
                        target.Opacity = d;
                    }, 1, 0, Easing);

                    group.Add(FadeStartTime, FadeEndTime, fade);
                }

                Animation slide = new Animation(d =>
                {
                    target.TranslationY = d;
                }, 0, SlideOffset, Easing);

                group.Add(0, 1, slide);
            }
            else // out
            {
                if (IsFadeEnabled)
                {
                    Animation fade = new Animation(d =>
                    {
                        target.Opacity = d;
                    }, 1, 0, Easing);

                    group.Add(FadeStartTime, FadeEndTime, fade);
                }

                Animation slide = new Animation(d =>
                {
                    target.TranslationY = d;
                }, 0, -SlideOffset, Easing);

                group.Add(0, 1, slide);
            }

            return group;
        }
    }
}
