using MicrobloggingSystem.Models.DTOs;

namespace MicrobloggingSystem.Models.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<FeedPostViewModel> Posts { get; set; } = Enumerable.Empty<FeedPostViewModel>();
        public bool IsAuthenticated { get; set; }
        public int TotalPosts { get; set; }
        public int HighlightPosts { get; set; }
        public int ActiveGames { get; set; }
    }

    public class FeedPostViewModel
    {
        public PostResponseDto Post { get; set; } = new();
        public bool IsLikedByCurrentUser { get; set; }
        public bool IsFollowingAuthor { get; set; }
        public bool IsOwnPost { get; set; }
    }
}
