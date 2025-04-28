# InnovAIte Planner Board API

The backend API for InnovAIte Planner Board, a project management application for Deakin's InnovAIte Company. This RESTful API handles authentication, project management, and user operations.

## Technologies Used

- **Node.js** - JavaScript runtime
- **Express.js** - Web application framework
- **MongoDB** - NoSQL database with Mongoose ODM
- **JWT** - JSON Web Tokens for authentication
- **bcrypt** - Password hashing

## Getting Started

### Prerequisites

- Node.js (v14+)
- MongoDB (local installation or MongoDB Atlas account)
- Git

### Installation

1. Clone the repository:
```
git clone <your-repository-url>
cd deakin-trello-board/server
```

2. Install dependencies:
```
npm install
```

3. Set up environment variables:
Create a `.env` file in the root directory with the following variables:
```
PORT=5000
MONGO_URI=mongodb+srv://yourusername:yourpassword@cluster.mongodb.net/planner-board
JWT_SECRET=your-secret-key
JWT_EXPIRE=30d
```

4. Start the server:
```
npm run dev
```

The server will start on port 5000 (or the port specified in your environment variables).

## API Endpoints

### Authentication

- **POST /api/auth/register** - Register a new user
  - Request body: `{ name, email, password }`
  - Response: User object with JWT token

- **POST /api/auth/login** - Login a user
  - Request body: `{ email, password }`
  - Response: User object with JWT token

- **GET /api/auth/me** - Get current user profile
  - Headers: `Authorization: Bearer <token>`
  - Response: User object

- **PUT /api/auth/update** - Update user profile
  - Headers: `Authorization: Bearer <token>`
  - Request body: `{ name, email }`
  - Response: Updated user object

- **PUT /api/auth/update-password** - Update user password
  - Headers: `Authorization: Bearer <token>`
  - Request body: `{ currentPassword, newPassword }`
  - Response: Success message

- **GET /api/auth/logout** - Logout user
  - Headers: `Authorization: Bearer <token>`
  - Response: Success message

### Projects

- **GET /api/projects** - Get all projects
  - Headers: `Authorization: Bearer <token>`
  - Query params: `category`, `status`, `technology`, `search`
  - Response: Array of project objects

- **GET /api/projects/:id** - Get a single project
  - Headers: `Authorization: Bearer <token>`
  - Response: Project object

- **POST /api/projects** - Create a new project
  - Headers: `Authorization: Bearer <token>`
  - Request body: Project details
  - Response: Created project object

- **PUT /api/projects/:id** - Update a project
  - Headers: `Authorization: Bearer <token>`
  - Request body: Updated project details
  - Response: Updated project object

- **DELETE /api/projects/:id** - Delete a project
  - Headers: `Authorization: Bearer <token>`
  - Response: Success message

- **POST /api/projects/:id/team** - Add team member to project
  - Headers: `Authorization: Bearer <token>`
  - Request body: `{ email }`
  - Response: Updated project object

## Project Structure

```
server/
├── config/               # Configuration files
├── controllers/          # Route controllers
│   ├── authController.js # Authentication controller
│   └── projectController.js # Project controller
├── middleware/           # Custom middleware
│   └── auth.js           # Authentication middleware
├── models/               # Database models
│   ├── User.js           # User model
│   └── Project.js        # Project model
├── routes/               # API routes
│   ├── auth.js           # Auth routes
│   └── projects.js       # Project routes
├── .env                  # Environment variables (not in repo)
├── .gitignore            # Git ignore file
├── index.js              # Entry point
├── package.json          # Package info and scripts
└── README.md             # This file
```

## Development

### Running in Development Mode

```
npm run dev
```

This uses nodemon to automatically restart the server when files change.

### Running Tests (Future Implementation)

```
npm test
```

## Deployment

This API can be deployed to various platforms:

- **Render**: Connect your GitHub repository for automatic deployments
- **Heroku**: Use the Heroku CLI or GitHub integration
- **Railway**: Easy deployment with GitHub integration

Don't forget to set up your environment variables on your hosting platform.

## License

This project is licensed under the MIT License.