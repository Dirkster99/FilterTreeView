namespace FilterTreeViewLib.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class BoolToVisibilityConverter : IValueConverter
    {
        public BoolToVisibilityConverter()
        {
            this.True = Visibility.Visible;
            this.False = Visibility.Collapsed;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Binding.DoNothing;

            if (value is bool == false)
                return Binding.DoNothing;

            bool input = (bool)value;

            if (input == true)
                return True;

            return False;
        }

        public Visibility True { get; set; }

        public Visibility False { get; set; }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
