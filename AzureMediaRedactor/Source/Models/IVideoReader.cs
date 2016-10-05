using System;
using System.Threading.Tasks;

namespace AzureMediaRedactor.Models
{
    public interface IVideoReader : IDisposable
    {
        IVideoProperties Properties { get; }

        Task<bool> OpenAsync(string videoUrl);
        Task<IVideoFrame> QueryFrameAsync();
        void Seek(float timestamp);
    }
}
