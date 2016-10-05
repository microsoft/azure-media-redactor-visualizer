using System;
using AzureMediaRedactor.Models;
using System.Runtime.InteropServices;

namespace AzureMediaRedactor.Extensions.FFmpeg
{
    class VideoImage : IVideoImage
    {
        public IntPtr Data { get; private set; }
        public int Height { get; }
        public int Stride { get; }
        public int Width { get; }

        public VideoImage(int width, int height, int stride, byte[] buffer)
        {
            if(stride * height != buffer.Length)
            {
                throw new ArgumentException("The length of buffer doesn't match stride * height.");
            }

            this.Width = width;
            this.Height = height;
            this.Stride = stride;

            IntPtr data = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, data, buffer.Length);
            this.Data = data;
        }

        ~VideoImage()
        {
            if (this.Data != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this.Data);
                this.Data = IntPtr.Zero;
            }
        }

        [DllImport("kernel32.dll")]
        static extern void CopyMemory(IntPtr dest, IntPtr src, uint len);

        private VideoImage(int width, int height, int stride, IntPtr data)
        {
            this.Width = width;
            this.Height = height;
            this.Stride = stride;

            uint size = (uint)(stride * height);

            this.Data = Marshal.AllocHGlobal((int)size);
            CopyMemory(this.Data, data, size);
        }

        void IDisposable.Dispose()
        {
            Marshal.FreeHGlobal(this.Data);
            this.Data = IntPtr.Zero;
        }

        public IVideoImage Clone()
        {
            return new VideoImage(Width, Height, Stride, Data);
        }
    }
}
