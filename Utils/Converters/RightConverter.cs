using System;
using System.Globalization;
using System.Windows.Data;

namespace GridSetter.Utils.Converters
{
	/// <summary>
	/// Allow the children to be pushed to the right inside the canvas.
	/// </summary>
	internal sealed class RightConverter : IMultiValueConverter
	{
		/// <summary>
		/// Convert method.
		/// </summary>
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var canvasWidth = System.Convert.ToDouble(values[0]);
			var canvasHeight = System.Convert.ToDouble(values[1]);
			switch ((string)parameter)
			{
				case "top":
					return canvasHeight - 45;
				case "bottom":
					return canvasHeight + 45;
				case "left":
					return canvasWidth - 45;
				case "right":
					return canvasWidth + 45;
				default:
					return 0;
			}
		}

		/// <summary>
		/// NOT USED.
		/// </summary>
		/// <returns></returns>
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
