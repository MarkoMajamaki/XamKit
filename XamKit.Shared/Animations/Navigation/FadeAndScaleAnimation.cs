using Xamarin.Forms;

namespace XamKit
{
    public class FadeAndScaleAnimation : IAnimation
	{
        public enum FadeDirections { In, Out, BackIn, BackOut }

        public FadeDirections Direction { get; set; }

        public uint Duration { get; set; }

        public Easing Easing { get; set; }

		public Animation Create(View target)
		{
			Animation group = new Animation();

            double scaleDepth = 0.2;

            if (Direction == FadeDirections.In)
            {
                target.Opacity = 0;

                Animation fade = new Animation(d =>
                {
                    target.Opacity = d;
                }, 0, 1, Easing);

                Animation scale = new Animation(d =>
                {
                    target.Scale = d;
                }, 1 - scaleDepth, 1, Easing);

                group.Add(0, 1, fade);
                group.Add(0, 1, scale);
            }
            else if (Direction == FadeDirections.BackIn)
            {
                target.Opacity = 0;

                Animation fade = new Animation(d =>
                {
                    target.Opacity = d;
                }, 0, 1, Easing);

                Animation scale = new Animation(d =>
                {
                    target.Scale = d;
                }, 1 + scaleDepth, 1, Easing);

                group.Add(0, 1, fade);
                group.Add(0, 1, scale);
            }
            else if (Direction == FadeDirections.BackOut)
            {
                Animation fade = new Animation(d =>
                {
                    target.Opacity = d;
                }, 1, 0, Easing);

                Animation scale = new Animation(d =>
                {
                    target.Scale = d;
                }, 1, 1 - scaleDepth, Easing);

                group.Add(0, 1, fade);
                group.Add(0, 1, scale);
            }
            else // out
            {
                Animation fade = new Animation(d =>
                {
                    target.Opacity = d;
                }, 1, 0, Easing);

                Animation scale = new Animation(d =>
                {
                    target.Scale = d;
                }, 1, 1 + scaleDepth, Easing);

                group.Add(0, 1, fade);
                group.Add(0, 1, scale);
            }

            return group;
		}
	}
}

