using System;

namespace AzureMediaRedactor.Models
{
    public interface IVideoFrame : IDisposable
    {
        IVideoImage Image { get; }
        float Timestamp { get; }

        IVideoFrame Clone();
    }
}
