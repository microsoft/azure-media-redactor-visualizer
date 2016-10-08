using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace AzureMediaRedactor.Extensions.FFmpeg
{
    class FFmpegClient
    {
        private readonly string _ffmpegToolPath;
        private readonly string _videoUrl;
        private readonly int _width;
        private readonly int _height;
        private readonly float _start;
        private BufferBlock<VideoImage> _imageBuffer;

        public BufferBlock<VideoImage> ImageBuffer => _imageBuffer;

        public FFmpegClient(string ffmpegToolPath, string videoUrl, int width, int height, float start)
        {
            _ffmpegToolPath = ffmpegToolPath;
            _videoUrl = videoUrl;
            _width = width;
            _height = height;
            _start = start;
            _imageBuffer = new BufferBlock<VideoImage>();
        }

        public async Task StartAsync(CancellationToken token)
        {
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = _ffmpegToolPath,
                    Arguments = $"-ss {_start} -i \"{_videoUrl}\" -f image2pipe -pix_fmt bgr24 -vcodec rawvideo -",
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };

            if(!process.Start())
            {
                throw new Exception("Start FFMPEG client failed.");
            }

            TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
            process.Exited += (s, e) =>
            {
                source.TrySetResult(true);
            };

            token.Register(() =>
            {
                process.StandardInput.Write('q');
                process.StandardInput.Flush();
            });

            HandleStandError(process.StandardError);
            HandleStandOutput(process.StandardOutput);

            await source.Task;
        }

        async void HandleStandError(StreamReader reader)
        {
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                Debug.WriteLine(line);
            }
        }

        async void HandleStandOutput(StreamReader reader)
        {
            byte[] buffer = new byte[_width * _height * 3];

            do
            {
                int index = 0;
                int count = buffer.Length;
                int read = 0;

                do
                {
                    read = await reader.BaseStream.ReadAsync(buffer, index, count);
                    count -= read;
                    index += read;
                } while (read != 0 && count != 0);

                if (read != 0)
                {
                    await _imageBuffer.SendAsync(new VideoImage(_width, _height, _width * 3, buffer));
                }
                else
                {
                    _imageBuffer.Complete();
                    break;
                }

            } while (true);
        }
    }
}
