using Microsoft.EntityFrameworkCore;
using MicrobloggingSystem.Data;
using MicrobloggingSystem.Interfaces;
using MicrobloggingSystem.Models;
using MicrobloggingSystem.Models.DTOs;

namespace MicrobloggingSystem.Services
{
    /// <summary>
    /// Service for user profile operations
    /// Handles profile retrieval and updates
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get user profile by UserId
        /// </summary>
        public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .Include(u => u.Following)
                    .Include(u => u.Followers)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogWarning("User not found with ID: {UserId}", userId);
                    return null;
                }

                return MapToUserProfileDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get user profile by Username (Email in this case)
        /// </summary>
        public async Task<UserProfileDto?> GetUserByUsernameAsync(string username)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Email == username || u.UserName == username)
                    .Include(u => u.Following)
                    .Include(u => u.Followers)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogWarning("User not found with username: {Username}", username);
                    return null;
                }

                return MapToUserProfileDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by username: {Username}", username);
                throw;
            }
        }

        /// <summary>
        /// Update user profile information
        /// </summary>
        public async Task<bool> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updateDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for update: {UserId}", userId);
                    return false;
                }

                // Update fields
                user.DisplayName = updateDto.DisplayName ?? user.DisplayName;
                user.Bio = updateDto.Bio;
                user.ProfilePictureUrl = updateDto.ProfilePictureUrl;
                user.Region = updateDto.Region ?? user.Region;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User profile updated: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Search users by display name, username, or region
        /// </summary>
        public async Task<IEnumerable<UserSearchResultDto>> SearchUsersAsync(string query, string? currentUserId = null, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Enumerable.Empty<UserSearchResultDto>();
                }

                var users = await _context.Users
                    .Include(u => u.Followers)
                    .Where(u =>
                        u.DisplayName.Contains(query) ||
                        u.UserName.Contains(query) ||
                        (u.Region != null && u.Region.Contains(query))
                    )
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Get follow relationships for current user if provided
                var followingIds = new HashSet<string>();
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    followingIds = (await _context.Follows
                        .Where(f => f.FollowerId == currentUserId)
                        .Select(f => f.FollowingId)
                        .ToListAsync())
                        .ToHashSet();
                }

                return users.Select(user => new UserSearchResultDto
                {
                    Id = user.Id,
                    DisplayName = user.DisplayName ?? user.UserName,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    Region = user.Region,
                    FollowersCount = user.Followers?.Count ?? 0,
                    IsFollowing = followingIds.Contains(user.Id)
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users with query: {Query}", query);
                throw;
            }
        }

        /// <summary>
        /// Map ApplicationUser to UserProfileDto
        /// </summary>
        private UserProfileDto MapToUserProfileDto(ApplicationUser user)
        {
            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName ?? user.UserName ?? string.Empty,
                Bio = user.Bio,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Region = user.Region,
                FollowersCount = user.Followers?.Count ?? 0,
                FollowingCount = user.Following?.Count ?? 0
            };
        }
    }
}
