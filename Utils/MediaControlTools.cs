using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

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
		private const double ZoomMinTreshold = 0.10;

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
			image.RenderTransformOrigin = new Point(0.5, 0.5);
			var layoutScaleTransform = (ScaleTransform)((TransformGroup)image.LayoutTransform).Children.First(tr => tr is ScaleTransform);
		    var renderScaleTransform = (ScaleTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is ScaleTransform);
            var zoom = args.Delta > 0 ? ZoomRatio : -ZoomRatio;

		    var grid = UserInterfaceTools.FindParent(image);
		    var relativePointCache = image.TranslatePoint(new Point(0, 0), grid);
            grid.Height = grid.ActualHeight;
		    grid.Width = grid.ActualWidth;

            ScaleTransform scaleTransform;
		    if (image.ActualHeight * zoom <= grid.ActualHeight || image.ActualWidth * zoom <= grid.ActualWidth)
		        scaleTransform = renderScaleTransform;
		    else
		        scaleTransform = layoutScaleTransform;

            if (scaleTransform.ScaleX + zoom > ZoomMinTreshold && scaleTransform.ScaleX + zoom < ZoomMaxTreshold
				&& scaleTransform.ScaleY + zoom > ZoomMinTreshold && scaleTransform.ScaleY + zoom < ZoomMaxTreshold)
			{
				scaleTransform.ScaleX += zoom;
				scaleTransform.ScaleY += zoom;
			}

			grid.Height = Double.NaN;
		    grid.Width = Double.NaN;

            if (zoom > 0)
				return;

			var imageHeight = image.ActualHeight * scaleTransform.ScaleY;
			var imageWidth = image.ActualWidth * scaleTransform.ScaleX;
			var translateTransform = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
			var relativePoint = image.TranslatePoint(new Point(0, 0), grid);
			var mathRelativePositionY = Math.Round(relativePoint.Y - relativePointCache.Y, MidpointRounding.AwayFromZero);
			var mathRelativePositionX = Math.Round(relativePoint.X - relativePointCache.X, MidpointRounding.AwayFromZero);
			var mathTranslateY = Math.Round(translateTransform.Y, MidpointRounding.AwayFromZero);
			var mathTranslateX = Math.Round(translateTransform.X, MidpointRounding.AwayFromZero);

			if (imageHeight < grid.ActualHeight)
				translateTransform.Y = 0;
			else if (relativePoint.Y > 0)
				translateTransform.Y = mathTranslateY - mathRelativePositionY;
			else if (relativePoint.Y + imageHeight < grid.ActualHeight)
				translateTransform.Y = mathTranslateY + mathRelativePositionY;

			if (imageWidth < grid.ActualWidth)
				translateTransform.X = 0;
			else if (relativePoint.X > 0)
				translateTransform.X = mathTranslateX - mathRelativePositionX;
			else if (relativePoint.X + imageWidth < grid.ActualWidth)
				translateTransform.X = mathTranslateX + mathRelativePositionX;
		}

		public static void ForceZoom(Image image, double zoom, double previous)
		{
			image.RenderTransformOrigin = new Point(0.5, 0.5);
			var scaleTransform = (ScaleTransform)((TransformGroup)image.LayoutTransform).Children.First(tr => tr is ScaleTransform);

			var grid = UserInterfaceTools.FindParent(image);
			grid.Height = grid.ActualHeight;
			grid.Width = grid.ActualWidth;

			scaleTransform.ScaleX = zoom;
			scaleTransform.ScaleY = zoom;

			grid.Height = Double.NaN;
			grid.Width = Double.NaN;

			if (zoom > previous)
				return;

			var imageHeight = image.ActualHeight * scaleTransform.ScaleY;
			var imageWidth = image.ActualWidth * scaleTransform.ScaleX;
			var translateTransform = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
			var relativePoint = image.TranslatePoint(new Point(0, 0), grid);

			if (imageHeight < grid.ActualHeight 
			    || relativePoint.Y > 0
			    || relativePoint.Y + imageHeight < grid.ActualHeight)
				translateTransform.Y = 0;
			else if (imageHeight > grid.ActualHeight)
			{
				// ???
			}

			if (imageWidth < grid.ActualWidth
			    || relativePoint.X > 0
			    || relativePoint.X + imageWidth < grid.ActualWidth)
				translateTransform.X = 0;
			else if (imageWidth > grid.ActualWidth)
			{
				// ???
			}
		}

		/// <summary>
		/// The drop actions method.
		/// </summary>
		public static void ImageDrop(object sender, DragEventArgs dragEventArgs)
		{
			var files = (string[])dragEventArgs.Data.GetData(DataFormats.FileDrop);
			if (files == null || files.Length > 1)
				return;

			if (!(sender is Grid))
				return;

			var currentGrid = (Grid)sender;
			if (files.First().Contains(".gif") || files.First().Contains(".jpg") || files.First().Contains(".png") || files.First().Contains(".jpeg"))
			{
				var imageControl = currentGrid.Children.Cast<UIElement>().FirstOrDefault(c => c is Image);
				if (!(imageControl is Image))
					return;

			    Image control = imageControl as Image;
			    control.Stretch = Stretch.None;
			    control.ClipToBounds = true;
				control.HorizontalAlignment = HorizontalAlignment.Center;
				control.VerticalAlignment = VerticalAlignment.Center;

			    var scaleTransform = (ScaleTransform)((TransformGroup)control.LayoutTransform).Children.First(tr => tr is ScaleTransform);
			    var translateTransform = (TranslateTransform)((TransformGroup)control.RenderTransform).Children.First(tr => tr is TranslateTransform);
			    translateTransform.Y = 0;
			    translateTransform.X = 0;
			    scaleTransform.ScaleX = 1;
			    scaleTransform.ScaleY = 1;

                BitmapImage imageToDrop = new BitmapImage();
			    imageToDrop.BeginInit();
			    imageToDrop.UriSource = new Uri(files.First());
			    imageToDrop.EndInit();

				control.Width = imageToDrop.Width;
				control.Height = imageToDrop.Height;

				ImageBehavior.SetAnimatedSource(control, imageToDrop);
            }
			else if (files.First().Contains(".avi") || files.First().Contains(".mp4") || files.First().Contains(".wmv") || files.First().Contains("webm"))
			{
				throw new NotImplementedException();
			}
		}
	}
}
