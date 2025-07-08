# InnovAIte Projects Dashboard - Backend API

Backend API service for the InnovAIte Projects Dashboard application built with .NET 8 and MongoDB.

## Features

- **User Authentication**: Secure login/registration with Argon2 password hashing
- **Project Management**: CRUD operations for projects with status tracking
- **Comment System**: User comments on projects with automatic approval
- **MongoDB Integration**: Persistent data storage with MongoDB
- **API Documentation**: Swagger/OpenAPI documentation

## Tech Stack

- **.NET 8**: Web API framework
- **MongoDB**: Database
- **Argon2**: Password hashing
- **Swagger**: API documentation
- **Basic Authentication**: Security implementation

## Environment Variables

Set these environment variables for production:

```
MONGODB_CONNECTION_STRING=your_mongodb_connection_string
ASPNETCORE_ENVIRONMENT=Production
```

## Deployment

This application is configured for deployment to Render.com:

1. Connect your GitHub repository to Render
2. Set the build command: `dotnet publish -c Release -o out`
3. Set the start command: `dotnet out/innovaite-projects-dashboard.dll`
4. Add environment variables in Render dashboard

## Local Development

```bash
dotnet restore
dotnet run
```

The API will be available at `https://localhost:7251` with Swagger UI at `/swagger`.

## API Endpoints

- `GET /api/projects` - Get all projects
- `POST /api/projects` - Create new project
- `GET /api/projects/{id}` - Get project by ID
- `PUT /api/projects/{id}` - Update project
- `DELETE /api/projects/{id}` - Delete project
- `GET /api/comments/project/{projectId}` - Get project comments
- `POST /api/comments` - Create comment
- `POST /api/users/register` - Register user
- `POST /api/users/login` - Login user

## Authentication

The API uses Basic Authentication. Include credentials in the Authorization header:

```
Authorization: Basic base64(email:password)
```

## Database Schema

### Users
- Id (ObjectId)
- FirstName (string)
- LastName (string)
- Email (string)
- PasswordHash (string)
- Role (string) - "User" or "Admin"
- CreatedDate (DateTime)
- ModifiedDate (DateTime)

### Projects
- Id (ObjectId)
- Title (string)
- Description (string)
- UserId (ObjectId)
- UserName (string)
- GitHubUrl (string, optional)
- LiveSiteUrl (string, optional)
- Status (string) - "Not Started", "In Progress", "Completed"
- Tools (string, optional)
- CreatedDate (DateTime)
- ModifiedDate (DateTime)

### Comments
- Id (ObjectId)
- ProjectId (ObjectId)
- UserId (ObjectId)
- UserName (string)
- Content (string)
- Approved (boolean)
- CreatedDate (DateTime)
- ModifiedDate (DateTime)
