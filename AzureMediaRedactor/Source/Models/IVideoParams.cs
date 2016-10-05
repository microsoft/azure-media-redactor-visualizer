namespace AzureMediaRedactor.Models
{
    public interface IVideoProperties
    {
        int Width { get; }
        int Height { get; }
        float FrameRate { get; }
        float TimeScale { get; }
        float Duration { get; }
    }
}
