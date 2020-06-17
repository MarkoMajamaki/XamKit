// Xamarin
using Xamarin.Forms;

namespace XamKit
{
    public class PopAnimation : IAnimation
    {
        public uint Duration { get; set; }

        public double OpacityStartTime { get; set; } = 0;
        public double OpacityEndTime { get; set; } = 1;
        public double? OpacityInit { get; set; } = null;
        public double? OpacityStart { get; set; } = null;
        public double? OpacityEnd { get; set; } = null;
        public Easing OpacityEasing { get; set; }

        public double ScaleStartTime { get; set; } = 0;
        public double ScaleEndTime { get; set; } = 1;
        public double? ScaleInit { get; set; } = null;
        public double? ScaleStart { get; set; } = null;
        public double? ScaleEnd { get; set; } = null;
        public Easing ScaleEasing { get; set; }

        public double TranslationYStartTime { get; set; } = 0;
        public double TranslationYEndTime { get; set; } = 1;
        public double? TranslationYInit { get; set; } = null;
        public double? TranslationYStart { get; set; } = null;
        public double? TranslationYEnd { get; set; } = null;
        public Easing TranslationYEasing { get; set; }

        public Animation Create(View target)
        {
            Animation group = new Animation();

            if (ScaleEnd != null)
            {
                if (ScaleInit != null)
                {
                    target.Scale = ScaleInit.Value;
                }

                double actualStart = ScaleStart ?? target.Scale;

                group.Add(ScaleStartTime, ScaleEndTime, new Animation(d =>
                {
                    target.Scale = d;
                }, actualStart, ScaleEnd.Value, ScaleEasing));
            }

            if (OpacityEnd != null)
            {
                if (OpacityInit != null)
                {
                    target.Opacity = OpacityInit.Value;
                }

                double actualStart = OpacityStart ?? target.Opacity;

                group.Add(OpacityStartTime, OpacityEndTime, new Animation(d =>
                {
                    target.Opacity = d;
                }, actualStart, OpacityEnd.Value, OpacityEasing));
            }

            if (TranslationYEnd != null)
            {
                if (TranslationYInit.HasValue && double.IsNaN(TranslationYInit.Value) && target.Parent is View p1)
                {
                    target.TranslationY = p1.Height;
                }
                else if (TranslationYInit != null)
                {
                    target.TranslationY = TranslationYInit.Value;
                }

                double actualStart = 0;

                if (TranslationYStart.HasValue && double.IsNaN(TranslationYStart.Value) && target.Parent is View p2)
                {
                    actualStart = p2.Height;
                }
                else if (TranslationYStart != null)
                {
                    actualStart = TranslationYStart.Value;
                }
                else
                {
                    actualStart = target.TranslationY;
                }

                double actualEnd = 0;

                if (TranslationYEnd.HasValue && double.IsNaN(TranslationYEnd.Value) && target.Parent is View p3)
                {
                    actualEnd = p3.Height;
                }
                else if (TranslationYEnd != null)
                {
                    actualEnd = TranslationYEnd.Value;
                }
                else
                {
                    actualEnd = target.TranslationY;
                }

                group.Add(TranslationYStartTime, TranslationYEndTime, new Animation(d =>
                {
                    target.TranslationY = d;
                }, actualStart, actualEnd, TranslationYEasing));
            }

            return group;
        }
    }
}
