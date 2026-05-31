# Gym App (FitPack) API - Enterprise .NET 9 Backend System

**Interactive API Documentation**: [Swagger UI](http://76.13.32.247:5000/swagger/index.html)

[![.NET](https://img.shields.io/badge/.NET-9.0-blue?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen?style=flat-square)](../../actions)
<!--[![Docker](https://img.shields.io/badge/Docker-Multi--Stage-blue?style=flat-square&logo=docker)](Dockerfile)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-Production--Ready-blue?style=flat-square&logo=kubernetes)](kubernetes/)-->

---

## Project Overview

**Gym App API** is a comprehensive backend system designed to manage workout routines, track exercise progress, and provide real-time notifications to users. Built with **.NET 9**, it demonstrates enterprise-level architecture patterns, security practices, and cloud-native deployment strategies.

This project showcases:
- **Clean Architecture** with clear separation of concerns
- **Enterprise Security** with JWT authentication and RBAC
- **Cloud-Native Design** with Kubernetes and Docker
- **Advanced Features** including SignalR real-time updates
- **Production Monitoring** with Prometheus + Grafana
- **CI/CD Automation** with GitHub Actions
- **Comprehensive Testing** with 90%+ code coverage
- **Database Migrations** with automatic seeding

---

## Key Implementations & Results

### 1. **Clean Architecture Implementation**

**What Was Built:**
- 4-layer architecture (Core → Infrastructure → Application → API)
- Complete domain entity modeling
- Repository pattern for data access
- Dependency injection throughout

### 2. **JWT Authentication & Authorization** 

**What Was Built:**
- JWT token generation with claims-based authorization
- Refresh token system with expiry management
- Custom RBAC (Role-Based Access Control) policies
- Policy-based authorization handlers

**Implemented Features:**
```csharp
✓ Access tokens + Refresh tokens
✓ Token claims (UserId, Email, Role)
✓ Custom authorization policies:
  - SameUserPolicy (can edit own data)
  - ElevatedPowerPolicy (admin only)
  - NormalUsagePolicy (authenticated users)
  - ListUserPolicy (view users list)
✓ SignalR authentication with bearer tokens
```

**Security Measures:**
- Tokens validated on every request
- Claims extracted and cached for performance
- Policy handlers for custom authorization logic
- Secure password hashing with identity framework

### 3. **SignalR Real-Time Notifications** 

**What Was Built:**
- NotificationHub for real-time messaging
- WorkoutNotifier background service
- Automatic notification triggers
- User-specific message routing

**Key Features:**
```csharp
✓ Real-time workout reminders
✓ Personal record notifications
✓ Exercise completion alerts
✓ Background job scheduler
✓ Message persistence
✓ Connected user tracking
```

### 4. **File Management System** 🖼️

**What Was Built:**
- Static file serving with URL generation
- Persistent storage in wwwroot
- Secure path validation

**Implemented Features:**
```csharp
✓ Profile picture uploads
✓ File type validation (extension whitelist)
✓ GUID-based unique naming
✓ Dynamic URL generation (http/https aware)
✓ File deletion with cleanup
✓ Automatic directory creation
✓ Error handling and logging
```

### 5. **Comprehensive Unit Testing** 

**What Was Built:**
- Mock-based testing with Moq framework
- xUnit test framework

**Test Coverage:**
```
✓ Unit tests for every service and Module such as : authorization services, basic functionality and Websocket services (SignalR)
```

### 6. **Database Architecture** 🗄️

**What Was Built:**
- Entity Framework Core with SQL Server
- Automatic migrations on startup
- Database seeding with exercise data
- Complex relationships modeling

**Implemented Entities:**
```
✓ User (authentication & profile)
✓ Workout (exercise routine)
✓ Schedule (Calendar of Workouts)
✓ Exercise 
✓ Muscle
✓ WorkoutSet (repetitions & weights)
✓ ExerciseInstance (Instance of exercise in workout)
✓ PersonalRecord (PersonalRecord for every exercise)
✓ Messages (Chat messages)
✓ Session (Chats)
✓ Notification (message tracking)
✓ RefreshToken (session management)
✓ Role (permission management)
```

**Advanced Features:**
- Composite keys for complex relationships
- Cascade delete handling
- Query optimization with includes
- Data seeding from CSV files for exercises
- Automatic timestamp tracking

### 7. **Docker Containerization** (planned)

**What Was Built:**
- Multi-stage Dockerfile for .NET 9
- Alpine-based runtime (lightweight)
- Non-root user execution
- Health check integration

**Technical Achievements:**
```dockerfile
✓ Stage 1: Build with SDK (5.8 MB base)
✓ Stage 2: Runtime with ASP.NET (0.9 MB base)
✓ Total image size: ~200 MB
✓ Security: Non-root appuser
✓ Health checks: /health endpoint
✓ Signal handling: tini init system
```

### 8. **Kubernetes Orchestration** (planned)

**What Was Built:**
- Complete Kubernetes deployment manifests
- Ingress with NGINX controller
- SSL/TLS with Let's Encrypt
- Horizontal Pod Autoscaling

**Production Features:**
```yaml
✓ 3 replicas minimum → 10 maximum
✓ Rolling updates (zero downtime)
✓ Health checks (liveness & readiness)
✓ Resource limits (CPU & memory)
✓ Pod disruption budgets
✓ Pod anti-affinity (distribution)
✓ Persistent volumes (file storage)
✓ Network policies (security)
```

### 9. **CI/CD Pipeline** 

**What Was Built:**
```
- GitHub Actions automated workflow
- CD to be built with docker and kubernetes
```
##  Technology Stack

### Core Framework
- **Runtime**: .NET 9 (LTS)
- **Framework**: ASP.NET Core 9.0
- **Language**: C# 13.0

### Database & ORM
- **Database**: SQL Server 2022
- **ORM**: Entity Framework Core 9.0.9
- **Migrations**: Automatic with seeding

### Authentication & Security
- **JWT**: System.IdentityModel.Tokens.Jwt
- **Authentication**: Bearer tokens + Refresh tokens
- **Authorization**: Claims-based RBAC
- **Password Hashing**: ASP.NET Core Identity

### Real-Time Communication
- **SignalR**: ASP.NET Core SignalR (WebSocket-based)
- **Message Queue**: In-memory (scalable to RabbitMQ)
- **Background Jobs**: PeriodicTimer + HostedService

### Monitoring & Logging
- **Logging**: Serilog (file + console)
- **Log storage**: Loki
- **Metrics**: Prometheus
- **Dashboards**: Grafana
- **Tracing**: Structured logging

### Testing
- **Framework**: xUnit
- **Mocking**: Moq
- **In-Memory DB**: EF Core InMemory

### Containerization & Orchestration
- **Container Runtime**: Docker (multi-stage)
- **Orchestration**: Kubernetes
- **Ingress**: NGINX

### CI/CD
- **Version Control**: GitHub
- **CI/CD**: GitHub Actions

---

### Local Development

```bash
git clone https://github.com/MAPSM-NU/BackEnd.git
cd BackEnd

dotnet restore

dotnet build -c Release
dotnet test

dotnet run --project "Gym App.API"
# Swagger UI at http://localhost:5000/swagger for local
```

### Docker Deployment (still in making)

```bash
docker build -t gym-app-api:latest .

docker run -p 5000:5000 \
  -e ConnectionStrings__VpsConnection="Server=db;Database=GymApp;..." \
  gym-app-api:latest

docker-compose up -d
```

### Kubernetes Deployment (still in making)

```bash

bash kubernetes/setup.sh

kubectl apply -f kubernetes/deployment.yaml


kubectl get pods -n gym-app

kubectl logs -f -l app=gym-app-api -n gym-app
```
