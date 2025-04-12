using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace TravelPlanner.Converter
{
    public class DateConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                return date.ToString("MMM d, yyyy"); // Format the date
            }
            return value; // Or handle null/invalid values differently
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Often not needed for display-only conversions
            throw new NotImplementedException();
        }

    }
}
