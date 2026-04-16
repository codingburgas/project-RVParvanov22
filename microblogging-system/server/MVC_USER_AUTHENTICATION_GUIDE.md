# ASP.NET Core MVC UI Guide: User Authentication & Profile Display

This guide explains how to extract user information in ASP.NET Core MVC and display it in Razor views.

---

## Overview

Your application has:
- **AccountController** - MVC controller with Login, Register, Logout, and Profile actions
- **_Layout.cshtml** - Master layout with dark gaming sidebar UI
- **Profile.cshtml** - User profile page with stats and information

---

## How to Extract User Information

### Method 1: From User.Identity (Recommended for HttpContext)

Available in all Controllers and Views:

```csharp
// In Controller
public class MyController : Controller
{
    public IActionResult MyAction()
    {
        // Check if user is authenticated
        if (User.Identity?.IsAuthenticated == true)
        {
            // Get username (usually email in this project)
            var username = User.Identity.Name;  // "user@example.com"
            
            // Get user ID
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            // Get DisplayName (from custom claim added by JwtService)
            var displayName = User.FindFirst("displayName")?.Value;
            
            // Get email
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        }
        
        return View();
    }
}
```

### Method 2: Using UserManager (Recommended for Controllers)

```csharp
using Microsoft.AspNetCore.Identity;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public AccountController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        // Get current user ID
        var userId = _userManager.GetUserId(User);
        
        // Find user by ID
        var user = await _userManager.FindByIdAsync(userId);
        
        if (user == null)
            return NotFound();

        // Access user properties
        var displayName = user.DisplayName;
        var email = user.Email;
        var profilePictureUrl = user.ProfilePictureUrl;
        var bio = user.Bio;
        var region = user.Region;
        
        return View(user);
    }
}
```

### Method 3: Using Dependency Injection in Views

```html
@using Microsoft.AspNetCore.Identity
@inject UserManager<ApplicationUser> userManager

@{
    var user = await userManager.GetUserAsync(User);
}

<p>Hello, @user?.DisplayName</p>
```

### Method 4: From JWT Claims (API/Backend)

```csharp
private string? GetUserIdFromClaims()
{
    // Custom claim added by JwtService
    var userId = User.FindFirst("userId")?.Value;
    
    // Fallback to standard NameIdentifier (sub)
    if (string.IsNullOrEmpty(userId))
        userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    // Alternative: access "sub" claim
    if (string.IsNullOrEmpty(userId))
        userId = User.FindFirst("sub")?.Value;

    return userId;
}
```

---

## Accessing User Info in Different Scenarios

### Scenario 1: Display Current User in Layout (_Layout.cshtml)

```html
@{
    // Extract displayName and email from claims
    var displayName = User.FindFirst("displayName")?.Value ?? User.Identity?.Name ?? "Guest";
    var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty;
}

@if (User.Identity?.IsAuthenticated == true)
{
    <div class="user-profile">
        <p>@displayName</p>
        <p>@userEmail</p>
    </div>
}
```

### Scenario 2: Get User with Related Data (Posts, Followers)

```csharp
[Authorize]
public async Task<IActionResult> Profile()
{
    var userId = _userManager.GetUserId(User);
    
    // Include related data
    var user = await _context.Users
        .Include(u => u.Posts)
        .Include(u => u.Followers)
        .Include(u => u.Following)
        .FirstOrDefaultAsync(u => u.Id == userId);

    if (user == null)
        return NotFound();

    var viewModel = new UserProfileViewModel
    {
        Id = user.Id,
        Email = user.Email,
        DisplayName = user.DisplayName,
        PostsCount = user.Posts?.Count ?? 0,
        FollowersCount = user.Followers?.Count ?? 0,
        FollowingCount = user.Following?.Count ?? 0
    };

    return View(viewModel);
}
```

### Scenario 3: Create Custom Middleware to Inject User Info

```csharp
public class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = userManager.GetUserId(context.User);
            var user = await userManager.FindByIdAsync(userId);
            
            // Store in HttpContext.Items for access throughout request
            context.Items["CurrentUser"] = user;
        }

        await _next(context);
    }
}

// In Program.cs
app.UseMiddleware<UserContextMiddleware>();
```

Then access in controllers:
```csharp
var user = HttpContext.Items["CurrentUser"] as ApplicationUser;
```

---

## Passing User Data to Views

### Option 1: Model

```csharp
// Controller
[Authorize]
public async Task<IActionResult> Profile()
{
    var userId = _userManager.GetUserId(User);
    var user = await _userManager.FindByIdAsync(userId);
    
    return View(user);  // Pass user as model
}

// View
@model ApplicationUser

<h1>@Model.DisplayName</h1>
<p>@Model.Email</p>
```

### Option 2: ViewModel (Recommended)

```csharp
// ViewModel
public class ProfileViewModel
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public string ProfilePictureUrl { get; set; }
}

// Controller
[Authorize]
public async Task<IActionResult> Profile()
{
    var userId = _userManager.GetUserId(User);
    var user = await _userManager.FindByIdAsync(userId);
    
    var viewModel = new ProfileViewModel
    {
        Id = user.Id,
        DisplayName = user.DisplayName,
        Email = user.Email,
        ProfilePictureUrl = user.ProfilePictureUrl
    };
    
    return View(viewModel);
}

// View
@model ProfileViewModel

<h1>@Model.DisplayName</h1>
```

### Option 3: ViewBag

```csharp
// Controller
public IActionResult MyPage()
{
    ViewBag.CurrentUserName = User.FindFirst("displayName")?.Value;
    ViewBag.CurrentUserId = _userManager.GetUserId(User);
    
    return View();
}

// View
<h1>Hello, @ViewBag.CurrentUserName</h1>
```

### Option 4: ViewData

```csharp
// Controller
public IActionResult MyPage()
{
    ViewData["UserEmail"] = User.FindFirst(ClaimTypes.Email)?.Value;
    
    return View();
}

// View
<p>@ViewData["UserEmail"]</p>
```

### Option 5: Dependency Injection in View

```csharp
// View
@using Microsoft.AspNetCore.Identity

@inject UserManager<ApplicationUser> userManager

@{
    var currentUser = await userManager.GetUserAsync(User);
}

<h1>@currentUser?.DisplayName</h1>
<p>@currentUser?.Email</p>
```

---

## Example Implementation: Account Controller Profile Action

```csharp
[Authorize]
public async Task<IActionResult> Profile()
{
    // Method 1: Get user ID from UserManager
    var userId = _userManager.GetUserId(User);
    if (string.IsNullOrEmpty(userId))
    {
        return Unauthorized();
    }

    // Method 2: Retrieve full user with relations
    var user = await _context.Users
        .Include(u => u.Posts)
        .Include(u => u.Followers)
        .Include(u => u.Following)
        .FirstOrDefaultAsync(u => u.Id == userId);

    if (user == null)
    {
        return NotFound("User profile not found");
    }

    // Method 3: Map to ViewModel (clean separation)
    var viewModel = new UserProfileViewModel
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
        PostsCount = user.Posts?.Count ?? 0
    };

    return View(viewModel);
}
```

---

## JWT Claims Available in User.Identity

When a user authenticates, the JWT token contains these claims (added by JwtService):

```csharp
// In a controller with [Authorize]
var claims = new Dictionary<string, string>
{
    ["sub"] = User.FindFirst("sub")?.Value,                           // User ID
    ["email"] = User.FindFirst(ClaimTypes.Email)?.Value,              // Email
    ["displayName"] = User.FindFirst("displayName")?.Value,           // DisplayName
    ["userId"] = User.FindFirst("userId")?.Value,                     // Custom userId claim
    ["jti"] = User.FindFirst("jti")?.Value,                           // JWT ID
    ["http://schemas.microsoft.com/ws/2008/06/identity/claims/nameidentifier"] 
        = User.FindFirst(ClaimTypes.NameIdentifier)?.Value            // NameIdentifier
};
```

---

## Common Claim Types

```csharp
using System.Security.Claims;

// Standard claims you can use
var standardClaims = new Dictionary<string, string>
{
    // Identity
    [ClaimTypes.NameIdentifier] = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,  // User ID
    [ClaimTypes.Name] = User.FindFirst(ClaimTypes.Name)?.Value,                      // Username
    [ClaimTypes.Email] = User.FindFirst(ClaimTypes.Email)?.Value,                    // Email
    
    // Profile info
    [ClaimTypes.GivenName] = User.FindFirst(ClaimTypes.GivenName)?.Value,            // First name
    [ClaimTypes.Surname] = User.FindFirst(ClaimTypes.Surname)?.Value,                // Last name
    
    // Roles
    [ClaimTypes.Role] = User.FindFirst(ClaimTypes.Role)?.Value,                      // Role
    
    // Custom claims
    ["displayName"] = User.FindFirst("displayName")?.Value,                          // Custom
    ["userId"] = User.FindFirst("userId")?.Value                                      // Custom
};
```

---

## Sidebar Integration

The sidebar in `_Layout.cshtml` extracts user info like this:

```html
@{
    // Extract from claims (works with both Identity and JWT)
    var displayName = User.FindFirst("displayName")?.Value 
                      ?? User.Identity?.Name 
                      ?? "Guest";
    
    var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
                    ?? string.Empty;
}

@if (User.Identity?.IsAuthenticated == true)
{
    <div class="sidebar-user-section">
        <a asp-controller="Account" asp-action="Profile">
            <div class="user-name">@displayName</div>
            <div class="user-email">@userEmail</div>
        </a>
        <form method="post" asp-controller="Account" asp-action="Logout">
            <button type="submit">Logout</button>
        </form>
    </div>
}
```

---

## Best Practices

### ✅ DO
- Use `UserManager<T>.GetUserId(User)` in controllers
- Use `User.FindFirst("claim-name")?.Value` to get specific claims
- Create ViewModels to pass data to views
- Use `[Authorize]` attribute to protect actions
- Store user data in consistent manner across app

### ❌ DON'T
- Don't request user info multiple times in same request
- Don't expose sensitive data in views unnecessary
- Don't trust User.Identity.Name directly (use claims instead)
- Don't hardcode user lookups without includes
- Don't lack null checks on user lookups

---

## Security Considerations

1. **Always use [Authorize]** before accessing User.Identity
2. **Validate JWT signature** (done automatically by middleware)
3. **Use HTTPS only** in production
4. **Never log sensitive data** (passwords, tokens)
5. **Verify user ownership** before allowing edits:

```csharp
[Authorize]
public async Task<IActionResult> EditProfile(string userId)
{
    var currentUserId = _userManager.GetUserId(User);
    
    // Verify ownership
    if (currentUserId != userId)
    {
        return Forbid();  // 403 Forbidden
    }
    
    // Continue with edit...
}
```

---

## Testing Claims Extraction

```csharp
public class DebugController : Controller
{
    [Authorize]
    public IActionResult DebugClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return Ok(claims);
    }
}
```

Access at: `/Debug/DebugClaims`

Output:
```json
[
    { "type": "sub", "value": "user-id-here" },
    { "type": "email", "value": "user@example.com" },
    { "type": "displayName", "value": "John Doe" },
    { "type": "userId", "value": "user-id-here" },
    ...
]
```

---

## Related Files

- **AccountController.cs** - MVC controller with Profile action
- **Views/Account/Profile.cshtml** - Profile page view
- **Views/Shared/_Layout.cshtml** - Master layout with sidebar
- **Models/ViewModels/UserProfileViewModel.cs** - Profile view model
- **Services/JwtService.cs** - JWT token generation

