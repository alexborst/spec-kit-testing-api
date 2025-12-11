# Feature Specification: Movie Ratings

**Feature ID**: `MOVIE-RATINGS-001`  
**Feature Name**: Movie Rating Management  
**Version**: 1.0.0  
**Created**: 2025-12-10  
**Status**: Draft  
**Dependencies**: System Specification v1.0.0, Movie Catalog Feature v1.0.0, User Seen List Feature v1.0.0  
**Constitutional Compliance**: ✅ Verified

## Feature Overview

The Movie Ratings feature enables users to rate movies on a 1-5 scale and optionally provide written reviews. Users can only rate movies they have previously marked as seen, enforcing the business rule that ratings require viewing experience. The system supports rating updates (overwrites) and provides comprehensive rating retrieval capabilities.

## Functional Requirements

### FR-MR-001: Submit Movie Rating
**Description**: Users can submit numerical ratings (1-5) and optional written reviews for movies they have marked as seen.

**Acceptance Criteria**:
- System accepts ratings only for movies in user's seen list
- Rating scale is 1-5 integer values (no decimals)
- Optional written review can accompany the rating
- Existing ratings are overwritten with new values
- System tracks when ratings were submitted and last updated

### FR-MR-002: Retrieve User's Movie Ratings
**Description**: Users can retrieve their complete rating history with pagination and filtering options.

**Acceptance Criteria**:
- System returns paginated list of user's movie ratings
- Results include rating data, movie details, and seen movie information
- Support filtering by rating value ranges
- Results ordered by rating date (most recent first) by default
- Empty rating lists return valid response structure

## API Specification

### Endpoint 1: Submit Movie Rating

**Function Name**: `SubmitMovieRating`  
**HTTP Method**: `POST`  
**Route**: `/api/users/{userId}/movie-ratings`  
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
  "movieId": "string",              // Required, must exist in user's seen list
  "rating": 4,                      // Required, integer 1-5
  "review": "string"                // Optional, written review text
}
```

**Parameter Validation**:
- `userId`: Must be valid GUID format, non-empty
- `movieId`: Must be non-empty string, must exist in user's seen list
- `rating`: Must be integer between 1-5 inclusive
- `review`: Optional string, 1-2000 characters when provided

#### Response Schema

**Success Response (201 Created)** - New Rating:
```json
{
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "movieId": "mov_001",
  "rating": 4,
  "review": "Great movie with amazing visual effects and compelling story.",
  "ratedAt": "2025-12-10T15:30:00Z",
  "movie": {
    "id": "mov_001",
    "title": "The Matrix",
    "releaseYear": 1999,
    "genre": "Sci-Fi",
    "director": "The Wachowskis",
    "synopsis": "A computer programmer discovers reality is a simulation...",
    "runtimeMinutes": 136
  },
  "seenMovie": {
    "dateSeen": "2025-12-08T20:00:00Z",
    "notes": "Watched with friends at home"
  }
}
```

**Success Response (200 OK)** - Updated Rating:
```json
{
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "movieId": "mov_001",
  "rating": 5,
  "review": "Updated: Even better on second viewing!",
  "ratedAt": "2025-12-10T15:30:00Z",
  "previousRating": {
    "rating": 4,
    "ratedAt": "2025-12-09T10:15:00Z"
  },
  "movie": {
    "id": "mov_001",
    "title": "The Matrix",
    "releaseYear": 1999,
    "genre": "Sci-Fi",
    "director": "The Wachowskis",
    "synopsis": "A computer programmer discovers reality is a simulation...",
    "runtimeMinutes": 136
  },
  "seenMovie": {
    "dateSeen": "2025-12-08T20:00:00Z",
    "notes": "Watched with friends at home"
  }
}
```

#### Error Responses

**400 Bad Request** (Invalid Rating):
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid rating data",
    "details": [
      {
        "field": "rating",
        "message": "Rating must be an integer between 1 and 5"
      },
      {
        "field": "review",
        "message": "Review cannot exceed 2000 characters"
      }
    ],
    "timestamp": "2025-12-10T15:30:00Z",
    "traceId": "abc123"
  }
}
```

**404 Not Found** (Movie Not in Seen List):
```json
{
  "error": {
    "code": "NOT_FOUND",
    "message": "Movie not found in user's seen list",
    "details": [
      {
        "field": "movieId",
        "message": "User must mark movie as seen before rating. Movie ID: mov_001"
      }
    ],
    "timestamp": "2025-12-10T15:30:00Z",
    "traceId": "def456"
  }
}
```

**409 Conflict** (Business Rule Violation):
```json
{
  "error": {
    "code": "CONFLICT",
    "message": "Cannot rate movie not marked as seen",
    "details": [
      {
        "field": "movieId",
        "message": "Add movie to seen list before submitting rating"
      }
    ],
    "timestamp": "2025-12-10T15:30:00Z",
    "traceId": "ghi789"
  }
}
```

### Endpoint 2: Get User's Movie Ratings

**Function Name**: `GetUserMovieRatings`  
**HTTP Method**: `GET`  
**Route**: `/api/users/{userId}/movie-ratings`  
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
  minRating?: number;       // Filter by minimum rating (1-5)
  maxRating?: number;       // Filter by maximum rating (1-5)
  sortBy?: string;          // Sort field: "ratedAt" | "rating" | "title" (default: "ratedAt")
  sortOrder?: string;       // "asc" | "desc" (default: "desc")
  includeReview?: boolean;  // Include review text (default: true)
}
```

**Parameter Validation**:
- `userId`: Must be valid GUID format, non-empty
- `page`: Must be positive integer, defaults to 1
- `pageSize`: Must be between 1-100 inclusive, defaults to 20
- `minRating`: Must be integer 1-5 when provided
- `maxRating`: Must be integer 1-5 when provided, must be >= minRating
- `sortBy`: Must be "ratedAt", "rating", or "title", defaults to "ratedAt"
- `sortOrder`: Must be "asc" or "desc", defaults to "desc"
- `includeReview`: Boolean flag, defaults to true

#### Response Schema

**Success Response (200 OK)**:
```json
{
  "ratings": [
    {
      "userId": "123e4567-e89b-12d3-a456-426614174000",
      "movieId": "mov_001",
      "rating": 5,
      "review": "Absolutely brilliant! Mind-bending concepts.",
      "ratedAt": "2025-12-10T15:30:00Z",
      "movie": {
        "id": "mov_001",
        "title": "The Matrix",
        "releaseYear": 1999,
        "genre": "Sci-Fi",
        "director": "The Wachowskis",
        "synopsis": "A computer programmer discovers reality is a simulation...",
        "runtimeMinutes": 136
      },
      "seenMovie": {
        "dateSeen": "2025-12-08T20:00:00Z",
        "notes": "Watched with friends at home"
      }
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalCount": 12,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "summary": {
    "totalRatings": 12,
    "averageRating": 4.2,
    "ratingDistribution": {
      "1": 0,
      "2": 1,
      "3": 2,
      "4": 6,
      "5": 3
    }
  }
}
```

#### Error Responses

**400 Bad Request** (Invalid Filters):
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid filter parameters",
    "details": [
      {
        "field": "minRating",
        "message": "Minimum rating must be between 1 and 5"
      },
      {
        "field": "maxRating",
        "message": "Maximum rating must be greater than or equal to minimum rating"
      }
    ],
    "timestamp": "2025-12-10T15:30:00Z",
    "traceId": "jkl012"
  }
}
```

**200 OK** (Empty Ratings):
```json
{
  "ratings": [],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalCount": 0,
    "totalPages": 0,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "summary": {
    "totalRatings": 0,
    "averageRating": null,
    "ratingDistribution": {
      "1": 0,
      "2": 0,
      "3": 0,
      "4": 0,
      "5": 0
    }
  }
}
```

## Data Schema

### UserMovieRating Entity Schema

```csharp
public sealed record UserMovieRating
{
    public string UserId { get; init; }                // Required, GUID format
    public string MovieId { get; init; }               // Required, foreign key to Movie
    public int Rating { get; init; }                   // Required, 1-5 integer
    public string? Review { get; init; }               // Optional, 1-2000 characters
    public DateTime RatedAt { get; init; }             // Required, UTC timestamp of rating
}
```

### SubmitRatingRequest Schema

```csharp
public sealed record SubmitRatingRequest
{
    public string MovieId { get; init; }               // Required
    public int Rating { get; init; }                   // Required, 1-5
    public string? Review { get; init; }               // Optional
}
```

### UserMovieRatingResponse Schema

```csharp
public sealed record UserMovieRatingResponse
{
    public string UserId { get; init; }
    public string MovieId { get; init; }
    public int Rating { get; init; }
    public string? Review { get; init; }
    public DateTime RatedAt { get; init; }
    public Movie Movie { get; init; }                   // Joined movie details
    public UserSeenMovie SeenMovie { get; init; }      // Joined seen movie details
    public PreviousRating? PreviousRating { get; init; } // For updates only
}

public sealed record PreviousRating
{
    public int Rating { get; init; }
    public DateTime RatedAt { get; init; }
}
```

### RatingSummary Schema

```csharp
public sealed record RatingSummary
{
    public int TotalRatings { get; init; }
    public decimal? AverageRating { get; init; }        // Null when no ratings
    public Dictionary<int, int> RatingDistribution { get; init; }
}
```

## Business Rules

### BR-MR-001: Seen Movie Prerequisite
- Users can only rate movies in their seen list
- Rating submission requires existing UserSeenMovie record
- Invalid attempts return 409 Conflict or 404 Not Found
- Business rule enforced at application layer and database constraints

### BR-MR-002: Rating Scale Constraints
- Rating must be integer between 1-5 inclusive (no decimals, no zero)
- Rating scale represents: 1=Poor, 2=Fair, 3=Good, 4=Very Good, 5=Excellent
- Out-of-range values return 400 Bad Request
- Rating field is required (not nullable)

### BR-MR-003: Rating Updates (Overwrites)
- Users can update existing ratings by submitting new values
- Previous rating is overwritten, not versioned
- RatedAt timestamp updates to current UTC time
- Response includes previous rating information for confirmation

### BR-MR-004: Review Content Rules
- Reviews are optional text fields accompanying ratings
- Review length limited to 2000 characters
- Empty or whitespace-only reviews treated as null
- No content filtering or profanity checking (out of scope)

### BR-MR-005: Data Consistency
- Composite primary key: (UserId, MovieId)
- Foreign key constraint to UserSeenMovie table
- Cascading delete when UserSeenMovie is removed
- Referential integrity maintained at database level

## Validation Rules

### Request Validation
- UserId must be valid GUID format (36 characters with hyphens)
- MovieId must be non-empty string, trimmed of whitespace
- Rating must be integer 1-5 (no floats, no out-of-range values)
- Review must be 1-2000 characters when provided
- Request body must be valid JSON with correct field types

### Business Validation
- Movie must exist in user's seen list before rating
- Rating value must be within 1-5 scale
- Review length validation applied after trimming
- Filter parameters (minRating/maxRating) must be valid and logical

### Query Parameter Validation
- Pagination parameters must be positive integers within limits
- Sort fields must be from allowed list
- Rating filter values must be 1-5 integers
- maxRating must be >= minRating when both provided

## Performance Requirements

### Response Time
- Submit rating: < 250ms for 95th percentile
- Retrieve ratings (20 items): < 400ms for 95th percentile
- Large rating lists (100 items): < 800ms for 95th percentile

### Throughput
- Submit rating: 200 requests per minute per user
- Get ratings: 1000 requests per minute per user
- Support 100 concurrent rating operations across all users

### Data Volume
- Support users with 5,000+ movie ratings
- Efficient pagination for large rating datasets
- Database indexing on (UserId, RatedAt) for performance

## Dependencies

### External Dependencies
- User Seen List repository for prerequisite validation
- Movie catalog repository for movie details
- Movie rating repository for persistence
- Logging service for rating audit trail

### Internal Dependencies
- User Seen List Feature: Prerequisite validation
- Movie Catalog Feature: Movie detail enrichment
- Domain model validation services
- Pagination and sorting utilities

## Error Handling

### Validation Errors
- Invalid rating scale returns 400 with specific range guidance
- Missing required fields return 400 with field-specific errors
- Invalid GUID formats return 400 with format requirements

### Business Rule Violations
- Rating non-seen movies returns 409 with clear prerequisite message
- Non-existent movies return 404 with movie reference error
- Missing seen movie relationship returns dedicated error response

### System Errors
- Repository failures return 500 with generic error message
- Database constraint violations return 500 with trace ID
- Concurrent rating updates handled gracefully (last write wins)

## Testing Considerations

### Test Scenarios
1. **Happy Path**: Valid rating submission and retrieval
2. **Rating Updates**: Overwrite existing ratings with new values
3. **Prerequisite Validation**: Attempt rating without seen movie
4. **Scale Boundaries**: Test rating values 1, 5, and invalid values
5. **Review Lengths**: Test maximum length and empty reviews
6. **Filtering**: Test rating range filters and edge cases

### Mock Data Requirements
- Test users with varying rating counts (0, 1, 50, 1000+ ratings)
- Distribution across all rating values (1-5)
- Mix of ratings with and without reviews
- Edge cases: maximum review length, special characters

## Integration Points

### Upstream Dependencies
- User Seen List API: Prerequisite validation for rating submissions
- Movie Catalog API: Movie existence and detail retrieval

### Downstream Consumers
- User Seen List API: Optional rating inclusion in seen movie responses
- Future Analytics: Rating pattern analysis and recommendation systems
- Frontend Applications: Rating display and submission interfaces

## Future Enhancements

### Planned Features
- Rating history versioning (track rating changes over time)
- Bulk rating operations for multiple movies
- Rating comparison between users (social features)
- Advanced filtering by review content or rating date ranges
- Export rating data functionality

### API Versioning
- Current implementation: v1
- Rating scale changes would require new major version
- Review functionality could be extended additively

**Dependencies for Next Features**: This specification completes the core Movie Preferences API functionality, enabling future recommendation engine integration and advanced analytics features.