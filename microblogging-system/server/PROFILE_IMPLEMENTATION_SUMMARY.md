# User Profile Support Implementation Summary

## ✅ Completed Implementation

Your microblogging system now has full user profile support with JWT authentication. Here's what was added:

---

## 📁 Files Created/Modified

### 1. **New Service: `Services/UserService.cs`**
   - Implements `IUserService` interface
   - Methods:
     - `GetUserByIdAsync(userId)` - retrieves profile with follower counts
     - `GetUserByUsernameAsync(username)` - searches by email or username
     - `UpdateUserProfileAsync(userId, updateDto)` - updates profile fields
     - `UserExistsAsync(userId)` - checks user existence
   - Includes detailed logging for debugging

### 2. **New Controller: `Controllers/ProfileController.cs`**
   - **GET /api/profile/me** - Returns current user's profile (requires JWT)
   - **GET /api/profile/{username}** - Returns public profile (no auth required)
   - **PUT /api/profile** - Updates current user's profile (requires JWT)
   - Includes helper method `GetUserIdFromClaims()` for JWT extraction with fallbacks
   - Full error handling and validation

### 3. **Updated: `Program.cs`**
   - Registered `IUserService` in dependency injection container
   - Added: `builder.Services.AddScoped<IUserService, UserService>();`

### 4. **Documentation: `PROFILE_API_DOCUMENTATION.md`**
   - Complete API reference
   - JWT token structure explanation
   - Code examples and cURL tests
   - Security considerations

---

## 🏗️ Architecture

```
Request Flow for GET /api/profile/me
├── Client sends JWT in Authorization header
├── ProfileController.GetCurrentUserProfile()
│   ├── Extracts userId from JWT claims via GetUserIdFromClaims()
│   ├── Calls userService.GetUserByIdAsync(userId)
│   │   ├── Queries database with EF Core
│   │   ├── Includes follower/following relationships
│   │   └── Maps to UserProfileDto
│   └── Returns UserProfileDto response
└── Response with profile data (follower counts, bio, picture, etc.)
```

---

## 🔐 JWT Token Structure

When user logs in/registers, token includes:
```json
{
  "sub": "user-id",              // User ID from Identity
  "email": "user@example.com",   // User email
  "displayName": "John Doe",     // Display name
  "userId": "user-id",           // Custom claim for easy extraction
  "jti": "unique-id",            // JWT ID
  "exp": 1234567890,             // Expiration time
  "iss": "MicrobloggingSystem",  // Issuer
  "aud": "MicrobloggingSystemAPI"// Audience
}
```

---

## 📦 Data Model (ApplicationUser Fields)

```
ApplicationUser (extends IdentityUser)
├── DisplayName: string?          // User's display name
├── Bio: string?                  // User biography (max 250 chars)
├── ProfilePictureUrl: string?    // URL to profile image
├── Region: string?               // Geographic region
├── CreatedAt: DateTime           // Account creation date
├── Posts: ICollection<Post>      // User's posts
├── Comments: ICollection<Comment>// User's comments
├── PostLikes: ICollection<PostLike>// User's likes
├── Following: ICollection<Follow> // Users this user follows
└── Followers: ICollection<Follow> // Users following this user
```

---

## 🔌 Service Example

```csharp
// In ProfileController
[HttpGet("me")]
[Authorize]
public async Task<ActionResult<UserProfileDto>> GetCurrentUserProfile()
{
    // Extract userId from JWT
    var userId = GetUserIdFromClaims();
    
    // Call service
    var profile = await _userService.GetUserByIdAsync(userId);
    
    // Returns UserProfileDto with follower counts
    return Ok(profile);
}

// GetUserIdFromClaims() extracts userId from JWT with fallbacks:
// 1. Custom "userId" claim
// 2. ClaimTypes.NameIdentifier
// 3. "sub" (subject) claim
```

---

## 🧪 Testing the Endpoints

### 1. Register User
```bash
curl -X POST http://localhost:5050/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "displayName": "John Doe",
    "email": "john@example.com",
    "password": "Test123!",
    "confirmPassword": "Test123!",
    "region": "Sofia, Bulgaria"
  }'
```
**Response includes JWT token in `token` field**

### 2. Get Current User Profile (Protected)
```bash
curl -X GET http://localhost:5050/api/profile/me \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```
**Response:**
```json
{
  "id": "user-123",
  "email": "john@example.com",
  "displayName": "John Doe",
  "bio": null,
  "profilePictureUrl": null,
  "region": "Sofia, Bulgaria",
  "followersCount": 0,
  "followingCount": 0
}
```

### 3. Get Public Profile (No Auth)
```bash
curl -X GET http://localhost:5050/api/profile/john@example.com
```

### 4. Update Profile (Protected)
```bash
curl -X PUT http://localhost:5050/api/profile \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "displayName": "John Updated",
    "bio": "I love coding",
    "profilePictureUrl": "https://example.com/photo.jpg",
    "region": "New York, USA"
  }'
```

---

## ⚙️ Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=PlayerPulse.db"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-characters",
    "Issuer": "MicrobloggingSystem",
    "Audience": "MicrobloggingSystemAPI",
    "ExpiryInMinutes": 60
  }
}
```

The JWT secret key is configured in appsettings.json and read by JwtService.

---

## 🔒 Security Features

| Feature | Implementation |
|---------|----------------|
| **JWT Validation** | Signature, issuer, audience, expiry verified |
| **Authorization** | `[Authorize]` attribute on protected endpoints |
| **Public Data** | `/api/profile/{username}` intentionally public |
| **Password Hashing** | ASP.NET Core Identity handles hashing |
| **CORS** | Configured for localhost:5173-5185 and :3000 |
| **Input Validation** | Model validation on DTOs (URL validation, lengths) |

---

## 🚀 Application Status

✅ **Build**: Successful (with benign NuGet warnings)
✅ **Runtime**: Running on http://localhost:5050
✅ **Database**: SQLite (PlayerPulse.db) with roles and admin user seeded
✅ **Endpoints**: All profile endpoints responding correctly
✅ **Authentication**: JWT working with custom claims

---

## 📋 What's Already in Place

Your project already had:
- ✅ ApplicationUser model with profile fields
- ✅ Identity with roles (Admin/User)
- ✅ JWT token generation (JwtService)
- ✅ AuthController with login/register
- ✅ DTOs (UserProfileDto, UpdateUserProfileDto)
- ✅ Database context with relationships (Posts, Comments, Follows)
- ✅ CORS configuration
- ✅ Entity Framework Core with SQLite

**Added:**
- ✅ UserService for profile operations
- ✅ ProfileController with 3 endpoints
- ✅ JWT claim extraction helper
- ✅ Dependency injection setup

---

## 🔄 Request/Response Examples

### Request: GET /api/profile/me
```
GET /api/profile/me HTTP/1.1
Host: localhost:5050
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Response: 200 OK
```json
{
  "id": "5be86359-073c-4169-8040-7ab64a3b3fa2",
  "email": "user@example.com",
  "displayName": "John Doe",
  "bio": "Software developer",
  "profilePictureUrl": "https://example.com/photo.jpg",
  "region": "Sofia, Bulgaria",
  "followersCount": 42,
  "followingCount": 28
}
```

### Error Response: 401 Unauthorized
```json
{
  "error": "Invalid token claims"
}
```

---

## 🧠 How JWT Extraction Works

The `GetUserIdFromClaims()` method tries multiple approaches to extract userId:

```csharp
private string? GetUserIdFromClaims()
{
    // Attempt 1: Custom "userId" claim (added by JwtService)
    var userId = User.FindFirst("userId")?.Value;
    
    // Attempt 2: Standard NameIdentifier claim
    if (string.IsNullOrEmpty(userId))
        userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    // Attempt 3: JWT "sub" (subject) claim
    if (string.IsNullOrEmpty(userId))
        userId = User.FindFirst("sub")?.Value;

    return userId;
}
```

This ensures robustness across different token formats.

---

## 📊 Database Relationships

```
ApplicationUser
├── 1:N → Post (Posts)
├── 1:N → Comment (Comments)
├── 1:N → PostLike (PostLikes)
├── 1:N → Follow (as follower in Following)
└── 1:N → Follow (as following in Followers)

Follow
├── FK → ApplicationUser (Follower)
└── FK → ApplicationUser (Following)
```

The system tracks:
- Who each user follows
- Who follows each user
- User interaction counts

---

## 🎯 Next Steps (Optional)

1. **Upload Profile Pictures**
   - Add file upload to AWS S3/Blob Storage
   - Return signed URLs

2. **User Search**
   - Add search endpoint to find users
   - Filter by displayName or region

3. **Follow System**
   - Add endpoints to follow/unfollow users
   - Track followers in profile

4. **Pagination**
   - Add pagination to followers/following counts

5. **Email Verification**
   - Verify email before account activation

6. **Two-Factor Authentication**
   - Add 2FA for security

---

## 📞 Support

All code follows:
- ✅ Clean Architecture (Services, Controllers, Models separated)
- ✅ SOLID Principles (Dependency injection, interfaces)
- ✅ Error handling (try-catch, detailed logging)
- ✅ Input validation (ModelState checks)
- ✅ Security best practices (JWT, authorization, CORS)

The implementation is production-ready with comprehensive logging for debugging.
