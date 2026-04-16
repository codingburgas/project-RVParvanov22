using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicrobloggingSystem.Data;
using MicrobloggingSystem.Models;
using MicrobloggingSystem.Models.DTOs.Auth;
using MicrobloggingSystem.Models.DTOs;
using MicrobloggingSystem.Models.ViewModels;

namespace MicrobloggingSystem.Controllers
{
    public class AccountController : Controller
    {
        private static readonly IReadOnlyList<GameProfileDefinition> SupportedGames =
        [
            new("Valorant", "Tactical FPS profile for team scouts and highlight reels.", ["valorant", "radiant", "immortal", "vct", "ace", "clutch"]),
            new("Counter-Strike 2", "Round control, entry impact and LAN-ready consistency.", ["cs2", "counter-strike", "counter strike", "faceit", "premier", "frag"]),
            new("League of Legends", "Ranked ladder progress, macro reads and teamfight moments.", ["league", "lol", "ranked", "diamond", "master", "pentakill"]),
            new("Fortnite", "Tournament placements, clips and endgame decision making.", ["fortnite", "fncs", "victory royale", "build fight", "zero build"]),
            new("Rocket League", "Mechanical ceiling, rotations and ranked grind.", ["rocket league", "grand champ", "champion", "flip reset", "air dribble"])
        ];

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public IActionResult Register()
        {
            return View(new RegisterDto());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "A user with this email already exists.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                DisplayName = model.DisplayName,
                Region = model.Region,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "User");
            await _signInManager.SignInAsync(user, false);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginDto());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Display user profile
        /// GET /Account/Profile
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Profile(string? id = null)
        {
            var currentUserId = _userManager.GetUserId(User);
            var userId = string.IsNullOrWhiteSpace(id) ? currentUserId : id;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .Include(u => u.Posts!)
                    .ThenInclude(p => p.Comments)
                .Include(u => u.Posts!)
                    .ThenInclude(p => p.PostLikes)
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User profile not found");
            }

            var recentPosts = (user.Posts ?? [])
                .OrderByDescending(p => p.CreatedAt)
                .Take(6)
                .Select(p => new PostResponseDto
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
                    UserId = user.Id,
                    UserDisplayName = user.DisplayName ?? user.UserName,
                    UserProfilePictureUrl = user.ProfilePictureUrl
                })
                .ToList();

            var gameCards = BuildGameCards(user);
            return View(BuildUserProfileViewModel(user, currentUserId, recentPosts, gameCards));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UpdateUserProfileDto model)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .Include(u => u.Posts!)
                    .ThenInclude(p => p.Comments)
                .Include(u => u.Posts!)
                    .ThenInclude(p => p.PostLikes)
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (user == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var fallbackRecentPosts = (user.Posts ?? [])
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(6)
                    .Select(p => new PostResponseDto
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
                        UserId = user.Id,
                        UserDisplayName = user.DisplayName ?? user.UserName,
                        UserProfilePictureUrl = user.ProfilePictureUrl
                    })
                    .ToList();

                var invalidViewModel = BuildUserProfileViewModel(user, currentUserId, fallbackRecentPosts, BuildGameCards(user));
                invalidViewModel.EditProfile = model;
                return View("Profile", invalidViewModel);
            }

            user.DisplayName = model.DisplayName;
            user.Bio = model.Bio;
            user.ProfilePictureUrl = model.ProfilePictureUrl;
            user.Region = model.Region;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Profile));
        }

        private static List<PlayerGameCardViewModel> BuildGameCards(ApplicationUser user)
        {
            var posts = (user.Posts ?? [])
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            return SupportedGames
                .Select(game =>
                {
                    var matchingPosts = posts
                        .Where(post => MatchesGame(post, game))
                        .ToList();

                    var topMoments = matchingPosts
                        .Take(3)
                        .Select(post => post.Content.Length > 90
                            ? $"{post.Content[..87]}..."
                            : post.Content)
                        .ToList();

                    return new PlayerGameCardViewModel
                    {
                        GameName = game.Name,
                        Tagline = game.Tagline,
                        StatusLabel = matchingPosts.Count > 0 ? "Active profile" : "No highlights yet",
                        HighlightCount = matchingPosts.Count,
                        AchievementCount = matchingPosts.Count(post =>
                            string.Equals(post.PostType, "Achievement", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(post.PostType, "MatchResult", StringComparison.OrdinalIgnoreCase)),
                        TotalLikes = matchingPosts.Sum(post => post.PostLikes?.Count ?? 0),
                        TotalComments = matchingPosts.Sum(post => post.Comments?.Count ?? 0),
                        TopMoments = topMoments
                    };
                })
                .ToList();
        }

        private static bool MatchesGame(Post post, GameProfileDefinition game)
        {
            var searchableText = $"{post.GameTitle} {post.Content} {post.PostType} {post.MediaType}";
            return game.Keywords.Any(keyword =>
                searchableText.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private static UserProfileViewModel BuildUserProfileViewModel(
            ApplicationUser user,
            string? currentUserId,
            IReadOnlyList<PostResponseDto> recentPosts,
            IReadOnlyList<PlayerGameCardViewModel> gameCards)
        {
            var isCurrentUser = string.Equals(user.Id, currentUserId, StringComparison.Ordinal);
            return new UserProfileViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName ?? user.UserName ?? string.Empty,
                Bio = user.Bio,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Region = user.Region,
                CreatedAt = user.CreatedAt,
                FollowersCount = user.Followers?.Count ?? 0,
                FollowingCount = user.Following?.Count ?? 0,
                PostsCount = user.Posts?.Count ?? 0,
                IsCurrentUser = isCurrentUser,
                ActiveGamesCount = gameCards.Count(g => g.HighlightCount > 0),
                TotalHighlightsCount = gameCards.Sum(g => g.HighlightCount),
                Games = gameCards,
                RecentPosts = recentPosts,
                CanFollow = !isCurrentUser && !string.IsNullOrEmpty(currentUserId),
                IsFollowingUser = !string.IsNullOrEmpty(currentUserId) &&
                    (user.Followers?.Any(f => f.FollowerId == currentUserId) ?? false),
                EditProfile = new UpdateUserProfileDto
                {
                    DisplayName = user.DisplayName ?? user.UserName ?? string.Empty,
                    Bio = user.Bio,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    Region = user.Region
                }
            };
        }

        private sealed record GameProfileDefinition(string Name, string Tagline, IReadOnlyList<string> Keywords);
    }
}
