using AzureMediaRedactor.Controls;
using AzureMediaRedactor.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AzureMediaRedactor.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged, IVideoFilterProvider
    {
        private const int FIRST_PAGE = 0;
        private const int SECOND_PAGE = 1;

        public enum VideoFilterType
        {
            DEBUG_ACTIVE = 0,
            DEBUG_INACTIVE = 1,
            BLUR_ACTIVE = 2
        }

        private readonly RelayCommand _uploadVideoCommand;
        private readonly RelayCommand _uploadMetadataCommand;
        private readonly RelayCommand _uploadThumbnailsCommand;
        private readonly RelayCommand _nextCommand;
        private readonly RelayCommand _backCommand;
        private readonly IVideoFilter[] _videoFilters;
        private readonly ObservableCollection<ThumbnailViewModel> _thumbnails;
        private readonly HashSet<int> _activeIdSet;

        private int _activePageIndex;
        private bool _isBlur;
        private string _videoUrl;
        private string _metadataUrl;
        private string _thumbnailUrls;
        private string _activeIds;
        private VideoVideoModel _video;
        private Metadata _metadata;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand UploadVideoCommand => _uploadVideoCommand;
        public ICommand UploadMetadataCommand => _uploadMetadataCommand;
        public ICommand UploadThumbnailsCommand => _uploadThumbnailsCommand;
        public ICommand NextCommand => _nextCommand;
        public ICommand BackCommand => _backCommand;
        public IEnumerable<ThumbnailViewModel> Thumbnails => _thumbnails;
        public int ActivePageIndex
        {
            get
            {
                return _activePageIndex;
            }
            set
            {
                if (_activePageIndex != value)
                {
                    _activePageIndex = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActivePageIndex)));
                }
            }
        }
        public bool IsBlur
        {
            get
            {
                return _isBlur;
            }
            set
            {
                if(_isBlur != value)
                {
                    _isBlur = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBlur)));
                    Video.Refresh();
                }
            }
        }
        public string ActiveIds
        {
            get
            {
                return _activeIds;
            }
            set
            {
                if(_activeIds != value)
                {
                    _activeIds = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveIds)));
                }
            }
        }
        public string VideoUrl
        {
            get
            {
                return _videoUrl;
            }
            set
            {
                if (_videoUrl != value)
                {
                    _videoUrl = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VideoUrl)));
                }
            }
        }
        public string MetadataUrl
        {
            get
            {
                return _metadataUrl;
            }
            set
            {
                if (_metadataUrl != value)
                {
                    _metadataUrl = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MetadataUrl)));
                }
            }
        }
        public string ThumbnailUrls
        {
            get
            {
                return _thumbnailUrls;
            }
            set
            {
                if (_thumbnailUrls != value)
                {
                    _thumbnailUrls = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ThumbnailUrls)));
                }
            }
        }
        public VideoVideoModel Video
        {
            get
            {
                return _video;
            }
            set
            {
                if(_video != value)
                {
                    _video = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Video)));
                }
            }
        }

        public MainWindowViewModel()
        {
            _thumbnails = new ObservableCollection<ThumbnailViewModel>();
            _activeIdSet = new HashSet<int>();

            _uploadVideoCommand = new RelayCommand(_ =>
            {
                OpenFileDialog dialog = new OpenFileDialog()
                {
                    Filter = "Videos (*.mp4;*.wmv;*.mov;*.avi)|*.mp4;*.wmv;*.mov;*.avi",
                    Multiselect = false
                };

                bool? ret = dialog.ShowDialog();

                if (!ret.HasValue || !ret.Value)
                {
                    return;
                }

                VideoUrl = dialog.FileName;
            });

            _uploadMetadataCommand = new RelayCommand(_ =>
            {
                OpenFileDialog dialog = new OpenFileDialog()
                {
                    Filter = "Metadata (*.json)|*.json",
                    Multiselect = false
                };

                bool? ret = dialog.ShowDialog();

                if (!ret.HasValue || !ret.Value)
                {
                    return;
                }

                MetadataUrl = dialog.FileName;
            });

            _uploadThumbnailsCommand = new RelayCommand(_ =>
            {
                OpenFileDialog dialog = new OpenFileDialog()
                {
                    Filter = "Thumbnails (*.jpg;*.jpeg)|*.jpg;*.jpeg",
                    Multiselect = true
                };

                bool? ret = dialog.ShowDialog();

                if (!ret.HasValue || !ret.Value || dialog.FileNames.Length == 0)
                {
                    return;
                }

                List<string> fileNames = new List<string>() { dialog.FileNames[0] };
                fileNames.AddRange(dialog.FileNames.Skip(1).Select(f => Path.GetFileName(f)));

                ThumbnailUrls = string.Join("; ", fileNames);
            });

            _nextCommand = new RelayCommand(_ =>
            {
                if (File.Exists(_videoUrl) && File.Exists(_metadataUrl) && _thumbnailUrls.Split(';').Any(File.Exists))
                {
                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += LoadData;

                    SplashWindow splash = new SplashWindow(worker);
                    worker.RunWorkerCompleted += (s, e) =>
                    {
                        foreach(ThumbnailViewModel thumbnail in (e.Result as IEnumerable<ThumbnailViewModel>))
                        {
                            _thumbnails.Add(thumbnail);
                        }
                        ActivePageIndex = 1;
                    };

                    worker.RunWorkerAsync();
                    splash.ShowDialog();
                }
            });

            _backCommand = new RelayCommand(_ =>
            {
                ClearData();
                ActivePageIndex = 0;
            });

            _videoFilters = new IVideoFilter[3];
            ModuleLoader.Load("DEBUG", "Color", System.Drawing.Color.Blue, out _videoFilters[0]);
            _videoFilters[0].SetProvider(this, (int)VideoFilterType.DEBUG_ACTIVE);
            ModuleLoader.Load("DEBUG", "Color", System.Drawing.Color.Yellow, out _videoFilters[1]);
            _videoFilters[1].SetProvider(this, (int)VideoFilterType.DEBUG_INACTIVE);
            ModuleLoader.Load("BLUR", out _videoFilters[2]);
            _videoFilters[2].SetProvider(this, (int)VideoFilterType.BLUR_ACTIVE);
        }


        IEnumerable<Annotation> IVideoFilterProvider.OnFiltering(float time, int userData)
        {
            if (IsBlur && userData != (int)VideoFilterType.BLUR_ACTIVE ||
                    !IsBlur && userData != (int)VideoFilterType.DEBUG_ACTIVE && userData != (int)VideoFilterType.DEBUG_INACTIVE)
            {
                yield break;
            }

            if(_metadata == null)
            {
                yield break;
            }

            Frame frame = _metadata.GetFrame(time);
            if (frame == null)
            {
                yield break;
            }

            foreach (Annotation annotation in frame.Annotations)
            {
                if (_activeIdSet.Contains(annotation.Id))
                {
                    if (userData == (int)VideoFilterType.DEBUG_ACTIVE || userData == (int)VideoFilterType.BLUR_ACTIVE)
                    {
                        yield return annotation;
                    }
                }
                else
                {
                    if (userData == (int)VideoFilterType.DEBUG_INACTIVE)
                    {
                        yield return annotation;
                    }
                }
            }
        }

        private void LoadData(object sender, DoWorkEventArgs e)
        {
            VideoVideoModel video = new VideoVideoModel(_videoUrl, _videoFilters);
            Task task = video.LoadAsync();

            _metadata = Metadata.Parse(_metadataUrl);

            string[] thumbnailUrls = _thumbnailUrls.Split(new string[] { "; " }, StringSplitOptions.None);
            if(thumbnailUrls.Length != 0)
            {
                string folder = Path.GetDirectoryName(thumbnailUrls[0]);
                for(int i = 1; i < thumbnailUrls.Length; i++)
                {
                    thumbnailUrls[i] = Path.Combine(folder, thumbnailUrls[i]);
                }
            }

            List<ThumbnailViewModel> thumbnails = new List<ThumbnailViewModel>();
            foreach (string url in thumbnailUrls)
            {
                if(!File.Exists(url))
                {
                    continue;
                }

                string fileName = Path.GetFileNameWithoutExtension(url);
                int id = int.Parse(fileName.Substring(fileName.Length - 6, 6));

                ThumbnailViewModel thumabnail = new ThumbnailViewModel(id, _metadata.GetObjectTimestamp(id), url);
                thumabnail.CheckChanged += (s, _) =>
                {
                    UpdateActiveIds();
                    Video.StopBy((s as ThumbnailViewModel).FrameTimestamp, false);
                };
                thumabnail.Clicked += (s, _) =>
                {
                    Video.StopBy((s as ThumbnailViewModel).FrameTimestamp, true);
                };
                thumbnails.Add(thumabnail);
                _activeIdSet.Add(id);
            }

            ActiveIds = string.Join(", ", _activeIdSet);
            e.Result = thumbnails;

            task.Wait();
            Video = video;
        }

        private void ClearData()
        {
            Video.Dispose();
            Video = null;
            _metadata = null;
            _activeIdSet.Clear();
            _thumbnails.Clear();
            ActiveIds = null;
            VideoUrl = null;
            MetadataUrl = null;
            ThumbnailUrls = null;
        }

        private void UpdateActiveIds()
        {
            _activeIdSet.Clear();
            foreach(ThumbnailViewModel thumbnail in _thumbnails)
            {
                if(thumbnail.IsChecked)
                {
                    _activeIdSet.Add(thumbnail.Id);
                }
            }
            ActiveIds = string.Join(", ", _activeIdSet);
        }
    }
}
