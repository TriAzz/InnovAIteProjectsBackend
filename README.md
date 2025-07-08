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
MONGODB_CONNECTION_STRING=mongodb+srv://username:password@cluster.mongodb.net/database_name
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
```

## Deployment

This application is configured for deployment to Render.com:

### **Option 1: Using Dockerfile (Recommended)**
1. Connect your GitHub repository to Render
2. Render will auto-detect the Dockerfile
3. Add environment variables in Render dashboard:
   - `MONGODB_CONNECTION_STRING`: Your MongoDB connection string
   - `ASPNETCORE_ENVIRONMENT`: `Production`

### **Option 2: Using Build Commands**
If Dockerfile isn't detected, use these settings:
- **Build Command**: `dotnet publish -c Release -o out`
- **Start Command**: `dotnet out/innovaite-projects-dashboard.dll`
- **Environment Variables**: Same as above

### **Required Environment Variables in Render:**
- `MONGODB_CONNECTION_STRING`: `mongodb+srv://tomasfleming:A8tpKPpBRqNLkuIw@cluster0.5c2od.mongodb.net/innovaite_projects_dashboard?retryWrites=true&w=majority`
- `ASPNETCORE_ENVIRONMENT`: `Production`

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
