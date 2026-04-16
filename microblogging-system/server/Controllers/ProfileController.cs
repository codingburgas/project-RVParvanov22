using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IUserService userService, ILogger<ProfileController> logger)
        {
            _userService = userService;
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

                var profile = await _userService.GetUserByIdAsync(userId);
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

                // Verify user exists
                var userExists = await _userService.UserExistsAsync(userId);
                if (!userExists)
                {
                    return NotFound(new { error = "User not found" });
                }

                var updatedProfile = await _userService.UpdateUserProfileAsync(userId, updateDto);
                if (updatedProfile == null)
                {
                    return NotFound(new { error = "Failed to update profile" });
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
