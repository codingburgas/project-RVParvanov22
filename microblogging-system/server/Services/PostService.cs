using Microsoft.EntityFrameworkCore;
using MicrobloggingSystem.Data;
using MicrobloggingSystem.Interfaces;
using MicrobloggingSystem.Models;
using MicrobloggingSystem.Models.DTOs;

namespace MicrobloggingSystem.Services
{
    public class PostService : IPostService
    {
        private readonly ApplicationDbContext _context;

        public PostService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PostResponseDto>> GetPostsAsync(int pageNumber, int pageSize)
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                .Include(p => p.PostLikes)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return posts.Select(post => ToDto(post)).ToList();
        }

        public async Task<PostResponseDto?> GetPostByIdAsync(int id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                .Include(p => p.PostLikes)
                .FirstOrDefaultAsync(p => p.Id == id);

            return post == null ? null : ToDto(post);
        }

        public async Task<PostResponseDto> CreatePostAsync(CreatePostDto createPostDto)
        {
            var user = await _context.Users.FindAsync(createPostDto.UserId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            var post = new Post
            {
                GameTitle = createPostDto.GameTitle,
                Content = createPostDto.Content,
                PostType = createPostDto.PostType,
                MediaPath = createPostDto.MediaPath,
                MediaType = createPostDto.MediaType,
                UserId = createPostDto.UserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return ToDto(post, user);
        }

        public async Task<bool> UpdatePostAsync(int id, UpdatePostDto updatePostDto)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return false;
            }

            post.Content = updatePostDto.Content;
            post.GameTitle = updatePostDto.GameTitle;
            post.PostType = updatePostDto.PostType ?? post.PostType;
            post.MediaPath = updatePostDto.MediaPath ?? post.MediaPath;
            post.MediaType = updatePostDto.MediaType ?? post.MediaType;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePostAsync(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return false;
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<PostResponseDto>> GetFeedAsync(string userId, int pageNumber, int pageSize)
        {
            var followedIds = await _context.Follows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowingId)
                .ToListAsync();

            if (followedIds.Count == 0)
            {
                return await GetPostsAsync(pageNumber, pageSize);
            }

            var posts = await _context.Posts
                .Where(p => followedIds.Contains(p.UserId) || p.UserId == userId)
                .Include(p => p.User)
                .Include(p => p.Comments)
                .Include(p => p.PostLikes)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return posts.Select(post => ToDto(post)).ToList();
        }

        private static PostResponseDto ToDto(Post post, ApplicationUser? user = null)
        {
            if (user == null)
            {
                user = post.User;
            }

            return new PostResponseDto
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
                UserDisplayName = user?.DisplayName ?? user?.UserName,
                UserProfilePictureUrl = user?.ProfilePictureUrl
            };
        }
    }
}
