namespace MicrobloggingSystem.Models.DTOs
{
    public class CreateGameProfileDto
    {
        public string UserId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string InGameName { get; set; } = string.Empty;
        public string? Rank { get; set; }
        public string? MainRole { get; set; }
        public string? PeakRank { get; set; }
        public string? FavoriteCharacter { get; set; }
    }

    public class GameProfileResponseDto
    {
        public int Id { get; set; }
        public string GameName { get; set; } = string.Empty;
        public string InGameName { get; set; } = string.Empty;
        public string? Rank { get; set; }
        public string? MainRole { get; set; }
        public string? PeakRank { get; set; }
        public string? FavoriteCharacter { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? UserDisplayName { get; set; }
        public int MatchEntriesCount { get; set; }
    }
}
