using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicrobloggingSystem.Data;
using MicrobloggingSystem.Interfaces;
using MicrobloggingSystem.Models;
using MicrobloggingSystem.Models.DTOs;
using MicrobloggingSystem.Services;
using System.Security.Claims;

namespace MicrobloggingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            IUserService userService,
            ApplicationDbContext context,
            ILogger<ProfileController> logger)
        {
            _userService = userService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/profile/me
        /// Returns the currently logged-in user's profile data (based on JWT)
        /// Requires: [Authorize]
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserProfileDto>> GetCurrentUserProfile()
        {
            try
            {
                // Extract userId from JWT claims
                var userId = GetUserIdFromClaims();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unable to extract userId from JWT claims");
                    return Unauthorized(new { error = "Invalid token claims" });
                }

                var profile = await _userService.GetUserProfileAsync(userId);
                if (profile == null)
                {
                    return NotFound(new { error = "User profile not found" });
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user profile");
                return StatusCode(500, new { error = "An error occurred while retrieving profile" });
            }
        }

        /// <summary>
        /// GET /api/profile/{username}
        /// Returns public profile data by username or email
        /// Public endpoint - no authorization required
        /// </summary>
        [HttpGet("{username}")]
        public async Task<ActionResult<UserProfileDto>> GetUserProfile(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return BadRequest(new { error = "Username cannot be empty" });
                }

                var profile = await _userService.GetUserByUsernameAsync(username);
                if (profile == null)
                {
                    return NotFound(new { error = $"User '{username}' not found" });
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile for username: {Username}", username);
                return StatusCode(500, new { error = "An error occurred while retrieving profile" });
            }
        }

        /// <summary>
        /// PUT /api/profile
        /// Update the currently logged-in user's profile
        /// Requires: [Authorize]
        /// </summary>
        [HttpPut]
        [Authorize]
        public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateUserProfileDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        error = "Invalid input data",
                        details = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                // Extract userId from JWT claims
                var userId = GetUserIdFromClaims();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unable to extract userId from JWT claims during profile update");
                    return Unauthorized(new { error = "Invalid token claims" });
                }

                // Update user profile
                var success = await _userService.UpdateUserProfileAsync(userId, updateDto);
                if (!success)
                {
                    return NotFound(new { error = "Failed to update profile" });
                }

                // Get updated profile
                var updatedProfile = await _userService.GetUserProfileAsync(userId);
                if (updatedProfile == null)
                {
                    return NotFound(new { error = "Failed to retrieve updated profile" });
                }

                _logger.LogInformation("User profile updated successfully: {UserId}", userId);
                return Ok(updatedProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, new { error = "An error occurred while updating profile" });
            }
        }

        /// <summary>
        /// POST /api/profile/follow/{targetUserId}
        /// Follow a user
        /// Requires: [Authorize]
        /// </summary>
        [HttpPost("follow/{targetUserId}")]
        [Authorize]
        public async Task<ActionResult> FollowUser(string targetUserId)
        {
            try
            {
                var currentUserId = GetUserIdFromClaims();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new { error = "Invalid token claims" });
                }

                if (currentUserId == targetUserId)
                {
                    return BadRequest(new { error = "Cannot follow yourself" });
                }

                // Check if target user exists
                var targetUser = await _context.Users.FindAsync(targetUserId);
                if (targetUser == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                // Check if already following
                var existingFollow = await _context.Follows
                    .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == targetUserId);

                if (existingFollow != null)
                {
                    return BadRequest(new { error = "Already following this user" });
                }

                // Create follow relationship
                var follow = new Follow
                {
                    FollowerId = currentUserId,
                    FollowingId = targetUserId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Follows.Add(follow);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {FollowerId} followed user {FollowingId}", currentUserId, targetUserId);
                return Ok(new { message = "Successfully followed user" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error following user {TargetUserId}", targetUserId);
                return StatusCode(500, new { error = "An error occurred while following user" });
            }
        }

        /// <summary>
        /// DELETE /api/profile/follow/{targetUserId}
        /// Unfollow a user
        /// Requires: [Authorize]
        /// </summary>
        [HttpDelete("follow/{targetUserId}")]
        [Authorize]
        public async Task<ActionResult> UnfollowUser(string targetUserId)
        {
            try
            {
                var currentUserId = GetUserIdFromClaims();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new { error = "Invalid token claims" });
                }

                // Find existing follow relationship
                var existingFollow = await _context.Follows
                    .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == targetUserId);

                if (existingFollow == null)
                {
                    return BadRequest(new { error = "Not following this user" });
                }

                // Remove follow relationship
                _context.Follows.Remove(existingFollow);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {FollowerId} unfollowed user {FollowingId}", currentUserId, targetUserId);
                return Ok(new { message = "Successfully unfollowed user" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unfollowing user {TargetUserId}", targetUserId);
                return StatusCode(500, new { error = "An error occurred while unfollowing user" });
            }
        }

        /// <summary>
        /// GET /api/profile/follow/{targetUserId}/status
        /// Check if current user is following target user
        /// Requires: [Authorize]
        /// </summary>
        [HttpGet("follow/{targetUserId}/status")]
        [Authorize]
        public async Task<ActionResult> GetFollowStatus(string targetUserId)
        {
            try
            {
                var currentUserId = GetUserIdFromClaims();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new { error = "Invalid token claims" });
                }

                var isFollowing = await _context.Follows
                    .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == targetUserId);

                return Ok(new { isFollowing });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking follow status for user {TargetUserId}", targetUserId);
                return StatusCode(500, new { error = "An error occurred while checking follow status" });
            }
        }

        /// <summary>
        /// Helper method to extract userId from JWT claims
        /// The userId claim is added by JwtService.GenerateToken()
        /// </summary>
        private string? GetUserIdFromClaims()
        {
            // Try to get userId from custom "userId" claim first
            var userId = User.FindFirst("userId")?.Value;
            
            // Fallback to subject (sub) claim
            if (string.IsNullOrEmpty(userId))
            {
                userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }

            // Alternative fallback to JwtRegisteredClaimNames.Sub
            if (string.IsNullOrEmpty(userId))
            {
                userId = User.FindFirst("sub")?.Value;
            }

            return userId;
        }
    }
}
