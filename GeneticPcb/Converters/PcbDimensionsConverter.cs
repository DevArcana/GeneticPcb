using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace GeneticPcb.Converters
{
    public class PcbDimensionsConverter : IValueConverter
    {
        private static int _scale = 32;
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double) ((int) value * _scale);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}