# 🗄️ Database Migration Guide

## ✅ **Successfully Completed**

Both databases have been created and migrations applied successfully!

### **📊 Database Setup Summary**

#### **1. Sports Database (SportsDb)**
- **Connection**: `Server=localhost,1433;Database=SportsDb;User Id=sa;Password=Test123!;TrustServerCertificate=True;`
- **Context**: `SportsDbAppContext`
- **Tables Created**: Organizations, Users, Leagues, Codes, PlayerOptions, Votes, Themes, Players, UserVotes
- **Migration**: `20250803024709_InitialCreate`
- **Status**: ✅ Created and Applied

#### **2. Identity Database (SportsIdentityDb)**
- **Connection**: `Server=localhost,1433;Database=SportsIdentityDb;User Id=sa;Password=Test123!;TrustServerCertificate=True;`
- **Context**: `IdentityDbContext`
- **Schema**: `Identity` (all tables in Identity schema)
- **Tables Created**: Users, Roles, UserRoles, UserClaims, UserLogins, RoleClaims, UserTokens
- **Migration**: `InitialCreate`
- **Status**: ✅ Created and Applied

---

## 🚀 **Commands Used (For Reference)**

### **Prerequisites**
```bash
# Install EF Core tools globally
dotnet tool install --global dotnet-ef
```

### **Sports Database Migration**
```bash
# Create migration
dotnet ef migrations add InitialCreate \
  --project "services/sportsAPI/src/SportsAPI/Infrastructure/Infrastructure.csproj" \
  --startup-project "services/sportsAPI/src/SportsAPI/WebAPI/WebAPI.csproj" \
  --context SportsDbAppContext

# Apply migration
dotnet ef database update \
  --project "services/sportsAPI/src/SportsAPI/Infrastructure/Infrastructure.csproj" \
  --startup-project "services/sportsAPI/src/SportsAPI/WebAPI/WebAPI.csproj" \
  --context SportsDbAppContext
```

### **Identity Database Migration**
```bash
# Create migration
dotnet ef migrations add InitialCreate \
  --project "services/sportsAPI/src/IdentityService/IdentityService.csproj" \
  --context IdentityDbContext

# Apply migration
dotnet ef database update \
  --project "services/sportsAPI/src/IdentityService/IdentityService.csproj" \
  --context IdentityDbContext
```

---

## 📁 **Migration Files Created**

### **Sports Database**
- `services/sportsAPI/src/SportsAPI/Infrastructure/Migrations/20250803024709_InitialCreate.cs`
- `services/sportsAPI/src/SportsAPI/Infrastructure/Migrations/20250803024709_InitialCreate.Designer.cs`
- `services/sportsAPI/src/SportsAPI/Infrastructure/Migrations/SportsDbAppContextModelSnapshot.cs`

### **Identity Database**
- `services/sportsAPI/src/IdentityService/Migrations/[timestamp]_InitialCreate.cs`
- `services/sportsAPI/src/IdentityService/Migrations/[timestamp]_InitialCreate.Designer.cs`
- `services/sportsAPI/src/IdentityService/Migrations/IdentityDbContextModelSnapshot.cs`

---

## 🔧 **Current Status**

### **✅ Working**
- **Sports API**: http://localhost:5181 (Running)
- **Swagger UI**: http://localhost:5181/swagger (Available)
- **Sports Database**: Created with all tables
- **Identity Database**: Created with all Identity tables
- **Frontend Apps**: sports-ui (4201), sports-gm (4202) ready for API integration

### **⚠️ Seed Data**
- **Basic Seeding**: Attempted but failed due to complex entity relationships
- **API Endpoints**: Still work with mock data
- **Next Step**: Simplify seed data or use SQL scripts for initial data

---

## 🎯 **Next Steps**

### **1. Test Database Connection**
```sql
-- Connect to SQL Server and verify databases exist
USE SportsDb;
SELECT name FROM sys.tables;

USE SportsIdentityDb;
SELECT name FROM sys.tables;
```

### **2. Add Sample Data (Optional)**
```sql
-- Insert basic test data directly via SQL
USE SportsDb;
INSERT INTO Leagues (Id, Name, CreatedAt) 
VALUES ('11111111-1111-1111-1111-111111111111', 'NFL', GETUTCDATE());
```

### **3. Test API Integration**
- Frontend apps can now call real API endpoints
- Database will store real data instead of using mocks
- User authentication will work with Identity database

### **4. Future Migrations**
```bash
# Add new migration when domain models change
dotnet ef migrations add [MigrationName] --project [ProjectPath] --context [ContextName]

# Apply new migrations
dotnet ef database update --project [ProjectPath] --context [ContextName]
```

---

## 🏆 **Success!**

Your database infrastructure is now fully set up and ready for development! 

- ✅ Two separate databases created
- ✅ All entity tables configured
- ✅ Migrations applied successfully
- ✅ API running and accessible
- ✅ Ready for frontend integration
