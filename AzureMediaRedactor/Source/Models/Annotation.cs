namespace AzureMediaRedactor.Models
{
    public class Annotation
    {
        public int Id { get; }
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }

        public Annotation(int id, float x, float y, float width, float height)
        {
            this.Id = id;
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }
    }
}
