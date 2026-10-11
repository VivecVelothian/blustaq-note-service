namespace bluestaqNotesService.Models
{
    public class User
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;
        public string Username { get; set; } = string.Empty;
    }
}
