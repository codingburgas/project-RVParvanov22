# Visual Walkthrough: Dark Gaming Sidebar UI

## What You'll See When Running the App

### 1. Login Page
```
┌─────────────────────────────────────────────────┐
│ ┌──────────────┐                               │
│ │ MP PlayerPulse                                │
│ ├──────────────┤                               │
│ │ ● Feed                                        │
│ │ ● Create Post                                 │
│ │ ● Reports                                     │
│ │                                              │
│ │ (Empty space)                                │
│ │                                              │
│ │ Or Login?                                    │
│ └────────────────────────────────────────────┘

                    [Login Form]
        Email: _______________
        Password: _______________
        
        [Login Button]
```

### 2. After Login - Home Page with Sidebar

```
┌─────────────────────────────────────────────────────────────────┐
│ ┌──────────────┐ ┌────────────────────────────────────────────┐ │
│ │ MP           │ │ [Navigation Bar]                           │ │
│ │ PlayerPulse  │ │                                             │ │
│ ├──────────────┤ ├── HOME PAGE CONTENT ──────────────────────┤ │
│ │              │ │                                             │ │
│ │ ● Feed       │ │ Feed Posts...                              │ │
│ │ ● Create Post│ │ [Post Card 1]                              │ │
│ │ ● Reports    │ │ [Post Card 2]                              │ │
│ │              │ │ [Post Card 3]                              │ │
│ │              │ │                                             │ │
│ │              │ │ (Scrollable content)                        │ │
│ │              │ │                                             │ │
│ │ ┌──────────┐ │ │                                             │ │
│ │ │ ┌──────┐ │ │ │                                             │ │
│ │ │ │ User │ │ │ │                                             │ │
│ │ │ │Avatar│ │ │ │                                             │ │
│ │ │ └──────┘ │ │ │                                             │ │
│ │ │ John     │ │ │  ← Click here                              │ │
│ │ │ john@... │ │ │  to go to                                  │ │
│ │ │          │ │ │  /Profile                                  │ │
│ │ └──────────┘ │ │                                             │ │
│ │              │ │                                             │ │
│ │ [Logout]     │ │                                             │ │
│ │ (Red Button) │ │                                             │ │
│ └──────────────┘ └────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘

Sidebar Information:
- Logo: "MP" in green gradient box + "PlayerPulse" text
- Navigation: Feed, Create Post, Reports with icons
- User Card: Avatar placeholder, DisplayName, Email
- Logout: Red button with sign-out icon
```

### 3. Profile Page

```
┌─────────────────────────────────────────────────────────────────┐
│ ┌──────────────┐                                               │
│ │ MP           │                                               │
│ │ PlayerPulse  │  ┌─────────────────────────────────────────┐ │
│ ├──────────────┤  │                                         │ │
│ │              │  │  ┌─────────┐                            │ │
│ │ ● Feed       │  │  │ Avatar  │ John Doe                  │ │
│ │ ● Create Post│  │  │[140x140]│ john@example.com          │ │
│ │ ● Reports    │  │  │         │ 📍 San Francisco, CA      │ │
│ │              │  │  └─────────┘                            │ │
│ │              │  │  "Software developer..."                │ │
│ │ ┌──────────┐ │  │                                         │ │
│ │ │ John     │ │  │ ┌────────┬────────┬────────┐           │ │
│ │ │ john@... │ │  │ │ Posts: │Follow: │Follow: │           │ │
│ │ │          │ │  │ │   42   │  150   │   87   │           │ │
│ │ │          │ │  │ └────────┴────────┴────────┘           │ │
│ │ └──────────┘ │  │                                         │ │
│ │              │  │ Member since: April 14, 2024            │ │
│ │ [Logout]     │  │                                         │ │
│ │              │  │ [← Back to Feed] [✏️ Edit Profile]     │ │
│ │              │  └─────────────────────────────────────────┘ │
│ └──────────────┘                                               │
└─────────────────────────────────────────────────────────────────┘
```

### 4. Mobile View (max-width: 768px)

```
┌───────────────────────────────────────┐
│ MP ● Feed ● Create ● Reports [User] │  ← Horizontal sidebar
├───────────────────────────────────────┤
│                                       │
│ [Home Page Content]                  │
│                                       │
│ Feed Posts...                         │
│ [Post Card 1]                         │
│ [Post Card 2]                         │
│ [Post Card 3]                         │
│                                       │
│                                       │
└───────────────────────────────────────┘

```

---

## Color Scheme in Action

### Sidebar Colors
```
Background Gradient:
Top:    #1a1a2e  (Dark navy)
Bottom: #16213e  (Slightly lighter navy)
```

### Accent Colors
```
Green:    #50fa7b  (Buttons, active states, highlights)
Blue:     #8be9fd  (Links, secondary text)
Red:      #ff5555  (Logout button, danger actions)
```

### Text Colors
```
Primary:  #f8f8f2  (Main text, white)
Secondary:#d0d0d0  (Navigation items)
Muted:    #b0b0b0  (Hint text, labels)
```

---

## Interactive Elements

### Sidebar Navigation Item (Hover)
```
Before:
├─ ● Feed         [Gray text]

After hover:
├─ ● Feed         [Blue text, slightly indented right]
   ↳ Smooth transition 0.3s
```

### User Profile Card (Hover)
```
Before:
┌──────────────┐
│ Avatar       │  [Subtle border]
│ John         │
│ john@...     │
└──────────────┘

After hover:
┌──────────────┐
│ Avatar       │  [Brighter border, lifted up]
│ John         │
│ john@...     │
└──────────────┘
   (transform: translateY(-2px))
```

### Stat Cards on Profile (Hover)
```
Before:
┌──────────┐
│  42      │  [Dark background]
│ Posts    │
└──────────┘

After hover:
┌──────────┐
│  42      │  [Brighter background, lifted]
│ Posts    │
└──────────┘
```

### Buttons
```
Primary Button (Green):
┌─────────────────┐
│ ✎ Edit Profile  │  [Gradient: #50fa7b to #39da8a]
└─────────────────┘
Hover: Lift up with shadow

Secondary Button (Blue):
┌─────────────────┐
│ ← Back to Feed  │  [Transparent blue border]
└─────────────────┘
Hover: Fill with blue background
```

---

## Data Displayed

### Sidebar User Card
```
┌──────────────────┐
│ ┌──────────────┐ │
│ │ [Placeholder]│ │  ← Avatar (50x50)
│ │   📋 User    │ │     If user has ProfilePictureUrl, 
│ │              │ │     shows actual image instead
│ └──────────────┘ │
│                  │
│ John Doe         │  ← DisplayName from JWT claim
│ john@example.com │  ← Email from JWT claim
└──────────────────┘
```

### Profile Page
```
Avatar:           [140x140px image or placeholder]
Display Name:     John Doe (from user.DisplayName)
Email:            john@example.com (from user.Email)
Region:           San Francisco, CA (from user.Region)
Bio:              "Software developer..." (from user.Bio)

Stats:
- Posts:          42 (count of user.Posts)
- Followers:      150 (count of user.Followers)
- Following:      87 (count of user.Following)

Member Since:     April 14, 2024 (from user.CreatedAt)
```

---

## How to Test

### Test 1: Login
1. Navigate to `http://localhost:5050/Account/Login`
2. Enter any test account details
3. You should see the sidebar with your DisplayName

### Test 2: View Profile
1. In sidebar, look for your DisplayName at the bottom
2. Click on it
3. Navigation to `/Account/Profile`
4. See your profile with all stats

### Test 3: Logout
1. Click red logout button in sidebar
2. You should be redirected to home
3. Sidebar should now show Login/Register links

### Test 4: Mobile
1. Open Chrome DevTools (F12)
2. Toggle device toolbar (Cmd+Shift+M)
3. Set width to 375px
4. Sidebar should become horizontal

---

## CSS Animations

### Smooth Transitions
```css
transition: all 0.3s ease;
```
Applied to:
- Sidebar items on hover
- User profile card on hover
- Stat cards on hover
- Button states
- Color changes

### Transform Effects
```css
transform: translateX(4px);    /* Nav items - slide right */
transform: translateY(-2px);   /* Cards on hover - lift up */
```

### Box Shadows
```css
/* Sidebar shadow */
box-shadow: 2px 0 15px rgba(0, 0, 0, 0.3);

/* Profile container */
box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);

/* Stat cards on hover */
box-shadow: 0 4px 12px rgba(80, 250, 123, 0.3);
```

---

## Responsive Breakpoints

### Desktop (> 768px)
```
┌─────────────────────────────┐
│ [Sidebar] [Main Content]    │
│  280px        ~1000px       │
└─────────────────────────────┘
```

### Tablet (768px)
```
┌──────────────────────────────┐
│ [Sidebar] [Main Content]     │
│  ~240px      ~530px          │
└──────────────────────────────┘
```

### Mobile (< 768px)
```
┌──────────────────────────┐
│ [Horizontal Sidebar]     │
├──────────────────────────┤
│   [Full Width Content]   │
└──────────────────────────┘
```

---

## Color Palette Reference

### Dark Theme
- Base: `#1a1a2e` - Sidebar background
- Surface: `#16213e` - Cards, modals
- Background: `#0f3460` - Page background
- Border: `rgba(98, 114, 164, 0.3)` - Subtle borders

### Accents
- Success: `#50fa7b` - Buttons, positive action
- Info: `#8be9fd` - Links, secondary text
- Danger: `#ff5555` - Logout, warning

### Text
- Primary: `#f8f8f2` - Main text (98% white)
- Secondary: `#d0d0d0` - Secondary text (82% white)
- Muted: `#b0b0b0` - Hints (69% white)

---

## Summary

The UI is:
- ✅ **Dark themed** with gaming aesthetics
- ✅ **Responsive** on all devices
- ✅ **Interactive** with smooth animations
- ✅ **Accessible** with clear contrast
- ✅ **Professional** with attention to detail
- ✅ **User-friendly** with one-click profile access

Users see their **DisplayName** prominently displayed and can access their profile with a single click!
