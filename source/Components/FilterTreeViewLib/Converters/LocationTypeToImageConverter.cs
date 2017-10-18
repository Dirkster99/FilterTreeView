namespace FilterTreeViewLib.Converters
{
    using BusinessLib.Models;
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Converts an enum <seealso cref="LocationType"/> into its image resource.
    /// The corresponding image resource must be present in the applications's
    /// resource dictionary.
    /// </summary>
    [ValueConversion(typeof(LocationType), typeof(System.Windows.Media.Imaging.BitmapImage))]
    public class LocationTypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Binding.DoNothing;

            if (value is LocationType == false)
                return Binding.DoNothing;

            LocationType typeOfLocation = (LocationType)value;
            string locationIconName = string.Empty;

            try
            {
                switch (typeOfLocation)
                {
                    case LocationType.Country:
                        locationIconName = "CountryImage";
                        break;

                    case LocationType.Region:
                        locationIconName = "RegionImage";
                        break;

                    case LocationType.City:
                        locationIconName = "CityImage";
                        break;

                    case LocationType.Unknown:
                        return Binding.DoNothing;

                    default:
                        throw new ArgumentOutOfRangeException(typeOfLocation.ToString());
                }

                return Application.Current.Resources[locationIconName];
            }
            catch
            {
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
