using MicrobloggingSystem.Models.DTOs;

namespace MicrobloggingSystem.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetUserProfileAsync(string userId);
        Task<UserProfileDto?> GetUserByUsernameAsync(string username);
        Task<bool> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updateDto);
        Task<IEnumerable<UserSearchResultDto>> SearchUsersAsync(string query, string? currentUserId = null, int pageNumber = 1, int pageSize = 20);
    }
}