namespace MicrobloggingSystem.Models.ViewModels
{
    /// <summary>
    /// ViewModel for user profile display
    /// Used in /Account/Profile view
    /// </summary>
    public class UserProfileViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Region { get; set; }
        public DateTime CreatedAt { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public int PostsCount { get; set; }
        public bool IsCurrentUser { get; set; }
        public int ActiveGamesCount { get; set; }
        public int TotalHighlightsCount { get; set; }
        public IReadOnlyList<PlayerGameCardViewModel> Games { get; set; } = Array.Empty<PlayerGameCardViewModel>();
        public IReadOnlyList<DTOs.PostResponseDto> RecentPosts { get; set; } = Array.Empty<DTOs.PostResponseDto>();
        public bool IsFollowingUser { get; set; }
        public bool CanFollow { get; set; }
        public DTOs.UpdateUserProfileDto EditProfile { get; set; } = new();
    }

    public class PlayerGameCardViewModel
    {
        public string GameName { get; set; } = string.Empty;
        public string Tagline { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public int HighlightCount { get; set; }
        public int AchievementCount { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComments { get; set; }
        public IReadOnlyList<string> TopMoments { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// ViewModel for layout/sidebar - passed to _Layout.cshtml
    /// Contains minimal user info for display in navigation
    /// </summary>
    public class LayoutUserViewModel
    {
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsAuthenticated { get; set; }
    }
}
