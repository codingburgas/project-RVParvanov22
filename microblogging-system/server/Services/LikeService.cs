using Microsoft.EntityFrameworkCore;
using MicrobloggingSystem.Data;
using MicrobloggingSystem.Interfaces;
using MicrobloggingSystem.Models;
using MicrobloggingSystem.Models.DTOs;

namespace MicrobloggingSystem.Services
{
    public class LikeService : ILikeService
    {
        private readonly ApplicationDbContext _context;

        public LikeService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all likes for a post with optional user preference info
        /// </summary>
        public async Task<PostLikesDto> GetPostLikesAsync(int postId, string? currentUserId = null)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                throw new InvalidOperationException("Post not found");
            }

            var likes = await _context.PostLikes
                .Where(pl => pl.PostId == postId)
                .Include(pl => pl.User)
                .OrderByDescending(pl => pl.CreatedAt)
                .ToListAsync();

            var currentUserLiked = currentUserId != null && 
                await _context.PostLikes.AnyAsync(pl => pl.PostId == postId && pl.UserId == currentUserId);

            return new PostLikesDto
            {
                PostId = postId,
                TotalLikes = likes.Count,
                CurrentUserLiked = currentUserLiked,
                Likes = likes.Select(pl => new LikeResponseDto
                {
                    Id = pl.Id,
                    UserId = pl.UserId,
                    PostId = pl.PostId,
                    CreatedAt = pl.CreatedAt,
                    UserDisplayName = pl.User?.DisplayName ?? pl.User?.UserName
                }).ToList()
            };
        }

        /// <summary>
        /// Check if a specific user has liked a specific post
        /// </summary>
        public async Task<bool> UserLikedPostAsync(string userId, int postId)
        {
            return await _context.PostLikes
                .AnyAsync(pl => pl.UserId == userId && pl.PostId == postId);
        }

        /// <summary>
        /// Like a post (create a like)
        /// </summary>
        public async Task<LikeResponseDto> LikePostAsync(CreateLikeDto createLikeDto)
        {
            // Validate user exists
            var user = await _context.Users.FindAsync(createLikeDto.UserId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Validate post exists
            var post = await _context.Posts.FindAsync(createLikeDto.PostId);
            if (post == null)
            {
                throw new InvalidOperationException("Post not found");
            }

            // Check if already liked
            var existingLike = await _context.PostLikes
                .FirstOrDefaultAsync(pl => pl.UserId == createLikeDto.UserId && pl.PostId == createLikeDto.PostId);
            
            if (existingLike != null)
            {
                throw new InvalidOperationException("User has already liked this post");
            }

            var like = new PostLike
            {
                UserId = createLikeDto.UserId,
                PostId = createLikeDto.PostId,
                CreatedAt = DateTime.UtcNow
            };

            _context.PostLikes.Add(like);
            await _context.SaveChangesAsync();

            return new LikeResponseDto
            {
                Id = like.Id,
                UserId = like.UserId,
                PostId = like.PostId,
                CreatedAt = like.CreatedAt,
                UserDisplayName = user.DisplayName ?? user.UserName
            };
        }

        /// <summary>
        /// Unlike a post (delete a like)
        /// </summary>
        public async Task<bool> UnlikePostAsync(string userId, int postId)
        {
            var like = await _context.PostLikes
                .FirstOrDefaultAsync(pl => pl.UserId == userId && pl.PostId == postId);

            if (like == null)
            {
                return false;
            }

            _context.PostLikes.Remove(like);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Get total number of likes for a post
        /// </summary>
        public async Task<int> GetLikeCountAsync(int postId)
        {
            return await _context.PostLikes
                .CountAsync(pl => pl.PostId == postId);
        }

        /// <summary>
        /// Get all posts liked by a specific user
        /// </summary>
        public async Task<IEnumerable<PostResponseDto>> GetUserLikedPostsAsync(string userId, int pageNumber, int pageSize)
        {
            var likedPostIds = await _context.PostLikes
                .Where(pl => pl.UserId == userId)
                .Select(pl => pl.PostId)
                .ToListAsync();

            var posts = await _context.Posts
                .Where(p => likedPostIds.Contains(p.Id))
                .Include(p => p.User)
                .Include(p => p.Comments)
                .Include(p => p.PostLikes)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return posts.Select(post => new PostResponseDto
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
            }).ToList();
        }
    }
}
