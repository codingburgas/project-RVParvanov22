namespace MicrobloggingSystem.Models
{
    public class MatchEntry
    {
        public int Id { get; set; }
        public string MatchName { get; set; } = string.Empty; // Match name or description
        public string Result { get; set; } = string.Empty; // Win, Loss, Draw, etc.
        public string? StatsSummary { get; set; } // KDA, score, or other stats
        public DateTime MatchDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key
        public int GameProfileId { get; set; }

        // Navigation property
        public GameProfile? GameProfile { get; set; }
    }
}
