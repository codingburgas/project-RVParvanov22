# Microblogging System - API Documentation

## Overview

Complete REST API for a social microblogging platform with posts, likes, comments, and follow system.

**Base URL:** `http://localhost:5050/api`

---

## Table of Contents

1. [Authentication](#authentication)
2. [Posts API](#posts-api)
3. [Likes API](#likes-api)
4. [Comments API](#comments-api)
5. [Follows API](#follows-api)
6. [DTOs & Response Models](#dtos--response-models)
7. [Example Workflows](#example-workflows)
8. [Error Handling](#error-handling)

---

## Authentication

### JWT Token Format

All protected endpoints require a valid JWT token in the `Authorization` header:

```
Authorization: Bearer <jwt_token>
```

### JWT Claims

The token contains:
- `userId`: User ID
- `displayName`: User's display name
- `email`: User's email

### Roles

- **Admin**: Full access, can delete any post/comment
- **User**: Standard user access

---

## Posts API

### 1. Get All Posts (Public)

**Endpoint:** `GET /posts`

**Auth:** Not required

**Query Parameters:**
- `pageNumber` (int, default: 1) - Page number for pagination
- `pageSize` (int, default: 20) - Number of posts per page

**Response:**
```json
{
  "pageNumber": 1,
  "pageSize": 20,
  "totalPosts": 42,
  "data": [
    {
      "id": 1,
      "content": "Great day for coding!",
      "postType": "General",
      "mediaPath": null,
      "mediaType": null,
      "createdAt": "2024-04-14T10:30:00Z",
      "commentsCount": 3,
      "likesCount": 15,
      "userId": "user-123",
      "userDisplayName": "John Doe",
      "userProfilePictureUrl": "https://..."
    }
  ]
}
```

**Example:**
```bash
curl http://localhost:5050/api/posts?pageNumber=1&pageSize=10
```

---

### 2. Get Single Post

**Endpoint:** `GET /posts/{id}`

**Auth:** Not required

**Response:**
```json
{
  "id": 1,
  "content": "Great day for coding!",
  "postType": "General",
  "createdAt": "2024-04-14T10:30:00Z",
  "commentsCount": 3,
  "likesCount": 15,
  "userId": "user-123",
  "userDisplayName": "John Doe"
}
```

**Example:**
```bash
curl http://localhost:5050/api/posts/1
```

---

### 3. Get Posts by User

**Endpoint:** `GET /posts/user/{userId}`

**Auth:** Not required

**Query Parameters:**
- `pageNumber` (int, default: 1)
- `pageSize` (int, default: 20)

**Response:**
```json
{
  "userId": "user-123",
  "pageNumber": 1,
  "pageSize": 20,
  "totalPosts": 8,
  "data": [...]
}
```

**Example:**
```bash
curl http://localhost:5050/api/posts/user/user-123
```

---

### 4. Create Post ⭐ Required Auth

**Endpoint:** `POST /posts`

**Auth:** Required (JWT token)

**Request Body:**
```json
{
  "content": "This is my first post!",
  "userId": "user-123",
  "postType": "General",
  "mediaPath": null,
  "mediaType": null
}
```

**Validation:**
- `content`: Required, max 280 characters
- `userId`: Required, must match current user ID (cannot create posts for other users)

**Response:** 201 Created
```json
{
  "id": 42,
  "content": "This is my first post!",
  "createdAt": "2024-04-14T10:35:00Z",
  "commentsCount": 0,
  "likesCount": 0,
  "userId": "user-123",
  "userDisplayName": "John Doe"
}
```

**Example:**
```bash
curl -X POST http://localhost:5050/api/posts \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "content": "Hello world!",
    "userId": "user-123"
  }'
```

---

### 5. Update Post ⭐ Required Auth

**Endpoint:** `PUT /posts/{id}`

**Auth:** Required (post owner or admin)

**Request Body:**
```json
{
  "content": "Updated post content",
  "postType": "Achievement"
}
```

**Response:** 204 No Content

**Example:**
```bash
curl -X PUT http://localhost:5050/api/posts/42 \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "content": "Updated content here"
  }'
```

---

### 6. Delete Post ⭐ Required Auth

**Endpoint:** `DELETE /posts/{id}`

**Auth:** Required (post owner or admin)

**Response:** 204 No Content

**Example:**
```bash
curl -X DELETE http://localhost:5050/api/posts/42 \
  -H "Authorization: Bearer <token>"
```

---

### 7. Get Personalized Feed ⭐ Required Auth

**Endpoint:** `GET /posts/feed/personalized`

**Auth:** Required (JWT token)

**Query Parameters:**
- `pageNumber` (int, default: 1)
- `pageSize` (int, default: 20)

**Returns:** Posts from users that the current user follows

**Response:**
```json
{
  "pageNumber": 1,
  "pageSize": 20,
  "data": [...]
}
```

**Example:**
```bash
curl http://localhost:5050/api/posts/feed/personalized \
  -H "Authorization: Bearer <token>"
```

---

## Likes API

### 1. Get Likes for a Post

**Endpoint:** `GET /likes/post/{postId}`

**Auth:** Not required

**Response:**
```json
{
  "postId": 1,
  "totalLikes": 15,
  "currentUserLiked": false,
  "likes": [
    {
      "id": 1,
      "userId": "user-456",
      "postId": 1,
      "createdAt": "2024-04-14T11:00:00Z",
      "userDisplayName": "Jane Smith"
    }
  ]
}
```

**Example:**
```bash
curl http://localhost:5050/api/likes/post/1
```

---

### 2. Get Like Count for a Post

**Endpoint:** `GET /likes/post/{postId}/count`

**Auth:** Not required

**Response:**
```json
{
  "postId": 1,
  "likeCount": 15
}
```

**Example:**
```bash
curl http://localhost:5050/api/likes/post/1/count
```

---

### 3. Check if User Liked a Post ⭐ Required Auth

**Endpoint:** `GET /likes/post/{postId}/user-liked`

**Auth:** Required (JWT token)

**Response:**
```json
{
  "postId": 1,
  "userLiked": true
}
```

**Example:**
```bash
curl http://localhost:5050/api/likes/post/1/user-liked \
  -H "Authorization: Bearer <token>"
```

---

### 4. Like a Post ⭐ Required Auth

**Endpoint:** `POST /likes`

**Auth:** Required (JWT token)

**Request Body:**
```json
{
  "userId": "user-123",
  "postId": 1
}
```

**Validation:**
- `userId`: Must match current authenticated user
- `postId`: Valid post must exist
- Cannot like the same post twice

**Response:** 201 Created
```json
{
  "id": 42,
  "userId": "user-123",
  "postId": 1,
  "createdAt": "2024-04-14T11:05:00Z",
  "userDisplayName": "John Doe"
}
```

**Example:**
```bash
curl -X POST http://localhost:5050/api/likes \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "user-123",
    "postId": 1
  }'
```

---

### 5. Unlike a Post ⭐ Required Auth

**Endpoint:** `DELETE /likes/post/{postId}`

**Auth:** Required (JWT token)

**Response:** 204 No Content

**Example:**
```bash
curl -X DELETE http://localhost:5050/api/likes/post/1 \
  -H "Authorization: Bearer <token>"
```

---

### 6. Get Posts Liked by User

**Endpoint:** `GET /likes/user/{userId}`

**Auth:** Not required

**Query Parameters:**
- `pageNumber` (int, default: 1)
- `pageSize` (int, default: 20)

**Response:**
```json
{
  "userId": "user-123",
  "pageNumber": 1,
  "pageSize": 20,
  "data": [...]
}
```

**Example:**
```bash
curl http://localhost:5050/api/likes/user/user-123
```

---

## Comments API

### 1. Get Comments for a Post

**Endpoint:** `GET /comments/post/{postId}`

**Auth:** Not required

**Response:**
```json
[
  {
    "id": 1,
    "content": "Great post!",
    "createdAt": "2024-04-14T11:10:00Z",
    "userId": "user-456",
    "userDisplayName": "Jane Smith",
    "postId": 1
  }
]
```

**Example:**
```bash
curl http://localhost:5050/api/comments/post/1
```

---

### 2. Get Single Comment

**Endpoint:** `GET /comments/{id}`

**Auth:** Not required

**Response:**
```json
{
  "id": 1,
  "content": "Great post!",
  "createdAt": "2024-04-14T11:10:00Z",
  "userId": "user-456",
  "userDisplayName": "Jane Smith",
  "postId": 1
}
```

**Example:**
```bash
curl http://localhost:5050/api/comments/1
```

---

### 3. Create Comment ⭐ Required Auth

**Endpoint:** `POST /comments`

**Auth:** Required (JWT token)

**Request Body:**
```json
{
  "content": "Great post!",
  "userId": "user-123",
  "postId": 1
}
```

**Validation:**
- `content`: Required, max 280 characters
- `userId`: Must match current authenticated user
- `postId`: Must reference existing post

**Response:** 201 Created

**Example:**
```bash
curl -X POST http://localhost:5050/api/comments \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "content": "This is awesome!",
    "userId": "user-123",
    "postId": 1
  }'
```

---

### 4. Update Comment ⭐ Required Auth

**Endpoint:** `PUT /comments/{id}`

**Auth:** Required (comment owner)

**Request Body:**
```json
{
  "content": "Updated comment text"
}
```

**Response:** 204 No Content

**Example:**
```bash
curl -X PUT http://localhost:5050/api/comments/1 \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "content": "Updated comment"
  }'
```

---

### 5. Delete Comment ⭐ Required Auth

**Endpoint:** `DELETE /comments/{id}`

**Auth:** Required (comment owner or admin)

**Response:** 204 No Content

**Example:**
```bash
curl -X DELETE http://localhost:5050/api/comments/1 \
  -H "Authorization: Bearer <token>"
```

---

## Follows API

### 1. Get Followers

**Endpoint:** `GET /follows/followers/{userId}`

**Auth:** Not required

**Response:**
```json
[
  {
    "id": 1,
    "followerId": "user-456",
    "followingId": "user-123",
    "createdAt": "2024-04-14T10:00:00Z"
  }
]
```

**Example:**
```bash
curl http://localhost:5050/api/follows/followers/user-123
```

---

### 2. Get Following List

**Endpoint:** `GET /follows/following/{userId}`

**Auth:** Not required

**Response:**
```json
[
  {
    "id": 2,
    "followerId": "user-123",
    "followingId": "user-789",
    "createdAt": "2024-04-14T10:05:00Z"
  }
]
```

**Example:**
```bash
curl http://localhost:5050/api/follows/following/user-123
```

---

### 3. Follow User ⭐ Required Auth

**Endpoint:** `POST /follows`

**Auth:** Required (JWT token)

**Request Body:**
```json
{
  "followerId": "user-123",
  "followingId": "user-789"
}
```

**Validation:**
- `followerId`: Must match current authenticated user
- `followingId`: Must reference existing user
- Cannot follow yourself
- Cannot follow the same user twice

**Response:** 201 Created
```json
{
  "id": 1,
  "followerId": "user-123",
  "followingId": "user-789",
  "createdAt": "2024-04-14T10:06:00Z"
}
```

**Example:**
```bash
curl -X POST http://localhost:5050/api/follows \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "followerId": "user-123",
    "followingId": "user-789"
  }'
```

---

### 4. Unfollow User ⭐ Required Auth

**Endpoint:** `DELETE /follows/{id}`

**Auth:** Required (JWT token)

**Response:** 204 No Content

**Example:**
```bash
curl -X DELETE http://localhost:5050/api/follows/1 \
  -H "Authorization: Bearer <token>"
```

---

## DTOs & Response Models

### CreatePostDto
```json
{
  "content": "string (required, max 280)",
  "userId": "string (required)",
  "postType": "string (optional)",
  "mediaPath": "string (optional)",
  "mediaType": "string (optional)"
}
```

### UpdatePostDto
```json
{
  "content": "string (required, max 280)",
  "postType": "string (optional)",
  "mediaPath": "string (optional)",
  "mediaType": "string (optional)"
}
```

### PostResponseDto
```json
{
  "id": "int",
  "content": "string",
  "postType": "string",
  "mediaPath": "string",
  "mediaType": "string",
  "createdAt": "datetime",
  "commentsCount": "int",
  "likesCount": "int",
  "userId": "string",
  "userDisplayName": "string",
  "userProfilePictureUrl": "string"
}
```

### CreateCommentDto
```json
{
  "content": "string (required, max 280)",
  "userId": "string (required)",
  "postId": "int (required)"
}
```

### CreateLikeDto
```json
{
  "userId": "string (required)",
  "postId": "int (required)"
}
```

### CreateFollowDto
```json
{
  "followerId": "string (required)",
  "followingId": "string (required)"
}
```

---

## Example Workflows

### Workflow 1: Register and Create a Post

1. **Register User**
   ```bash
   curl -X POST http://localhost:5050/api/auth/register \
     -H "Content-Type: application/json" \
     -d '{
       "email": "john@example.com",
       "password": "password123",
       "displayName": "John Doe",
       "region": "US"
     }'
   ```
   Response: JWT token

2. **Create Post**
   ```bash
   curl -X POST http://localhost:5050/api/posts \
     -H "Authorization: Bearer <token>" \
     -H "Content-Type: application/json" \
     -d '{
       "content": "Hello world!",
       "userId": "user-id-from-token"
     }'
   ```

---

### Workflow 2: Like and Comment on a Post

1. **Get Post**
   ```bash
   curl http://localhost:5050/api/posts/1
   ```

2. **Like the Post**
   ```bash
   curl -X POST http://localhost:5050/api/likes \
     -H "Authorization: Bearer <token>" \
     -H "Content-Type: application/json" \
     -d '{
       "userId": "user-123",
       "postId": 1
     }'
   ```

3. **Add Comment**
   ```bash
   curl -X POST http://localhost:5050/api/comments \
     -H "Authorization: Bearer <token>" \
     -H "Content-Type: application/json" \
     -d '{
       "content": "Great post!",
       "userId": "user-123",
       "postId": 1
     }'
   ```

4. **Get All Likes and Comments**
   ```bash
   curl http://localhost:5050/api/likes/post/1
   curl http://localhost:5050/api/comments/post/1
   ```

---

### Workflow 3: Follow User and View Feed

1. **Follow User**
   ```bash
   curl -X POST http://localhost:5050/api/follows \
     -H "Authorization: Bearer <token>" \
     -H "Content-Type: application/json" \
     -d '{
       "followerId": "user-123",
       "followingId": "user-789"
     }'
   ```

2. **Get Your Feed**
   ```bash
   curl http://localhost:5050/api/posts/feed/personalized \
     -H "Authorization: Bearer <token>"
   ```

3. **Get User Profile**
   ```bash
   curl http://localhost:5050/api/posts/user/user-789
   curl http://localhost:5050/api/follows/followers/user-789
   curl http://localhost:5050/api/follows/following/user-789
   ```

---

## Error Handling

### Standard Error Response Format

```json
{
  "error": "Error message description"
}
```

### Common HTTP Status Codes

| Status | Meaning | Example |
|--------|---------|---------|
| 200 | OK | GET request successful |
| 201 | Created | POST request successful |
| 204 | No Content | DELETE/PUT successful |
| 400 | Bad Request | Invalid input data |
| 401 | Unauthorized | Missing/invalid JWT token |
| 403 | Forbidden | Not authorized (cannot delete other's post) |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Duplicate like/follow |
| 500 | Server Error | Internal server error |

### Example Error Responses

**Unauthorized (Missing Token):**
```json
{
  "error": "Unauthorized"
}
```

**Forbidden (Wrong User):**
```json
{
  "error": "You can only delete your own posts"
}
```

**Bad Request (Validation):**
```json
{
  "error": "User has already liked this post"
}
```

---

## Security Notes

✅ **Protected Features:**
- Create posts (only own posts)
- Update posts (only own posts)
- Delete posts (own posts or admin)
- Like posts
- Create comments
- Update comments (own comments)
- Delete comments (own comments or admin)
- Follow users

✅ **Authorization Checks:**
- Every [Authorize] endpoint validates JWT token
- userId in request must match authenticated user (except for admins)
- Post/Comment ownership verified before modification

✅ **Data Validation:**
- Content length limited to 280 characters
- Email format validation
- Password requirements enforced
- User existence verified before operations

---

## Rate Limiting & Pagination

- **Default Page Size:** 20 items
- **Max Page Size:** Configurable (default 20)
- **Sorting:** Posts sorted by CreatedAt (descending)

---

## Testing with cURL

### Quick Test Script

```bash
#!/bin/bash

BASE_URL="http://localhost:5050/api"

# 1. Register
echo "=== Registering user ==="
REGISTER_RESPONSE=$(curl -s -X POST $BASE_URL/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "displayName": "Test User",
    "region": "US"
  }')

TOKEN=$(echo $REGISTER_RESPONSE | grep -o '"token":"[^"]*' | cut -d'"' -f4)
USER_ID=$(echo $REGISTER_RESPONSE | grep -o '"userId":"[^"]*' | cut -d'"' -f4)

echo "Token: $TOKEN"
echo "User ID: $USER_ID"

# 2. Create post
echo -e "\n=== Creating post ==="
curl -s -X POST $BASE_URL/posts \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"content\": \"Hello from the API!\",
    \"userId\": \"$USER_ID\"
  }" | jq .

# 3. Get feed
echo -e "\n=== Getting feed ==="
curl -s $BASE_URL/posts/feed/personalized \
  -H "Authorization: Bearer $TOKEN" | jq .
```

---

## Next Steps

1. Test all endpoints using cURL or Postman
2. Implement pagination on client side
3. Add real-time notifications (WebSockets)
4. Implement media upload for posts
5. Add search functionality
6. Add hashtag support

---

**Last Updated:** April 14, 2024
**API Version:** 1.0
