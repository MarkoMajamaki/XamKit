using System;
using System.Globalization;

// Xamarin
using Xamarin.Forms;

namespace XamKit
{
	public class BoolToOpacityConverter : IValueConverter
	{
		public double OpacityIfTrue { get; set; } = 1;
		public double OpacityIfFalse { get; set; } = 0;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool v = (bool)value;

			if (v)
			{
				return OpacityIfTrue;
			}
			else
			{
				return OpacityIfFalse;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
