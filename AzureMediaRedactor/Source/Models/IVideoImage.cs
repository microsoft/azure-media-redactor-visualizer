using System;

namespace AzureMediaRedactor.Models
{
    public interface IVideoImage : IDisposable
    {
        IntPtr Data { get; }
        int Stride { get; }
        int Width { get; }
        int Height { get; }

        IVideoImage Clone();
    }
}
