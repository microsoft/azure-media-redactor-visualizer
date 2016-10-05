using System;
using System.Windows.Input;

namespace AzureMediaRedactor.Models
{
    class ThumbnailViewModel
    {
        public int Id { get; }
        public float FrameTimestamp { get; }
        public string Url { get; }

        private bool _isChecked;
        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                if(_isChecked != value)
                {
                    _isChecked = value;
                    CheckChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        private readonly RelayCommand _command;
        public ICommand Command => _command;

        public ThumbnailViewModel(int id, float frameTimestamp, string url)
        {
            this.Id = id;
            this.FrameTimestamp = frameTimestamp;
            this.Url = url;
            this._isChecked = true;

            _command = new RelayCommand(_ =>
            {
                Clicked?.Invoke(this, new EventArgs());
            });
        }

        public event EventHandler CheckChanged;
        public event EventHandler Clicked;
    }
}
