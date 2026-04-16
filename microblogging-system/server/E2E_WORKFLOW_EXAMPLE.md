# Complete End-to-End Workflow Example

This document shows a complete workflow demonstrating all user profile functionality.

## Full Example Flow

### Step 1: Register a New User

**Request:**
```bash
curl -X POST http://localhost:5050/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "displayName": "Alice Johnson",
    "email": "alice@example.com",
    "password": "SecurePass123!",
    "confirmPassword": "SecurePass123!",
    "region": "San Francisco, CA"
  }'
```

**Response (200 Created):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5MzU4MzY0Ny1iODI5LTRhMDctYTlhNi1kODQyZjI5YzMxZGIiLCJlbWFpbCI6ImFsaWNlQGV4YW1wbGUuY29tIiwiZGlzcGxheU5hbWUiOiJBbGljZSBKb2huc29uIiwidXNlcklkIjoiOTM1ODM2NDctYjgyOS00YTA3LWE5YTYtZDg0MmYyOWMzMWRiIiwianRpIjoiODc2NTQzMjEtZGVmNy00YjAwLWE5ZGEtYjZjZGM1ZTYyNzY0IiwiaWF0IjoxNzEzMTQ5MDAwLCJleHAiOjE3MTMxNTI2MDAsImlzcyI6Ik1pY3JvYmxvZ2dpbmdTeXN0ZW0iLCJhdWQiOiJNaWNyb2Jsb2dnaW5nU3lzdGVtQVBJIn0.placeholder",
  "userId": "93583647-b829-4a07-a9a6-d842f29c31db",
  "email": "alice@example.com",
  "displayName": "Alice Johnson",
  "region": "San Francisco, CA",
  "expiresAt": "2026-04-14T15:40:00Z"
}
```

**Save this token:**
```bash
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

### Step 2: Retrieve Your Own Profile (Protected)

**Request:**
```bash
curl -X GET http://localhost:5050/api/profile/me \
  -H "Authorization: Bearer $TOKEN"
```

**Response (200 OK):**
```json
{
  "id": "93583647-b829-4a07-a9a6-d842f29c31db",
  "email": "alice@example.com",
  "displayName": "Alice Johnson",
  "bio": null,
  "profilePictureUrl": null,
  "region": "San Francisco, CA",
  "followersCount": 0,
  "followingCount": 0
}
```

At this point:
- ✅ User is registered
- ✅ JWT token is valid
- ✅ Can retrieve own profile
- ✅ No followers/following yet

---

### Step 3: Update Your Profile (Protected)

**Request:**
```bash
curl -X PUT http://localhost:5050/api/profile \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "displayName": "Alice J.",
    "bio": "Software engineer | Open source enthusiast | Coffee lover",
    "profilePictureUrl": "https://example.com/avatars/alice.jpg",
    "region": "San Francisco Bay Area, USA"
  }'
```

**Response (200 OK):**
```json
{
  "id": "93583647-b829-4a07-a9a6-d842f29c31db",
  "email": "alice@example.com",
  "displayName": "Alice J.",
  "bio": "Software engineer | Open source enthusiast | Coffee lover",
  "profilePictureUrl": "https://example.com/avatars/alice.jpg",
  "region": "San Francisco Bay Area, USA",
  "followersCount": 0,
  "followingCount": 0
}
```

✅ Profile updated successfully!

---

### Step 4: View Your Public Profile (No Auth)

Anyone can view your public profile without authentication:

**Request:**
```bash
curl -X GET http://localhost:5050/api/profile/alice@example.com
```

**Response (200 OK):**
```json
{
  "id": "93583647-b829-4a07-a9a6-d842f29c31db",
  "email": "alice@example.com",
  "displayName": "Alice J.",
  "bio": "Software engineer | Open source enthusiast | Coffee lover",
  "profilePictureUrl": "https://example.com/avatars/alice.jpg",
  "region": "San Francisco Bay Area, USA",
  "followersCount": 0,
  "followingCount": 0
}
```

✅ Public profile visible!

---

### Step 5: Search for Another User's Profile

Suppose another user exists with email "bob@example.com":

**Request:**
```bash
curl -X GET http://localhost:5050/api/profile/bob@example.com
```

**Response (200 OK):**
```json
{
  "id": "12345678-90ab-cdef-1234-567890abcdef",
  "email": "bob@example.com",
  "displayName": "Bob Smith",
  "bio": "Game developer | Indie studio owner",
  "profilePictureUrl": "https://example.com/avatars/bob.jpg",
  "region": "Austin, Texas",
  "followersCount": 150,
  "followingCount": 87
}
```

---

## Code Integration Examples

### Example 1: Get Current User in a Service

```csharp
[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
    private readonly IUserService _userService;

    public MyController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("user-info")]
    [Authorize]
    public async Task<IActionResult> GetUserInfo()
    {
        // Extract userId from JWT
        var userId = User.FindFirst("userId")?.Value;
        
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("Invalid token");

        // Get full profile
        var profile = await _userService.GetUserByIdAsync(userId);
        
        if (profile == null)
            return NotFound("User not found");

        return Ok(profile);
    }
}
```

### Example 2: Check User Exists Before Action

```csharp
[HttpPost("follow/{userId}")]
[Authorize]
public async Task<IActionResult> FollowUser(string userId)
{
    var currentUserId = User.FindFirst("userId")?.Value;
    
    // Check if target user exists
    var userExists = await _userService.UserExistsAsync(userId);
    
    if (!userExists)
        return NotFound("User does not exist");
    
    // Prevent self-following
    if (currentUserId == userId)
        return BadRequest("Cannot follow yourself");
    
    // Proceed with follow logic...
    return Ok();
}
```

### Example 3: Get User Info by Email

```csharp
public async Task<IActionResult> ViewProfile(string email)
{
    var profile = await _userService.GetUserByUsernameAsync(email);
    
    if (profile == null)
        return NotFound($"No user found with email: {email}");
    
    return Ok(profile);
}
```

### Example 4: Update User Profile

```csharp
[HttpPatch("profile-picture")]
[Authorize]
public async Task<IActionResult> UpdateProfilePicture([FromBody] UpdateUserProfileDto updateDto)
{
    var userId = User.FindFirst("userId")?.Value;
    
    if (string.IsNullOrEmpty(userId))
        return Unauthorized();

    // Validate only picture URL
    var validationModel = new UpdateUserProfileDto
    {
        DisplayName = "Keep", // Unchanged
        ProfilePictureUrl = updateDto.ProfilePictureUrl
    };

    var updated = await _userService.UpdateUserProfileAsync(userId, validationModel);
    
    return Ok(updated);
}
```

---

## Error Scenarios

### Scenario 1: Invalid JWT Token

**Request:**
```bash
curl -X GET http://localhost:5050/api/profile/me \
  -H "Authorization: Bearer invalid.token.format"
```

**Response (401 Unauthorized):**
```json
{
  "error": "Invalid token claims"
}
```

---

### Scenario 2: User Not Found

**Request:**
```bash
curl -X GET http://localhost:5050/api/profile/nonexistent@example.com
```

**Response (404 Not Found):**
```json
{
  "error": "User 'nonexistent@example.com' not found"
}
```

---

### Scenario 3: Invalid Update Data

**Request:**
```bash
curl -X PUT http://localhost:5050/api/profile \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "displayName": "a",  # Too short (min 2 chars)
    "profilePictureUrl": "not-a-valid-url"  # Invalid URL
  }'
```

**Response (400 Bad Request):**
```json
{
  "error": "Invalid input data",
  "details": [
    "Display name must be between 2 and 50 characters",
    "Profile picture must be a valid URL"
  ]
}
```

---

### Scenario 4: Missing Authorization Header

**Request:**
```bash
curl -X GET http://localhost:5050/api/profile/me
```

**Response (401 Unauthorized):**
```
(No response body - JWT middleware rejects)
```

---

## Testing with Postman

### Setup Collection

1. **Create Environment Variables:**
   - `base_url`: `http://localhost:5050`
   - `token`: (empty initially)

2. **Register Request**
   - Method: `POST`
   - URL: `{{base_url}}/api/auth/register`
   - Body (JSON):
     ```json
     {
       "displayName": "Test User",
       "email": "test@example.com",
       "password": "Test123!",
       "confirmPassword": "Test123!",
       "region": "Test Region"
     }
     ```
   - Tests (Script):
     ```javascript
     pm.environment.set("token", pm.response.json().token);
     ```

3. **Get Profile Request**
   - Method: `GET`
   - URL: `{{base_url}}/api/profile/me`
   - Headers:
     ```
     Authorization: Bearer {{token}}
     ```

4. **Update Profile Request**
   - Method: `PUT`
   - URL: `{{base_url}}/api/profile`
   - Headers:
     ```
     Authorization: Bearer {{token}}
     Content-Type: application/json
     ```
   - Body (JSON):
     ```json
     {
       "displayName": "Updated Name",
       "bio": "My bio",
       "profilePictureUrl": "https://example.com/pic.jpg",
       "region": "New Region"
     }
     ```

---

## Summary

This complete workflow demonstrates:

✅ **Registration** - Create account with profile info
✅ **Authentication** - JWT token generation and usage
✅ **Profile Retrieval** - Get own profile (protected) and others' profiles (public)
✅ **Profile Updates** - Modify your profile information
✅ **Error Handling** - Proper HTTP status codes and error messages
✅ **Security** - JWT authorization on protected endpoints
✅ **Validation** - Input validation with detailed error messages

The system is production-ready with:
- Proper separation of concerns (Controllers → Services → Database)
- Comprehensive error handling
- Security best practices
- Clean code architecture
- Detailed logging for debugging

