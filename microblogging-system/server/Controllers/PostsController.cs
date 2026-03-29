using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicrobloggingSystem.Data;
using MicrobloggingSystem.Models;
using MicrobloggingSystem.Models.DTOs;

namespace MicrobloggingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PostsController> _logger;

        public PostsController(ApplicationDbContext context, ILogger<PostsController> logger)
        {
            _context = context;
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
        [HttpPost]
        public async Task<ActionResult<PostResponseDto>> CreatePost([FromBody] CreatePostDto createPostDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createPostDto.Content))
                {
                    return BadRequest(new { error = "Content is required" });
                }

                if (string.IsNullOrWhiteSpace(createPostDto.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var user = await _context.Users.FindAsync(createPostDto.UserId);
                if (user == null)
                {
                    return BadRequest(new { error = "User not found" });
                }

                var post = new Post
                {
                    Content = createPostDto.Content,
                    PostType = createPostDto.PostType,
                    MediaPath = createPostDto.MediaPath,
                    MediaType = createPostDto.MediaType,
                    UserId = createPostDto.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                var postResponse = new PostResponseDto
                {
                    Id = post.Id,
                    Content = post.Content,
                    PostType = post.PostType,
                    MediaPath = post.MediaPath,
                    MediaType = post.MediaType,
                    CreatedAt = post.CreatedAt,
                    CommentsCount = 0,
                    LikesCount = 0,
                    UserId = post.UserId,
                    UserDisplayName = user.DisplayName ?? user.UserName,
                    UserProfilePictureUrl = user.ProfilePictureUrl
                };

                return CreatedAtAction(nameof(GetPost), new { id = post.Id }, postResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                return StatusCode(500, new { error = "An error occurred while creating the post" });
            }
        }
    }
}
