using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace XamKit.UWP
{
    public static class ColorConverter
    {
        public static Color ToWindows(this Xamarin.Forms.Color xamarinColor)
        {
            if (xamarinColor.A == -1)
            {
                return new Color()
                {
                    A = 0x00,
                    R = 0x00,
                    G = 0x00,
                    B = 0x00
                };
            }
            else
            {
                return new Color()
                {
                    A = Convert.ToByte(xamarinColor.A * 255),
                    B = Convert.ToByte(xamarinColor.B * 255),
                    G = Convert.ToByte(xamarinColor.G * 255),
                    R = Convert.ToByte(xamarinColor.R * 255)
                };
            }
        }

        public static Windows.UI.Xaml.Thickness ToWindows(this Xamarin.Forms.Thickness thickness)
        {
            return new Windows.UI.Xaml.Thickness(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
        }
    }
}
