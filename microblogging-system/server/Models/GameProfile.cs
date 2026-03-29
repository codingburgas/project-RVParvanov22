namespace MicrobloggingSystem.Models
{
    public class GameProfile
    {
        public int Id { get; set; }
        public string GameName { get; set; } = string.Empty; // Valorant, CS2, League of Legends, etc.
        public string InGameName { get; set; } = string.Empty;
        public string? Rank { get; set; } // Immortal, Global Elite, Diamond, etc.
        public string? MainRole { get; set; } // Duelist, Sentinel, Controller, etc.
        public string? PeakRank { get; set; }
        public string? FavoriteCharacter { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        public ApplicationUser? User { get; set; }
        public ICollection<MatchEntry>? MatchEntries { get; set; }
    }
}
