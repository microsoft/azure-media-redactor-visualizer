using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AzureMediaRedactor.Models
{
    public class VideoVideoModel : INotifyPropertyChanged, IDisposable
    {
        enum State
        {
            Stop,
            Play,
            StopBy,
            Refresh
        }

        private readonly RelayCommand _playCommand;
        private readonly RelayCommand _pauseCommand;
        private readonly IVideoFilter[] _videoFilters;
        private readonly IVideoReader _videoReader;
        private readonly BackgroundWorker _backgroundWorker;
        
        private IVideoProperties _videoProperties;
        private float _position;
        private int _progress;
        private State _state;
        private double _playElapsed;
        private DateTime _playUpdateTime;
        private double _stopByTargetTime;
        private IVideoFrame _currentFrame;
        private IVideoFrame _currentFilteredFrame;
        private int _currentFrameTag;
        private int _frameTagCounter;

        public event PropertyChangedEventHandler PropertyChanged;

        public string VideoUrl { get; }
        public int Width => _videoProperties.Width;
        public int Height => _videoProperties.Height;
        public string Position
        {
            get
            {
                int elapsed = (int)(_position * 1000);
                return string.Format("{0:00}:{1:00}:{2:00}.{3:000}",
            elapsed / 1000 / 60 / 60,
            elapsed / 1000 / 60 % 60,
            elapsed / 1000 % 60,
            elapsed % 1000);
            }
        }
        public string Length
        {
            get
            {
                int elapsed = _videoProperties == null ? 0 : (int)(_videoProperties.Duration * 1000);
                return string.Format("{0:00}:{1:00}:{2:00}.{3:000}",
            elapsed / 1000 / 60 / 60,
            elapsed / 1000 / 60 % 60,
            elapsed / 1000 % 60,
            elapsed % 1000);
            }
        }
        public ICommand PlayCommand => _playCommand;
        public ICommand PauseCommand => _pauseCommand;
        public int Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                if(_progress != value)
                {
                    _progress = value;
                    Seek(_videoProperties.Duration * value / 256);
                    _stopByTargetTime = 0.0;
                    _state = State.StopBy;
                }
            }
        }

        public VideoVideoModel(string videoUrl, IVideoFilter[] filters)
        {
            this.VideoUrl = videoUrl;
            _videoFilters = filters;
            _playCommand = new RelayCommand(_ => Play());
            _pauseCommand = new RelayCommand(_ => Stop());
            _videoReader = ModuleLoader.Load<IVideoReader>();
            _backgroundWorker = new BackgroundWorker() { WorkerSupportsCancellation = true };
            _backgroundWorker.DoWork += StartBackgroundJob;
        }

        public async Task LoadAsync()
        {
            if (!await _videoReader.OpenAsync(VideoUrl))
            {
                throw new Exception($"Open video \"{VideoUrl}\" failed.");
            }

            _videoProperties = _videoReader.Properties;
            _stopByTargetTime = 0.0;
            _state = State.StopBy;

            _backgroundWorker.RunWorkerAsync();
        }

        public async void StartBackgroundJob(object sender, DoWorkEventArgs e)
        {
            while (!_backgroundWorker.CancellationPending)
            {
                IVideoFrame frame = await QueryFrameAsync();
                if (frame != null)
                {
                    _currentFrame = frame;
                    IVideoFrame filteredFrame = frame.Clone();

                    if (_videoFilters != null)
                    {
                        foreach (IVideoFilter filter in _videoFilters)
                        {
                            filter.Filter(filteredFrame);
                        }
                    }

                    _currentFilteredFrame = filteredFrame;
                    _currentFrameTag = _frameTagCounter++;

                    int progress = (int)(256 * frame.Timestamp / _videoProperties.Duration);
                    if (progress != _progress)
                    {
                        _progress = progress;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
                    }

                    if (frame.Timestamp != _position)
                    {
                        _position = frame.Timestamp;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
                    }
                }
            }
        }

        public bool GetCurrentFrame(out IVideoFrame frame, out int frameTag)
        {
            frame = _currentFilteredFrame;
            frameTag = _currentFrameTag;
            return _currentFilteredFrame != null;
        }

        private async Task<IVideoFrame> QueryFrameAsync()
        {
            IVideoFrame frame = null;

            switch (_state)
            {
                case State.Stop:
                    break;
                case State.Refresh:
                    {
                        frame = _currentFrame;
                        _state = State.Stop;
                    }
                    break;
                case State.StopBy:
                    {
                        do
                        {
                            frame = await _videoReader.QueryFrameAsync();
                        }
                        while (frame != null && frame.Timestamp < _stopByTargetTime);

                        _state = State.Stop;
                        _stopByTargetTime = 0.0;
                    }
                    break;
                case State.Play:
                    {
                        double frameInterval = 1000.0 / _videoProperties.FrameRate;

                        DateTime currentTime = DateTime.Now;
                        if (_playElapsed + (currentTime - _playUpdateTime).TotalMilliseconds < frameInterval)
                        {
                            return null;
                        }

                        do
                        {
                            frame = await _videoReader.QueryFrameAsync();
                            _playElapsed -= frameInterval;
                            currentTime = DateTime.Now;
                        }
                        while (frame != null && _playElapsed + (currentTime - _playUpdateTime).TotalMilliseconds >= frameInterval);

                        _playElapsed += (currentTime - _playUpdateTime).TotalMilliseconds;
                        _playUpdateTime = currentTime;

                        if (frame == null)
                        {
                            _state = State.Stop;
                        }
                    }
                    break;
            }

            return frame;
        }

        private void Seek(float timestamp)
        {
            _videoReader.Seek(timestamp);
            _playElapsed = 0.0f;
            _playUpdateTime = DateTime.Now;
        }

        public void Play()
        {
            if(_state == State.Stop)
            {
                _playElapsed = 0.0;
                _playUpdateTime = DateTime.Now;
                _state = State.Play;
            }
        }

        public void Stop()
        {
            _state = State.Stop;
        }

        public void Refresh()
        {
            if (_state == State.Stop)
            {
                _state = State.Refresh;
            }
        }

        public void StopBy(float time, bool force)
        {
            if (_state == State.Stop || force)
            {
                Seek(time);
                _state = State.StopBy;
                _stopByTargetTime = time;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _videoReader.Dispose();
                    _backgroundWorker.CancelAsync();
                    _backgroundWorker.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Video() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
