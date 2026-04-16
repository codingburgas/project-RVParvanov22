using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicrobloggingSystem.Data;
using MicrobloggingSystem.Interfaces;
using MicrobloggingSystem.Models;
using MicrobloggingSystem.Models.DTOs;

namespace MicrobloggingSystem.Controllers
{
    [Authorize]
    public class SocialMvcController : Controller
    {
        private readonly ILikeService _likeService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SocialMvcController(
            ILikeService likeService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _likeService = likeService;
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLike(int postId, string? returnUrl = null)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var userAlreadyLiked = await _likeService.UserLikedPostAsync(userId, postId);
            if (userAlreadyLiked)
            {
                await _likeService.UnlikePostAsync(userId, postId);
            }
            else
            {
                await _likeService.LikePostAsync(new CreateLikeDto
                {
                    UserId = userId,
                    PostId = postId
                });
            }

            return RedirectToLocal(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFollow(string targetUserId, string? returnUrl = null)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (userId == targetUserId)
            {
                return RedirectToLocal(returnUrl);
            }

            var existingFollow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == userId && f.FollowingId == targetUserId);

            if (existingFollow != null)
            {
                _context.Follows.Remove(existingFollow);
            }
            else
            {
                _context.Follows.Add(new Follow
                {
                    FollowerId = userId,
                    FollowingId = targetUserId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToLocal(returnUrl);
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
