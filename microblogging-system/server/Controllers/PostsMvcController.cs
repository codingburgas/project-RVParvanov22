using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using MicrobloggingSystem.Data;
using MicrobloggingSystem.Interfaces;
using MicrobloggingSystem.Models;
using MicrobloggingSystem.Models.DTOs;
using MicrobloggingSystem.Models.ViewModels;

namespace MicrobloggingSystem.Controllers
{
    public class PostsMvcController : Controller
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PostsMvcController(
            IPostService postService,
            ICommentService commentService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _postService = postService;
            _commentService = commentService;
            _userManager = userManager;
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            var posts = await _postService.GetPostsAsync(pageNumber, 20);
            return View(posts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            var comments = await _commentService.GetCommentsForPostAsync(id);
            var currentUserId = _userManager.GetUserId(User);
            var model = new PostDetailsViewModel
            {
                Post = post,
                Comments = comments,
                NewComment = new CreateCommentDto { PostId = id },
                IsOwnPost = currentUserId == post.UserId,
                IsLikedByCurrentUser = !string.IsNullOrEmpty(currentUserId) &&
                    await _context.PostLikes.AnyAsync(l => l.UserId == currentUserId && l.PostId == id),
                IsFollowingAuthor = !string.IsNullOrEmpty(currentUserId) &&
                    await _context.Follows.AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == post.UserId)
            };

            return View(model);
        }

        [Authorize]
        public IActionResult Create()
        {
            return View(new CreatePostDto());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(CreatePostDto createPostDto, IFormFile? videoFile)
        {
            await AttachUploadedVideoAsync(createPostDto, videoFile);

            if (!ModelState.IsValid)
            {
                return View(createPostDto);
            }

            createPostDto.UserId = _userManager.GetUserId(User) ?? string.Empty;
            var createdPost = await _postService.CreatePostAsync(createPostDto);
            return RedirectToAction(nameof(Details), new { id = createdPost.Id });
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            if (!await CanEditPostAsync(post))
            {
                return Forbid();
            }

            var model = new UpdatePostDto
            {
                Content = post.Content,
                GameTitle = post.GameTitle ?? string.Empty,
                PostType = post.PostType,
                MediaPath = post.MediaPath,
                MediaType = post.MediaType
            };

            ViewBag.PostId = id;
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(int id, UpdatePostDto updatePostDto, IFormFile? videoFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PostId = id;
                return View(updatePostDto);
            }

            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            if (!await CanEditPostAsync(post))
            {
                return Forbid();
            }

            var uploadedMediaPath = await SaveUploadedVideoAsync(videoFile);
            if (!string.IsNullOrEmpty(uploadedMediaPath))
            {
                DeleteExistingMedia(post.MediaPath);
                updatePostDto.MediaPath = uploadedMediaPath;
                updatePostDto.MediaType = "video";
            }
            else
            {
                updatePostDto.MediaPath = post.MediaPath;
                updatePostDto.MediaType = post.MediaType;
            }

            var success = await _postService.UpdatePostAsync(id, updatePostDto);
            if (!success)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            if (!await CanEditPostAsync(post))
            {
                return Forbid();
            }

            await _postService.DeletePostAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task AttachUploadedVideoAsync(CreatePostDto createPostDto, IFormFile? videoFile)
        {
            var uploadedMediaPath = await SaveUploadedVideoAsync(videoFile);
            if (!string.IsNullOrEmpty(uploadedMediaPath))
            {
                createPostDto.MediaPath = uploadedMediaPath;
                createPostDto.MediaType = "video";
            }
        }

        private async Task<string?> SaveUploadedVideoAsync(IFormFile? videoFile)
        {
            if (videoFile == null || videoFile.Length == 0)
            {
                return null;
            }

            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".mp4",
                ".mov",
                ".webm"
            };

            var extension = Path.GetExtension(videoFile.FileName);
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("MediaPath", "Only .mp4, .mov and .webm videos are allowed.");
                return null;
            }

            const long maxFileSizeBytes = 100 * 1024 * 1024;
            if (videoFile.Length > maxFileSizeBytes)
            {
                ModelState.AddModelError("MediaPath", "Video size must be 100MB or less.");
                return null;
            }

            var uploadsDirectory = Path.Combine(_environment.WebRootPath, "uploads", "videos");
            Directory.CreateDirectory(uploadsDirectory);

            var safeFileName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(uploadsDirectory, safeFileName);

            await using var stream = new FileStream(physicalPath, FileMode.Create);
            await videoFile.CopyToAsync(stream);

            return $"/uploads/videos/{safeFileName}";
        }

        private void DeleteExistingMedia(string? mediaPath)
        {
            if (string.IsNullOrWhiteSpace(mediaPath) || !mediaPath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var relativePath = mediaPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var physicalPath = Path.Combine(_environment.WebRootPath, relativePath);
            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
        }

        private async Task<bool> CanEditPostAsync(PostResponseDto post)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.Equals(currentUserId, post.UserId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var currentUser = await _userManager.GetUserAsync(User);
            return currentUser != null && await _userManager.IsInRoleAsync(currentUser, "Admin");
        }
    }
}
