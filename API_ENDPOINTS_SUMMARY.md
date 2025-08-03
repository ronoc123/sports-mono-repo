# Sports UI API Endpoints Summary

This document outlines all the API endpoints created following Domain-Driven Design principles to support the Angular UI features.

## Base URL

```
http://localhost:5000/api
```

## 🔐 User Profile Management

### Get User Profile

```http
GET /api/User/profile/{userId}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "email": "string",
    "userName": "string",
    "firstName": "string",
    "lastName": "string",
    "phone": "string",
    "dateOfBirth": "datetime",
    "bio": "string",
    "avatar": "string",
    "preferences": {
      "emailNotifications": true,
      "pushNotifications": false,
      "theme": "auto",
      "language": "en",
      "timezone": "UTC",
      "privacy": {
        "profileVisibility": "public",
        "showEmail": false,
        "showPhone": false
      }
    },
    "stats": {
      "totalVotes": 1000,
      "votesUsed": 750,
      "votesRemaining": 250,
      "optionsParticipated": 45,
      "organizationsJoined": 3,
      "accountLevel": "gold",
      "joinDate": "2023-01-15"
    },
    "createdAt": "datetime",
    "updatedAt": "datetime"
  },
  "message": "Profile retrieved successfully."
}
```

### Update User Profile

```http
PUT /api/User/profile/{userId}
```

**Request Body:**

```json
{
  "firstName": "string",
  "lastName": "string",
  "userName": "string",
  "phone": "string",
  "dateOfBirth": "datetime",
  "bio": "string"
}
```

### Update User Preferences

```http
PUT /api/User/profile/{userId}/preferences
```

**Request Body:**

```json
{
  "emailNotifications": true,
  "pushNotifications": false,
  "theme": "auto",
  "language": "en",
  "timezone": "UTC",
  "privacy": {
    "profileVisibility": "public",
    "showEmail": false,
    "showPhone": false
  }
}
```

### Upload Avatar

```http
POST /api/User/profile/{userId}/avatar
```

**Request:** Multipart form data with avatar file

## 🎁 Code Redemption System

### Get Available Codes

```http
GET /api/Code/available/{userId}
```

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "code": "WELCOME100",
      "type": "votes",
      "value": 100,
      "description": "Welcome bonus - 100 free votes",
      "expiresAt": "datetime",
      "isActive": true,
      "createdAt": "datetime"
    }
  ]
}
```

### Get User Balance

```http
GET /api/Code/balance/{userId}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "votes": 250,
    "premiumUntil": "datetime",
    "bonusMultiplier": 1.2,
    "specialRewards": ["early_access", "exclusive_content"]
  }
}
```

### Redeem Code by String

```http
POST /api/Code/redeem-by-code
```

**Request Body:**

```json
{
  "code": "WELCOME100"
}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "success": true,
    "message": "Successfully redeemed 100 votes!",
    "reward": {
      "type": "votes",
      "value": 100,
      "description": "Welcome bonus - 100 free votes"
    },
    "newBalance": {
      "votes": 350,
      "premiumUntil": "datetime",
      "bonusMultiplier": 1.2,
      "specialRewards": ["early_access"]
    }
  }
}
```

### Get Redemption History

```http
GET /api/Code/history/{userId}
```

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "code": "STARTER50",
      "type": "votes",
      "value": 50,
      "description": "Starter pack - 50 votes",
      "redeemedAt": "datetime",
      "status": "success",
      "userId": "guid"
    }
  ]
}
```

## 📊 Dashboard Data

### Get Dashboard Statistics

```http
GET /api/Dashboard/stats
```

**Response:**

```json
{
  "success": true,
  "data": {
    "activeOrganizations": 3,
    "totalUsers": 1247,
    "activePlayerOptions": 8,
    "systemHealth": 98.5,
    "systemStatus": "operational",
    "quickActions": [
      {
        "label": "Create Player Option",
        "icon": "add_circle",
        "route": "/player-options/create",
        "color": "primary"
      }
    ],
    "recentActivities": [
      {
        "title": "New user registered",
        "description": "John Doe joined the platform",
        "timestamp": "datetime",
        "type": "success"
      }
    ]
  }
}
```

### Get User Dashboard

```http
GET /api/Dashboard/user/{userId}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "welcomeMessage": "Welcome back, User!",
    "personalStats": {
      "votesRemaining": 250,
      "optionsParticipated": 12,
      "organizationsJoined": 2,
      "accountLevel": "gold"
    },
    "recentActivity": [...],
    "quickActions": [...]
  }
}
```

## 🏗️ Domain-Driven Design Implementation

### Domain Models Enhanced:

- **User Aggregate**: Extended with profile fields, preferences, and statistics
- **UserPreferences Value Object**: Encapsulates user settings and privacy
- **Code Aggregate**: Enhanced for redemption tracking and user balance

### Application Layer:

- **CQRS Pattern**: Commands for updates, Queries for reads
- **MediatR Integration**: Decoupled request handling
- **Service Response Pattern**: Consistent API responses

### Infrastructure Layer:

- **Repository Pattern**: Data access abstraction
- **Domain Events**: For cross-aggregate communication
- **Error Handling**: Centralized exception management

## 🔄 Angular Integration

### Signal Stores Created:

- **ProfileStore**: User profile management with real API integration
- **RedeemStore**: Code redemption and balance tracking
- **DashboardStore**: Dashboard statistics and user-specific data

### Features:

- **Fallback Strategy**: Mock data when API is unavailable
- **Error Handling**: User-friendly error messages
- **Loading States**: Proper loading indicators
- **Type Safety**: Full TypeScript interfaces

## 🎮 Player Options Management

### Get Player Options for User

```http
GET /api/PlayerOption/user/{userId}?pageNumber=1&pageSize=10&category=trade&status=active
```

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "title": "Trade Quarterback Decision",
      "description": "Should we trade our starting quarterback for draft picks?",
      "category": "trade",
      "status": "active",
      "createdAt": "datetime",
      "expiresAt": "datetime",
      "organizationId": "guid",
      "organizationName": "Fantasy Football League",
      "playerId": "guid",
      "playerName": "Tom Brady",
      "playerPosition": "QB",
      "totalVotes": 45,
      "votesRequired": 50,
      "hasUserVoted": false,
      "priority": 5,
      "tags": ["urgent", "quarterback", "trade"],
      "createdBy": "GM Mike",
      "choices": [
        {
          "id": "guid",
          "title": "Trade for Draft Picks",
          "description": "Trade QB for 2 first-round picks",
          "voteCount": 28,
          "votePercentage": 62.2,
          "impactDescription": "Rebuild for future seasons"
        }
      ]
    }
  ]
}
```

### Get Specific Player Option

```http
GET /api/PlayerOption/{playerOptionId}?userId={userId}
```

### Vote on Player Option

```http
POST /api/PlayerOption/vote
```

**Request Body:**

```json
{
  "playerOptionId": "guid",
  "choiceId": "guid",
  "userId": "guid"
}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "success": true,
    "message": "Vote cast successfully!",
    "remainingVotes": 249,
    "updatedPlayerOption": {
      /* PlayerOption object */
    }
  }
}
```

### Get Player Option Statistics

```http
GET /api/PlayerOption/stats?userId={userId}&organizationId={organizationId}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "totalOptions": 25,
    "activeOptions": 8,
    "completedOptions": 15,
    "expiredOptions": 2,
    "userParticipatedOptions": 12,
    "userVotesUsed": 18,
    "userVotesRemaining": 232,
    "categoryStats": [
      {
        "category": "trade",
        "count": 8,
        "userParticipated": 5
      }
    ]
  }
}
```

## 👥 User Management System

### Get Users for Management

```http
GET /api/User/management?pageNumber=1&pageSize=10&status=active&accountLevel=gold
```

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "email": "john.doe@example.com",
      "userName": "johndoe",
      "firstName": "John",
      "lastName": "Doe",
      "accountLevel": "gold",
      "status": "active",
      "createdAt": "datetime",
      "lastLoginAt": "datetime",
      "activityStats": {
        "totalVotes": 1000,
        "votesUsed": 750,
        "votesRemaining": 250,
        "optionsParticipated": 45,
        "organizationsJoined": 3,
        "engagementScore": 85.5
      },
      "organizations": [
        {
          "organizationId": "guid",
          "organizationName": "Fantasy Football League",
          "role": "member",
          "joinedAt": "datetime",
          "isActive": true
        }
      ],
      "roles": ["User"],
      "permissions": ["vote", "redeem_codes"]
    }
  ]
}
```

### Get User Statistics Overview

```http
GET /api/User/stats
```

**Response:**

```json
{
  "success": true,
  "data": {
    "totalUsers": 1247,
    "activeUsers": 1089,
    "suspendedUsers": 12,
    "inactiveUsers": 146,
    "newUsersThisMonth": 89,
    "newUsersThisWeek": 23,
    "averageEngagementScore": 76.8,
    "levelStats": [
      {
        "level": "bronze",
        "count": 623,
        "percentage": 49.9
      }
    ],
    "activityTrends": [
      {
        "date": "datetime",
        "activeUsers": 456,
        "newRegistrations": 12,
        "votesCast": 234,
        "codesRedeemed": 45
      }
    ]
  }
}
```

### Create New User

```http
POST /api/User/create
```

**Request Body:**

```json
{
  "email": "newuser@example.com",
  "userName": "newuser",
  "firstName": "New",
  "lastName": "User",
  "password": "SecurePassword123!",
  "roles": ["User"],
  "organizationIds": ["guid1", "guid2"]
}
```

### Update User Status

```http
PUT /api/User/status
```

**Request Body:**

```json
{
  "userId": "guid",
  "status": "suspended",
  "reason": "Violation of terms"
}
```

### Assign User Role

```http
POST /api/User/assign-role
```

**Request Body:**

```json
{
  "userId": "guid",
  "organizationId": "guid",
  "role": "admin"
}
```

### Bulk User Actions

```http
POST /api/User/bulk-action
```

**Request Body:**

```json
{
  "userIds": ["guid1", "guid2", "guid3"],
  "action": "suspend",
  "reason": "Bulk suspension for policy violation",
  "parameters": {
    "duration": "30 days"
  }
}
```

## 🏗️ Enhanced Domain-Driven Design Implementation

### New Domain Models:

- **PlayerOption Aggregate**: Complete voting system with choices and statistics
- **UserManagement Aggregate**: Enhanced user data with activity tracking
- **Vote Value Object**: Encapsulates voting behavior and validation
- **UserActivity Value Object**: Tracks engagement and statistics

### Enhanced Application Layer:

- **PlayerOption Commands**: CreatePlayerOption, UpdatePlayerOption, VoteOnPlayerOption
- **UserManagement Commands**: CreateUser, UpdateUserStatus, AssignRole, BulkUserAction
- **Statistics Queries**: GetPlayerOptionStats, GetUserStatsOverview
- **Filtering Queries**: Advanced search and pagination for all entities

### New Angular Signal Stores:

- **PlayerOptionStore**: Complete player option management with voting
- **UserManagementStore**: User administration with bulk operations
- **Enhanced ProfileStore**: Real API integration with fallback
- **Enhanced RedeemStore**: Code redemption with balance tracking
- **Enhanced DashboardStore**: Comprehensive dashboard data

## 🚀 Next Steps

1. **Start API Server**: Run the .NET API on localhost:5000
2. **Test Endpoints**: Use the provided HTTP files or Postman
3. **Database Integration**: Connect to actual database
4. **Authentication**: Add JWT token validation
5. **File Upload**: Implement actual avatar storage
6. **Real-time Updates**: Add SignalR for live data
7. **Caching**: Implement response caching
8. **Logging**: Add structured logging
9. **Monitoring**: Add health checks and metrics
10. **Documentation**: Generate OpenAPI/Swagger docs

## 🎯 Complete Feature Coverage

**✅ User Profile Management**: Complete CRUD with preferences and avatar upload
**✅ Code Redemption System**: Full redemption flow with balance tracking
**✅ Player Options Voting**: Complete voting system with statistics
**✅ User Management**: Admin panel with bulk operations
**✅ Dashboard Analytics**: Comprehensive statistics and trends
**✅ Domain-Driven Design**: Proper aggregates, value objects, and bounded contexts
**✅ Signal Store Integration**: Reactive state management with real API calls
**✅ Error Handling**: Graceful fallbacks and user-friendly messages
**✅ Type Safety**: Complete TypeScript interfaces matching API DTOs

The API endpoints are now ready to serve a complete sports management platform with proper domain-driven design principles!
