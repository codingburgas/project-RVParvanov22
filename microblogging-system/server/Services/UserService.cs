using Microsoft.EntityFrameworkCore;
using MicrobloggingSystem.Data;
using MicrobloggingSystem.Models;
using MicrobloggingSystem.Models.DTOs;

namespace MicrobloggingSystem.Services
{
    /// <summary>
    /// Service interface for user profile operations
    /// </summary>
    public interface IUserService
    {
        Task<UserProfileDto?> GetUserByIdAsync(string userId);
        Task<UserProfileDto?> GetUserByUsernameAsync(string username);
        Task<UserProfileDto?> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updateDto);
        Task<bool> UserExistsAsync(string userId);
    }

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
        public async Task<UserProfileDto?> GetUserByIdAsync(string userId)
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
        public async Task<UserProfileDto?> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updateDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for update: {UserId}", userId);
                    return null;
                }

                // Update fields
                user.DisplayName = updateDto.DisplayName ?? user.DisplayName;
                user.Bio = updateDto.Bio;
                user.ProfilePictureUrl = updateDto.ProfilePictureUrl;
                user.Region = updateDto.Region ?? user.Region;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User profile updated: {UserId}", userId);

                // Reload with relationships
                await _context.Entry(user).ReloadAsync();
                await _context.Entry(user).Collection(u => u.Following!).LoadAsync();
                await _context.Entry(user).Collection(u => u.Followers!).LoadAsync();

                return MapToUserProfileDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Check if user exists
        /// </summary>
        public async Task<bool> UserExistsAsync(string userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
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
