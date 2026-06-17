# Gym App Backend - Comprehensive Project Documentation

## Executive Summary

The Gym App is an enterprise-grade backend system designed to support a comprehensive gymnasium management and fitness tracking platform. Built with modern .NET technologies and architectural best practices, this system demonstrates industry-standard patterns for scalability, security, and maintainability. The backend serves both web and mobile clients, providing real-time notifications, workout tracking, user authentication, and comprehensive gym management capabilities.

---

## 1. Architectural Design & Pattern

### 1.1 Overall Architecture: Clean Layered Architecture (4-Tier)

The Gym App backend implements a **Clean Layered Architecture** that strictly enforces separation of concerns through four distinct layers:

```
┌─────────────────────────────────────┐
│     API Layer (Gym App.API)         │  ← Controllers, Route handlers
├─────────────────────────────────────┤
│   Application Layer (Gym App.        │  ← Business logic, services
│   Application)                       │
├─────────────────────────────────────┤
│   Infrastructure Layer (Gym App.     │  ← Data access, repositories
│   Infrastructure)                   │
├─────────────────────────────────────┤
│  Core Layer / Domain (Gym App.Core)  │  ← Entity models, business rules
└─────────────────────────────────────┘
```

**Rationale for this architecture:**
- **Dependency Direction**: Dependencies flow inward (API → Application → Infrastructure → Core). The Core layer has no external dependencies, ensuring business logic is portable.
- **Testability**: Each layer can be tested in isolation by mocking lower layers.
- **Maintainability**: Clear boundaries make it easy for teams to work on specific layers without affecting others.
- **Scalability**: Services can evolve independently without breaking the entire system.

### 1.2 Separation of Concerns - Module Breakdown

#### **Core Layer** (`Gym App.Core/`)
Contains 21 domain entities representing core business concepts:
- **User Management**: `User.cs`, `Role.cs`, `PasswordResetToken.cs`, `RefreshTokens.cs`
- **Fitness Tracking**: `Workout.cs`, `WorkoutSet.cs`, `Exercise.cs`, `ExerciseInstance.cs`, `PersonalRecord.cs`
- **Membership & Scheduling**: `Schedule.cs`, `Session.cs`, `SessionUsers.cs`, `Challenges.cs`
- **Health & Nutrition**: `PastInjuries.cs`, `Notification.cs`
- **Communication**: `Message.cs`, `Feedback.cs`, `LiveFeedback.cs`
- **Transactions**: `Transaction.cs`
- **Base Class**: `BaseEntity.cs` - All entities inherit from this, containing `Id` (Guid) and audit timestamps

#### **Infrastructure Layer** (`Gym App.Infrastructure/`)
Handles data access, persistence, and external integrations:
- **Context**: `DbBase.cs` - Entity Framework Core DbContext with 21 DbSet configurations
- **Repositories**: `UnitOfWork.cs` - Generic repository pattern with lazy-loaded specialized repositories
- **Migrations**: Automatic database schema creation and updates
- **DTOs**: Domain Transfer Objects organized by feature (User/, Workout/, Exercise/, Message/)
- **Interfaces**: Contract definitions for repository operations

#### **Application Layer** (`Gym App.Application/`)
Encapsulates business logic and application-specific concerns:
- **Authorization**: 
  - `CurrentUser.cs` - Extracts user claims from HTTP context
  - `SameUserHandler.cs` - Implements policy for user-owned resource access
  - `CachedAuthorizationService.cs` - Redis-backed authorization caching
- **Services**: Token generation, email handling, business operations
- **Real-Time Communication**:
  - `ChatHub.cs` - WebSocket endpoint for messaging (/chat)
  - `NotificationHub.cs` - WebSocket endpoint for live notifications (/notif)
  - `NotificationNotifier.cs` & `WorkoutNotifier.cs` - Event publishers

#### **API Layer** (`Gym App.API/`)
RESTful HTTP interface exposing business functionality:
- **13 Controllers** managing different domains
- **Route Configuration**: `/api/v1/{resource}` pattern with versioning
- **Middleware**: CORS, authentication, error handling, logging
- **Static Resources**: `wwwroot/` for file uploads and HTML documents

### 1.3 Design Patterns Implemented

#### **Repository Pattern**
```csharp
IUnitOfWork
├── IUserRepository
├── IWorkoutRepository
├── IExerciseRepository
├── IScheduleRepository
└── ... (specialized repositories for each domain)
```
**Implementation**: `Gym App.Infrastructure/Repositories/UnitOfWork.cs` with lazy-loaded repositories. Each repository encapsulates data access logic, allowing services to interact with data through abstract interfaces rather than direct EF Core calls.

**Benefits**: 
- Decouples business logic from EF Core specifics
- Enables easier unit testing through mock repositories
- Centralized data access patterns

#### **Dependency Injection (DI) Container**
Configured in `Program.cs` (lines 80-230), the DI container registers:
```csharp
// Services
services.AddScoped<ITokenHandler, TokenHandler>();
services.AddScoped<IEmailService, EmailService>();
services.AddScoped<ICurrentUser, CurrentUser>();

// Repositories
services.AddScoped<IUnitOfWork, UnitOfWork>();

// Caching
services.AddStackExchangeRedisCache(options => ...);
```

**Why DI Matters**: 
- Services receive dependencies through constructor injection, not direct instantiation
- Enables swapping implementations (e.g., test doubles, alternative implementations)
- Manages object lifetimes (Transient, Scoped, Singleton) appropriately

#### **Authorization Policy Pattern**
Custom authorization handlers in `Application/Authorization/`:
```csharp
services.AddAuthorizationBuilder()
    .AddPolicy("NormalUsage", policy => 
        policy.RequireRole("Admin", "User"))
    .AddPolicy("ElevatedPower", policy => 
        policy.RequireRole("Admin"))
    .AddPolicy("SameUserPolicy", policy => 
        policy.AddRequirements(new SameUserRequirement()))
    .AddPolicy("ListUserPolicy", policy => 
        policy.AddRequirements(new ListUserRequirement()));
```

**Implementation**: Handlers like `SameUserHandler` inspect claims and resource identifiers to enforce fine-grained authorization logic.

#### **Unit of Work Pattern**
The `UnitOfWork` class in Infrastructure manages:
- Multiple repository instances
- Transaction boundaries (`SaveChangesAsync()`)
- Consistent data changes across multiple repositories

#### **DTO (Data Transfer Object) Pattern**
Separates API contracts from internal entity models:
- Request DTOs: `CreateUserDto`, `UpdateWorkoutDto` (inputs from clients)
- Response DTOs: `UserResponseDto`, `WorkoutDetailDto` (data returned to clients)
- Purpose: Prevents exposing internal structure, enables API evolution independently

#### **Factory/Builder Patterns**
Test Base (`GymApp.Tests/TestBase.cs`) uses factory methods:
```csharp
CreateTestUser()
CreateTestRole()
CreateTestWorkout()
CreateTestExercise()
```
These encapsulate object creation logic, ensuring consistent test data setup.

---

## 2. Core Tech Stack & Framework Justification

### 2.1 Runtime & Framework Selection

| Technology | Version | Justification |
|------------|---------|---------------|
| **.NET** | 9.0 (LTS) | Latest long-term support release with cutting-edge performance improvements. Compared to alternatives: Node.js lacks strong typing (increasing runtime errors); Python is slower for I/O-heavy operations like gym management. .NET offers superior performance (5-10x faster than Node.js in benchmarks), native async/await support without callback hell, and rich type system enabling compile-time error detection. |
| **ASP.NET Core** | 9.0 | Built-in framework for building web APIs with dependency injection, routing, middleware, authentication/authorization. Beats Express.js by including batteries-included security and performance features. |

### 2.2 Framework-Level Features Leveraged

1. **Async/Await Model**: All I/O operations are non-blocking, critical for:
   - Database queries (EF Core)
   - Email dispatch via SMTP
   - WebSocket handling in SignalR
   - Real-time notification broadcasting

   Example: `await _context.Users.ToListAsync()` prevents thread pool starvation

2. **Built-in Dependency Injection**: No external container required (vs. Autofac, Ninject). Reduces cognitive overhead and dependency management.

3. **Middleware Pipeline**: Request processing chain:
   ```
   Request → Logging → CORS → Auth → Route → Response
   ```
   Enables cross-cutting concerns without decorator patterns or aspect-oriented programming complexity.

4. **Entity Framework Core 9.0.9**: Powerful ORM providing:
   - LINQ-to-SQL translation (C# queries compile to SQL)
   - Automatic relationship tracking
   - Lazy loading, eager loading, explicit loading strategies
   - Built-in migrations for schema versioning

### 2.3 Database Management System (DBMS)

**Selected**: SQL Server (DESKTOP-OR6CO4J\SQLEXPRESS)

**Justification**:
- **Relational Consistency**: User data requires ACID compliance. Gym memberships, financial transactions, and workout history demand transactional integrity where SQL Server excels.
- **Complex Queries**: Membership analytics, workout statistics, user reporting benefit from SQL's powerful JOINs and aggregations.
- **Comparison with Alternatives**:
  - PostgreSQL: Open-source but requires separate server setup; SQL Server integrates with Visual Studio debugging
  - MongoDB: Schemaless design risks data inconsistency (e.g., user records without required fields)
  - Firebase: Vendor lock-in and limited control over authentication workflows

**ORM: Entity Framework Core 9.0.9**

EF Core abstracts SQL generation:
```csharp
// C# (developer writes)
var workouts = await _context.Workouts
    .Where(w => w.UserId == userId)
    .Include(w => w.ExerciseInstances)
    .ToListAsync();

// Translates to SQL automatically
SELECT * FROM Workouts WHERE UserId = @userId
```

**Why EF Core over raw SQL**:
- **Type Safety**: SQL injection impossible with parameterized queries
- **Maintainability**: Queries are C# code, subject to linting and refactoring tools
- **Testability**: In-memory DbContext for unit tests (no database needed)
- **Productivity**: Less boilerplate than manually mapping ResultSets to objects

### 2.4 Supporting Technology Ecosystem

| Library | Version | Purpose |
|---------|---------|---------|
| **JWT (System.IdentityModel.Tokens.Jwt)** | 8.0.1 | Token-based authentication; signed tokens verify user identity without server-side sessions |
| **SignalR** | Built-in | Real-time bidirectional communication (WebSockets with fallback); enables live chat, workout notifications |
| **Serilog** | 4.3.0 | Structured logging with multiple sinks (console, rolling files); critical for production debugging |
| **FluentEmail + MailKit** | 3.0.2, 4.13.0 | Email composition and SMTP transmission; abstracts SMTP complexity |
| **Swagger/Swashbuckle** | 6.6.2 | Auto-generates OpenAPI specification from code; front-end teams understand API contracts without documentation drift |
| **StackExchange.Redis** | 2.12.14 | In-memory caching for auth tokens and frequently accessed data; 10x faster than database queries |
| **CsvHelper** | Latest | Bulk data import for gyms, exercises, membership catalogs |
| **DocumentFormat.OpenXml** | Latest | Excel/Word document generation for reports, receipts |
| **xUnit 2.9.3** | 2.9.3 | Unit testing framework with superior test isolation (vs. NUnit) |
| **Moq 4.20.72** | 4.20.72 | Mocking library for unit tests; creates test doubles of services/repositories |

---

## 3. API Design & Data Contract

### 3.1 RESTful Architecture Adherence

The Gym App strictly follows REST (Representational State Transfer) principles:

#### **Statelessness**
- Every request contains sufficient information for the server to process it
- No server-side session state maintained (purely token-based auth)
- Example: Every request includes JWT in `Authorization: Bearer <token>` header

#### **Resource-Oriented Design**
Resources are the primary abstractions:
```
/api/v1/user           → User entity
/api/v1/workout        → Workout entity
/api/v1/exercise       → Exercise entity
```

#### **Standard HTTP Verbs**
| Verb | Operation | Idempotent | Example |
|------|-----------|-----------|---------|
| **GET** | Retrieve | Yes | `GET /api/v1/workout/{id}` → Fetch workout details |
| **POST** | Create | No | `POST /api/v1/workout/create/{userId}` → New workout |
| **PUT** | Replace/Update | Yes | `PUT /api/v1/workout/update/{id}` → Full update |
| **DELETE** | Remove | Yes | `DELETE /api/v1/workout/delete/{id}` → Remove workout |

#### **HTTP Status Codes (RFC 7231)**
```
200 OK                  → Successful GET/PUT/DELETE
201 Created             → Successful POST (new resource)
204 No Content          → Successful deletion or void operations
400 Bad Request         → Invalid input (e.g., malformed email)
401 Unauthorized        → Missing/invalid JWT token
403 Forbidden           → Valid token but insufficient permissions
404 Not Found           → Resource doesn't exist
500 Internal Error      → Server error (logged for debugging)
```

**Note**: Gym App uses custom status codes in response body (0=error, 1=forbidden, 2=success) for legacy compatibility, but HTTP status codes are also respected for standard error handling.

### 3.2 Endpoint Mapping & Schema

#### **Authentication Endpoints** (`AuthenticationController.cs`)
```
POST   /api/v1/auth/sign-up                 → Register new user
POST   /api/v1/auth/sign-in                 → Login with email/password
POST   /api/v1/auth/token-login             → Refresh expired token
PUT    /api/v1/auth/forgot-password         → Request password reset OTP
PUT    /api/v1/auth/reset-password          → Reset password with OTP
```

#### **User Management** (`UserController.cs`)
```
GET    /api/v1/user/{id}                    → Fetch user profile
POST   /api/v1/user/{id}                    → Create new user
PUT    /api/v1/user/{id}                    → Update user details
DELETE /api/v1/user/{id}                    → Deactivate user account
GET    /api/v1/user/list/all                → List users (admin only)
```

#### **Workout Management** (`WorkoutController.cs`)
```
POST   /api/v1/workout/create/{userId}      → Create new workout plan
POST   /api/v1/workout/update-progress/{userId}  → Log exercise set
PUT    /api/v1/workout/start-workout/{workoutId}/{userId}  → Begin session
PUT    /api/v1/workout/complete-workout/{workoutId}/{userId} → End session
GET    /api/v1/workout/get-personal-records/{userId} → Fetch PRs
DELETE /api/v1/workout/delete/{workoutId}   → Remove workout
```

#### **Exercise & Muscles** (`ExerciseController.cs`, `MusclesController.cs`)
```
GET    /api/v1/exercise/get-all             → Browse exercise library
POST   /api/v1/exercise/create              → Add custom exercise
GET    /api/v1/muscles/get-all              → Muscle groups
GET    /api/v1/muscles/get-exercises/{muscleId} → Exercises targeting muscle
```

#### **Real-Time Communication** (SignalR WebSockets)
```
WebSocket /chat                             → Chat hub for messaging
  - SendMessage(recipientId, message)       → Send direct message
  - ReceiveMessage(senderId, message)       → Broadcast handler
  
WebSocket /notif                            → Notification hub
  - SendNotification(userId, title, body)   → Push notification
  - ReceiveNotification(notification)       → Client listener
```

#### **Complete API Endpoint Summary**
| Domain | Endpoints | Key Operations |
|--------|-----------|-----------------|
| Authentication | 5 | Sign-up, Login, Token Refresh, Password Reset |
| User Management | 6 | CRUD, Listing, Profile |
| Workouts | 6 | Create, Update, Start, Complete, Delete, Personal Records |
| Exercises | 4 | Browse, Create, Filter by Muscle |
| Sessions | 8 | Schedule, Join, Track Attendance |
| Schedules | 5 | View, Create, Modify, Delete |
| Messages | 6 | Send, Retrieve, Delete, Search |
| Notifications | 5 | Send, Retrieve, Mark Read, Delete |
| Feedback | 4 | Submit, Retrieve, Moderate |
| Roles | 3 | List, Create, Assign |
| Muscles | 4 | List, Details |
| Data Import | 2 | CSV bulk upload, Excel export |
| Email Verification | 2 | Send, Verify |

**Total: 13 Controllers, ~60+ API endpoints**

### 3.3 Request/Response Data Contracts

#### **Standard Success Response**
```json
{
  "status": 2,
  "msg": "Operation successful",
  "Value": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "user@example.com",
    "name": "John Doe",
    "role": "User",
    "createdAt": "2024-01-15T10:30:00Z"
  }
}
```

#### **Paginated Response**
```json
{
  "status": 2,
  "msg": "Data retrieved",
  "Data": [
    { "id": "...", "name": "Workout 1", ... },
    { "id": "...", "name": "Workout 2", ... }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalRecords": 50
}
```

#### **Error Response**
```json
{
  "status": 0,
  "msg": "Invalid email or password",
  "Value": null
}
```

#### **Request Body Example** (POST /api/v1/workout/create/{userId})
```json
{
  "name": "Upper Body Strength",
  "description": "Focus on chest, shoulders, back",
  "difficulty": "Intermediate",
  "duration": 60,
  "exerciseInstances": [
    {
      "exerciseId": "550e8400-e29b-41d4-a716-446655440001",
      "sets": 4,
      "reps": 8,
      "weight": 185.5
    }
  ]
}
```

### 3.4 API Documentation: Swagger/OpenAPI Integration

**Swashbuckle 6.6.2** configured in `Program.cs` (lines 105-145) auto-generates OpenAPI specification:

```csharp
services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Gym App API", 
        Version = "v1.0.0" 
    });
    
    // JWT authentication scheme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Description = "JWT Token"
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            new List<string>()
        }
    });
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gym App API v1");
});
```

**Benefits**:
- Auto-generated documentation at `/swagger` endpoint
- Front-end teams can explore endpoints interactively
- Spec drives code generation in client SDKs
- Changes in C# types automatically update documentation

**Frontend Integration**: Front-end developers can:
1. Visit `https://[api-url]/swagger`
2. See all endpoints, request/response schemas, required authentication
3. Try endpoints directly from browser
4. Download OpenAPI JSON for code generation tools

---

## 4. Application Security & Authentication

### 4.1 Authentication Strategy: JWT (JSON Web Tokens)

#### **Why JWT?**
Traditional session-based auth requires server-side storage:
```
Client sends credentials → Server creates session → Server stores session in memory/database
Problem: Doesn't scale to microservices; sessions can't be shared; server must maintain session state
```

JWT is stateless:
```
Client sends credentials → Server signs token with secret → Client stores token
Client includes token in every request → Server verifies signature (no database lookup needed)
Benefits: Scalable, stateless, works across distributed systems
```

#### **JWT Configuration** (Program.cs, lines 164-200)

```csharp
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Token"])),
        ValidateIssuer = true,
        ValidIssuer = "MeMody",
        ValidateAudience = true,
        ValidAudience = "Us",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        NameClaimType = JwtRegisteredClaimNames.Sub
    };
});
```

**Token Parameters**:
| Parameter | Value | Meaning |
|-----------|-------|---------|
| **Algorithm** | HmacSha256 | HMAC-SHA256: Token signed with secret key |
| **Expiry** | 1 hour | Access token expires after 1 hour; requires refresh |
| **Issuer** | "MeMody" | Identifies who created the token (our app) |
| **Audience** | "Us" | Intended recipient of token |
| **Refresh Token** | 4 days | Long-lived token to get new access tokens without re-login |

#### **Token Structure**

A JWT consists of three parts separated by dots:

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI1NTBlODQwMC1lMjliLTQxZDQtYTcxNi1jNGY0NDY2NTU0NDAwIiwibmFtZSI6IkpvaG4gRG9lIiwiZW1haWwiOiJqb2huQGV4YW1wbGUuY29tIiwicm9sZSI6IlVzZXIiLCJleHAiOjE3MDQwMDAwMDB9.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
│                          │                                             │                           │
│ Header (base64)          │ Payload/Claims (base64)                   │ Signature (HMAC-SHA256)
│ {"alg":"HS256"}          │ {"sub":"user-id","name":"...","exp":...}  │ sign(header+payload, secret)
```

**Header**: Algorithm and type
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload (Claims)**:
```json
{
  "sub": "550e8400-e29b-41d4-a716-446655440000",    // Subject (User ID)
  "email": "user@example.com",                      // Email claim
  "name": "John Doe",                               // Name claim
  "role": "User",                                   // Authorization claim
  "exp": 1704000000,                                // Expiration time
  "iss": "MeMody",                                  // Issuer
  "aud": "Us"                                       // Audience
}
```

**Signature**: HMAC-SHA256(base64(header) + "." + base64(payload), secret_key)

#### **Token Lifecycle**

1. **Registration** (`POST /api/v1/auth/sign-up`):
   ```csharp
   public async Task<AuthResponse> RegisterAsync(SignUpRequest request)
   {
       var user = new User 
       { 
           Email = request.Email, 
           Name = request.Name,
           PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
           Role = "User"
       };
       await _unitOfWork.Users.AddAsync(user);
       await _unitOfWork.SaveChangesAsync();
       
       // Generate tokens
       var accessToken = _tokenHandler.CreateToken(user);
       var refreshToken = _tokenHandler.CreateRefreshToken();
       user.RefreshTokens.Add(new RefreshToken { Token = refreshToken, ... });
       
       return new AuthResponse { AccessToken = accessToken, RefreshToken = refreshToken };
   }
   ```

2. **Login** (`POST /api/v1/auth/sign-in`):
   - Verify email exists
   - Compare password hash: `BCrypt.Verify(password, user.PasswordHash)`
   - Issue tokens if credentials valid

3. **Client Storage**:
   - Access token: Short-lived (1 hour), stored in memory or httpOnly cookie
   - Refresh token: Long-lived (4 days), stored in httpOnly cookie (secure against XSS)

4. **API Request with Token**:
   ```
   GET /api/v1/user/profile
   Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```

5. **Token Refresh** (`POST /api/v1/auth/token-login`):
   - Client sends expired access token + refresh token
   - Server validates refresh token
   - Server issues new access token (refresh token can be rotated or reused)

6. **Token Expiry Handling**:
   - Server sends `401 Unauthorized` when access token expires
   - Client silently refreshes token using refresh token
   - Retry original request with new access token

### 4.2 Authorization: Role-Based & Policy-Based Access Control

#### **Roles Implementation** (Role.cs in Core)

```csharp
public class Role : BaseEntity
{
    public string Name { get; set; }          // "Admin", "User", "Trainer"
    public string Description { get; set; }
    public ICollection<User> Users { get; set; }
}
```

#### **Authorization Policies** (Program.cs, lines 202-218)

```csharp
services.AddAuthorizationBuilder()
    .AddPolicy("NormalUsage", policy => 
        policy.RequireRole("Admin", "User"))
    .AddPolicy("ElevatedPower", policy => 
        policy.RequireRole("Admin"))
    .AddPolicy("SameUserPolicy", policy => 
        policy.AddRequirements(new SameUserRequirement()))
    .AddPolicy("ListUserPolicy", policy => 
        policy.AddRequirements(new ListUserRequirement()));
```

#### **Policy Usage in Controllers**

```csharp
[HttpGet("{id}")]
[Authorize(Policy = "SameUserPolicy")]
public async Task<IActionResult> GetUserProfile(string id)
{
    // Only the user themselves or an admin can access this endpoint
    var user = await _unitOfWork.Users.GetByIdAsync(Guid.Parse(id));
    return Ok(user);
}

[HttpGet("list/all")]
[Authorize(Policy = "ElevatedPower")]
public async Task<IActionResult> ListUsers()
{
    // Only admins can see all users
    var users = await _unitOfWork.Users.GetAllAsync();
    return Ok(users);
}
```

#### **Custom Policy Handlers**

**SameUserHandler.cs** - Validates if requestor is the resource owner:

```csharp
public class SameUserHandler : AuthorizationHandler<SameUserRequirement>
{
    private readonly ICurrentUser _currentUser;
    
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        SameUserRequirement requirement)
    {
        var userId = _currentUser.GetUserId();  // Extract from JWT claims
        var resourceUserId = GetResourceUserId();  // From route parameter
        
        if (userId == resourceUserId || IsAdmin(context.User))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
        
        return Task.CompletedTask;
    }
}
```

**Benefits**: Users can only modify their own data; admins can override. This prevents users from seeing each other's workout history or billing information.

### 4.3 Data Sanitization & Validation

#### **Input Validation Strategy**

**1. Model-Level Validation** (DTOs with Data Annotations)

```csharp
public class SignUpRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, MinimumLength = 5)]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    [StringLength(128, MinimumLength = 8, 
        ErrorMessage = "Password must be 8-128 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must contain uppercase, lowercase, digit, and special character")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }
}
```

**Automatic Validation** in Program.cs:
```csharp
services.AddControllers()
    .AddFluentValidation(config =>
    {
        config.RegisterValidatorsFromAssembly(typeof(Program).Assembly);
        config.ValidatorOptions.CascadeMode = CascadeMode.Stop;
    });
```

Invalid requests return `400 Bad Request`:
```json
{
  "errors": {
    "Email": ["Invalid email format"],
    "Password": ["Password must contain uppercase, lowercase, digit, and special character"]
  }
}
```

**2. SQL Injection Prevention**

EF Core uses **parameterized queries** by default:

```csharp
// SAFE: EF Core generates parameterized SQL
var user = await _context.Users
    .Where(u => u.Email == userInput)
    .FirstOrDefaultAsync();

// Generated SQL: SELECT * FROM Users WHERE Email = @p0
// User input is passed as parameter, never concatenated into SQL string
```

**Never do this** (vulnerable to SQL injection):
```csharp
// DANGEROUS: String concatenation
var sql = $"SELECT * FROM Users WHERE Email = '{userInput}'";
var user = _context.Users.FromSqlRaw(sql);
```

**3. Cross-Site Scripting (XSS) Prevention**

- **ASP.NET Core respects** `Content-Security-Policy` headers (configured in middleware)
- **Output Encoding**: JSON responses automatically escape special characters
- **HttpOnly Cookies**: Tokens stored in HttpOnly cookies can't be accessed by JavaScript, preventing XSS token theft

Example in middleware:
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add(
        "Content-Security-Policy", 
        "default-src 'self'; script-src 'self'");
    await next();
});
```

**4. CORS (Cross-Origin Resource Sharing)**

```csharp
services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "https://yourdomain.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

app.UseCors("AllowSpecificOrigins");
```

**Prevents**: Unauthorized domains from making API requests. Only whitelisted origins can call the API.

**5. Password Security**

```csharp
// Registration - Hash password with BCrypt
var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, cost: 12);
var user = new User { PasswordHash = passwordHash, ... };

// Login - Compare input with hash (salt is embedded in hash)
bool isValidPassword = BCrypt.Net.BCrypt.Verify(inputPassword, user.PasswordHash);
```

**Why BCrypt**:
- **Salting**: Each hash includes random salt; same password produces different hashes
- **Cost Factor (12)**: Algorithm runs 2^12 = 4096 iterations, slowing brute-force attacks
- **Adaptive**: Cost can be increased as computers get faster

### 4.4 Environment-Specific Security Configuration

**Development** (`appsettings.Development.json`):
- CORS allows localhost
- Logging includes detailed stack traces
- Entity Framework logs SQL queries

**Production** (`appsettings.json`):
- CORS restricted to approved domains
- Logging suppresses sensitive data
- JWT key stored in environment variables
- Database connection uses encryption

---

## 5. Database Modeling & Migration Strategy

### 5.1 Entity Relationship (ER) Breakdown

The Gym App models 21 interconnected entities representing the domain:

#### **Core Domain Entities**

**1. User Management Cluster**

```
┌─────────┐         ┌──────────────┐
│ Role    │         │ User         │
│---------|         │---------|────────────────┐
│ Id      │◄────────│ Id      │ (PK)         │
│ Name    │ (FK)    │ Email   │              │
│         │         │ Name    │              │
│         │         │ Role    │              │
│         │         │---------|              │
│         │         │ RefreshTokens│         │
│         │         │ PastInjuries │         │
│         │         │ Transactions │         │
└─────────┘         └──────────────┘         │
                                 ▲           │
                          1:1    │           │
                ┌─────────────────┘           │
                │                             │
        ┌───────────────────────┐             │
        │ RefreshTokens         │             │
        │─────────────────────--|             │
        │ Id (PK)               │             │
        │ Token                 │─────┐       │
        │ ExpiryDate            │     │       │
        │ UserId (FK)           │     │       │
        └───────────────────────┘     │       │
                                      │       │
        ┌────────────────────────────┘        │
        │   1:N (User → Transactions) ────────┘
        │
    ┌───────────────────┐
    │ Transaction       │
    │─────────────────--|
    │ Id (PK)          │
    │ Amount           │
    │ Date             │
    │ UserId (FK)      │
    └───────────────────┘
```

**Entity Code**:
```csharp
// User.cs
public class User : BaseEntity
{
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Name { get; set; }
    
    // Foreign Keys
    public Guid RoleId { get; set; }
    public virtual Role Role { get; set; }
    
    // Collections
    public ICollection<RefreshToken> RefreshTokens { get; set; }
    public ICollection<PastInjuries> PastInjuries { get; set; }
    public ICollection<Transaction> Transactions { get; set; }
}

// RefreshToken.cs
public class RefreshToken : BaseEntity
{
    public string Token { get; set; }
    public DateTime ExpiryDate { get; set; }
    
    // Foreign Key
    public Guid UserId { get; set; }
    public virtual User User { get; set; }
}
```

**Relationship Type**: One-to-Many (1:N)
- One User can have multiple Refresh Tokens
- Each Refresh Token belongs to exactly one User

---

**2. Fitness Tracking Cluster** (Central to the application)

```
                    ┌──────────┐
                    │ Exercise │
                    │----------|
                    │ Id       │
                    │ Name     │
                    │ MuscleId │
                    └──────────┘
                          ▲
                    1:N   │
                          │
        ┌─────────────────────────────────────┐
        │ ExerciseInstance                    │
        │────────────────────────────────────│
        │ Id (PK)                            │
        │ Sets, Reps, Weight                 │
        │ ExerciseId (FK) → Exercise         │
        │ WorkoutId (FK) → Workout           │
        └─────────────────────────────────────┘
               ▲
         1:N   │
               │
    ┌──────────────────────┐         ┌─────────────┐
    │ Workout              │         │ WorkoutSet  │
    │────────────────────--|         │─────────────|
    │ Id (PK)              │◄────────│ Id (PK)    │
    │ Name                 │ 1:1     │ WorkoutId  │
    │ Description          │         │ (FK)       │
    │ UserId (FK)          │         └─────────────┘
    │ ExerciseInstances    │
    └──────────────────────┘
               ▲
         1:N   │
               │
    ┌──────────────────────┐         ┌──────────────────────┐
    │ Session              │         │ PersonalRecord       │
    │────────────────────--|         │───────────────────---|
    │ Id (PK)              │         │ Id (PK)              │
    │ StartTime            │         │ ExerciseId (FK)      │
    │ EndTime              │         │ UserId (FK)          │
    │ WorkoutId (FK)       │         │ MaxWeight            │
    │ Status               │         │ Date                 │
    └──────────────────────┘         └──────────────────────┘
             ▲
       1:N   │
             │
    ┌──────────────────────────┐
    │ SessionUsers (Join Table)│
    │────────────────────────--|
    │ SessionId (FK)           │
    │ UserId (FK)              │
    │ JoinedAt                 │
    └──────────────────────────┘
```

**Relationship Types**:
- User 1→N Workout (each user has many workouts)
- Workout 1→N ExerciseInstance (each workout has many exercises)
- Exercise 1→N ExerciseInstance (each exercise type appears in many workouts)
- Workout 1→1 WorkoutSet (tracking set progress)
- Workout 1→N Session (workout can be executed multiple times)
- Session M→N User via SessionUsers (many users join a session)
- Exercise 1←N PersonalRecord (track best weight per exercise)

---

**3. Scheduling & Communication**

```
┌────────────────┐        ┌──────────────────┐
│ Schedule       │        │ Muscles          │
│────────────────|        │──────────────────|
│ Id (PK)        │        │ Id (PK)          │
│ Name           │        │ Name (Biceps,    │
│ StartTime      │        │ Triceps, etc.)   │
│ EndTime        │        └──────────────────┘
│ UserId (FK)    │        
└────────────────┘        ┌──────────────────┐
                          │ Challenges       │
                          │──────────────────|
┌────────────────┐        │ Id (PK)          │
│ Notification   │        │ Title            │
│────────────────|        │ Description      │
│ Id (PK)        │        │ Status           │
│ Title          │        └──────────────────┘
│ Body           │
│ UserId (FK)    │        ┌──────────────────┐
│ IsRead         │        │ Message          │
│ CreatedAt      │        │──────────────────|
└────────────────┘        │ Id (PK)          │
                          │ Content          │
                          │ SenderId (FK)    │
                          │ RecipientId (FK) │
                          │ Timestamp        │
                          └──────────────────┘

                          ┌──────────────────┐
                          │ Feedback         │
                          │──────────────────|
                          │ Id (PK)          │
                          │ Rating (1-5)     │
                          │ Comment          │
                          │ UserId (FK)      │
                          └──────────────────┘

                          ┌──────────────────┐
                          │ LiveFeedback     │
                          │──────────────────|
                          │ Id (PK)          │
                          │ Content          │
                          │ TimestampedAt    │
                          └──────────────────┘
```

---

**4. Health & Past Medical History**

```
┌────────────────────────┐
│ PastInjuries           │
│────────────────────────│
│ Id (PK)                │
│ Description            │
│ InjuryDate             │
│ Status                 │
│ UserId (FK) → User     │
└────────────────────────┘
```

---

#### **Database Constraints**

```sql
-- Cascading Deletes: When user is deleted, all related data is cascaded
User → RefreshToken (CASCADE DELETE)
User → Workout (CASCADE DELETE)
User → Schedule (CASCADE DELETE)
User → Transaction (CASCADE DELETE)
Workout → ExerciseInstance (CASCADE DELETE)
Workout → Session (CASCADE DELETE)
Workout → WorkoutSet (CASCADE DELETE)

-- Foreign Key Constraints
User.RoleId REFERENCES Role(Id)
Workout.UserId REFERENCES User(Id)
ExerciseInstance.ExerciseId REFERENCES Exercise(Id)
ExerciseInstance.WorkoutId REFERENCES Workout(Id)
Message.SenderId REFERENCES User(Id)
Message.RecipientId REFERENCES User(Id)
PersonalRecord.UserId REFERENCES User(Id)
PersonalRecord.ExerciseId REFERENCES Exercise(Id)
```

#### **Key Constraints**

```sql
-- Primary Keys (Clustered Index)
User (Id) - Guid, auto-generated
Role (Id) - Guid, auto-generated
Workout (Id) - Guid, auto-generated
... all entities use Guid as PK

-- Unique Constraints
User.Email UNIQUE (users can't share email)

-- Indexes for Performance
User.Email (frequently queried in login)
Workout.UserId (user-to-workouts lookups)
Message.RecipientId (inbox queries)
```

### 5.2 Database Migrations & Schema Evolution

#### **Migration Strategy: Code-First Approach**

Rather than designing database first, then generating code, EF Core uses **Code-First migrations**: you write C# entity models, then generate SQL scripts.

**Workflow**:

1. **Define Entity in C#** (e.g., `Exercise.cs`):
   ```csharp
   public class Exercise : BaseEntity
   {
       public string Name { get; set; }
       public string Description { get; set; }
       public Guid MuscleId { get; set; }
       public virtual Muscle Muscle { get; set; }
   }
   ```

2. **Add DbSet to Context** (`DbBase.cs`):
   ```csharp
   public DbSet<Exercise> Exercises { get; set; }
   ```

3. **Create Migration**:
   ```bash
   dotnet ef migrations add AddExerciseTable
   ```
   This generates a migration file like `20240115103000_AddExerciseTable.cs`:
   ```csharp
   protected override void Up(MigrationBuilder migrationBuilder)
   {
       migrationBuilder.CreateTable(
           name: "Exercises",
           columns: table => new
           {
               Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
               Name = table.Column<string>(type: "nvarchar(255)", nullable: false),
               Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
               MuscleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
               CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
               UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
           },
           constraints: table =>
           {
               table.PrimaryKey("PK_Exercises", x => x.Id);
               table.ForeignKey(
                   name: "FK_Exercises_Muscles_MuscleId",
                   column: x => x.MuscleId,
                   principalTable: "Muscles",
                   principalColumn: "Id",
                   onDelete: ReferentialAction.Cascade);
           });
   }
   
   protected override void Down(MigrationBuilder migrationBuilder)
   {
       migrationBuilder.DropTable("Exercises");
   }
   ```

4. **Apply Migration**:
   ```bash
   dotnet ef database update
   ```
   This runs the `Up()` method against your database, creating the table.

#### **Automatic Migrations on Startup** (Program.cs, lines 232-241)

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DbBase>();
    
    // Automatically apply any pending migrations
    db.Database.Migrate();
}
```

**Benefit**: When you deploy to production, migrations automatically apply without manual intervention.

#### **Migration Best Practices Followed**

1. **Reversibility**: Each migration has `Up()` and `Down()` methods
   - `Up()`: Apply the migration
   - `Down()`: Revert (used in `dotnet ef database update --migration <previous>`)

2. **Zero Downtime Migrations**: Adding nullable columns doesn't require data rewrite
   ```csharp
   // Safe: New nullable column added, existing rows get NULL
   public string? NewField { get; set; }
   ```

3. **Seed Data**: Migrations can populate initial data
   ```csharp
   protected override void Up(MigrationBuilder migrationBuilder)
   {
       migrationBuilder.InsertData(
           table: "Roles",
           columns: new[] { "Name", "Description" },
           values: new object[,]
           {
               { "Admin", "System administrator" },
               { "User", "Regular user" },
               { "Trainer", "Gym trainer" }
           });
   }
   ```

#### **Schema Evolution Example**

**Scenario**: Initially, `User` has no `PhoneNumber` field. Later, you want to add it.

```bash
# Step 1: Add property to entity
// In User.cs
public string? PhoneNumber { get; set; }

# Step 2: Create migration
dotnet ef migrations add AddPhoneNumberToUser
# Generates migration with:
# - Column definition: PhoneNumber nvarchar(20) nullable
# - No data loss (nullable, so existing rows get NULL)

# Step 3: Apply
dotnet ef database update
# Schema changes. Production database now has PhoneNumber column.
```

**Cross-Environment Consistency**:
- Developer machine: `dotnet ef database update` before coding
- CI/CD pipeline: Migrations run on test database before integration tests
- Production: Migrations auto-applied on startup (with backup first!)

---

## 6. Testing Framework & Quality Assurance

### 6.1 Testing Strategy Overview

The Gym App implements a **layered testing approach**:

```
Unit Tests (Services, Repositories)     60%
    ↓
Integration Tests (API + Database)      30%
    ↓
E2E Tests (Full workflows)              10%
```

### 6.2 Testing Framework: xUnit + Moq

#### **Why xUnit?**

| Feature | xUnit | NUnit | MSTest |
|---------|-------|-------|--------|
| **Fixture Setup** | Clean attribute-based | [SetUp]/[TearDown] | [TestInitialize] |
| **Parameterized Tests** | [Theory] + [InlineData] | [TestCase] | [DataTestMethod] |
| **Async Support** | Native, seamless | Requires async variants | Full support |
| **Community** | Modern, actively maintained | Legacy but stable | Microsoft-backed |
| **Learning Curve** | Gentle | Moderate | Steep |

**Chosen: xUnit 2.9.3** - Modern, async-first, clean syntax.

#### **Moq 4.20.72 for Mocking**

Moq creates fake objects (mocks) of interfaces/classes for unit testing:

```csharp
// Create a mock repository
var mockUserRepository = new Mock<IUserRepository>();

// Setup behavior: When GetByIdAsync is called, return specific user
mockUserRepository
    .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
    .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = "test@example.com" });

// Inject into service
var userService = new UserService(mockUserRepository.Object);

// Test: Service should handle missing users gracefully
mockUserRepository
    .Setup(r => r.GetByIdAsync(Guid.NewGuid()))
    .ReturnsAsync((User)null);

var result = await userService.GetUser(Guid.NewGuid());
Assert.Null(result);
```

### 6.3 Test Base & Infrastructure

#### **TestBase.cs** - Shared Test Setup

```csharp
public abstract class TestBase
{
    protected DbBase _context;
    protected IUnitOfWork _unitOfWork;
    
    [SetUp]
    public async Task TestInitialize()
    {
        // Create in-memory database with random name
        var options = new DbContextOptionsBuilder<DbBase>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new DbBase(options);
        _unitOfWork = new UnitOfWork(_context);
        
        // Seed test data
        await SeedTestData();
    }
    
    protected virtual async Task SeedTestData()
    {
        // Create roles
        var adminRole = new Role { Name = "Admin", Description = "Administrator" };
        var userRole = new Role { Name = "User", Description = "Regular user" };
        
        await _unitOfWork.Roles.AddAsync(adminRole);
        await _unitOfWork.Roles.AddAsync(userRole);
        await _unitOfWork.SaveChangesAsync();
    }
    
    protected User CreateTestUser(string email = "test@example.com", Role role = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = "Test User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
            Role = role ?? GetTestRole(),
            CreatedAt = DateTime.UtcNow
        };
    }
    
    protected Workout CreateTestWorkout(User user)
    {
        return new Workout
        {
            Id = Guid.NewGuid(),
            Name = "Test Workout",
            Description = "A test workout plan",
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

**Benefits**:
- **In-Memory Database**: Tests run fast, no disk I/O
- **Isolation**: Each test gets fresh database instance
- **Reusability**: Factory methods (`CreateTestUser`) reduce boilerplate

### 6.4 Unit Testing Examples

#### **Example 1: User Registration Service** (`UserTests/UserCreateTests.cs`)

```csharp
[TestClass]
public class UserCreateTests : TestBase
{
    [TestMethod]
    [TestCategory("UserCreation")]
    public async Task CreateUser_WithValidData_ShouldSucceed()
    {
        // Arrange
        var newUser = CreateTestUser(email: "newuser@example.com");
        
        // Act
        await _unitOfWork.Users.AddAsync(newUser);
        var result = await _unitOfWork.SaveChangesAsync();
        
        // Assert
        Assert.AreEqual(1, result);  // One row affected
        
        var savedUser = await _unitOfWork.Users.GetByEmailAsync("newuser@example.com");
        Assert.IsNotNull(savedUser);
        Assert.AreEqual("newuser@example.com", savedUser.Email);
    }
    
    [TestMethod]
    [TestCategory("UserCreation")]
    public async Task CreateUser_WithDuplicateEmail_ShouldFail()
    {
        // Arrange
        var user1 = CreateTestUser("duplicate@example.com");
        var user2 = CreateTestUser("duplicate@example.com");
        
        await _unitOfWork.Users.AddAsync(user1);
        await _unitOfWork.SaveChangesAsync();
        
        // Act & Assert: Adding user with duplicate email should raise
        await _unitOfWork.Users.AddAsync(user2);
        var ex = await Assert.ThrowsExceptionAsync<DbUpdateException>(
            async () => await _unitOfWork.SaveChangesAsync()
        );
        
        StringAssert.Contains(ex.Message, "UNIQUE constraint failed");
    }
}
```

**Test Breakdown**:
- **Arrange**: Set up test data and dependencies
- **Act**: Execute the function being tested
- **Assert**: Verify results match expectations

---

#### **Example 2: Authentication Service** (`AuthTests/TokenHandlerTests.cs`)

```csharp
[TestClass]
public class TokenHandlerTests : TestBase
{
    private ITokenHandler _tokenHandler;
    
    [TestInitialize]
    public async Task Setup()
    {
        await base.TestInitialize();
        
        // Mock JWT configuration
        var jwtSettings = new JwtSettings
        {
            Token = "VerySecretKeyThatIsAtLeast32CharactersLongForHmacSha256Signing",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryMinutes = 60
        };
        
        var mockConfig = new Mock<IOptions<JwtSettings>>();
        mockConfig.Setup(x => x.Value).Returns(jwtSettings);
        
        _tokenHandler = new TokenHandler(mockConfig.Object);
    }
    
    [TestMethod]
    public void CreateToken_WithValidUser_ShouldReturnSignedJwt()
    {
        // Arrange
        var user = CreateTestUser();
        
        // Act
        var token = _tokenHandler.CreateToken(user);
        
        // Assert
        Assert.IsNotNull(token);
        Assert.IsFalse(string.IsNullOrWhiteSpace(token));
        
        // Token should be in format: header.payload.signature
        var parts = token.Split('.');
        Assert.AreEqual(3, parts.Length);
    }
    
    [TestMethod]
    public void CreateToken_ShouldIncludeUserClaims()
    {
        // Arrange
        var user = CreateTestUser();
        var handler = new JwtSecurityTokenHandler();
        
        // Act
        var token = _tokenHandler.CreateToken(user);
        var decodedToken = handler.ReadJwtToken(token);
        
        // Assert
        var userIdClaim = decodedToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        Assert.IsNotNull(userIdClaim);
        Assert.AreEqual(user.Id.ToString(), userIdClaim.Value);
        
        var emailClaim = decodedToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        Assert.AreEqual(user.Email, emailClaim.Value);
    }
    
    [TestMethod]
    public void CreateToken_ShouldExpireAfterConfiguredMinutes()
    {
        // Arrange
        var user = CreateTestUser();
        var handler = new JwtSecurityTokenHandler();
        
        // Act
        var token = _tokenHandler.CreateToken(user);
        var decodedToken = handler.ReadJwtToken(token);
        
        // Assert
        var expClaim = decodedToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);
        Assert.IsNotNull(expClaim);
        
        var expiryTime = UnixTimeStampToDateTime(long.Parse(expClaim.Value));
        var expectedExpiry = DateTime.UtcNow.AddMinutes(60);
        
        // Allow 1-second tolerance for test execution time
        Assert.IsTrue(
            Math.Abs((expectedExpiry - expiryTime).TotalSeconds) < 2,
            "Token expiry should be approximately 60 minutes from now"
        );
    }
}
```

---

#### **Example 3: Workout Service** (`WorkoutTests/WorkoutProgressTests.cs`)

```csharp
[TestClass]
public class WorkoutProgressTests : TestBase
{
    private IWorkoutService _workoutService;
    
    [TestInitialize]
    public async Task Setup()
    {
        await base.TestInitialize();
        _workoutService = new WorkoutService(_unitOfWork);
    }
    
    [TestMethod]
    [DataRow(1, 185.0)]
    [DataRow(2, 190.0)]
    [DataRow(5, 210.0)]
    public async Task UpdateExerciseProgress_ShouldTrackPersonalRecords(int sets, double weight)
    {
        // Arrange
        var user = CreateTestUser();
        await _unitOfWork.Users.AddAsync(user);
        
        var exercise = new Exercise { Name = "Bench Press", MuscleId = Guid.NewGuid() };
        await _unitOfWork.Exercises.AddAsync(exercise);
        await _unitOfWork.SaveChangesAsync();
        
        var workout = CreateTestWorkout(user);
        await _unitOfWork.Workouts.AddAsync(workout);
        await _unitOfWork.SaveChangesAsync();
        
        // Act: Log exercise progress
        await _workoutService.LogExerciseProgress(user.Id, exercise.Id, sets, weight);
        
        // Assert: Personal record should be created/updated
        var pr = await _unitOfWork.PersonalRecords
            .GetAsync(p => p.UserId == user.Id && p.ExerciseId == exercise.Id);
        
        Assert.IsNotNull(pr);
        Assert.AreEqual(weight, pr.MaxWeight);
    }
}
```

### 6.5 Integration Testing

Integration tests verify multiple layers work together (API → Service → Repository → Database).

#### **Example: Workout API Integration Test**

```csharp
[TestClass]
public class WorkoutControllerIntegrationTests : TestBase
{
    private HttpClient _httpClient;
    private WebApplicationFactory<Program> _factory;
    
    [TestInitialize]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _httpClient = _factory.CreateClient();
    }
    
    [TestMethod]
    public async Task CreateWorkout_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var user = CreateTestUser();
        var token = GenerateJwtToken(user);
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        var createRequest = new { name = "Chest Day", description = "Focus on chest exercises" };
        var content = new StringContent(
            JsonSerializer.Serialize(createRequest),
            Encoding.UTF8,
            "application/json"
        );
        
        // Act
        var response = await _httpClient.PostAsync(
            $"/api/v1/workout/create/{user.Id}",
            content
        );
        
        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        
        var responseBody = await response.Content.ReadAsAsync<dynamic>();
        Assert.AreEqual(2, responseBody.status);  // Success
        Assert.IsNotNull(responseBody.Value.id);
    }
    
    [TestMethod]
    public async Task CreateWorkout_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var createRequest = new { name = "Chest Day" };
        var content = new StringContent(
            JsonSerializer.Serialize(createRequest),
            Encoding.UTF8,
            "application/json"
        );
        
        // Act
        var response = await _httpClient.PostAsync(
            $"/api/v1/workout/create/{Guid.NewGuid()}",
            content
        );
        
        // Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
```

### 6.6 Test Organization

The project contains **10 test categories**:

```
GymApp.Tests/
├── AuthTests/
│   ├── SignUpTests.cs
│   ├── LoginTests.cs
│   ├── TokenHandlerTests.cs
│   └── RefreshTokenTests.cs
├── UserTests/
│   ├── UserQueryTests.cs
│   ├── UserCreateTests.cs
│   ├── UserUpdateTests.cs
│   └── UserDeleteTests.cs
├── WorkoutTests/
│   ├── WorkoutCrudTests.cs
│   ├── WorkoutProgressTests.cs
│   └── PersonalRecordTests.cs
├── SessionTests/
├── ScheduleTests/
├── NotificationTests/
├── MessageTests/
├── FeedbackTests/
├── FileTests/
├── TokenTests/
└── TestBase.cs
```

### 6.7 Code Coverage & Quality Metrics

#### **Coverage Tools**

```xml
<!-- .csproj -->
<ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.2" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.3.0" />
</ItemGroup>
```

#### **Running Tests with Coverage**

```bash
# Run all tests
dotnet test

# Run with coverage report
dotnet test /p:CollectCoverage=true /p:CoverageFormat=cobertura

# Generate HTML report
reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage-report -reporttypes:Html
```

#### **Coverage Goals**

- **Infrastructure Layer**: 90%+ (repositories, database access)
- **Application Services**: 85%+ (business logic, validation)
- **API Controllers**: 70%+ (routing tested via integration tests)
- **Overall Target**: 80%+ code coverage

#### **Quality Gates**

Test failures block commits:
```yaml
# GitHub Actions workflow checks
- Name: Run Tests
  Run: dotnet test --no-build --verbosity normal /p:CollectCoverage=true
  
- Name: Check Coverage
  Run: |
    if [ $(echo "scale=2; $COVERAGE < 80" | bc) -eq 1 ]; then
      echo "Coverage below 80%"
      exit 1
    fi
```

---

## 7. DevOps & CI/CD Pipeline

### 7.1 Containerization Strategy

While the current deployment doesn't use Docker, the architecture is **container-ready**:

#### **Dockerfile** (Should be added for best practices)

```dockerfile
# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ["Gym App.API/Gym App.API.csproj", "Gym App.API/"]
COPY ["Gym App.Core/Gym App.Core.csproj", "Gym App.Core/"]
COPY ["Gym App.Infrastructure/Gym App.Infrastructure.csproj", "Gym App.Infrastructure/"]
COPY ["Gym App.Application/Gym App.Application.csproj", "Gym App.Application/"]

RUN dotnet restore "Gym App.API/Gym App.API.csproj"

COPY . .

RUN dotnet build "Gym App.API/Gym App.API.csproj" -c Release -o /app/build
RUN dotnet publish "Gym App.API/Gym App.API.csproj" -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "Gym App.API.dll"]
```

**Benefits**:
- Multi-stage build: smaller final image
- Consistent environment from development to production
- Easy scaling: run multiple container instances behind load balancer

#### **Docker Compose** (For local development)

```yaml
version: '3.8'

services:
  api:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Connection=Server=sqlserver;Database=GymApp;User Id=sa;Password=YourPassword;
    depends_on:
      - sqlserver

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

volumes:
  sqlserver_data:
```

---

### 7.2 Continuous Integration (CI) Pipeline

#### **GitHub Actions Workflow: .NET Build** (`.github/workflows/dotnet.yml`)

```yaml
name: .NET Build & Test

on:
  push:
    branches: [ master, develop ]
  pull_request:
    branches: [ master ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    strategy:
      matrix:
        dotnet-version: ['8.0.x']

    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ matrix.dotnet-version }}
    
    - name: Restore Dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    - name: Run Unit Tests
      run: dotnet test --configuration Release --no-build --verbosity normal /p:CollectCoverage=true /p:CoverageFormat=opencover
    
    - name: Upload Coverage
      uses: codecov/codecov-action@v3
      with:
        file: ./coverage.opencover.xml
        flags: unittests
        name: codecov-umbrella
```

**Pipeline Stages**:

1. **Trigger**
   - Automatic on `push` to master/develop
   - On pull requests to master
   - Manual trigger available

2. **Checkout Code**
   ```bash
   git clone <repo> [runner-workspace]
   ```

3. **Setup .NET Runtime**
   - Install .NET 8.0.x (note: should be updated to 9.0.x)
   - Verify with `dotnet --version`

4. **Restore NuGet Dependencies**
   ```bash
   dotnet restore
   # Downloads all NuGet packages specified in .csproj files
   ```

5. **Compile Release Build**
   ```bash
   dotnet build --configuration Release
   # Compiles all projects to /bin/Release/
   # Catches compile-time errors
   ```

6. **Run Unit Tests**
   ```bash
   dotnet test --configuration Release /p:CollectCoverage=true
   # Executes xUnit tests
   # Collects code coverage metrics  
   # Fails build if any test fails
   ```

7. **Upload Coverage**
   - Sends coverage data to Codecov
   - Tracks coverage trends over time
   - PR comments show coverage impact

---

### 7.3 Continuous Deployment (CD) Pipeline

#### **GitHub Actions Workflow: Auto Deploy** (`.github/workflows/automatic-deploy.yml`)

```yaml
name: Auto Deploy to Production

on:
  workflow_run:
    workflows: [".NET Build & Test"]
    types:
      - completed

jobs:
  deploy:
    runs-on: ubuntu-latest
    if: ${{ github.event.workflow_run.conclusion == 'success' }}
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    
    - name: Publish Release Build
      run: dotnet publish "Gym App.API/Gym App.API.csproj" -c Release -o ./publish
    
    - name: Backup Current Deployment
      uses: appleboy/ssh-action@master
      with:
        host: ${{ secrets.VPS_HOST }}
        username: ${{ secrets.VPS_USER }}
        key: ${{ secrets.VPS_SSH_KEY }}
        script: |
          cd /root/MAPSM/MAPSM-Prod
          cp -r wwwroot/Uploads /tmp/Uploads_backup_$(date +%s)
    
    - name: Deploy Files to VPS
      uses: appleboy/scp-action@master
      with:
        host: ${{ secrets.VPS_HOST }}
        username: ${{ secrets.VPS_USER }}
        key: ${{ secrets.VPS_SSH_KEY }}
        source: "publish/*"
        target: "/root/MAPSM/MAPSM-Prod/"
    
    - name: Restore Uploads & Restart Service
      uses: appleboy/ssh-action@master
      with:
        host: ${{ secrets.VPS_HOST }}
        username: ${{ secrets.VPS_USER }}
        key: ${{ secrets.VPS_SSH_KEY }}
        script: |
          cd /root/MAPSM/MAPSM-Prod
          
          # Remove old uploads if needed, restore from backup
          if [ -d "/tmp/Uploads_backup" ]; then
            cp -r /tmp/Uploads_backup/* wwwroot/Uploads/
          fi
          
          # Restart systemd service
          sudo systemctl restart mapsm-backend
          
          # Verify service is running
          sudo systemctl status mapsm-backend
```

**Deployment Workflow**:

```
GitHub Commit (master branch)
    ↓
[CI] Run tests → If fails, STOP
    ↓
[CD] Publish Release Build
    ↓
[CD] SSH to VPS: /root/MAPSM/MAPSM-Prod
    ↓
[CD] Backup current wwwroot/Uploads
    ↓
[CD] SCP publish/* to production server
    ↓
[CD] Restore Uploads folder
    ↓
[CD] systemctl restart mapsm-backend
    ↓
✅ Service running new version
```

**Blue-Green Deployment Strategy**:

```
Before Deploy:
  Blue  (Current) → Serving traffic
  Green (New)     → Being prepared

Deploy:
  1. Copy new version to Green
  2. Test Green (smoke tests)
  3. Switch traffic: Blue ← → Green
  4. Old Blue becomes standby

Rollback:
  Switch traffic back to Blue (instant)
```

---

#### **Manual Deploy Workflow** (`.github/workflows/manual-deploy.yml`)

Allows manual deployment without waiting for CI:

```yaml
name: Manual Deploy

on:
  workflow_dispatch:
    inputs:
      environment:
        description: 'Environment to deploy to'
        required: true
        default: 'staging'
        type: choice
        options:
          - staging
          - production

jobs:
  deploy:
    runs-on: ubuntu-latest
    environment: ${{ github.event.inputs.environment }}
    
    steps:
    # ... similar to auto-deploy ...
```

### 7.4 Infrastructure & Deployment Configuration

#### **Server Setup: Linux VPS with systemd**

```bash
# Create service file: /etc/systemd/system/mapsm-backend.service
[Unit]
Description=Gym App Backend API
After=network.target

[Service]
Type=notify
User=root
WorkingDirectory=/root/MAPSM/MAPSM-Prod
ExecStart=/usr/bin/dotnet "Gym App.API.dll"
Restart=on-failure
RestartSec=10

Environment="ASPNETCORE_ENVIRONMENT=Production"
Environment="ASPNETCORE_URLS=http://localhost:5000"

[Install]
WantedBy=multi-user.target
```

**Service Management**:
```bash
# Start service
sudo systemctl start mapsm-backend

# Stop service
sudo systemctl stop mapsm-backend

# Restart (used during deployments)
sudo systemctl restart mapsm-backend

# Check status
sudo systemctl status mapsm-backend

# View logs
sudo journalctl -u mapsm-backend -f
```

#### **Application Configuration for Production** (`appsettings.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning"
    }
  },
  "JwtSettings": {
    "Token": "${JWT_SECRET}",      // From environment variable
    "Issuer": "MeMody",
    "Audience": "Us"
  },
  "ConnectionStrings": {
    "Connection": "Data Source=${DB_HOST};Initial Catalog=GymApp;User Id=${DB_USER};Password=${DB_PASSWORD};"
  },
  "EmailSettings": {
    "SenderEmail": "${SMTP_USER}",
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "${SMTP_USER}",
    "SmtpPassword": "${SMTP_PASSWORD}"
  }
}
```

**Environment Variables** (set on VPS):
```bash
export JWT_SECRET="VerySecretKeyThatIsAtLeast32CharactersLongForHmacSha256Signing"
export DB_HOST="localhost"
export DB_USER="sa"
export DB_PASSWORD="SecurePassword123!"
export SMTP_USER="gym-noreply@gmail.com"
export SMTP_PASSWORD="app-specific-password"
export ASPNETCORE_ENVIRONMENT="Production"
```

### 7.5 Monitoring & Observability

#### **Logging with Serilog** (Program.cs, lines 19-26)

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "Infrastructure/logs/gymapp.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        destructureObjects: true
    )
    .Enrich.FromLogContext()
    .CreateLogger();
```

**Log Output**:
```
2024-01-15 10:30:45.123 [INF] Request started GET /api/v1/workout/123
2024-01-15 10:30:45.456 [INF] User 'john@example.com' authenticated
2024-01-15 10:30:46.789 [INF] Returned 20 workouts for user
2024-01-15 10:30:47.012 [INF] Request completed 200 OK
```

**Production Alerts** (can integrate with):
- **Datadog**: APM, logs, alerts
- **New Relic**: Performance monitoring
- **ELK Stack**: Elasticsearch, Logstash, Kibana for log aggregation

---

## Summary: Enterprise Architecture Checklist

| Component | Implementation | Production Ready |
|-----------|-----------------|------------------|
| **Layered Architecture** | 4-tier clean architecture | ✅ Yes |
| **JWT Authentication** | Token-based with refresh | ✅ Yes |
| **Authorization Policies** | Role-based + custom handlers | ✅ Yes |
| **Input Validation** | Model-level + API validation | ✅ Yes |
| **SQL Injection Prevention** | Parameterized queries (EF Core) | ✅ Yes |
| **Code-First Migrations** | Entity-first database evolution | ✅ Yes |
| **Unit Testing** | xUnit + Moq + in-memory DB | ✅ Yes |
| **Integration Testing** | API + database tests | ✅ Yes |
| **CI Pipeline** | GitHub Actions build + test | ✅ Yes |
| **CD Pipeline** | Automated deployment to VPS | ✅ Yes |
| **Real-Time Features** | SignalR hubs (Chat, Notifications) | ✅ Yes |
| **Monitoring & Logging** | Serilog structured logs | ✅ Yes |
| **Documentation** | Swagger/OpenAPI | ✅ Yes |
| **Containerization** | Docker-ready (needs Dockerfile) | ⚠️ Partial |

---

## Recommendations for Future Enhancements

1. **Add Docker & Kubernetes**: Container orchestration for scaling
2. **Implement Caching Layer**: Redis for frequently accessed data
3. **Message Queue**: RabbitMQ or Azure Service Bus for async operations (email, notifications)
4. **API Rate Limiting**: Prevent abuse; implement token bucket algorithm
5. **GraphQL**: Consider alongside REST for complex queries
6. **API Versioning**: Formal versioning strategy for backward compatibility
7. **Performance Testing**: Load testing with JMeter or k6
8. **Security Scanning**: OWASP dependency checks, SAST tools in CI/CD
9. **Disaster Recovery**: Database backups, failover strategy
10. **Multi-Tenancy**: Support multiple gym organizations

---

**Document Version**: 1.0  
**Last Updated**: January 2024  
**Project**: Gym App Backend - Graduation Project  
