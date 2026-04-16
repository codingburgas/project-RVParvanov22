# User Profile API Implementation

## Overview

This document describes the user profile support added to the microblogging system. It includes endpoints for retrieving and updating user profiles with JWT-based authorization.

## Architecture

### Components

1. **Models**
   - `ApplicationUser`: Identity User with profile fields (DisplayName, Bio, ProfilePictureUrl, Region)
   - `UserProfileDto`: DTO for reading user profile data
   - `UpdateUserProfileDto`: DTO for updating user profile

2. **Services**
   - `IUserService` / `UserService`: Business logic for user profile operations
   - `IJwtService` / `JwtService`: JWT token generation with userId claim

3. **Controllers**
   - `ProfileController`: API endpoints for profile operations
   - `AuthController`: Authentication endpoints (login/register)

4. **Database**
   - `ApplicationDbContext`: EF Core context with User, Post, Comment, Follow entities

---

## API Endpoints

### 1. Get Current User Profile
**Endpoint**: `GET /api/profile/me`
**Authorization**: Required (JWT)
**Response**: `UserProfileDto`

```http
GET /api/profile/me HTTP/1.1
Host: localhost:7050
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response (200 OK)**:
```json
{
  "id": "user-123",
  "email": "user@example.com",
  "displayName": "John Doe",
  "bio": "Software developer and gaming enthusiast",
  "profilePictureUrl": "https://example.com/profile.jpg",
  "region": "Sofia, Bulgaria",
  "followersCount": 45,
  "followingCount": 32
}
```

**Error Responses**:
- `401 Unauthorized`: Invalid or missing JWT token
- `404 Not Found`: User profile not found
- `500 Internal Server Error`: Server error

---

### 2. Get Public User Profile
**Endpoint**: `GET /api/profile/{username}`
**Authorization**: Not required (Public endpoint)
**Response**: `UserProfileDto`

```http
GET /api/profile/user@example.com HTTP/1.1
Host: localhost:7050
```

**Response (200 OK)**:
```json
{
  "id": "user-123",
  "email": "user@example.com",
  "displayName": "John Doe",
  "bio": "Software developer and gaming enthusiast",
  "profilePictureUrl": "https://example.com/profile.jpg",
  "region": "Sofia, Bulgaria",
  "followersCount": 45,
  "followingCount": 32
}
```

**Parameters**:
- `username` (string, required): Email or username to search

**Error Responses**:
- `400 Bad Request`: Empty username
- `404 Not Found`: User not found
- `500 Internal Server Error`: Server error

---

### 3. Update Current User Profile
**Endpoint**: `PUT /api/profile`
**Authorization**: Required (JWT)
**Request Body**: `UpdateUserProfileDto`
**Response**: `UserProfileDto`

```http
PUT /api/profile HTTP/1.1
Host: localhost:7050
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "displayName": "John Doe Updated",
  "bio": "New bio content",
  "profilePictureUrl": "https://example.com/new-profile.jpg",
  "region": "New York, USA"
}
```

**Response (200 OK)**:
```json
{
  "id": "user-123",
  "email": "user@example.com",
  "displayName": "John Doe Updated",
  "bio": "New bio content",
  "profilePictureUrl": "https://example.com/new-profile.jpg",
  "region": "New York, USA",
  "followersCount": 45,
  "followingCount": 32
}
```

**Validation Rules**:
- `displayName` (2-50 characters): Required
- `bio` (max 250 characters): Optional
- `profilePictureUrl`: Must be valid URL format
- `region`: Optional

**Error Responses**:
- `400 Bad Request`: Invalid input data
- `401 Unauthorized`: Invalid or missing JWT token
- `404 Not Found`: User not found
- `500 Internal Server Error`: Server error

---

## JWT Token Structure

When a user logs in or registers, the JWT token includes the following claims:

```json
{
  "sub": "user-id-string",
  "email": "user@example.com",
  "displayName": "John Doe",
  "userId": "user-id-string",
  "jti": "unique-token-id",
  "iat": 1234567890,
  "exp": 1234568890,
  "iss": "MicrobloggingSystem",
  "aud": "MicrobloggingSystemAPI"
}
```

### Key Claims
- **sub**: Subject (user ID)
- **email**: User's email address
- **displayName**: User's display name
- **userId**: Custom claim for user ID (convenience for extraction)
- **iat**: Issued at timestamp
- **exp**: Expiration timestamp (typically 60 minutes)
- **iss**: Issuer
- **aud**: Audience

---

## Extracting userId from JWT

### Method 1: Using ProfileController Helper
The `ProfileController` includes a helper method `GetUserIdFromClaims()` that extracts userId with fallbacks:

```csharp
private string? GetUserIdFromClaims()
{
    // Try custom "userId" claim first
    var userId = User.FindFirst("userId")?.Value;
    
    // Fallback to NameIdentifier (sub)
    if (string.IsNullOrEmpty(userId))
    {
        userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    // Alternative fallback
    if (string.IsNullOrEmpty(userId))
    {
        userId = User.FindFirst("sub")?.Value;
    }

    return userId;
}
```

### Method 2: Direct Claim Access in Controller
```csharp
[Authorize]
[HttpGet("example")]
public IActionResult Example()
{
    // Get userId from JWT claims
    var userId = User.FindFirst("userId")?.Value;
    
    if (string.IsNullOrEmpty(userId))
    {
        return Unauthorized();
    }
    
    // Use userId...
    return Ok(userId);
}
```

### Method 3: In Services
If you need userId in a service, pass it as a parameter from the controller:

```csharp
[HttpGet("me")]
[Authorize]
public async Task<IActionResult> GetCurrentUser()
{
    var userId = User.FindFirst("userId")?.Value;
    var profile = await _userService.GetUserByIdAsync(userId);
    return Ok(profile);
}
```

---

## UserService Implementation

### Interface
```csharp
public interface IUserService
{
    Task<UserProfileDto?> GetUserByIdAsync(string userId);
    Task<UserProfileDto?> GetUserByUsernameAsync(string username);
    Task<UserProfileDto?> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updateDto);
    Task<bool> UserExistsAsync(string userId);
}
```

### Key Features
- **GetUserByIdAsync**: Retrieves user by ID, includes follower/following counts
- **GetUserByUsernameAsync**: Searches by email or username
- **UpdateUserProfileAsync**: Updates profile fields with validation
- **UserExistAsync**: Checks user existence (useful for authorization)

### Dependencies
- `ApplicationDbContext`: Database access
- `ILogger<UserService>`: Logging for debugging and monitoring

---

## Integration Steps

### 1. Register Service in Program.cs
```csharp
builder.Services.AddScoped<IUserService, UserService>();
```

### 2. Use in Controller
```csharp
private readonly IUserService _userService;

public ProfileController(IUserService userService)
{
    _userService = userService;
}
```

### 3. Call Service Methods
```csharp
var profile = await _userService.GetUserByIdAsync(userId);
```

---

## Database Schema

### Users Table (AspNetUsers)
- **Id** (PK): User identifier
- **Email**: User email (also used as username)
- **UserName**: Identity username
- **DisplayName**: User's display name
- **Bio**: User biography
- **ProfilePictureUrl**: URL to profile picture
- **Region**: User's geographic region
- **CreatedAt**: Account creation timestamp
- Other Identity fields (PasswordHash, Email confirmed, etc.)

### Related Tables
- **Posts**: User's posts (ForeignKey: UserId)
- **Comments**: User's comments (ForeignKey: UserId)
- **PostLikes**: User's likes (ForeignKey: UserId)
- **Follows**: User followers/following relationships

---

## Configuration

### appsettings.json
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

---

## Testing Examples

### Using cURL

**1. Register User**
```bash
curl -X POST http://localhost:5050/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "displayName": "John Doe",
    "email": "john@example.com",
    "password": "Password123",
    "confirmPassword": "Password123",
    "region": "Sofia, Bulgaria"
  }'
```

**2. Login and Get Token**
```bash
curl -X POST http://localhost:5050/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "Password123"
  }'
```

**3. Get Current User Profile**
```bash
curl -X GET http://localhost:5050/api/profile/me \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**4. Get Public Profile**
```bash
curl -X GET http://localhost:5050/api/profile/john@example.com
```

**5. Update Profile**
```bash
curl -X PUT http://localhost:5050/api/profile \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "displayName": "John Doe Updated",
    "bio": "Software developer",
    "profilePictureUrl": "https://example.com/photo.jpg",
    "region": "New York, USA"
  }'
```

---

## Error Handling

### Common Errors

| Status | Error | Cause |
|--------|-------|-------|
| 400 | Invalid input data | Validation failure (e.g., invalid URL) |
| 401 | Invalid token claims | Missing/malformed JWT |
| 404 | User profile not found | User doesn't exist |
| 500 | Internal Server Error | Unexpected server exception |

### Response Format
```json
{
  "error": "User not found",
  "details": ["Additional information"]
}
```

---

## Security Considerations

1. **JWT Validation**: All protected endpoints validate JWT signature, issuer, and audience
2. **CORS**: Configured to allow localhost:5173-5185 and localhost:3000
3. **Authorization**: `/api/profile/me` and `PUT /api/profile` require `[Authorize]`
4. **Public Data**: `/api/profile/{username}` has no authorization (intentionally public)
5. **Password Security**: EF Core Identity handles password hashing
6. **Claim Validation**: Multiple fallback claims ensure token extraction robustness

---

## Performance Notes

- UserService includes eager loading of `Following` and `Followers` relationships
- Database queries are logged via EF Core logging (visible in console)
- Consider indexing Email and UserName for faster lookups

---

## Future Enhancements

- [ ] Profile picture upload/storage
- [ ] Pagination for followers/following
- [ ] User search functionality
- [ ] Profile badges/achievements
- [ ] Email verification
- [ ] Two-factor authentication
- [ ] Social media links in profile

