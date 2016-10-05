using System.Collections.Generic;

namespace AzureMediaRedactor.Models
{
    public interface IVideoFilterProvider
    {
        IEnumerable<Annotation> OnFiltering(float time, int userData);
    }

    public interface IVideoFilter
    {
        void SetProvider(IVideoFilterProvider provider, int userData);
        void Filter(IVideoFrame frame);
    }
}
