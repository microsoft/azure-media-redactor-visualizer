using AzureMediaRedactor.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AzureMediaRedactor.Controls
{
    /// <summary>
    /// Interaction logic for VideoPlayer.xaml
    /// </summary>
    public partial class VideoPlayer : UserControl
    {
        private WriteableBitmap _bitmap;
        private int _lastFrameTag;

        public static readonly DependencyProperty VideoProperty =
            DependencyProperty.Register("HostVideo", typeof(VideoVideoModel), typeof(VideoPlayer), new PropertyMetadata(PropertyChangedCallback));

        public VideoVideoModel HostVideo
        {
            get
            {
                return base.GetValue(VideoProperty) as VideoVideoModel;
            }
            set
            {
                base.SetValue(VideoProperty, value);
            }
        }

        public VideoPlayer()
        {
            InitializeComponent();
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.Property.Name != "HostVideo")
            {
                return;
            }

            VideoPlayer that = d as VideoPlayer;

            /*Clear resource at first*/
            that._bitmap = null;

            if(e.NewValue != null)
            {
                VideoVideoModel hostVideo = e.NewValue as VideoVideoModel;
                that._bitmap = new WriteableBitmap(
                hostVideo.Width,
                hostVideo.Height,
                96.0, 96.0,
                PixelFormats.Bgr24,
                null);
                that._lastFrameTag = int.MinValue;

                that.screen.Source = that._bitmap;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if(HostVideo == null)
            {
                return;
            }

            IVideoFrame frame = null;
            int frameTag;
            if (!HostVideo.GetCurrentFrame(out frame, out frameTag))
            {
                return;
            }

            if(frameTag != _lastFrameTag)
            { 
                Int32Rect rect = new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight);

                _bitmap.Lock();
                _bitmap.WritePixels(rect, frame.Image.Data, frame.Image.Stride * frame.Image.Height, frame.Image.Stride);
                _bitmap.AddDirtyRect(rect);
                _bitmap.Unlock();

                _lastFrameTag = frameTag;
            }
        }
    }
}