using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicrobloggingSystem.Data;
using MicrobloggingSystem.Interfaces;
using MicrobloggingSystem.Models;
using MicrobloggingSystem.Models.DTOs;

namespace MicrobloggingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPostService _postService;
        private readonly ILogger<PostsController> _logger;

        public PostsController(ApplicationDbContext context, IPostService postService, ILogger<PostsController> logger)
        {
            _context = context;
            _postService = postService;
            _logger = logger;
        }

        /// <summary>
        /// Get all posts with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetPosts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var posts = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Comments)
                    .Include(p => p.PostLikes)
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var postsResponse = posts.Select(p => new PostResponseDto
                {
                    Id = p.Id,
                    GameTitle = p.GameTitle,
                    Content = p.Content,
                    PostType = p.PostType,
                    MediaPath = p.MediaPath,
                    MediaType = p.MediaType,
                    CreatedAt = p.CreatedAt,
                    CommentsCount = p.Comments?.Count ?? 0,
                    LikesCount = p.PostLikes?.Count ?? 0,
                    UserId = p.UserId,
                    UserDisplayName = p.User?.DisplayName ?? p.User?.UserName,
                    UserProfilePictureUrl = p.User?.ProfilePictureUrl
                }).ToList();

                return Ok(new
                {
                    pageNumber,
                    pageSize,
                    totalPosts = await _context.Posts.CountAsync(),
                    data = postsResponse
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts");
                return StatusCode(500, new { error = "An error occurred while fetching posts" });
            }
        }

        /// <summary>
        /// Get a specific post by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PostResponseDto>> GetPost(int id)
        {
            try
            {
                var post = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Comments)
                    .Include(p => p.PostLikes)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (post == null)
                {
                    return NotFound(new { error = "Post not found" });
                }

                var postResponse = new PostResponseDto
                {
                    Id = post.Id,
                    GameTitle = post.GameTitle,
                    Content = post.Content,
                    PostType = post.PostType,
                    MediaPath = post.MediaPath,
                    MediaType = post.MediaType,
                    CreatedAt = post.CreatedAt,
                    CommentsCount = post.Comments?.Count ?? 0,
                    LikesCount = post.PostLikes?.Count ?? 0,
                    UserId = post.UserId,
                    UserDisplayName = post.User?.DisplayName ?? post.User?.UserName,
                    UserProfilePictureUrl = post.User?.ProfilePictureUrl
                };

                return Ok(postResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post");
                return StatusCode(500, new { error = "An error occurred while fetching the post" });
            }
        }

        /// <summary>
        /// Create a new post
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<PostResponseDto>> CreatePost([FromBody] CreatePostDto createPostDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(createPostDto.Content))
                {
                    return BadRequest(new { error = "Content is required" });
                }

                // Verify user is creating post for themselves
                var currentUserId = GetCurrentUserId();
                if (currentUserId != createPostDto.UserId)
                {
                    return Forbid("You can only create posts for your own account");
                }

                var post = await _postService.CreatePostAsync(createPostDto);
                return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                return StatusCode(500, new { error = "An error occurred while creating the post" });
            }
        }

        /// <summary>
        /// Get posts by a specific user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<object>> GetPostsByUser(string userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                // Verify user exists
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                var posts = await _context.Posts
                    .Where(p => p.UserId == userId)
                    .Include(p => p.User)
                    .Include(p => p.Comments)
                    .Include(p => p.PostLikes)
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var postsResponse = posts.Select(p => new PostResponseDto
                {
                    Id = p.Id,
                    GameTitle = p.GameTitle,
                    Content = p.Content,
                    PostType = p.PostType,
                    MediaPath = p.MediaPath,
                    MediaType = p.MediaType,
                    CreatedAt = p.CreatedAt,
                    CommentsCount = p.Comments?.Count ?? 0,
                    LikesCount = p.PostLikes?.Count ?? 0,
                    UserId = p.UserId,
                    UserDisplayName = p.User?.DisplayName ?? p.User?.UserName,
                    UserProfilePictureUrl = p.User?.ProfilePictureUrl
                }).ToList();

                var totalUserPosts = await _context.Posts.CountAsync(p => p.UserId == userId);

                return Ok(new
                {
                    userId,
                    pageNumber,
                    pageSize,
                    totalPosts = totalUserPosts,
                    data = postsResponse
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts by user {UserId}", userId);
                return StatusCode(500, new { error = "An error occurred while fetching user posts" });
            }
        }

        /// <summary>
        /// Update a post (only post owner can update)
        /// </summary>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostDto updatePostDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get the post to verify ownership
                var post = await _context.Posts.FindAsync(id);
                if (post == null)
                {
                    return NotFound(new { error = "Post not found" });
                }

                var currentUserId = GetCurrentUserId();
                if (post.UserId != currentUserId)
                {
                    return Forbid("You can only update your own posts");
                }

                var success = await _postService.UpdatePostAsync(id, updatePostDto);
                if (!success)
                {
                    return NotFound(new { error = "Post not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post {PostId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the post" });
            }
        }

        /// <summary>
        /// Delete a post (only post owner or admin can delete)
        /// </summary>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                // Get the post to verify ownership
                var post = await _context.Posts.FindAsync(id);
                if (post == null)
                {
                    return NotFound(new { error = "Post not found" });
                }

                var currentUserId = GetCurrentUserId();
                var isAdmin = User.IsInRole("Admin");

                if (post.UserId != currentUserId && !isAdmin)
                {
                    return Forbid("You can only delete your own posts");
                }

                var success = await _postService.DeletePostAsync(id);
                if (!success)
                {
                    return NotFound(new { error = "Post not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post {PostId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the post" });
            }
        }

        /// <summary>
        /// Get user's personalized feed (posts from followed users)
        /// </summary>
        [Authorize]
        [HttpGet("feed/personalized")]
        public async Task<ActionResult<object>> GetPersonalizedFeed([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                var posts = await _postService.GetFeedAsync(userId, pageNumber, pageSize);
                return Ok(new
                {
                    pageNumber,
                    pageSize,
                    data = posts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting personalized feed");
                return StatusCode(500, new { error = "An error occurred while fetching feed" });
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
