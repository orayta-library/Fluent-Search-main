using Avalonia.Media.Imaging;

namespace MySearchApp.Models
{
    public class SearchResult
    {
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public Bitmap? Thumbnail { get; set; }
        public string? Path { get; set; }
    }
}
