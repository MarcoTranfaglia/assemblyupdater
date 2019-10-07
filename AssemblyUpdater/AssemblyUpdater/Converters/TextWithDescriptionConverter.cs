using System;
using System.Globalization;
using System.Windows.Data;

namespace AssemblyUpdater.Converter
{
    public class TextWithDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value?.ToString() is string valText && parameter is string descriptionText)
            {
                return descriptionText + valText;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
