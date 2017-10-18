namespace FilterTreeViewLib.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class CountToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            if (value is int == false)
                return false;

            int convertValue = (int)value;

            if (convertValue > 0)
                return true;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
