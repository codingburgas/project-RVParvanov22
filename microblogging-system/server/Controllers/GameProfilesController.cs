using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicrobloggingSystem.Data;
using MicrobloggingSystem.Models;
using MicrobloggingSystem.Models.DTOs;

namespace MicrobloggingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameProfilesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GameProfilesController> _logger;

        public GameProfilesController(ApplicationDbContext context, ILogger<GameProfilesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all game profiles
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GameProfileResponseDto>>> GetGameProfiles([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var gameProfiles = await _context.GameProfiles
                    .Include(gp => gp.User)
                    .Include(gp => gp.MatchEntries)
                    .OrderByDescending(gp => gp.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var gameProfilesResponse = gameProfiles.Select(gp => new GameProfileResponseDto
                {
                    Id = gp.Id,
                    GameName = gp.GameName,
                    InGameName = gp.InGameName,
                    Rank = gp.Rank,
                    MainRole = gp.MainRole,
                    PeakRank = gp.PeakRank,
                    FavoriteCharacter = gp.FavoriteCharacter,
                    CreatedAt = gp.CreatedAt,
                    UserId = gp.UserId,
                    UserDisplayName = gp.User?.DisplayName ?? gp.User?.UserName,
                    MatchEntriesCount = gp.MatchEntries?.Count ?? 0
                }).ToList();

                return Ok(new
                {
                    pageNumber,
                    pageSize,
                    totalProfiles = await _context.GameProfiles.CountAsync(),
                    data = gameProfilesResponse
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting game profiles");
                return StatusCode(500, new { error = "An error occurred while fetching game profiles" });
            }
        }

        /// <summary>
        /// Get a specific game profile by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<GameProfileResponseDto>> GetGameProfile(int id)
        {
            try
            {
                var gameProfile = await _context.GameProfiles
                    .Include(gp => gp.User)
                    .Include(gp => gp.MatchEntries)
                    .FirstOrDefaultAsync(gp => gp.Id == id);

                if (gameProfile == null)
                {
                    return NotFound(new { error = "Game profile not found" });
                }

                var gameProfileResponse = new GameProfileResponseDto
                {
                    Id = gameProfile.Id,
                    GameName = gameProfile.GameName,
                    InGameName = gameProfile.InGameName,
                    Rank = gameProfile.Rank,
                    MainRole = gameProfile.MainRole,
                    PeakRank = gameProfile.PeakRank,
                    FavoriteCharacter = gameProfile.FavoriteCharacter,
                    CreatedAt = gameProfile.CreatedAt,
                    UserId = gameProfile.UserId,
                    UserDisplayName = gameProfile.User?.DisplayName ?? gameProfile.User?.UserName,
                    MatchEntriesCount = gameProfile.MatchEntries?.Count ?? 0
                };

                return Ok(gameProfileResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting game profile");
                return StatusCode(500, new { error = "An error occurred while fetching the game profile" });
            }
        }

        /// <summary>
        /// Get game profiles by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<GameProfileResponseDto>>> GetUserGameProfiles(string userId)
        {
            try
            {
                var gameProfiles = await _context.GameProfiles
                    .Include(gp => gp.User)
                    .Include(gp => gp.MatchEntries)
                    .Where(gp => gp.UserId == userId)
                    .ToListAsync();

                var gameProfilesResponse = gameProfiles.Select(gp => new GameProfileResponseDto
                {
                    Id = gp.Id,
                    GameName = gp.GameName,
                    InGameName = gp.InGameName,
                    Rank = gp.Rank,
                    MainRole = gp.MainRole,
                    PeakRank = gp.PeakRank,
                    FavoriteCharacter = gp.FavoriteCharacter,
                    CreatedAt = gp.CreatedAt,
                    UserId = gp.UserId,
                    UserDisplayName = gp.User?.DisplayName ?? gp.User?.UserName,
                    MatchEntriesCount = gp.MatchEntries?.Count ?? 0
                }).ToList();

                return Ok(gameProfilesResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user game profiles");
                return StatusCode(500, new { error = "An error occurred while fetching user game profiles" });
            }
        }

        /// <summary>
        /// Create a new game profile
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<GameProfileResponseDto>> CreateGameProfile([FromBody] CreateGameProfileDto createGameProfileDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createGameProfileDto.GameName))
                {
                    return BadRequest(new { error = "GameName is required" });
                }

                if (string.IsNullOrWhiteSpace(createGameProfileDto.InGameName))
                {
                    return BadRequest(new { error = "InGameName is required" });
                }

                if (string.IsNullOrWhiteSpace(createGameProfileDto.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var user = await _context.Users.FindAsync(createGameProfileDto.UserId);
                if (user == null)
                {
                    return BadRequest(new { error = "User not found" });
                }

                var gameProfile = new GameProfile
                {
                    GameName = createGameProfileDto.GameName,
                    InGameName = createGameProfileDto.InGameName,
                    Rank = createGameProfileDto.Rank,
                    MainRole = createGameProfileDto.MainRole,
                    PeakRank = createGameProfileDto.PeakRank,
                    FavoriteCharacter = createGameProfileDto.FavoriteCharacter,
                    UserId = createGameProfileDto.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.GameProfiles.Add(gameProfile);
                await _context.SaveChangesAsync();

                var gameProfileResponse = new GameProfileResponseDto
                {
                    Id = gameProfile.Id,
                    GameName = gameProfile.GameName,
                    InGameName = gameProfile.InGameName,
                    Rank = gameProfile.Rank,
                    MainRole = gameProfile.MainRole,
                    PeakRank = gameProfile.PeakRank,
                    FavoriteCharacter = gameProfile.FavoriteCharacter,
                    CreatedAt = gameProfile.CreatedAt,
                    UserId = gameProfile.UserId,
                    UserDisplayName = user.DisplayName ?? user.UserName,
                    MatchEntriesCount = 0
                };

                return CreatedAtAction(nameof(GetGameProfile), new { id = gameProfile.Id }, gameProfileResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating game profile");
                return StatusCode(500, new { error = "An error occurred while creating the game profile" });
            }
        }
    }
}
