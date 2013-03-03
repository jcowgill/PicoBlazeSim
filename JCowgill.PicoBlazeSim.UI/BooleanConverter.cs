using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JCowgill.PicoBlazeSim.UI
{
    /// <summary>
    /// Converts a boolean to another type
    /// </summary>
    [ValueConversion(typeof(bool), typeof(object))]
    public class BooleanConverter : IValueConverter
    {
        /// <summary>
        /// Value to produce if the input is true
        /// </summary>
        public object True { get; set; }

        /// <summary>
        /// Value to produce if the input is false
        /// </summary>
        public object False { get; set; }

        /// <summary>
        /// Creates a new BooleanConveter using default values for True and False
        /// </summary>
        public BooleanConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Do boolean conversion
            if (value is bool)
                return ((bool) value ? this.True : this.False);
            else
                return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Is the value True or False?
            if (value.Equals(this.True))
                return true;
            else if (value.Equals(this.False))
                return false;

            return DependencyProperty.UnsetValue;
        }
    }
}
