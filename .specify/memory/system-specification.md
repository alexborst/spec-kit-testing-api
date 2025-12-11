# Movie Preferences API - System Specification

**Version**: 1.0.0  
**Created**: 2025-12-10  
**Status**: Draft  
**Constitutional Compliance**: ✅ Verified

## System Overview

The Movie Preferences API provides Azure Function-based endpoints for managing user movie preferences, including catalog browsing, seen movie tracking, and rating functionality. The system operates with mock user context and serves as a backend for React frontend applications.

## Domain Models

### Movie
Primary entity representing a film in the catalog.

**Properties:**
- `Id` (string, required): Unique identifier for the movie
- `Title` (string, required): Display title of the movie
- `ReleaseYear` (int, required): Year the movie was released
- `Genre` (string, required): Primary genre classification
- `Director` (string, optional): Movie director name
- `Synopsis` (string, optional): Brief movie description
- `RuntimeMinutes` (int, optional): Movie duration in minutes

**Validation Rules:**
- Title must be 1-500 characters
- ReleaseYear must be between 1900 and current year + 2
- Genre must be from predefined list: Action, Comedy, Drama, Horror, Romance, Sci-Fi, Thriller, Documentary, Animation, Fantasy
- Director must be 1-200 characters when provided
- Synopsis must be 1-2000 characters when provided
- RuntimeMinutes must be between 1-600 when provided

**Business Rules:**
- Movie Id must be unique across the catalog
- Movies are read-only after creation (no updates via API)

### UserSeenMovie
Represents the relationship between a user and a movie they have marked as seen.

**Properties:**
- `UserId` (string, required): Identifier for the user
- `MovieId` (string, required): Reference to Movie.Id
- `DateSeen` (DateTime, required): When the user marked the movie as seen
- `Notes` (string, optional): User's personal notes about the movie

**Validation Rules:**
- UserId must be valid GUID format
- MovieId must reference existing movie
- DateSeen cannot be in the future
- Notes must be 1-1000 characters when provided

**Business Rules:**
- Composite primary key: (UserId, MovieId)
- User can only mark a movie as seen once
- DateSeen defaults to current UTC time when not specified

### UserMovieRating
Represents a user's rating for a movie they have seen.

**Properties:**
- `UserId` (string, required): Identifier for the user
- `MovieId` (string, required): Reference to Movie.Id
- `Rating` (int, required): Numeric rating from 1-10
- `RatedAt` (DateTime, required): When the rating was submitted
- `Review` (string, optional): User's written review

**Validation Rules:**
- UserId must be valid GUID format
- MovieId must reference existing movie
- Rating must be integer between 1-5 inclusive
- RatedAt cannot be in the future
- Review must be 1-2000 characters when provided

**Business Rules:**
- Composite primary key: (UserId, MovieId)
- User can only rate a movie they have marked as seen
- User can update their rating (overwrites previous rating)
- RatedAt updates to current UTC time on rating changes

## Domain Relationships

```
Movie (1) ← (0..many) UserSeenMovie ← (0..1) UserMovieRating
  ↑                        ↑                      ↑
  └── MovieId ──────────────┴── MovieId ──────────┘
                           
User (conceptual) ← (0..many) UserSeenMovie
                  ← (0..many) UserMovieRating
```

**Relationship Rules:**
- UserMovieRating requires corresponding UserSeenMovie record
- Deleting UserSeenMovie should cascade delete UserMovieRating
- Movies exist independently of user interactions

## API Capabilities

### 1. Get Movie Catalog
**Endpoint**: `GET /api/movies`  
**Purpose**: Retrieve paginated list of available movies  
**Query Parameters**:
- `page` (int, optional, default=1): Page number for pagination
- `pageSize` (int, optional, default=20, max=100): Number of results per page
- `genre` (string, optional): Filter by genre

**Success Response**: HTTP 200
- Array of Movie objects
- Pagination metadata (totalCount, currentPage, totalPages)

**Error Conditions**:
- 400 Bad Request: Invalid query parameters
- 500 Internal Server Error: System failure

### 2. Get Movie By ID
**Endpoint**: `GET /api/movies/{movieId}`  
**Purpose**: Retrieve specific movie details  
**Path Parameters**:
- `movieId` (string, required): Movie identifier

**Success Response**: HTTP 200
- Single Movie object

**Error Conditions**:
- 400 Bad Request: Invalid movieId format
- 404 Not Found: Movie does not exist
- 500 Internal Server Error: System failure

### 3. Add Movie to User's Seen List
**Endpoint**: `POST /api/users/{userId}/seen-movies`  
**Purpose**: Mark a movie as seen by user  
**Path Parameters**:
- `userId` (string, required): User identifier

**Request Body**:
```json
{
  "movieId": "string",
  "dateSeen": "2025-12-10T10:30:00Z", // optional
  "notes": "string" // optional
}
```

**Success Response**: HTTP 201
- Created UserSeenMovie object
- Location header with resource URL

**Error Conditions**:
- 400 Bad Request: Invalid userId, movieId, or request body
- 404 Not Found: Movie does not exist
- 409 Conflict: Movie already marked as seen by user
- 500 Internal Server Error: System failure

### 4. Rate a Seen Movie
**Endpoint**: `POST /api/users/{userId}/movie-ratings`  
**Purpose**: Submit or update rating for a seen movie  
**Path Parameters**:
- `userId` (string, required): User identifier

**Request Body**:
```json
{
  "movieId": "string",
  "rating": 8,
  "review": "string" // optional
}
```

**Success Response**: 
- HTTP 201 for new rating
- HTTP 200 for updated rating
- UserMovieRating object

**Error Conditions**:
- 400 Bad Request: Invalid userId, movieId, rating, or request body
- 404 Not Found: Movie does not exist
- 409 Conflict: User has not marked movie as seen
- 500 Internal Server Error: System failure

### 5. Get User's Seen List
**Endpoint**: `GET /api/users/{userId}/seen-movies`  
**Purpose**: Retrieve user's list of seen movies with optional ratings  
**Path Parameters**:
- `userId` (string, required): User identifier

**Query Parameters**:
- `includeRatings` (bool, optional, default=false): Include rating data
- `page` (int, optional, default=1): Page number
- `pageSize` (int, optional, default=20, max=100): Results per page

**Success Response**: HTTP 200
- Array of UserSeenMovie objects with Movie details
- Optional UserMovieRating data when includeRatings=true
- Pagination metadata

**Error Conditions**:
- 400 Bad Request: Invalid userId or query parameters
- 404 Not Found: User has no seen movies (empty list, not error)
- 500 Internal Server Error: System failure

## Data Validation Requirements

### General Validation Rules
- All required fields must be present and non-null
- String fields must be trimmed of leading/trailing whitespace
- DateTime fields must be in ISO 8601 format
- GUID fields must be valid UUID format
- Numeric fields must be within specified ranges

### Business Validation Rules
- User can only rate movies marked as seen
- Movie IDs must reference existing catalog entries
- Date values cannot be in the future
- Rating values must be precise integers (no decimals)

## Error Response Format

All error responses follow consistent structure:

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred",
    "details": [
      {
        "field": "rating",
        "message": "Rating must be between 1 and 10"
      }
    ],
    "timestamp": "2025-12-10T10:30:00Z",
    "traceId": "abc123"
  }
}
```

**Error Codes**:
- `VALIDATION_ERROR`: Request data validation failed
- `NOT_FOUND`: Requested resource does not exist
- `CONFLICT`: Business rule violation
- `INTERNAL_ERROR`: System failure

## Business Rules Summary

1. **Movie Catalog Integrity**: Movies are immutable after creation
2. **Seen Movie Prerequisite**: Users must mark movie as seen before rating
3. **Single Seen Record**: User can only mark each movie as seen once
4. **Rating Updates**: Users can update ratings; latest timestamp recorded
5. **User Context**: All operations require valid userId (mock authentication)
6. **Data Consistency**: UserMovieRating requires corresponding UserSeenMovie
7. **Pagination**: Large result sets must be paginated for performance

## Technical Constraints

- Azure Functions isolated worker process (.NET 8)
- Stateless function execution
- Repository pattern for data access abstraction
- Domain-driven design with rich domain models
- OpenAPI specification generation
- CORS support for React frontend integration

## Implementation Decisions

### Data Storage
- **Initial Implementation**: JSON file-based storage for development and testing
- **Future Migration**: Azure Table Storage or Cosmos DB for production
- **Repository Pattern**: Abstract storage implementation for easy migration

### User Management
- **User IDs**: Accept any valid GUID format (no user validation store)
- **Authentication**: Mock user context with provided user IDs

### Movie Catalog
- **Initial Data**: Seed dataset with sample movies across all genres
- **Data Management**: Read-only catalog for this implementation phase

### Azure Functions Configuration
- **Hosting Plan**: Consumption plan (lowest cost, simplest setup)
- **Function App**: Single app with multiple functions
- **CORS**: Enabled for React frontend development

**Next Steps**: Individual feature specifications will detail implementation specifics for each API capability.