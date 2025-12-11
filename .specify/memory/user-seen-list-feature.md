# Feature Specification: User Seen List

**Feature ID**: `USER-SEEN-LIST-001`  
**Feature Name**: User Seen List Management  
**Version**: 1.0.0  
**Created**: 2025-12-10  
**Status**: Draft  
**Dependencies**: System Specification v1.0.0, Movie Catalog Feature v1.0.0  
**Constitutional Compliance**: ✅ Verified

## Feature Overview

The User Seen List feature enables users to track movies they have watched by maintaining a personal "seen" list. Users can add movies to their seen list and retrieve their complete viewing history. This feature serves as the prerequisite for movie rating functionality and provides the foundation for future recommendation features.

## Functional Requirements

### FR-USL-001: Add Movie to Seen List
**Description**: Users can mark a movie as "seen" by adding it to their personal seen list with optional viewing date and notes.

**Acceptance Criteria**:
- System accepts valid movie ID and user ID to create seen record
- Optional viewing date defaults to current UTC time if not provided
- Optional personal notes can be included with the seen record
- Duplicate entries for same user-movie combination are prevented
- System validates movie exists in catalog before allowing addition

### FR-USL-002: Retrieve User's Seen List
**Description**: Users can retrieve their complete list of seen movies with pagination and optional detailed movie information.

**Acceptance Criteria**:
- System returns paginated list of user's seen movies
- Results include basic seen record data (date, notes) and movie details
- Optional parameter to include movie rating data when available
- Results ordered by date seen (most recent first)
- Empty lists return valid response structure

## API Specification

### Endpoint 1: Add Movie to Seen List

**Function Name**: `AddMovieToSeenList`  
**HTTP Method**: `POST`  
**Route**: `/api/users/{userId}/seen-movies`  
**Trigger Type**: HttpTrigger  
**Authorization Level**: Anonymous

#### Request Parameters

**Path Parameters**:
```typescript
{
  userId: string;       // User unique identifier (required)
}
```

**Request Body Schema**:
```json
{
  "movieId": "string",                    // Required, must exist in catalog
  "dateSeen": "2025-12-10T14:30:00Z",     // Optional, ISO 8601 format
  "notes": "string"                       // Optional, personal viewing notes
}
```

**Parameter Validation**:
- `userId`: Must be valid GUID format, non-empty
- `movieId`: Must be non-empty string, must reference existing movie
- `dateSeen`: Must be valid ISO 8601 datetime, cannot be in future, defaults to current UTC
- `notes`: Optional string, 1-1000 characters when provided

#### Response Schema

**Success Response (201 Created)**:
```json
{
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "movieId": "mov_001",
  "dateSeen": "2025-12-10T14:30:00Z",
  "notes": "Great movie, loved the plot twists!",
  "movie": {
    "id": "mov_001",
    "title": "The Matrix",
    "releaseYear": 1999,
    "genre": "Sci-Fi",
    "director": "The Wachowskis",
    "synopsis": "A computer programmer discovers reality is a simulation...",
    "runtimeMinutes": 136
  },
  "addedAt": "2025-12-10T14:35:00Z"
}
```

**Response Headers**:
```
Location: /api/users/123e4567-e89b-12d3-a456-426614174000/seen-movies/mov_001
Content-Type: application/json
```

#### Error Responses

**400 Bad Request** (Invalid Request):
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid request data",
    "details": [
      {
        "field": "userId",
        "message": "User ID must be a valid GUID"
      },
      {
        "field": "dateSeen",
        "message": "Date seen cannot be in the future"
      }
    ],
    "timestamp": "2025-12-10T14:35:00Z",
    "traceId": "abc123"
  }
}
```

**404 Not Found** (Movie Not Exists):
```json
{
  "error": {
    "code": "NOT_FOUND",
    "message": "Referenced movie not found",
    "details": [
      {
        "field": "movieId",
        "message": "No movie exists with ID: mov_999"
      }
    ],
    "timestamp": "2025-12-10T14:35:00Z",
    "traceId": "def456"
  }
}
```

**409 Conflict** (Already Seen):
```json
{
  "error": {
    "code": "CONFLICT",
    "message": "Movie already marked as seen",
    "details": [
      {
        "field": "movieId",
        "message": "User has already marked this movie as seen on 2025-12-01"
      }
    ],
    "timestamp": "2025-12-10T14:35:00Z",
    "traceId": "ghi789"
  }
}
```

### Endpoint 2: Get User's Seen List

**Function Name**: `GetUserSeenList`  
**HTTP Method**: `GET`  
**Route**: `/api/users/{userId}/seen-movies`  
**Trigger Type**: HttpTrigger  
**Authorization Level**: Anonymous

#### Request Parameters

**Path Parameters**:
```typescript
{
  userId: string;       // User unique identifier (required)
}
```

**Query Parameters**:
```typescript
{
  page?: number;            // Page number (default: 1, min: 1)
  pageSize?: number;        // Results per page (default: 20, min: 1, max: 100)
  includeRatings?: boolean; // Include rating data (default: false)
  sortBy?: string;          // Sort order: "dateSeen" | "title" (default: "dateSeen")
  sortOrder?: string;       // "asc" | "desc" (default: "desc")
}
```

**Parameter Validation**:
- `userId`: Must be valid GUID format, non-empty
- `page`: Must be positive integer, defaults to 1
- `pageSize`: Must be between 1-100 inclusive, defaults to 20
- `includeRatings`: Boolean flag, defaults to false
- `sortBy`: Must be "dateSeen" or "title", defaults to "dateSeen"
- `sortOrder`: Must be "asc" or "desc", defaults to "desc"

#### Response Schema

**Success Response (200 OK)**:
```json
{
  "seenMovies": [
    {
      "userId": "123e4567-e89b-12d3-a456-426614174000",
      "movieId": "mov_001",
      "dateSeen": "2025-12-10T14:30:00Z",
      "notes": "Great movie, loved the plot twists!",
      "movie": {
        "id": "mov_001",
        "title": "The Matrix",
        "releaseYear": 1999,
        "genre": "Sci-Fi",
        "director": "The Wachowskis",
        "synopsis": "A computer programmer discovers reality is a simulation...",
        "runtimeMinutes": 136
      },
      "rating": {                           // Only when includeRatings=true
        "rating": 9,
        "review": "Mind-blowing concepts and amazing special effects!",
        "ratedAt": "2025-12-10T15:00:00Z"
      }
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalCount": 45,
    "totalPages": 3,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

#### Error Responses

**400 Bad Request** (Invalid Parameters):
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid query parameters",
    "details": [
      {
        "field": "userId",
        "message": "User ID must be a valid GUID"
      },
      {
        "field": "sortBy",
        "message": "Sort field must be 'dateSeen' or 'title'"
      }
    ],
    "timestamp": "2025-12-10T14:35:00Z",
    "traceId": "jkl012"
  }
}
```

**200 OK** (Empty List):
```json
{
  "seenMovies": [],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalCount": 0,
    "totalPages": 0,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
}
```

## Data Schema

### UserSeenMovie Entity Schema

```csharp
public sealed record UserSeenMovie
{
    public string UserId { get; init; }                // Required, GUID format
    public string MovieId { get; init; }               // Required, foreign key to Movie
    public DateTime DateSeen { get; init; }            // Required, UTC datetime
    public string? Notes { get; init; }                // Optional, 1-1000 characters
    public DateTime AddedAt { get; init; }             // Required, record creation timestamp
}
```

### UserSeenMovieResponse Schema

```csharp
public sealed record UserSeenMovieResponse
{
    public string UserId { get; init; }
    public string MovieId { get; init; }
    public DateTime DateSeen { get; init; }
    public string? Notes { get; init; }
    public Movie Movie { get; init; }                   // Joined movie details
    public UserMovieRating? Rating { get; init; }      // Optional rating data
}
```

### AddSeenMovieRequest Schema

```csharp
public sealed record AddSeenMovieRequest
{
    public string MovieId { get; init; }               // Required
    public DateTime? DateSeen { get; init; }           // Optional, defaults to UtcNow
    public string? Notes { get; init; }                // Optional
}
```

## Business Rules

### BR-USL-001: Unique Seen Records
- Each user can mark a movie as seen only once
- Composite primary key: (UserId, MovieId)
- Duplicate attempts return 409 Conflict
- Updates to existing records not supported (create new API for modifications)

### BR-USL-002: Movie Catalog Dependency
- MovieId must reference existing movie in catalog
- Movie existence validated before creating seen record
- Invalid movie references return 404 Not Found
- Seen records maintain referential integrity

### BR-USL-003: Date Validation
- DateSeen cannot be in the future
- DateSeen defaults to current UTC time when not provided
- AddedAt always set to current UTC time (system controlled)
- All timestamps stored and returned in UTC format

### BR-USL-004: Notes Management
- Notes are optional personal annotations
- Notes length limited to 1000 characters
- Empty or whitespace-only notes treated as null
- Notes content not validated beyond length constraints

### BR-USL-005: Default Sorting
- Default sort: DateSeen descending (most recent first)
- Secondary sort: Movie title ascending for same dates
- Alternative sort by movie title available
- Sorting applied before pagination

## Validation Rules

### Request Validation
- UserId must be valid GUID format (36 characters with hyphens)
- MovieId must be non-empty string, trimmed of whitespace
- DateSeen must be valid ISO 8601 format when provided
- Notes must be 1-1000 characters when provided
- Request body must be valid JSON

### Business Validation
- Movie must exist in catalog before adding to seen list
- DateSeen cannot exceed current UTC time
- UserId + MovieId combination must be unique
- All required fields must be present and valid

## Performance Requirements

### Response Time
- Add movie to seen list: < 300ms for 95th percentile
- Retrieve seen list (20 items): < 400ms for 95th percentile
- Large seen lists (100 items): < 800ms for 95th percentile

### Throughput
- Add seen movie: 500 requests per minute per user
- Get seen list: 1000 requests per minute per user
- Support 50 concurrent operations per user

### Data Volume
- Support users with 10,000+ seen movies
- Efficient pagination for large datasets
- Database indexing on (UserId, DateSeen) for performance

## Dependencies

### External Dependencies
- Movie catalog repository for movie validation
- User seen movie repository for persistence
- Movie rating repository (when includeRatings=true)
- Logging service for audit trail

### Internal Dependencies
- Movie catalog feature for movie existence validation
- Domain model validation services
- Pagination utilities
- DateTime handling utilities

## Error Handling

### Validation Errors
- Invalid GUID format returns 400 with specific field errors
- Missing required fields return 400 with detailed validation messages
- Invalid date formats return 400 with parsing guidance

### Business Rule Violations
- Non-existent movies return 404 with clear movie reference error
- Duplicate seen records return 409 with existing record details
- Future dates return 400 with temporal validation message

### System Errors
- Repository failures return 500 with generic error message
- Database constraint violations return 500 with trace ID
- Network timeouts return 500 with retry guidance

## Testing Considerations

### Test Scenarios
1. **Happy Path**: Valid movie additions and list retrieval
2. **Duplicate Prevention**: Attempt to add same movie twice
3. **Movie Validation**: Add non-existent movie to seen list
4. **Date Boundaries**: Current time, past dates, future dates (invalid)
5. **Pagination**: Large seen lists, empty lists, boundary conditions
6. **Performance**: Concurrent additions, large dataset retrieval

### Mock Data Requirements
- Test users with varying seen list sizes (0, 1, 50, 1000+ movies)
- Mix of movies with and without ratings
- Various date ranges for comprehensive sorting tests
- Edge cases: very long notes, special characters in notes

## Integration Points

### Upstream Dependencies
- Movie Catalog API: Movie existence validation
- User context: UserId validation (mock authentication)

### Downstream Consumers
- Movie Rating Feature: Seen list as prerequisite for rating
- Future Analytics: User viewing behavior tracking
- Frontend Applications: User movie management interfaces

## Future Enhancements

### Planned Features
- Update seen movie notes and date
- Remove movies from seen list
- Bulk operations for multiple movies
- Export seen list functionality
- Advanced filtering by genre, year, rating status

### API Versioning
- Current implementation: v1
- Backward compatibility for seen list structure
- Additive changes preferred over breaking modifications

**Dependencies for Next Features**: This specification enables movie rating functionality which requires movies to be marked as seen before rating submission.