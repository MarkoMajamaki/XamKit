using System;
using System.Threading.Tasks;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
	public static class AnimationUtils
	{		
		/// <summary>
		/// Run color animation
		/// </summary>
		/// <returns>Animation task</returns>
		public static Task ColorTo(this VisualElement self, Color fromColor, Color toColor, string animationName, Action<Color> callback, uint length = 250, Easing easing = null)
		{
			easing = easing ?? Easing.Linear;
			var taskCompletionSource = new TaskCompletionSource<bool>();

            self.Animate(animationName, (t) =>
            {
                callback(ColorTransform(t, fromColor, toColor));

            }, 16, length, easing, (v, c) => taskCompletionSource.SetResult(c));

			return taskCompletionSource.Task;
		}

        /// <summary>
        /// Color transform which could use in animations
        /// </summary>
        /// <returns>Transformed color</returns>
        /// <param name="t">Time from 0 to 1</param>
        /// <param name="fromColor">From color</param>
        /// <param name="toColor">To color</param>
        public static Color ColorTransform(double t, Color fromColor, Color toColor)
        {
            return Color.FromRgba(fromColor.R + t * (toColor.R - fromColor.R),
                                  fromColor.G + t * (toColor.G - fromColor.G),
                                  fromColor.B + t * (toColor.B - fromColor.B),
                                  fromColor.A + t * (toColor.A - fromColor.A));
        }

		/// <summary>
		/// Quint easing function
		/// </summary>
		public static Func<double, double> EaseOutQuint = (double t) => 
		{
			t--;
			return (t * t * t * t * t + 1);		
		};

        /// <summary>
        /// Check has animation subanimations
        /// </summary>
        public static bool HasSubAnimations(this Animation self)
        {
            foreach (Animation subAnim in self)
            {
                return true;
            }

            return false;
        }
	}
}

