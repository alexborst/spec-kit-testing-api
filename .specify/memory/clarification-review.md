# Clarification Review - Movie Preferences API

**Review Date**: 2025-12-10  
**Specifications Reviewed**: Constitution v2.0.0, System Specification v1.0.0, Movie Catalog Feature v1.0.0, User Seen List Feature v1.0.0, Movie Ratings Feature v1.0.0

## Specification Review Summary

After reviewing all current specifications, I've identified several areas that require clarification before proceeding to implementation planning. These fall into categories of missing information, ambiguities, assumptions, and technical constraints.

## Critical Clarifications Needed

### 1. **Rating Scale Inconsistency** 🚨
**Issue**: There's a conflict between specifications regarding the rating scale:
- System Specification (UserMovieRating): "Rating must be integer between 1-10 inclusive"
- Movie Ratings Feature Specification: "Rating scale is 1-5 integer values"

**Clarification Required**: 
- What is the correct rating scale: 1-5 or 1-10?
- Should the system specification be updated to match the 1-5 scale from the feature spec?

### 2. **Data Storage & Persistence Layer**
**Missing Information**: The constitution mentions "future persistence layer implementation" but provides no guidance for the initial implementation.

**Clarification Required**:
- What storage mechanism should be used for the initial implementation?
- In-memory collections for prototyping?
- Local JSON files for development?
- Azure Table Storage for cloud deployment?
- Should repository implementations be created as interfaces with multiple providers?

### 3. **Movie Catalog Data Source**
**Assumption Gap**: The specifications describe movie catalog operations but don't specify where the initial movie data comes from.

**Clarification Required**:
- How will the movie catalog be initially populated?
- Should there be seed data with sample movies?
- Is there a specific movie dataset to use (IMDB subset, etc.)?
- Should there be admin endpoints for adding movies (outside current scope)?

### 4. **User ID Format & Validation**
**Inconsistency**: Specifications mention GUID format but lack specificity about validation and generation.

**Clarification Required**:
- Are user IDs system-generated GUIDs or can clients provide them?
- Should there be user ID validation against a user store, or accept any valid GUID?
- How should invalid user IDs be handled (400 vs 404)?

### 5. **Azure Functions Deployment Configuration**
**Missing Technical Details**: Constitution specifies Azure Functions but lacks deployment specifics.

**Clarification Required**:
- What Azure Functions hosting plan (Consumption vs Premium vs Dedicated)?
- Should functions be grouped in a single Function App or separated?
- What are the CORS requirements for React frontend integration?
- Any specific Azure regions or compliance requirements?

## Minor Clarifications & Ambiguities

### 6. **Error Response Consistency**
**Ambiguity**: Error response formats are detailed but some status code decisions need clarification.

**Questions**:
- Should invalid user IDs return 400 (bad request format) or 404 (user not found)?
- When a user has no seen movies, should it return 200 with empty array or 404?

### 7. **Pagination Defaults**
**Inconsistency**: Some endpoints specify different default page sizes.

**Clarification Required**:
- Standardize default page size across all endpoints (currently 20)?
- Should there be a global maximum page size configuration?

### 8. **DateTime Handling**
**Missing Specification**: Time zone handling not fully specified.

**Questions**:
- Should all DateTimes be stored and returned in UTC?
- How should client-provided dates be handled if they include timezone info?
- What's the expected date format for request parsing (ISO 8601 only)?

### 9. **Logging & Observability**
**Assumption**: Constitution mentions logging but doesn't specify requirements.

**Clarification Required**:
- What level of request/response logging is expected?
- Should there be structured logging with correlation IDs?
- Any specific Azure monitoring integration required (Application Insights)?

### 10. **Performance & Scaling Assumptions**
**Missing Context**: Performance targets specified but no context for expected load.

**Questions**:
- What's the expected number of concurrent users?
- Expected movie catalog size (hundreds, thousands, millions)?
- Are the performance targets for development/testing or production?

## Validation Rule Clarifications

### 11. **Genre List Management**
**Implementation Gap**: Predefined genre list specified but management unclear.

**Questions**:
- Should genres be hardcoded constants or configurable?
- How are new genres added in the future?
- Should genre matching be case-sensitive or insensitive?

### 12. **Review Content Validation**
**Missing Policy**: Review content rules are minimal.

**Questions**:
- Should there be any content filtering beyond length limits?
- How should HTML/special characters in reviews be handled?
- Any rate limiting on review submissions?

## Technical Implementation Questions

### 13. **Dependency Injection Configuration**
**Missing Details**: Constitution requires DI but no specifics on service lifetimes.

**Clarification Required**:
- What service lifetimes for repositories (Scoped, Singleton, Transient)?
- How should configuration be injected (IOptions pattern)?
- Any specific DI container preferences beyond built-in .NET DI?

### 14. **OpenAPI Documentation Generation**
**Implementation Details**: Constitution requires OpenAPI but specifics missing.

**Questions**:
- Should XML documentation comments be required for all public APIs?
- What level of detail in OpenAPI specs (examples, descriptions)?
- Should there be automated spec generation from code annotations?

### 15. **Testing Strategy Implications**
**Clarification**: Constitution states no testing implementation, but this affects architecture decisions.

**Questions**:
- Should interfaces be designed with future testability in mind?
- How does "no testing" affect repository pattern implementation?
- Should mock-friendly patterns still be used even without immediate tests?

## Recommended Next Steps

**High Priority** (Must resolve before planning):
1. ✅ Resolve rating scale inconsistency (1-5 vs 1-10)
2. ✅ Define initial data storage approach
3. ✅ Specify movie catalog data source/seeding strategy

**Medium Priority** (Should resolve before implementation):
4. Clarify user ID validation approach
5. Define Azure Functions deployment configuration
6. Standardize pagination and error handling

**Low Priority** (Can be decided during implementation):
7. Finalize logging and observability requirements
8. Determine genre management approach
9. Define OpenAPI documentation standards

## Clarifications Received (2025-12-11)

**✅ RESOLVED - Rating Scale**: Confirmed 1-5 scale. System specification updated to match feature specifications.

**✅ RESOLVED - Data Storage**: JSON files for initial implementation. Repository pattern will abstract storage for future Azure migration.

**✅ RESOLVED - Movie Catalog**: Create seed dataset with sample movies across all genres.

**✅ RESOLVED - Azure Functions**: Consumption plan (lowest cost). Single Function App with multiple functions.

**✅ RESOLVED - User IDs**: Accept any valid GUID format. No user validation store required.

## Status

**All critical clarifications have been resolved.** The specifications are now complete and consistent. Ready to proceed to the `/plan` phase with:

- Consistent 1-5 rating scale across all specifications
- Clear data storage approach with migration path
- Defined movie catalog seeding strategy
- Simple Azure Functions deployment approach
- Straightforward user ID validation

**Next Step**: Proceed to `/plan` phase for implementation planning.