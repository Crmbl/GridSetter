using System;
using System.Globalization;
using System.Windows.Data;

namespace GridSetter.Utils.Converters
{
    /// <summary>
    /// Allow the children to be centered bottom inside the canvas.
    /// </summary>
    internal sealed class BottomConverter : IMultiValueConverter
    {
        /// <summary>
        /// Convert method.
        /// </summary>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var canvasWidth = System.Convert.ToDouble(values[0]);
            var canvasHeight = System.Convert.ToDouble(values[1]);
            var controlWidth = System.Convert.ToDouble(values[2]);
            var controlHeight = System.Convert.ToDouble(values[3]);
            switch ((string)parameter)
            {
                case "top":
                    return canvasHeight - controlHeight;
                //case "bottom":
                //    return (canvasHeight - controlHeight) / 2;
                //case "left":
                //    return (canvasWidth + controlWidth) / 2;
                //case "right":
                //    return (canvasWidth - controlWidth) / 2;
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
