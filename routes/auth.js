const express = require('express');
const router = express.Router();
const {
  register,
  login,
  getProfile,
  updateProfile,
  changePassword,
  logout
} = require('../controllers/authController');
const { protect } = require('../middleware/auth');

// Public routes
router.post('/register', register);
router.post('/login', login);

// Database status route (for debugging)
router.get('/db-status', async (req, res) => {
  try {
    const User = require('../models/User');
    const mongoose = require('mongoose');

    // Get database information
    const dbInfo = {
      connected: mongoose.connection.readyState === 1,
      name: mongoose.connection.name,
      host: mongoose.connection.host,
      port: mongoose.connection.port
    };

    // Count users
    const userCount = await User.countDocuments();

    // Get a list of all users (without passwords)
    const users = await User.find().select('-password').limit(10);

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

// Protected routes
router.get('/me', protect, getProfile);
router.put('/update', protect, updateProfile);
router.put('/update-password', protect, changePassword);
router.get('/logout', protect, logout);

module.exports = router;