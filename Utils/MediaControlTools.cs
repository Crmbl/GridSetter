using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace GridSetter.Utils
{
	/// <summary>
	/// Defines the method for the media controls : image/video.
	/// </summary>
	public static class MediaControlTools
	{
		#region Properties

		/// <summary>
		/// Defines the zoom limit min.
		/// </summary>
		private const double ZoomMinTreshold = 0.1;

        /// <summary>
        /// Defines the zoom limit max.
        /// </summary>
        private const double ZoomMaxTreshold = 6.5;

        /// <summary>
        /// Defines the zoom update ratio.
        /// </summary>
        private const double ZoomRatio = 0.1;

		#endregion // Properties

		/// <summary>
		/// Allows to zoom in or out on the given image.
		/// </summary>
		/// <param name="image">The image to zoom.</param>
		/// <param name="args">The delta to zoom.</param>
		public static void ImageZoom(Image image, MouseWheelEventArgs args)
		{
		    // Tentative de zoom sur la position de la souris. Problèmes en tout genre.
		    //var mousePosition = args.GetPosition(image);
		    //image.RenderTransformOrigin = new Point(Math.Round(mousePosition.X / image.ActualWidth, 3), Math.Round(mousePosition.Y / image.ActualHeight, 3));

            var scaleTransform = (ScaleTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is ScaleTransform);
            var zoom = args.Delta > 0 ? ZoomRatio : -ZoomRatio;

		    if (!(VisualTreeHelper.GetParent(image) is Canvas canvas)) return;
		    var relativePointCache = image.TranslatePoint(new Point(0, 0), canvas);

		    if (scaleTransform.ScaleX + zoom > ZoomMinTreshold && scaleTransform.ScaleX + zoom < ZoomMaxTreshold && 
		        scaleTransform.ScaleY + zoom > ZoomMinTreshold && scaleTransform.ScaleY + zoom < ZoomMaxTreshold)
		    {
		        scaleTransform.ScaleX += zoom;
		        scaleTransform.ScaleY += zoom;
		    }

		    if (zoom > 0)
		        return;

		    var imageHeight = image.ActualHeight * scaleTransform.ScaleY;
		    var imageWidth = image.ActualWidth * scaleTransform.ScaleX;
		    var translateTransform = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
		    var relativePoint = image.TranslatePoint(new Point(0, 0), canvas);
		    var mathRelativePositionY = Math.Round(relativePoint.Y - relativePointCache.Y, MidpointRounding.AwayFromZero);
		    var mathRelativePositionX = Math.Round(relativePoint.X - relativePointCache.X, MidpointRounding.AwayFromZero);
		    var mathTranslateY = Math.Round(translateTransform.Y, MidpointRounding.AwayFromZero);
		    var mathTranslateX = Math.Round(translateTransform.X, MidpointRounding.AwayFromZero);

		    if (imageHeight < canvas.ActualHeight)
		        translateTransform.Y = 0;
		    else if (relativePoint.Y > 0)
		        translateTransform.Y = mathTranslateY - mathRelativePositionY;
		    else if (relativePoint.Y + imageHeight < canvas.ActualHeight)
		        translateTransform.Y = mathTranslateY + mathRelativePositionY;

		    if (imageWidth < canvas.ActualWidth)
		        translateTransform.X = 0;
		    else if (relativePoint.X > 0)
		        translateTransform.X = mathTranslateX - mathRelativePositionX;
		    else if (relativePoint.X + imageWidth < canvas.ActualWidth)
		        translateTransform.X = mathTranslateX + mathRelativePositionX;
        }

        /// <summary>
        /// The drop actions method.
        /// </summary>
        public static void ImageDrop(object sender, DragEventArgs dragEventArgs)
		{
			var files = (string[])dragEventArgs.Data.GetData(DataFormats.FileDrop);
			if (files == null || files.Length > 1) return;
			if (!(sender is Canvas canvas)) return;

			if (files.First().Contains(".gif") || files.First().Contains(".jpg") || files.First().Contains(".png") || files.First().Contains(".jpeg"))
			{
				var imageControl = canvas.Children.Cast<UIElement>().FirstOrDefault(c => c is Image);
				if (!(imageControl is Image image))
					return;

			    image.Stretch = Stretch.None;
			    image.ClipToBounds = true;
			    image.Visibility = Visibility.Visible;
			    image.HorizontalAlignment = HorizontalAlignment.Center;
			    image.VerticalAlignment = VerticalAlignment.Center;

			    var renderScaleTransform = (ScaleTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is ScaleTransform);
			    var translateTransform = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
			    renderScaleTransform.ScaleX = 1;
			    renderScaleTransform.ScaleY = 1;
			    translateTransform.Y = 0;
			    translateTransform.X = 0;

                BitmapImage imageToDrop = new BitmapImage();
			    imageToDrop.BeginInit();
			    imageToDrop.UriSource = new Uri(files.First());
			    imageToDrop.EndInit();

			    image.Width = imageToDrop.Width;
			    image.Height = imageToDrop.Height;
			    image.Tag = new Rect
			    {
			        Height = image.Height,
			        Width = image.Width
			    };

				ImageBehavior.SetAnimatedSource(image, imageToDrop);
            }
			else if (files.First().Contains(".avi") || files.First().Contains(".mp4") || files.First().Contains(".wmv") || files.First().Contains("webm"))
			{
				throw new NotImplementedException();
			}
		}
	}
}
