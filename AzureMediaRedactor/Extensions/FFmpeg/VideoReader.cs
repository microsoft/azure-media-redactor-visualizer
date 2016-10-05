using AzureMediaRedactor.Models;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AzureMediaRedactor.Extensions.FFmpeg
{
    [Export(typeof(IVideoReader))]
    class VideoReader : IVideoReader
    {
        private readonly string _ffmpegToolPath;
        private readonly string _ffprobeToolPath;
        private VideoFramesPool _framePool;
        private string _videoUrl;
        private VideoMetadata _metadata;

        public VideoReader()
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            _ffmpegToolPath = Path.Combine(directory, "ffmpeg.exe");
            _ffprobeToolPath = Path.Combine(directory, "ffprobe.exe");

            while(!File.Exists(_ffmpegToolPath) || !File.Exists(_ffprobeToolPath))
            {
                MessageBox.Show("Can't find ffmpeg.exe or ffprobe.exe in assembly folder, please download them from https://ffmpeg.zeranoe.com/builds/ then copy to the assembly folder, then click OK button.");
            }
        }

        public IVideoProperties Properties => new VideoProperties(_metadata);

        public async Task<bool> OpenAsync(string videoUrl)
        {
            _videoUrl = Path.GetFullPath(videoUrl);

            FFprobeClient ffprobeClient = new FFprobeClient(_ffprobeToolPath, _videoUrl);
            bool ret = await ffprobeClient.RunAsync();
            _metadata = ffprobeClient.Metadata;
            _framePool = new VideoFramesPool(_metadata, (start) => new FFmpegClient(_ffmpegToolPath, _videoUrl, _metadata.Width, _metadata.Height, start));
            return ret;
        }

        public async Task<IVideoFrame> QueryFrameAsync()
        {
            return await _framePool.GetNextFrameAsync();
        }

        public void Seek(float time)
        {
            _framePool.Seek(time);
        }

        void IDisposable.Dispose()
        {
            _framePool.Dispose();
        }
    }
}
