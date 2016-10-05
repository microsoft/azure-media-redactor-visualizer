using AzureMediaRedactor.Models;

namespace AzureMediaRedactor.Extensions.FFmpeg
{
    class VideoProperties : IVideoProperties
    {
        private readonly VideoMetadata _metadata;

        public float Duration => _metadata.Duration;

        public float FrameRate => _metadata.FrameRate;

        public int Height => _metadata.Height;

        public float TimeScale => _metadata.TimeScale;

        public int Width => _metadata.Width;

        public VideoProperties(VideoMetadata metadata)
        {
            _metadata = metadata;
        }
    }
}
