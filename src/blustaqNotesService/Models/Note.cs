namespace bluestaqNotesService.Models
{
    public class Note
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public Team Team { get; set; }

        public int CreatedById { get; set; }
        public User CreatedBy { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
