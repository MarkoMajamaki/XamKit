using System;

// Xamarin
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamKit
{
    [ContentProperty("Source")]
    public class ColorExtension : IMarkupExtension
    {
        public Color Source { get; set; }

        /// <summary>
        /// 1 = white, 0 = original color
        /// </summary>
        public string Lightness { get; set; } = null;

        /// <summary>
        /// 1 = black, 0 = original color
        /// </summary>
        public string Darkness { get; set; } = null;

        public object ProvideValue(IServiceProvider serviceProvider)
        {                            
            if (string.IsNullOrEmpty(Darkness) == false)
            {
                double darkness = 0;
                double.TryParse(Darkness, out darkness);
                return AnimationUtils.ColorTransform(Math.Min(1, darkness), Source, Color.Black);
            }
            else if (string.IsNullOrEmpty(Lightness) == false)
            {
                double lightness = 0;
                double.TryParse(Lightness, out lightness);

                Color c = AnimationUtils.ColorTransform(Math.Min(1, lightness), Source, Color.White);
                return c;
            }
            else
            {
                return Source;
            }
        }
    }
}
