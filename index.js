const express = require('express');
const mongoose = require('mongoose');
const cors = require('cors');
const dotenv = require('dotenv');
const path = require('path');

// Load environment variables
dotenv.config();

// Initialize Express app
const app = express();

// CORS configuration
const corsOptions = {
  origin: [
    'http://localhost:3000',
    'https://innovaiteprojects.netlify.com',
    'https://innovaiteprojects.netlify.app',
    'https://innovaiteprojectsdashboard.netlify.app'
  ],
  methods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS'],
  allowedHeaders: ['Content-Type', 'content-type', 'Authorization', 'Origin', 'X-Requested-With', 'Accept'],
  exposedHeaders: ['Content-Length', 'X-Confirm-Delete'],
  credentials: true,
  optionsSuccessStatus: 200,
  preflightContinue: false
};

// Middleware
app.use(cors(corsOptions));

// Handle OPTIONS preflight requests explicitly
app.options('*', cors(corsOptions));

app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Mount routers
app.use('/api/auth', require('./routes/auth'));
app.use('/api/projects', require('./routes/projects'));

// Basic route for testing API
app.get('/', (req, res) => {
  res.send('InnovAIte Planner Board API is running');
});

// Environment variables status route (without exposing sensitive data)
app.get('/env-status', (req, res) => {
  res.status(200).json({
    success: true,
    environment: {
      NODE_ENV: process.env.NODE_ENV || 'Not set',
      PORT: process.env.PORT || 'Not set',
      MONGO_URI_SET: process.env.MONGO_URI ? 'Yes' : 'No',
      JWT_SECRET_SET: process.env.JWT_SECRET ? 'Yes' : 'No',
      JWT_EXPIRE_SET: process.env.JWT_EXPIRE ? 'Yes' : 'No'
    }
  });
});

// Database status route (directly in main file for easier access)
app.get('/db-status', async (req, res) => {
  try {
    const User = require('./models/User');

    // Get database information
    const dbInfo = {
      connected: mongoose.connection.readyState === 1,
      name: mongoose.connection.name || 'Not connected',
      host: mongoose.connection.host || 'Not connected',
      port: mongoose.connection.port || 'Not connected'
    };

    // Count users
    let userCount = 0;
    let users = [];

    if (mongoose.connection.readyState === 1) {
      try {
        userCount = await User.countDocuments();
        users = await User.find().select('-password').limit(10);
      } catch (dbErr) {
        console.error('Error querying database:', dbErr);
      }
    }

    res.status(200).json({
      success: true,
      database: dbInfo,
      userCount,
      users
    });
  } catch (err) {
    console.error('Error in db-status route:', err);
    res.status(500).json({
      success: false,
      message: 'Error checking database status',
      error: err.message
    });
  }
});

// Error handling middleware
app.use((err, req, res, next) => {
  console.error(err.stack);
  res.status(500).send({ message: 'Something went wrong!', error: err.message });
});

// Set up MongoDB connection and server start
const PORT = process.env.PORT || 5000;
const MONGO_URI = process.env.MONGO_URI || 'mongodb://localhost:27017/planner-board';

// Start server regardless of MongoDB connection status
const server = app.listen(PORT, () => {
  console.log(`Server running on port ${PORT}`);
});

// Connect to MongoDB
console.log('Attempting to connect to MongoDB at:', MONGO_URI);
mongoose.connect(MONGO_URI, {
  useNewUrlParser: true,
  useUnifiedTopology: true,
  dbName: 'planner-board' // Explicitly set the database name
})
  .then(() => {
    console.log(`MongoDB connected successfully`);

    // Log database information
    const db = mongoose.connection;
    console.log('Connected to database:', db.name);

    // List all collections
    db.db.listCollections().toArray()
      .then(collections => {
        console.log('Available collections:');
        collections.forEach(collection => {
          console.log(`- ${collection.name}`);
        });
      })
      .catch(err => {
        console.error('Error listing collections:', err);
      });

    // Check for users collection specifically
    const User = require('./models/User');
    User.countDocuments()
      .then(count => {
        console.log(`Number of users in database: ${count}`);
      })
      .catch(err => {
        console.error('Error counting users:', err);
      });
  })
  .catch((err) => {
    console.error('Failed to connect to MongoDB', err);
    console.log('Server running in limited mode without database access');
  });