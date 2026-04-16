using MicrobloggingSystem.Models.DTOs;

namespace MicrobloggingSystem.Interfaces
{
    public interface IPostService
    {
        Task<IEnumerable<PostResponseDto>> GetPostsAsync(int pageNumber, int pageSize);
        Task<PostResponseDto?> GetPostByIdAsync(int id);
        Task<PostResponseDto> CreatePostAsync(CreatePostDto createPostDto);
        Task<bool> UpdatePostAsync(int id, UpdatePostDto updatePostDto);
        Task<bool> DeletePostAsync(int id);
        Task<IEnumerable<PostResponseDto>> GetFeedAsync(string userId, int pageNumber, int pageSize);
        Task<IEnumerable<PostResponseDto>> SearchPostsAsync(string query, string? gameTitle = null, string? postType = null, int pageNumber = 1, int pageSize = 20);
        Task<IEnumerable<PostResponseDto>> GetDraftsAsync(string userId, int pageNumber = 1, int pageSize = 20);
        Task<bool> PublishDraftAsync(int postId);
    }
}
