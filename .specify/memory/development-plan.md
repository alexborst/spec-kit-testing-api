# Movie Preferences API - Development Plan

**Plan Version**: 1.0.0  
**Created**: 2025-12-11  
**Status**: Approved  
**Based on**: Constitution v2.0.0, System Specification v1.0.0 + clarifications  
**Constitutional Compliance**: ✅ Verified

## Plan Overview

This development plan outlines the complete implementation strategy for the Movie Preferences API using C# Azure Functions with .NET 8. The plan follows constitutional principles of domain-driven design, stateless architecture, and clean separation of concerns while implementing JSON file-based persistence with future Azure migration capabilities.

## Architecture Plan

### Project Structure
```
src/
├── MoviePreferences.Api/                    # Azure Functions project
│   ├── Functions/                          # HTTP trigger functions
│   │   ├── MovieCatalogFunctions.cs
│   │   ├── UserSeenMovieFunctions.cs
│   │   └── MovieRatingFunctions.cs
│   ├── Models/                             # Request/Response DTOs
│   │   ├── Requests/
│   │   ├── Responses/
│   │   └── Common/
│   ├── host.json                           # Azure Functions configuration
│   ├── local.settings.json                # Local development settings
│   └── MoviePreferences.Api.csproj
├── MoviePreferences.Domain/                # Domain layer
│   ├── Entities/                          # Domain entities
│   │   ├── Movie.cs
│   │   ├── UserSeenMovie.cs
│   │   └── UserMovieRating.cs
│   ├── Services/                          # Domain services
│   │   ├── IMovieCatalogService.cs
│   │   ├── IUserSeenMovieService.cs
│   │   └── IMovieRatingService.cs
│   ├── Validators/                        # Domain validation
│   │   ├── MovieValidator.cs
│   │   ├── UserSeenMovieValidator.cs
│   │   └── MovieRatingValidator.cs
│   ├── Exceptions/                        # Domain-specific exceptions
│   └── MoviePreferences.Domain.csproj
├── MoviePreferences.Infrastructure/        # Infrastructure layer
│   ├── Repositories/                      # Data access implementations
│   │   ├── IMovieRepository.cs
│   │   ├── IUserSeenMovieRepository.cs
│   │   ├── IMovieRatingRepository.cs
│   │   └── JsonImplementations/           # JSON file storage
│   │       ├── JsonMovieRepository.cs
│   │       ├── JsonUserSeenMovieRepository.cs
│   │       └── JsonMovieRatingRepository.cs
│   ├── Services/                          # Application services
│   │   ├── MovieCatalogService.cs
│   │   ├── UserSeenMovieService.cs
│   │   └── MovieRatingService.cs
│   ├── Configuration/                     # Dependency injection setup
│   │   └── ServiceCollectionExtensions.cs
│   ├── Data/                              # Data storage and seeding
│   │   ├── SeedData/
│   │   │   └── movies.json                # Initial movie catalog
│   │   └── Storage/                       # Runtime data files
│   │       ├── user-seen-movies.json
│   │       └── movie-ratings.json
│   └── MoviePreferences.Infrastructure.csproj
└── MoviePreferences.sln                    # Solution file
```

### Layered Architecture Implementation

#### 1. Azure Functions Layer (API Boundary)
- **Responsibility**: HTTP request/response handling, routing, CORS
- **Components**: Function classes with HTTP triggers
- **Dependencies**: Infrastructure services via dependency injection
- **Constraints**: No business logic, stateless execution

#### 2. Domain Layer (Business Logic)
- **Responsibility**: Business rules, domain entities, validation
- **Components**: Entities, domain services, validators, exceptions
- **Dependencies**: No external dependencies (pure domain logic)
- **Constraints**: Framework-agnostic, rich domain models

#### 3. Infrastructure Layer (Implementation Details)
- **Responsibility**: Data access, external service integration, application services
- **Components**: Repositories, application services, configuration
- **Dependencies**: Domain layer, JSON file system, Azure SDK (future)
- **Constraints**: Implements domain contracts, persistence-ignorant

## Domain Model Implementation

### Core Entities

#### Movie Entity
```csharp
public sealed record Movie
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public int ReleaseYear { get; init; }
    public string Genre { get; init; } = string.Empty;
    public string? Director { get; init; }
    public string? Synopsis { get; init; }
    public int? RuntimeMinutes { get; init; }
    
    // Domain behavior methods
    public bool IsValidGenre() => SupportedGenres.Contains(Genre);
    public bool IsValidReleaseYear() => ReleaseYear >= 1900 && ReleaseYear <= DateTime.UtcNow.Year + 2;
}
```

#### UserSeenMovie Entity
```csharp
public sealed record UserSeenMovie
{
    public string UserId { get; init; } = string.Empty;
    public string MovieId { get; init; } = string.Empty;
    public DateTime DateSeen { get; init; }
    public string? Notes { get; init; }
    public DateTime AddedAt { get; init; }
    
    // Domain behavior
    public bool IsValidDateSeen() => DateSeen <= DateTime.UtcNow;
    public bool HasValidNotes() => string.IsNullOrWhiteSpace(Notes) || Notes.Length <= 1000;
}
```

#### UserMovieRating Entity
```csharp
public sealed record UserMovieRating
{
    public string UserId { get; init; } = string.Empty;
    public string MovieId { get; init; } = string.Empty;
    public int Rating { get; init; }
    public string? Review { get; init; }
    public DateTime RatedAt { get; init; }
    
    // Domain behavior
    public bool IsValidRating() => Rating >= 1 && Rating <= 5;
    public bool HasValidReview() => string.IsNullOrWhiteSpace(Review) || Review.Length <= 2000;
}
```

## API Endpoint Implementation Plan

### Function Group 1: Movie Catalog Functions

#### GetMovieCatalog Function
- **Route**: `GET /api/movies`
- **Function Name**: `GetMovieCatalog`
- **Query Parameters**: page, pageSize, genre
- **Response**: Paginated movie list with metadata
- **Dependencies**: IMovieCatalogService

#### GetMovieById Function
- **Route**: `GET /api/movies/{movieId}`
- **Function Name**: `GetMovieById`
- **Path Parameters**: movieId
- **Response**: Single movie details
- **Dependencies**: IMovieCatalogService

### Function Group 2: User Seen Movie Functions

#### AddMovieToSeenList Function
- **Route**: `POST /api/users/{userId}/seen-movies`
- **Function Name**: `AddMovieToSeenList`
- **Request Body**: AddSeenMovieRequest
- **Response**: UserSeenMovieResponse (201 Created)
- **Dependencies**: IUserSeenMovieService, IMovieCatalogService

#### GetUserSeenList Function
- **Route**: `GET /api/users/{userId}/seen-movies`
- **Function Name**: `GetUserSeenList`
- **Query Parameters**: page, pageSize, includeRatings, sortBy, sortOrder
- **Response**: Paginated seen movies with optional ratings
- **Dependencies**: IUserSeenMovieService, IMovieRatingService

### Function Group 3: Movie Rating Functions

#### SubmitMovieRating Function
- **Route**: `POST /api/users/{userId}/movie-ratings`
- **Function Name**: `SubmitMovieRating`
- **Request Body**: SubmitRatingRequest
- **Response**: UserMovieRatingResponse (201/200)
- **Dependencies**: IMovieRatingService, IUserSeenMovieService

#### GetUserMovieRatings Function
- **Route**: `GET /api/users/{userId}/movie-ratings`
- **Function Name**: `GetUserMovieRatings`
- **Query Parameters**: page, pageSize, minRating, maxRating, sortBy, sortOrder
- **Response**: Paginated ratings with summary statistics
- **Dependencies**: IMovieRatingService

## Data Persistence Strategy

### JSON File Storage Implementation

#### Storage Architecture
```
Data/
├── Storage/                               # Runtime data files
│   ├── user-seen-movies.json            # User viewing history
│   ├── movie-ratings.json               # User ratings and reviews
│   └── app-state.json                   # Application metadata
└── SeedData/                            # Initial/reference data
    └── movies.json                      # Movie catalog (read-only)
```

#### Repository Pattern Implementation

##### Base Repository Interface
```csharp
public interface IRepository<T, TKey>
{
    Task<T?> GetByIdAsync(TKey id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(TKey id);
}
```

##### JSON Repository Base Class
```csharp
public abstract class JsonRepositoryBase<T>
{
    protected readonly string FilePath;
    protected readonly JsonSerializerOptions JsonOptions;
    
    protected async Task<List<T>> LoadDataAsync();
    protected async Task SaveDataAsync(List<T> data);
    protected abstract string GetEntityId(T entity);
}
```

#### Migration Readiness
- Repository interfaces abstract storage implementation
- Azure Table/Cosmos implementations can replace JSON repos
- Configuration-driven storage provider selection
- Data migration utilities for file → cloud transition

## Validation and Error Handling Plan

### Validation Strategy

#### Input Validation Pipeline
1. **HTTP Parameter Validation**: Route and query parameter binding
2. **Request Body Validation**: JSON deserialization and model validation
3. **Domain Validation**: Business rule enforcement
4. **Cross-Entity Validation**: Referential integrity checks

#### Validator Implementation
```csharp
public interface IDomainValidator<T>
{
    ValidationResult Validate(T entity);
    Task<ValidationResult> ValidateAsync(T entity);
}

public class ValidationResult
{
    public bool IsValid { get; init; }
    public List<ValidationError> Errors { get; init; } = new();
}
```

### Error Handling Architecture

#### Exception Hierarchy
```
DomainException (base)
├── ValidationException
├── EntityNotFoundException
├── BusinessRuleViolationException
└── ConflictException
```

#### Global Error Handler
- Middleware for consistent error response formatting
- HTTP status code mapping from domain exceptions
- Structured error responses with correlation IDs
- Logging integration with Azure Application Insights

#### Error Response Format
```csharp
public record ErrorResponse
{
    public ErrorDetail Error { get; init; }
}

public record ErrorDetail
{
    public string Code { get; init; }
    public string Message { get; init; }
    public List<FieldError> Details { get; init; } = new();
    public DateTime Timestamp { get; init; }
    public string TraceId { get; init; }
}
```

## Testing Approach

### Testing Strategy (Future Implementation)
Per constitutional principle V, no testing infrastructure will be implemented in this phase. However, the architecture will support future testing:

#### Testability Design Patterns
- **Dependency Injection**: All dependencies injected via interfaces
- **Repository Pattern**: Data access abstracted for easy mocking
- **Pure Domain Logic**: Business rules in testable domain services
- **Stateless Functions**: Predictable input/output behavior

#### Future Testing Structure
```
tests/                                     # Future test implementation
├── MoviePreferences.Api.Tests/           # Integration tests
├── MoviePreferences.Domain.Tests/        # Unit tests
└── MoviePreferences.Infrastructure.Tests/ # Repository tests
```

## Implementation Milestones

### Milestone 1: Foundation Setup (Days 1-2)
**Deliverables**:
- ✅ Solution structure and project creation
- ✅ Domain entity definitions
- ✅ Repository interfaces
- ✅ Basic dependency injection configuration
- ✅ Azure Functions project setup with HTTP triggers

**Acceptance Criteria**:
- All projects compile successfully
- Domain entities have proper validation methods
- Repository contracts defined
- Basic function stubs respond to HTTP requests

### Milestone 2: Movie Catalog Implementation (Days 3-4)
**Deliverables**:
- ✅ Movie catalog seed data creation (20+ sample movies)
- ✅ JSON movie repository implementation
- ✅ Movie catalog service with business logic
- ✅ GetMovieCatalog and GetMovieById functions
- ✅ Pagination and filtering logic

**Acceptance Criteria**:
- Movie catalog endpoints return proper responses
- Pagination works correctly with metadata
- Genre filtering functions as specified
- Error handling for invalid movie IDs

### Milestone 3: User Seen List Implementation (Days 5-6)
**Deliverables**:
- ✅ UserSeenMovie repository and service
- ✅ AddMovieToSeenList function with validation
- ✅ GetUserSeenList function with optional ratings
- ✅ Business rule enforcement (movie existence, duplicates)

**Acceptance Criteria**:
- Users can add movies to seen list
- Duplicate prevention works correctly
- Seen list retrieval includes movie details
- Proper error responses for invalid operations

### Milestone 4: Movie Rating Implementation (Days 7-8)
**Deliverables**:
- ✅ UserMovieRating repository and service
- ✅ SubmitMovieRating function with prerequisite validation
- ✅ GetUserMovieRatings function with filtering
- ✅ Rating summary statistics calculation

**Acceptance Criteria**:
- Users can only rate seen movies
- Rating scale (1-5) enforced correctly
- Rating updates work properly
- Rating statistics calculated accurately

### Milestone 5: Integration and Polish (Days 9-10)
**Deliverables**:
- ✅ Complete error handling implementation
- ✅ OpenAPI documentation generation
- ✅ CORS configuration for React frontend
- ✅ Performance optimization and validation
- ✅ Deployment preparation

**Acceptance Criteria**:
- All endpoints respond within performance targets
- OpenAPI spec accurately reflects all endpoints
- Error responses follow consistent format
- CORS allows React frontend integration

## Development Sequence

### Phase 1: Core Infrastructure
1. Set up solution structure and projects
2. Define domain entities with validation
3. Create repository interfaces and base classes
4. Configure dependency injection container
5. Set up Azure Functions with basic routing

### Phase 2: Data Layer
1. Implement JSON storage utilities
2. Create movie seed data (diverse genres, years)
3. Implement movie repository with file operations
4. Add basic error handling and logging
5. Test data persistence and retrieval

### Phase 3: Movie Catalog Features
1. Implement movie catalog service logic
2. Build GetMovieCatalog function with pagination
3. Build GetMovieById function with validation
4. Add genre filtering and sorting
5. Implement error responses and status codes

### Phase 4: User Interactions
1. Implement seen movie repository and service
2. Build AddMovieToSeenList with business rules
3. Build GetUserSeenList with movie details
4. Add movie rating repository and service
5. Build rating submission with prerequisites

### Phase 5: Advanced Features
1. Implement rating retrieval with filtering
2. Add rating statistics and analytics
3. Complete error handling middleware
4. Generate OpenAPI documentation
5. Optimize performance and add caching headers

### Phase 6: Integration Readiness
1. Configure CORS for frontend integration
2. Set up Azure Functions deployment configuration
3. Create deployment documentation
4. Validate constitutional compliance
5. Prepare for React frontend integration

## Risk Mitigation

### Technical Risks
- **File Concurrency**: JSON file access in concurrent scenarios
  - *Mitigation*: File locking, eventual consistency patterns
- **Data Corruption**: Invalid JSON or file system issues
  - *Mitigation*: Data validation, backup strategies, recovery procedures
- **Performance**: Large datasets in JSON files
  - *Mitigation*: Lazy loading, pagination, migration planning

### Architectural Risks
- **Tight Coupling**: Dependencies between layers
  - *Mitigation*: Strict interface-based design, dependency injection
- **Azure Migration**: Difficulty transitioning from JSON to cloud storage
  - *Mitigation*: Repository abstraction, configuration-driven providers

## Success Criteria

### Functional Success
- ✅ All specified API endpoints operational
- ✅ Business rules properly enforced
- ✅ Data persistence working correctly
- ✅ Error handling comprehensive and consistent
- ✅ Performance targets met for all endpoints

### Technical Success
- ✅ Constitutional compliance verified
- ✅ Clean architecture principles followed
- ✅ Future testability maintained
- ✅ Azure migration readiness achieved
- ✅ OpenAPI documentation complete

### Integration Success
- ✅ CORS configured for React frontend
- ✅ JSON responses properly formatted
- ✅ Error responses suitable for client handling
- ✅ API contracts stable and documented

**Next Phase**: Ready to proceed to `/tasks` for detailed task breakdown and `/implement` for code generation.