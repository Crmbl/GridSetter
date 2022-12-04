using System;
using System.Collections.Generic;
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

        /// <summary>
        /// All the image types allowed for the Grid system.
        /// </summary>
	    private static readonly IEnumerable<string> ImageTypes = new[] { "gif", "jpg", "png", "jpeg", "bmp" };

        /// <summary>
        /// All the video types allowed for the Grid system.
        /// </summary>
	    private static readonly IEnumerable<string> VideoTypes = new[] { "avi", "mp4", "wmv", "webm", "mpg", "mpeg", "mov" };

        #endregion // Properties

        /// <summary>
        /// Allows to zoom in or out on the given media element.
        /// </summary>
        /// <param name="fElement">The media to zoom.</param>
        /// <param name="args">The delta to zoom.</param>
        public static void MediaZoom(FrameworkElement fElement, MouseWheelEventArgs args)
        {
            // Tentative de zoom sur la position de la souris. Problèmes en tout genre.
            //var mousePosition = args.GetPosition(image);
            //image.RenderTransformOrigin = new Point(Math.Round(mousePosition.X / image.ActualWidth, 3), Math.Round(mousePosition.Y / image.ActualHeight, 3));

            var scaleTransform = (ScaleTransform)((TransformGroup)fElement.RenderTransform).Children.First(tr => tr is ScaleTransform);
            var zoom = args.Delta > 0 ? ZoomRatio : -ZoomRatio;

            if (!(VisualTreeHelper.GetParent(fElement) is Canvas canvas)) return;
            var relativePointCache = fElement.TranslatePoint(new Point(0, 0), canvas);

            if (scaleTransform.ScaleX + zoom > ZoomMinTreshold && scaleTransform.ScaleX + zoom < ZoomMaxTreshold &&
                scaleTransform.ScaleY + zoom > ZoomMinTreshold && scaleTransform.ScaleY + zoom < ZoomMaxTreshold)
            {
                scaleTransform.ScaleX += zoom;
                scaleTransform.ScaleY += zoom;
            }

            if (zoom > 0)
                return;

            var imageHeight = fElement.ActualHeight * scaleTransform.ScaleY;
            var imageWidth = fElement.ActualWidth * scaleTransform.ScaleX;
            var translateTransform = (TranslateTransform)((TransformGroup)fElement.RenderTransform).Children.First(tr => tr is TranslateTransform);
            var relativePoint = fElement.TranslatePoint(new Point(0, 0), canvas);
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
        public static void MediaDrop(object sender, DragEventArgs dragEventArgs)
        {
            var files = (string[])dragEventArgs.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length > 1) return;
            if (!(sender is Canvas canvas)) return;

            var fileType = files.First().Split('.').Last();
            if (ImageTypes.Any(type => type.ToLower().Equals(fileType)))
            {
                var imageControl = canvas.Children.Cast<UIElement>().FirstOrDefault(c => c is Image);
                if (!(imageControl is Image image)) return;

                var videoControl = canvas.Children.Cast<UIElement>().FirstOrDefault(c => c is MediaElement);
                if (!(videoControl is MediaElement video)) return;
                video.Visibility = Visibility.Hidden;
                video.Source = null;

                var videoButtons = canvas.Children.Cast<FrameworkElement>().FirstOrDefault(e => e is Grid && e.Name == "VideoButtons") as Grid;
                var volumeButton = videoButtons?.Children.Cast<FrameworkElement>().FirstOrDefault(e => e is Button) as Button;
                var volumeSlider = videoButtons?.Children.Cast<FrameworkElement>().FirstOrDefault(e => e is Slider) as Slider;
                if (videoButtons != null && volumeButton != null)
                    volumeButton.Tag = Application.Current.Resources["MuteImage"] as BitmapImage;
                if (videoButtons != null && volumeSlider != null)
                    volumeSlider.Value = 0;

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
            else if (VideoTypes.Any(type => type.ToLower().Equals(fileType)))
            {
                var videoControl = canvas.Children.Cast<UIElement>().FirstOrDefault(c => c is MediaElement);
                if (!(videoControl is MediaElement video)) return;

                var imageControl = canvas.Children.Cast<UIElement>().FirstOrDefault(c => c is Image);
                if (!(imageControl is Image image)) return;
                image.Visibility = Visibility.Hidden;
                image.Source = null;

                var videoButtons = canvas.Children.Cast<FrameworkElement>().FirstOrDefault(e => e is Grid && e.Name == "VideoButtons") as Grid;
                var volumeButton = videoButtons?.Children.Cast<FrameworkElement>().FirstOrDefault(e => e is Button) as Button;
                var volumeSlider = videoButtons?.Children.Cast<FrameworkElement>().FirstOrDefault(e => e is Slider) as Slider;
                if (videoButtons != null && volumeButton != null)
                    volumeButton.Tag = Application.Current.Resources["MuteImage"] as BitmapImage;
                if (videoButtons != null && volumeSlider != null)
                    volumeSlider.Value = 0;

                video.Stretch = Stretch.None;
                video.ClipToBounds = true;
                video.Visibility = Visibility.Visible;
                video.HorizontalAlignment = HorizontalAlignment.Center;
                video.VerticalAlignment = VerticalAlignment.Center;
                video.LoadedBehavior = MediaState.Manual;
                video.ScrubbingEnabled = true;

                var renderScaleTransform = (ScaleTransform)((TransformGroup)video.RenderTransform).Children.First(tr => tr is ScaleTransform);
                var translateTransform = (TranslateTransform)((TransformGroup)video.RenderTransform).Children.First(tr => tr is TranslateTransform);
                renderScaleTransform.ScaleX = 1;
                renderScaleTransform.ScaleY = 1;
                translateTransform.Y = 0;
                translateTransform.X = 0;

                video.BeginInit();
                video.Source = new Uri(files.First());
                video.EndInit();

                video.Volume = 0;
                video.Play();
            }
        }
    }
}
