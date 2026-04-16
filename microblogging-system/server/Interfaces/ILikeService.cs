using MicrobloggingSystem.Models.DTOs;

namespace MicrobloggingSystem.Interfaces
{
    public interface ILikeService
    {
        /// <summary>
        /// Get all likes for a post
        /// </summary>
        Task<PostLikesDto> GetPostLikesAsync(int postId, string? currentUserId = null);

        /// <summary>
        /// Check if a user likes a specific post
        /// </summary>
        Task<bool> UserLikedPostAsync(string userId, int postId);

        /// <summary>
        /// Like a post
        /// </summary>
        Task<LikeResponseDto> LikePostAsync(CreateLikeDto createLikeDto);

        /// <summary>
        /// Unlike a post
        /// </summary>
        Task<bool> UnlikePostAsync(string userId, int postId);

        /// <summary>
        /// Get like count for a post
        /// </summary>
        Task<int> GetLikeCountAsync(int postId);

        /// <summary>
        /// Get all posts liked by a user
        /// </summary>
        Task<IEnumerable<PostResponseDto>> GetUserLikedPostsAsync(string userId, int pageNumber, int pageSize);
    }
}
