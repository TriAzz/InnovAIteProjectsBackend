const jwt = require('jsonwebtoken');
const User = require('../models/User');

// Protect routes - Middleware to verify user is authenticated
exports.protect = async (req, res, next) => {
  console.log('Auth middleware called for route:', req.originalUrl);
  let token;

  // Check if auth header exists and starts with Bearer
  if (
    req.headers.authorization &&
    req.headers.authorization.startsWith('Bearer')
  ) {
    // Extract token from Bearer token in header
    token = req.headers.authorization.split(' ')[1];
    console.log('Token found in Authorization header');
  }

  // Check if token exists
  if (!token) {
    console.log('No token found in request');
    return res.status(401).json({
      success: false,
      message: 'Not authorized to access this route'
    });
  }

  try {
    // Verify token
    console.log('Attempting to verify token');
    const decoded = jwt.verify(token, process.env.JWT_SECRET || '21ad00d6e46ece53b058b441320dc357');
    console.log('Token verified successfully, decoded:', decoded);

    // Get user from the token
    console.log('Looking up user with ID:', decoded.id);
    req.user = await User.findById(decoded.id).select('-password');

    if (!req.user) {
      console.log('User not found for ID:', decoded.id);
      return res.status(404).json({
        success: false,
        message: 'User not found'
      });
    }

    console.log('User found, proceeding with request');
    next();
  } catch (err) {
    console.error('Auth middleware error:', err);
    res.status(401).json({
      success: false,
      message: 'Not authorized to access this route'
    });
  }
};

// Role authorization middleware
exports.authorize = (...roles) => {
  return (req, res, next) => {
    if (!req.user || !roles.includes(req.user.role)) {
      return res.status(403).json({
        success: false,
        message: `User role ${req.user ? req.user.role : 'undefined'} is not authorized to access this route`
      });
    }
    next();
  };
};