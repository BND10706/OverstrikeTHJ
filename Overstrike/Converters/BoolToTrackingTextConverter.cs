using System;
using System.Globalization;
using System.Windows.Data;

namespace Overstrike.Converters
{
    /// <summary>
    /// Converter to change button text based on tracking state
    /// </summary>
    public class BoolToTrackingTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isTracking)
            {
                return isTracking ? "Stop Tracking" : "Start Tracking";
            }
            return "Start Tracking";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
