# Quick Reference: User Profile API

## 🚀 Running the Server

```bash
cd /Users/rumenp/project-RVParvanov22/microblogging-system/server
dotnet run
```

Server runs on: **http://localhost:5050** (HTTP) or **https://localhost:7050** (HTTPS)

---

## 📚 API Endpoints

| Method | Endpoint | Auth | Purpose |
|--------|----------|------|---------|
| POST | `/api/auth/register` | ❌ | Register new user → get JWT |
| POST | `/api/auth/login` | ❌ | Login → get JWT |
| GET | `/api/profile/me` | ✅ JWT | Get your profile |
| GET | `/api/profile/{username}` | ❌ | Get any user's public profile |
| PUT | `/api/profile` | ✅ JWT | Update your profile |

---

## 🔑 Authentication

All JWT-protected endpoints require header:
```
Authorization: Bearer YOUR_JWT_TOKEN
```

Get JWT from login/register response.

---

## 💻 Code Examples

### Extract userId in Controller
```csharp
[Authorize]
public IActionResult Example()
{
    // Method 1: Use helper from ProfileController
    var userId = User.FindFirst("userId")?.Value;
    
    // Method 2: Fallback to NameIdentifier
    if (string.IsNullOrEmpty(userId))
        userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    return Ok(userId);
}
```

### Use UserService
```csharp
private readonly IUserService _userService;

// Get by ID
var profile = await _userService.GetUserByIdAsync(userId);

// Get by username/email
var profile = await _userService.GetUserByUsernameAsync("user@example.com");

// Update profile
var updated = await _userService.UpdateUserProfileAsync(userId, updateDto);

// Check existence
var exists = await _userService.UserExistsAsync(userId);
```

---

## 📝 Request/Response Examples

### 1. Register
```bash
curl -X POST http://localhost:5050/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "displayName": "Jane Smith",
    "email": "jane@example.com",
    "password": "Test123!",
    "confirmPassword": "Test123!",
    "region": "London"
  }'
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "user-123",
  "email": "jane@example.com",
  "displayName": "Jane Smith",
  "region": "London",
  "expiresAt": "2026-04-14T15:30:00Z"
}
```

### 2. Get My Profile
```bash
curl -X GET http://localhost:5050/api/profile/me \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Response:**
```json
{
  "id": "user-123",
  "email": "jane@example.com",
  "displayName": "Jane Smith",
  "bio": null,
  "profilePictureUrl": null,
  "region": "London",
  "followersCount": 0,
  "followingCount": 0
}
```

### 3. Get Any User's Profile
```bash
curl -X GET http://localhost:5050/api/profile/jane@example.com
```

### 4. Update Profile
```bash
curl -X PUT http://localhost:5050/api/profile \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "displayName": "Jane Smith Updated",
    "bio": "Full-stack developer | Coffee enthusiast",
    "profilePictureUrl": "https://example.com/jane.jpg",
    "region": "London, UK"
  }'
```

---

## 🗂️ File Organization

```
server/
├── Controllers/
│   ├── AuthController.cs          # Login/Register
│   └── ProfileController.cs       # ✨ NEW - Profile endpoints
├── Services/
│   ├── JwtService.cs             # Token generation
│   └── UserService.cs            # ✨ NEW - Profile operations
├── Models/
│   ├── ApplicationUser.cs         # User model
│   └── DTOs/
│       ├── UserDtos.cs           # UserProfileDto, UpdateUserProfileDto
│       └── Auth/AuthDtos.cs      # LoginDto, RegisterDto
├── Data/
│   └── ApplicationDbContext.cs    # Database context
├── Program.cs                     # ✨ UPDATED - Service registration
├── PROFILE_API_DOCUMENTATION.md   # ✨ NEW - Full documentation
└── PROFILE_IMPLEMENTATION_SUMMARY.md # ✨ NEW - Implementation details
```

---

## 🔍 Key Classes

### ProfileController
- `GetCurrentUserProfile()` - requires JWT
- `GetUserProfile(username)` - public
- `UpdateProfile(updateDto)` - requires JWT
- `GetUserIdFromClaims()` - helper for JWT extraction

### UserService
- `GetUserByIdAsync(userId)` - with follower counts
- `GetUserByUsernameAsync(username)` - search by email/username
- `UpdateUserProfileAsync(userId, updateDto)` - update profile
- `UserExistsAsync(userId)` - existence check
- `MapToUserProfileDto()` - DTO conversion

### DTOs
- `UserProfileDto` - read-only profile data
- `UpdateUserProfileDto` - profile update request

---

## 🧪 Testing Checklist

- [x] Build succeeds: `dotnet build`
- [x] App starts: `dotnet run`
- [x] Register endpoint works
- [x] Login/JWT generation works
- [x] GET /profile/me (with JWT) works
- [x] GET /profile/{username} (public) works
- [x] PUT /profile (with JWT) works
- [x] Error handling for missing JWT
- [x] Error handling for invalid user
- [x] Validation on update (URL format, length)

---

## ⚙️ Configuration

**appsettings.json** (must contain):
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-minimum-32-characters-long",
    "Issuer": "MicrobloggingSystem",
    "Audience": "MicrobloggingSystemAPI",
    "ExpiryInMinutes": 60
  }
}
```

---

## 🐛 Common Issues

| Issue | Solution |
|-------|----------|
| "Scheme already exists" | Fixed - removed duplicate AddCookie() |
| Port 5050 in use | Kill process: `lsof -i :5050 \| kill -9` |
| JWT not extracted | Check token includes "userId" claim |
| 401 Unauthorized | Verify token in Authorization header |
| 404 User not found | Check username spelling (case-sensitive) |

---

## 📌 Important Notes

1. **JWT Token Claims**: JwtService adds "userId" claim for easy extraction
2. **Follower Counts**: Included in profile via eager loading
3. **Public Endpoint**: `GET /profile/{username}` has no authorization
4. **Update Validation**: Email validation, length limits enforced
5. **Error Messages**: Returned as JSON with `error` field

---

## 🔗 Related Documentation

- [PROFILE_API_DOCUMENTATION.md](./PROFILE_API_DOCUMENTATION.md) - Complete API reference
- [PROFILE_IMPLEMENTATION_SUMMARY.md](./PROFILE_IMPLEMENTATION_SUMMARY.md) - Implementation details

