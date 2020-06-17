using System;
using System.Globalization;
using Xamarin.Forms;

namespace XamKit
{
    public class IsNullOrEmptyToBoolConverter : IValueConverter
    {
        public bool IfTrue { get; set; } = true;
        public bool IfFalse { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return IfTrue;
            }
            else if (value is string stringValue && string.IsNullOrEmpty(stringValue))
            {
                return IfTrue;
            }
            else
            {
                return IfFalse;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
