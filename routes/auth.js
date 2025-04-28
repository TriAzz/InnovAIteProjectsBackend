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

// Protected routes
router.get('/me', protect, getProfile);
router.put('/update', protect, updateProfile);
router.put('/update-password', protect, changePassword);
router.get('/logout', protect, logout);

module.exports = router;