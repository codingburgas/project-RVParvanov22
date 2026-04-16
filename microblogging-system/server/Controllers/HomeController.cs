using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicrobloggingSystem.Data;
using MicrobloggingSystem.Interfaces;
using MicrobloggingSystem.Models;
using MicrobloggingSystem.Models.DTOs;
using MicrobloggingSystem.Models.ViewModels;

namespace MicrobloggingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPostService _postService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            IPostService postService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _postService = postService;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            IEnumerable<PostResponseDto> posts;
            var currentUserId = _userManager.GetUserId(User);
            var likedPostIds = new HashSet<int>();
            var followingUserIds = new HashSet<string>();
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = currentUserId ?? string.Empty;
                posts = await _postService.GetFeedAsync(userId, pageNumber, 20);
                likedPostIds = await _context.PostLikes
                    .Where(l => l.UserId == userId)
                    .Select(l => l.PostId)
                    .ToHashSetAsync();
                followingUserIds = await _context.Follows
                    .Where(f => f.FollowerId == userId)
                    .Select(f => f.FollowingId)
                    .ToHashSetAsync();
            }
            else
            {
                posts = await _postService.GetPostsAsync(pageNumber, 20);
            }

            var postCards = posts
                .Select(post => new FeedPostViewModel
                {
                    Post = post,
                    IsLikedByCurrentUser = likedPostIds.Contains(post.Id),
                    IsFollowingAuthor = followingUserIds.Contains(post.UserId),
                    IsOwnPost = currentUserId == post.UserId
                })
                .ToList();

            var model = new HomeViewModel
            {
                Posts = postCards,
                IsAuthenticated = User.Identity?.IsAuthenticated == true,
                TotalPosts = postCards.Count,
                HighlightPosts = postCards.Count(p => string.Equals(p.Post.PostType, "Clip", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Post.PostType, "Achievement", StringComparison.OrdinalIgnoreCase)),
                ActiveGames = postCards
                    .Select(p => p.Post.GameTitle)
                    .Where(game => !string.IsNullOrWhiteSpace(game))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count()
            };

            return View(model);
        }

        public async Task<IActionResult> Reports()
        {
            var topUsers = await _context.Users
                .Select(u => new TopUserViewModel
                {
                    UserId = u.Id,
                    DisplayName = u.DisplayName ?? u.UserName ?? "Unknown",
                    FollowersCount = _context.Follows.Count(f => f.FollowingId == u.Id)
                })
                .OrderByDescending(u => u.FollowersCount)
                .Take(5)
                .ToListAsync();

            var popularPost = await _context.Posts
                .Include(p => p.Comments)
                .Include(p => p.PostLikes)
                .Include(p => p.User)
                .OrderByDescending(p => (p.Comments!.Count + p.PostLikes!.Count))
                .FirstOrDefaultAsync();

            var averagePosts = await _context.Posts
                .GroupBy(p => p.UserId)
                .Select(g => g.Count())
                .DefaultIfEmpty(0)
                .AverageAsync();

            var model = new ReportsViewModel
            {
                TopUsers = topUsers,
                PopularPost = popularPost == null ? null : new PostResponseDto
                {
                    Id = popularPost.Id,
                    Content = popularPost.Content,
                    PostType = popularPost.PostType,
                    MediaPath = popularPost.MediaPath,
                    MediaType = popularPost.MediaType,
                    CreatedAt = popularPost.CreatedAt,
                    CommentsCount = popularPost.Comments?.Count ?? 0,
                    LikesCount = popularPost.PostLikes?.Count ?? 0,
                    UserId = popularPost.UserId,
                    UserDisplayName = popularPost.User?.DisplayName ?? popularPost.User?.UserName,
                    UserProfilePictureUrl = popularPost.User?.ProfilePictureUrl
                },
                AveragePostsPerUser = averagePosts
            };

            return View(model);
        }
    }
}
