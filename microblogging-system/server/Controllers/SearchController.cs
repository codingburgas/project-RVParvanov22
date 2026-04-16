using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MicrobloggingSystem.Interfaces;
using MicrobloggingSystem.Models;
using MicrobloggingSystem.Models.DTOs;

namespace MicrobloggingSystem.Controllers
{
    /// <summary>
    /// MVC Controller for search functionality
    /// Provides search views for users and posts
    /// </summary>
    public class SearchController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPostService _postService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SearchController> _logger;

        public SearchController(
            IUserService userService,
            IPostService postService,
            UserManager<ApplicationUser> userManager,
            ILogger<SearchController> logger)
        {
            _userService = userService;
            _postService = postService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Display search page
        /// GET /Search
        /// </summary>
        [HttpGet]
        public IActionResult Index(string? q, string? type)
        {
            ViewBag.Query = q;
            ViewBag.Type = type ?? "all";
            return View();
        }

        /// <summary>
        /// Search users
        /// GET /Search/Users
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Users(string q, int page = 1, int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                ViewBag.Query = q;
                return View(new List<UserSearchResultDto>());
            }

            try
            {
                var currentUserId = _userManager.GetUserId(User);
                var users = await _userService.SearchUsersAsync(q, currentUserId, page, pageSize);
                ViewBag.Query = q;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users with query: {Query}", q);
                TempData["Error"] = "An error occurred while searching users.";
                return View(new List<UserSearchResultDto>());
            }
        }

        /// <summary>
        /// Search posts
        /// GET /Search/Posts
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Posts(string q, string? gameTitle, string? postType, int page = 1, int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                ViewBag.Query = q;
                ViewBag.GameTitle = gameTitle;
                ViewBag.PostType = postType;
                return View(new List<PostResponseDto>());
            }

            try
            {
                var posts = await _postService.SearchPostsAsync(q, gameTitle, postType, page, pageSize);
                ViewBag.Query = q;
                ViewBag.GameTitle = gameTitle;
                ViewBag.PostType = postType;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                return View(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching posts with query: {Query}", q);
                TempData["Error"] = "An error occurred while searching posts.";
                return View(new List<PostResponseDto>());
            }
        }
    }
}