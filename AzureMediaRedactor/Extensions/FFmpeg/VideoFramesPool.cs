using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace AzureMediaRedactor.Extensions.FFmpeg
{
    class VideoFramesPool : IDisposable
    {
        private readonly VideoMetadata _metadata;
        private readonly Func<float, FFmpegClient> _ffmpegClientFactory;
        private readonly int _bufferCount;
        private readonly BufferBlock<VideoFrame> _framesQueue;
        private readonly AsyncLock _asyncLock;
        private readonly BackgroundWorker _backgroundWorker;
        private float _headTime;
        private float _tailTime;
        private volatile bool _backgroundJobInterruptFlag;

        public VideoFramesPool(VideoMetadata metadata, Func<float, FFmpegClient> ffmpegClientFactory)
        {
            _bufferCount = 0x40000000 / metadata.Width / metadata.Height / 3;
            _metadata = metadata;
            _ffmpegClientFactory = ffmpegClientFactory;
            _framesQueue = new BufferBlock<VideoFrame>();
            _asyncLock = new AsyncLock();
            _backgroundWorker = new BackgroundWorker() { WorkerSupportsCancellation = true };
            _headTime = 0.0f;
            _tailTime = 0.0f;

            _backgroundWorker.DoWork += BackgroundVideoDecoding;
            _backgroundWorker.RunWorkerAsync();
        }

        public async Task<VideoFrame> GetNextFrameAsync()
        {
            VideoFrame frame;
            while(_framesQueue.TryReceive(out frame) && frame.Timestamp < _headTime)
            {
                frame.Dispose();
            }
            if (frame == null)
            {
                try
                {
                    while ((frame = await _framesQueue.ReceiveAsync()) != null && frame.Timestamp < _headTime)
                    {
                        frame.Dispose();
                    }
                }
                catch(InvalidOperationException)
                {
                }
            }
            return frame;
        }

        public async void Seek(float time)
        {
            _headTime = time;
            _backgroundJobInterruptFlag = true;

            using (var locker = await _asyncLock.AcquireLockAsync(CancellationToken.None))
            {
                VideoFrame frame;
                while (_framesQueue.TryReceive(out frame))
                {
                    frame.Dispose();
                }
                _tailTime = time;
            }            
        }

        private async void BackgroundVideoDecoding(object sender, DoWorkEventArgs e)
        {
            while (!_backgroundWorker.CancellationPending)
            {
                await Task.Yield();

                if (_framesQueue.Count >= _bufferCount * 0.5)
                {
                    continue;
                }

                int pos = _metadata.FindFrameIndexByTime(_tailTime);
                if (pos == -1)
                {
                    //Already queue to the end of video
                    continue;
                }

                _backgroundJobInterruptFlag = false;

                FFmpegClient ffmpegClient = _ffmpegClientFactory(_tailTime);

                CancellationTokenSource bufferFullCTS = new CancellationTokenSource();
                Task task = ffmpegClient.StartAsync(bufferFullCTS.Token);

                int number = 0;
                while (!ffmpegClient.ImageBuffer.Completion.IsCompleted && !_backgroundJobInterruptFlag)
                {
                    using (var locker = await _asyncLock.AcquireLockAsync(CancellationToken.None))
                    {
                        try
                        {
                            VideoImage image = await ffmpegClient.ImageBuffer.ReceiveAsync();

                            if (pos + number < _metadata.Frames.Length)
                            {
                                FrameMetadata frame = _metadata.Frames[pos + number];
                                await _framesQueue.SendAsync(new VideoFrame(image, frame.Time));
                                _tailTime = frame.Time + 1.0f / _metadata.TimeScale;
                                number++;
                            }
                        }
                        catch (InvalidOperationException)
                        {
                        }

                        if (_framesQueue.Count >= _bufferCount)
                        {
                            bufferFullCTS.Cancel();
                        }
                    }
                }

                await task;
            }

            _framesQueue.Complete();
        }

        public void Dispose()
        {
            _backgroundWorker.CancelAsync();
            _backgroundWorker.Dispose();
            VideoFrame frame;
            while(!_framesQueue.Completion.IsCompleted)
            {
                while (_framesQueue.TryReceive(out frame))
                {
                    frame.Dispose();
                }
            }
        }
    }
}
