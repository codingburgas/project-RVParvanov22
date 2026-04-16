using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicrobloggingSystem.Interfaces;
using MicrobloggingSystem.Models.DTOs;

namespace MicrobloggingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LikesController : ControllerBase
    {
        private readonly ILikeService _likeService;
        private readonly ILogger<LikesController> _logger;

        public LikesController(ILikeService likeService, ILogger<LikesController> logger)
        {
            _likeService = likeService;
            _logger = logger;
        }

        /// <summary>
        /// Get all likes for a specific post
        /// </summary>
        [HttpGet("post/{postId}")]
        public async Task<ActionResult<PostLikesDto>> GetPostLikes(int postId)
        {
            try
            {
                var likes = await _likeService.GetPostLikesAsync(postId, GetCurrentUserId());
                return Ok(likes);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting likes for post {PostId}", postId);
                return StatusCode(500, new { error = "An error occurred while fetching likes" });
            }
        }

        /// <summary>
        /// Get like count for a post
        /// </summary>
        [HttpGet("post/{postId}/count")]
        public async Task<ActionResult<object>> GetLikeCount(int postId)
        {
            try
            {
                var count = await _likeService.GetLikeCountAsync(postId);
                return Ok(new { postId, likeCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting like count for post {PostId}", postId);
                return StatusCode(500, new { error = "An error occurred while fetching like count" });
            }
        }

        /// <summary>
        /// Check if current user likes a specific post
        /// </summary>
        [Authorize]
        [HttpGet("post/{postId}/user-liked")]
        public async Task<ActionResult<object>> CheckUserLiked(int postId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                var userLiked = await _likeService.UserLikedPostAsync(userId, postId);
                return Ok(new { postId, userLiked });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user liked post {PostId}", postId);
                return StatusCode(500, new { error = "An error occurred while checking like status" });
            }
        }

        /// <summary>
        /// Like a post
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<LikeResponseDto>> LikePost([FromBody] CreateLikeDto createLikeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verify the current user is liking their own like
                var currentUserId = GetCurrentUserId();
                if (currentUserId != createLikeDto.UserId)
                {
                    return Forbid("You can only like posts under your own account");
                }

                var like = await _likeService.LikePostAsync(createLikeDto);
                return CreatedAtAction(nameof(GetPostLikes), new { postId = like.PostId }, like);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking post");
                return StatusCode(500, new { error = "An error occurred while liking the post" });
            }
        }

        /// <summary>
        /// Unlike a post
        /// </summary>
        [Authorize]
        [HttpDelete("post/{postId}")]
        public async Task<IActionResult> UnlikePost(int postId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                var success = await _likeService.UnlikePostAsync(userId, postId);
                if (!success)
                {
                    return NotFound(new { error = "Like not found or already removed" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unliking post {PostId}", postId);
                return StatusCode(500, new { error = "An error occurred while unliking the post" });
            }
        }

        /// <summary>
        /// Get all posts liked by a user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<object>> GetUserLikedPosts(string userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var posts = await _likeService.GetUserLikedPostsAsync(userId, pageNumber, pageSize);
                return Ok(new
                {
                    userId,
                    pageNumber,
                    pageSize,
                    data = posts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting liked posts for user {UserId}", userId);
                return StatusCode(500, new { error = "An error occurred while fetching liked posts" });
            }
        }

        /// <summary>
        /// Helper method to get current user ID from JWT claims
        /// </summary>
        private string? GetCurrentUserId()
        {
            return User.FindFirst("userId")?.Value ?? User.FindFirst("sub")?.Value;
        }
    }
}
