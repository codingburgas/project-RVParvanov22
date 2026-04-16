# Microblogging System - Implementation Quick Start

## Project Structure Overview

```
server/
├── Controllers/
│   ├── AuthController.cs          # Login/Register
│   ├── PostsController.cs         # Posts CRUD + Feed
│   ├── LikesController.cs         # Like/Unlike NEW
│   ├── CommentsController.cs      # Comments CRUD
│   ├── FollowsController.cs       # Follow/Unfollow
│   ├── AccountController.cs       # MVC Auth
│   └── ProfileController.cs       # User Profiles
├── Models/
│   ├── ApplicationUser.cs         # User model
│   ├── Post.cs                    # Post model
│   ├── Comment.cs                 # Comment model
│   ├── PostLike.cs                # Like model NEW
│   ├── Follow.cs                  # Follow model
│   └── DTOs/
│       ├── PostDtos.cs
│       ├── CommentDtos.cs
│       ├── LikeDtos.cs            # NEW
│       ├── FollowDtos.cs
│       └── Auth/
├── Services/
│   ├── PostService.cs             # Post CRUD logic
│   ├── CommentService.cs          # Comment CRUD logic
│   ├── LikeService.cs             # Like logic NEW
│   ├── JwtService.cs
│   └── UserService.cs
├── Interfaces/
│   ├── IPostService.cs
│   ├── ICommentService.cs
│   └── ILikeService.cs            # NEW
├── Data/
│   └── ApplicationDbContext.cs    # EF Core DbContext
└── Program.cs
```

---

## Feature Summary

### ✅ Completed Features

1. **Authentication** (REST API)
   - User registration: `POST /api/auth/register`
   - User login: `POST /api/auth/login`
   - JWT token generation
   - Role-based access control (Admin/User)

2. **Posts System**
   - Create post: `POST /api/posts` [AUTH]
   - Get all posts: `GET /api/posts`
   - Get single post: `GET /api/posts/{id}`
   - Get user posts: `GET /api/posts/user/{userId}`
   - Update post: `PUT /api/posts/{id}` [AUTH, OWNER]
   - Delete post: `DELETE /api/posts/{id}` [AUTH, OWNER/ADMIN]
   - Personalized feed: `GET /api/posts/feed/personalized` [AUTH]
   - Max content: 280 characters

3. **Likes System** ⭐ NEW
   - Like post: `POST /api/likes` [AUTH]
   - Unlike post: `DELETE /api/likes/post/{postId}` [AUTH]
   - Get post likes: `GET /api/likes/post/{postId}`
   - Get like count: `GET /api/likes/post/{postId}/count`
   - Check user liked: `GET /api/likes/post/{postId}/user-liked` [AUTH]
   - Get user's liked posts: `GET /api/likes/user/{userId}`
   - Duplicate like prevention
   - One-like-per-user validation

4. **Comments System**
   - Create comment: `POST /api/comments` [AUTH]
   - Get post comments: `GET /api/comments/post/{postId}`
   - Get single comment: `GET /api/comments/{id}`
   - Update comment: `PUT /api/comments/{id}` [AUTH, OWNER]
   - Delete comment: `DELETE /api/comments/{id}` [AUTH, OWNER/ADMIN]
   - Max content: 280 characters

5. **Follow System**
   - Follow user: `POST /api/follows` [AUTH]
   - Unfollow user: `DELETE /api/follows/{id}` [AUTH]
   - Get followers: `GET /api/follows/followers/{userId}`
   - Get following: `GET /api/follows/following/{userId}`
   - Self-follow prevention
   - Duplicate follow prevention

6. **User Profiles** (MVC Views)
   - Profile display with stats
   - Follower/Following counts
   - Post counts

---

## Database Schema

### Key Models & Relationships

**ApplicationUser**
```csharp
public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; }
    public string Bio { get; set; }
    public string ProfilePictureUrl { get; set; }
    public string Region { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation Properties
    public ICollection<Post> Posts { get; set; }           // 1-to-Many
    public ICollection<Comment> Comments { get; set; }     // 1-to-Many
    public ICollection<PostLike> PostLikes { get; set; }   // 1-to-Many
    public ICollection<Follow> Following { get; set; }     // Self-ref: Users I follow
    public ICollection<Follow> Followers { get; set; }     // Self-ref: Users following me
}
```

**Post**
```csharp
public class Post
{
    public int Id { get; set; }
    public string Content { get; set; }          // Max 280 chars
    public string UserId { get; set; }           // Foreign Key
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public ApplicationUser User { get; set; }
    public ICollection<Comment> Comments { get; set; }
    public ICollection<PostLike> PostLikes { get; set; }    // NEW
}
```

**PostLike** (NEW)
```csharp
public class PostLike
{
    public int Id { get; set; }
    public string UserId { get; set; }          // Foreign Key
    public int PostId { get; set; }             // Foreign Key
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public ApplicationUser User { get; set; }
    public Post Post { get; set; }
    
    // Constraints: (UserId, PostId) must be unique
}
```

**Comment**
```csharp
public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; }         // Max 280 chars
    public string UserId { get; set; }          // Foreign Key
    public int PostId { get; set; }             // Foreign Key
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public ApplicationUser User { get; set; }
    public Post Post { get; set; }
}
```

**Follow** (Self-Referential)
```csharp
public class Follow
{
    public int Id { get; set; }
    public string FollowerId { get; set; }      // User doing the following
    public string FollowingId { get; set; }     // User being followed
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public ApplicationUser Follower { get; set; }
    public ApplicationUser Following { get; set; }
}
```

---

## Authorization Rules

### Public Endpoints (No Auth Required)
- GET /api/posts
- GET /api/posts/{id}
- GET /api/posts/user/{userId}
- GET /api/comments/post/{postId}
- GET /api/comments/{id}
- GET /api/likes/post/{postId}
- GET /api/follows/followers/{userId}
- GET /api/follows/following/{userId}

### Protected Endpoints (Auth Required)
- POST /api/posts - Create own post
- PUT /api/posts/{id} - Update own post
- DELETE /api/posts/{id} - Delete own post (or admin)
- GET /api/posts/feed/personalized - Your personalized feed
- POST /api/comments - Create comment on post
- PUT /api/comments/{id} - Update own comment
- DELETE /api/comments/{id} - Delete own comment (or admin)
- POST /api/likes - Like a post
- DELETE /api/likes/post/{postId} - Unlike a post
- GET /api/likes/post/{postId}/user-liked - Check if you liked
- POST /api/follows - Follow a user
- DELETE /api/follows/{id} - Unfollow a user

### Ownership Validation
- User can only create posts/comments as themselves
- User can only edit/delete their own posts/comments
- Admins can delete any post/comment
- Cannot delete your own follow record directly (use unfollow endpoint)

---

## Common API Patterns

### Pagination Pattern
All list endpoints support pagination:
```bash
GET /api/posts?pageNumber=1&pageSize=20
```

Response format:
```json
{
  "pageNumber": 1,
  "pageSize": 20,
  "totalPosts": 150,
  "data": [...]
}
```

### Error Responses
All errors follow standard format:
```json
{
  "error": "Descriptive error message"
}
```

### JWT Claims
```json
{
  "userId": "user-id-here",
  "displayName": "User Display Name",
  "email": "user@example.com",
  "roles": ["User"]
}
```

---

## Building a Client

### Example: React Client Implementation

```javascript
// API Service
const API_BASE = 'http://localhost:5050/api';

const apiCall = async (endpoint, options = {}) => {
  const token = localStorage.getItem('token');
  const headers = {
    'Content-Type': 'application/json',
    ...options.headers,
  };
  
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }
  
  const response = await fetch(`${API_BASE}${endpoint}`, {
    ...options,
    headers,
  });
  
  if (!response.ok) {
    throw new Error(`API Error: ${response.status}`);
  }
  
  return response.json();
};

// Posts
export const posts = {
  getAll: (page = 1, size = 20) =>
    apiCall(`/posts?pageNumber=${page}&pageSize=${size}`),
  
  getById: (id) =>
    apiCall(`/posts/${id}`),
  
  getByUser: (userId, page = 1, size = 20) =>
    apiCall(`/posts/user/${userId}?pageNumber=${page}&pageSize=${size}`),
  
  create: (content, userId, postType) =>
    apiCall('/posts', {
      method: 'POST',
      body: JSON.stringify({ content, userId, postType }),
    }),
  
  update: (id, content, postType) =>
    apiCall(`/posts/${id}`, {
      method: 'PUT',
      body: JSON.stringify({ content, postType }),
    }),
  
  delete: (id) =>
    apiCall(`/posts/${id}`, { method: 'DELETE' }),
  
  getFeed: () =>
    apiCall('/posts/feed/personalized'),
};

// Likes
export const likes = {
  getForPost: (postId) =>
    apiCall(`/likes/post/${postId}`),
  
  getCount: (postId) =>
    apiCall(`/likes/post/${postId}/count`),
  
  like: (userId, postId) =>
    apiCall('/likes', {
      method: 'POST',
      body: JSON.stringify({ userId, postId }),
    }),
  
  unlike: (postId) =>
    apiCall(`/likes/post/${postId}`, { method: 'DELETE' }),
  
  didUserLike: (postId) =>
    apiCall(`/likes/post/${postId}/user-liked`),
};

// Comments
export const comments = {
  getForPost: (postId) =>
    apiCall(`/comments/post/${postId}`),
  
  create: (content, userId, postId) =>
    apiCall('/comments', {
      method: 'POST',
      body: JSON.stringify({ content, userId, postId }),
    }),
  
  delete: (id) =>
    apiCall(`/comments/${id}`, { method: 'DELETE' }),
};

// Follows
export const follows = {
  getFollowers: (userId) =>
    apiCall(`/follows/followers/${userId}`),
  
  getFollowing: (userId) =>
    apiCall(`/follows/following/${userId}`),
  
  follow: (followerId, followingId) =>
    apiCall('/follows', {
      method: 'POST',
      body: JSON.stringify({ followerId, followingId }),
    }),
  
  unfollow: (id) =>
    apiCall(`/follows/${id}`, { method: 'DELETE' }),
};
```

---

## Testing Checklist

- [ ] POST /api/posts - Create a post
- [ ] GET /api/posts - View all posts (pagination works)
- [ ] GET /api/posts/{id} - View single post
- [ ] PUT /api/posts/{id} - Update post
- [ ] DELETE /api/posts/{id} - Delete post
- [ ] GET /api/posts/user/{userId} - Get user's posts
- [ ] POST /api/likes - Like a post
- [ ] DELETE /api/likes/post/{postId} - Unlike a post
- [ ] GET /api/likes/post/{postId} - Get post likes
- [ ] POST /api/comments - Add comment
- [ ] GET /api/comments/post/{postId} - Get comments
- [ ] POST /api/follows - Follow user
- [ ] GET /api/follows/followers/{userId} - Get followers
- [ ] GET /api/posts/feed/personalized - Get personalized feed

---

## Deployment Notes

### Environment Variables
```
JwtSettings:SecretKey=your-secret-key
JwtSettings:Issuer=http://localhost:5050
JwtSettings:Audience=http://localhost:5050
ConnectionString=Data Source=microblog.db
```

### Required Databases Migrations
```bash
# Already included in project
dotnet ef database update
```

### Start Server
```bash
cd server
dotnet run
```

Server runs on: **http://localhost:5050**

---

## Version History

### v1.0 - Complete Social Features
- ✅ Posts (CRUD)
- ✅ Likes (NEW)
- ✅ Comments (CRUD)
- ✅ Follows (Create/Delete)
- ✅ User Profiles
- ✅ JWT Authentication
- ✅ Role-based Authorization
- ✅ Pagination
- ✅ Error Handling

---

## Next Enhancements

1. **Media Support**
   - Image uploads for posts
   - Profile picture upload
   - Avatar support

2. **Advanced Features**
   - Hashtag support with search
   - Mention system (@username)
   - Real-time updates (WebSockets)
   - Notifications

3. **Performance**
   - Redis caching for popular posts
   - Database indexing optimization
   - Query optimization

4. **Admin Features**
   - Moderation tools
   - User suspension
   - Content flagging

---

**Last Updated:** April 14, 2024
**Status:** Production Ready ✅
