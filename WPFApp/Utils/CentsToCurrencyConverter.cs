using System;
using System.Globalization;
using System.Windows.Data;

namespace Utils
{
    public class CentsToCurrencyConverter : IValueConverter
    {
        // value: int/long en centavos
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "$0.00";

            if (!decimal.TryParse(value.ToString(), out var cents))
                return "$0.00";

            var dollars = cents / 100m; // de centavos a dólares

            // Formato dólar fijo (en-US)
            return string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:C}", dollars);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // No lo vamos a usar para editar, así que no implementamos la vuelta
            throw new NotImplementedException();
        }
    }
}
