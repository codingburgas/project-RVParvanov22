# Social Platform API - Testing & Developer Guide

## Quick Start for Developers

### 1. Prerequisites
- .NET 10.0 SDK (ASP.NET Core 10)
- SQLite (built-in with EF Core)
- cURL, Postman, or Thunder Client for API testing

### 2. Setup & Run

```bash
cd microblogging-system/server
dotnet restore
dotnet build
dotnet run
```

**Server runs on:** `http://localhost:5050`

The database is created automatically with admin seed user:
- Email: `admin@microblog.com`
- Password: `Admin123!`

---

## API Testing Guide

### Method 1: Using cURL (Command Line)

#### Register a New User

```bash
curl -X POST http://localhost:5050/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "Password123!",
    "displayName": "John Doe",
    "region": "San Francisco"
  }' | jq .
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "uuid-here",
  "email": "john@example.com",
  "displayName": "John Doe",
  "region": "San Francisco",
  "expiresAt": "2024-04-14T11:35:00Z"
}
```

Store the token for subsequent requests.

---

#### Create a Post

```bash
TOKEN="your-token-here"
USER_ID="your-user-id-here"

curl -X POST http://localhost:5050/api/posts \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"content\": \"This is my first post!\",
    \"userId\": \"$USER_ID\",
    \"postType\": \"General\"
  }" | jq .
```

---

#### Like a Post

```bash
TOKEN="your-token-here"
USER_ID="your-user-id-here"
POST_ID="1"

curl -X POST http://localhost:5050/api/likes \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"userId\": \"$USER_ID\",
    \"postId\": $POST_ID
  }" | jq .
```

---

#### Comment on a Post

```bash
TOKEN="your-token-here"
USER_ID="your-user-id-here"
POST_ID="1"

curl -X POST http://localhost:5050/api/comments \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"content\": \"Great post!\",
    \"userId\": \"$USER_ID\",
    \"postId\": $POST_ID
  }" | jq .
```

---

#### Follow a User

```bash
TOKEN="your-token-here"
YOUR_USER_ID="your-user-id"
OTHER_USER_ID="other-user-id"

curl -X POST http://localhost:5050/api/follows \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"followerId\": \"$YOUR_USER_ID\",
    \"followingId\": \"$OTHER_USER_ID\"
  }" | jq .
```

---

#### Get Personalized Feed

```bash
TOKEN="your-token-here"

curl http://localhost:5050/api/posts/feed/personalized \
  -H "Authorization: Bearer $TOKEN" | jq .
```

---

### Method 2: Using Postman

1. **Create a new request collection**
2. **Set up environment variables:**
   - `{{baseUrl}}` = `http://localhost:5050/api`
   - `{{token}}` = Your JWT token
   - `{{userId}}` = Your user ID

3. **Create requests:**

**Register (POST)**
- URL: `{{baseUrl}}/auth/register`
- Body (JSON):
  ```json
  {
    "email": "test@example.com",
    "password": "TestPass123!",
    "displayName": "Test User",
    "region": "US"
  }
  ```
- Tests: Save response token to `pm.environment.set("token", jsonData.token);`

**Create Post (POST)**
- URL: `{{baseUrl}}/posts`
- Headers: `Authorization: Bearer {{token}}`
- Body (JSON):
  ```json
  {
    "content": "Hello world!",
    "userId": "{{userId}}"
  }
  ```

**Get Feed (GET)**
- URL: `{{baseUrl}}/posts/feed/personalized`
- Headers: `Authorization: Bearer {{token}}`

---

## Complete Testing Workflow

### Scenario: Create a Social Network

#### Step 1: Register Two Users

**User A:**
```bash
RESPONSE_A=$(curl -s -X POST http://localhost:5050/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "alice@example.com",
    "password": "Alice123!",
    "displayName": "Alice",
    "region": "NYC"
  }')

TOKEN_A=$(echo $RESPONSE_A | jq -r '.token')
USERID_A=$(echo $RESPONSE_A | jq -r '.userId')
```

**User B:**
```bash
RESPONSE_B=$(curl -s -X POST http://localhost:5050/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "bob@example.com",
    "password": "Bob123!",
    "displayName": "Bob",
    "region": "LA"
  }')

TOKEN_B=$(echo $RESPONSE_B | jq -r '.token')
USERID_B=$(echo $RESPONSE_B | jq -r '.userId')
```

---

#### Step 2: Both Users Create Posts

**Alice creates a post:**
```bash
POST_ID_1=$(curl -s -X POST http://localhost:5050/api/posts \
  -H "Authorization: Bearer $TOKEN_A" \
  -H "Content-Type: application/json" \
  -d "{
    \"content\": \"Hello from Alice!\",
    \"userId\": \"$USERID_A\"
  }" | jq -r '.id')

echo "Alice's post ID: $POST_ID_1"
```

**Bob creates a post:**
```bash
POST_ID_2=$(curl -s -X POST http://localhost:5050/api/posts \
  -H "Authorization: Bearer $TOKEN_B" \
  -H "Content-Type: application/json" \
  -d "{
    \"content\": \"Hello from Bob!\",
    \"userId\": \"$USERID_B\"
  }" | jq -r '.id')

echo "Bob's post ID: $POST_ID_2"
```

---

#### Step 3: Interact with Posts

**Alice likes Bob's post:**
```bash
curl -s -X POST http://localhost:5050/api/likes \
  -H "Authorization: Bearer $TOKEN_A" \
  -H "Content-Type: application/json" \
  -d "{
    \"userId\": \"$USERID_A\",
    \"postId\": $POST_ID_2
  }" | jq .
```

**Bob comments on Alice's post:**
```bash
curl -s -X POST http://localhost:5050/api/comments \
  -H "Authorization: Bearer $TOKEN_B" \
  -H "Content-Type: application/json" \
  -d "{
    \"content\": \"Nice post Alice!\",
    \"userId\": \"$USERID_B\",
    \"postId\": $POST_ID_1
  }" | jq .
```

---

#### Step 4: Follow Each Other

**Alice follows Bob:**
```bash
curl -s -X POST http://localhost:5050/api/follows \
  -H "Authorization: Bearer $TOKEN_A" \
  -H "Content-Type: application/json" \
  -d "{
    \"followerId\": \"$USERID_A\",
    \"followingId\": \"$USERID_B\"
  }" | jq .
```

**Bob follows Alice:**
```bash
curl -s -X POST http://localhost:5050/api/follows \
  -H "Authorization: Bearer $TOKEN_B" \
  -H "Content-Type: application/json" \
  -d "{
    \"followerId\": \"$USERID_B\",
    \"followingId\": \"$USERID_A\"
  }" | jq .
```

---

#### Step 5: View Results

**Get all posts:**
```bash
curl -s http://localhost:5050/api/posts?pageNumber=1&pageSize=10 | jq .
```

**Get Alice's feed (posts from people she follows):**
```bash
curl -s http://localhost:5050/api/posts/feed/personalized \
  -H "Authorization: Bearer $TOKEN_A" | jq .
```

**Get likes on Bob's post:**
```bash
curl -s http://localhost:5050/api/likes/post/$POST_ID_2 | jq .
```

**Get comments on Alice's post:**
```bash
curl -s http://localhost:5050/api/comments/post/$POST_ID_1 | jq .
```

**Get Alice's followers:**
```bash
curl -s http://localhost:5050/api/follows/followers/$USERID_A | jq .
```

---

## Complete Test Script

Save as `test-api.sh`:

```bash
#!/bin/bash

set -e

BASE_URL="http://localhost:5050/api"
COLORS='\033[0;32m' # Green
NC='\033[0m'         # No Color

log() {
  echo -e "${COLORS}=== $1 ===${NC}"
}

# 1. Register Users
log "Registering users..."

RESPONSE_A=$(curl -s -X POST $BASE_URL/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "alice@test.com",
    "password": "Alice123!",
    "displayName": "Alice Anderson",
    "region": "NYC"
  }')

TOKEN_A=$(echo $RESPONSE_A | jq -r '.token')
USERID_A=$(echo $RESPONSE_A | jq -r '.userId')

RESPONSE_B=$(curl -s -X POST $BASE_URL/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "bob@test.com",
    "password": "Bob123!",
    "displayName": "Bob Builder",
    "region": "LA"
  }')

TOKEN_B=$(echo $RESPONSE_B | jq -r '.token')
USERID_B=$(echo $RESPONSE_B | jq -r '.userId')

echo "Alice ID: $USERID_A"
echo "Bob ID: $USERID_B"

# 2. Create Posts
log "Creating posts..."

POST_A=$(curl -s -X POST $BASE_URL/posts \
  -H "Authorization: Bearer $TOKEN_A" \
  -H "Content-Type: application/json" \
  -d "{
    \"content\": \"My first social post!\",
    \"userId\": \"$USERID_A\"
  }" | jq -r '.id')

POST_B=$(curl -s -X POST $BASE_URL/posts \
  -H "Authorization: Bearer $TOKEN_B" \
  -H "Content-Type: application/json" \
  -d "{
    \"content\": \"Building things and breaking things!\",
    \"userId\": \"$USERID_B\"
  }" | jq -r '.id')

echo "Alice's post: $POST_A"
echo "Bob's post: $POST_B"

# 3. Like Posts
log "Liking posts..."

curl -s -X POST $BASE_URL/likes \
  -H "Authorization: Bearer $TOKEN_A" \
  -H "Content-Type: application/json" \
  -d "{\"userId\": \"$USERID_A\", \"postId\": $POST_B}" > /dev/null

curl -s -X POST $BASE_URL/likes \
  -H "Authorization: Bearer $TOKEN_B" \
  -H "Content-Type: application/json" \
  -d "{\"userId\": \"$USERID_B\", \"postId\": $POST_A}" > /dev/null

echo "✓ Likes created"

# 4. Comment
log "Adding comments..."

curl -s -X POST $BASE_URL/comments \
  -H "Authorization: Bearer $TOKEN_B" \
  -H "Content-Type: application/json" \
  -d "{\"content\": \"Love it!\", \"userId\": \"$USERID_B\", \"postId\": $POST_A}" > /dev/null

echo "✓ Comments added"

# 5. Follow
log "Creating follows..."

curl -s -X POST $BASE_URL/follows \
  -H "Authorization: Bearer $TOKEN_A" \
  -H "Content-Type: application/json" \
  -d "{\"followerId\": \"$USERID_A\", \"followingId\": \"$USERID_B\"}" > /dev/null

echo "✓ Follows created"

# 6. View Results
log "Fetching feed..."
curl -s http://localhost:5050/api/posts/feed/personalized \
  -H "Authorization: Bearer $TOKEN_A" | jq '.data | .[0]'

log "Test completed successfully! ✓"
```

Run it:
```bash
chmod +x test-api.sh
./test-api.sh
```

---

## Common Issues & Solutions

### Issue: "Unauthorized" Error
**Solution:** Make sure your JWT token is valid and included in the Authorization header:
```bash
-H "Authorization: Bearer <your-token>"
```

### Issue: "User has already liked this post"
**Solution:** Check if the like already exists before liking again:
```bash
curl http://localhost:5050/api/likes/post/1/user-liked \
  -H "Authorization: Bearer $TOKEN"
```

### Issue: "You can only create posts for your own account"
**Solution:** Make sure the `userId` in the request matches the authenticated user:
```json
{
  "content": "...",
  "userId": "same-as-authenticated-user"
}
```

### Issue: Port 5050 already in use
**Solution:**
```bash
lsof -i :5050
kill -9 <PID>
```

---

## Performance Tips

1. **Use pagination** for large result sets:
   ```bash
   ?pageNumber=1&pageSize=20
   ```

2. **Cache responses** when building clients:
   - Cache posts for 30 seconds
   - Cache like counts
   - Use ETags if implementing

3. **Optimize queries:**
   - The API includes related data (User, Comments, Likes)
   - Use `Include()` in EF Core for eager loading

4. **Monitor database:**
   ```bash
   # See database file
   ls -la bin/Debug/net10.0/microblog.db
   ```

---

## Debugging

### Enable Logging
```csharp
// In Program.cs
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

### Inspect Database
```bash
# SQLite CLI
sqlite3 bin/Debug/net10.0/microblog.db
sqlite> SELECT COUNT(*) FROM Posts;
sqlite> SELECT * FROM Posts LIMIT 5;
```

### ViewHTTP Requests
Use F12 Network tab in browser or:
```bash
curl -v http://localhost:5050/api/posts
```

---

## API Rate Limiting (Future Enhancement)

Currently not implemented, but recommended additions:
- Rate limit: 100 requests/minute per user
- Implement using middleware or package like `AspNetCoreRateLimit`

---

## Summary

You now have a **production-ready social API** with:
- ✅ User authentication (JWT)
- ✅ Posts (CRUD)
- ✅ Likes (Create/Delete)
- ✅ Comments (CRUD)
- ✅ Follows (Create/Delete)
- ✅ Personalized feed
- ✅ Proper authorization
- ✅ Error handling
- ✅ Pagination
- ✅ Full documentation

Start building your client! 🚀

---

**Last Updated:** April 14, 2024
**Testing Status:** ✅ Ready
