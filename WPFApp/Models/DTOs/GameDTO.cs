namespace DTOs
{
    public class GameDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string Developer { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Amount { get; set; }
        public bool IsActive { get; set; }
        public string UrlImagen { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
    }
}
