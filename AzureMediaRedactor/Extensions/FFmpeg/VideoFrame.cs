using System;
using AzureMediaRedactor.Models;

namespace AzureMediaRedactor.Extensions.FFmpeg
{
    class VideoFrame : IVideoFrame
    {
        public IVideoImage Image { get; }
        public float Timestamp { get; }

        public VideoFrame(IVideoImage image, float timestamp)
        {
            Image = image;
            Timestamp = timestamp;
        }

        public void Dispose()
        {
            Image.Dispose();
        }

        public IVideoFrame Clone()
        {
            return new VideoFrame(Image.Clone(), Timestamp);
        }
    }
}
