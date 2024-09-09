using System;
using System.Globalization;
using System.Windows.Data;

namespace T4C_Translator
{
	public class NumberWithCommaConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string strValue && double.TryParse(strValue, out double number))
			{
				return number.ToString("N0", culture);
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string strValue)
			{
				strValue = strValue.Replace(",", "");
				if (double.TryParse(strValue, out double number))
				{
					return number.ToString();
				}
			}
			return value;
		}
	}
}