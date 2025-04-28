const User = require('../models/User');

/**
 * @desc    Register a new user
 * @route   POST /api/auth/register
 * @access  Public
 */
exports.register = async (req, res) => {
  try {
    const { name, email, password, role, department, position } = req.body;

    // Check if user already exists
    const existingUser = await User.findOne({ email });

    if (existingUser) {
      return res.status(400).json({
        success: false,
        message: 'User with this email already exists'
      });
    }

    // Create user
    const user = await User.create({
      name,
      email,
      password,
      role: role || 'user',
      department,
      position
    });

    // Generate JWT token
    const token = user.getSignedJwtToken();
    
    // Get the user object without the password
    const userWithoutPassword = {
      _id: user._id,
      name: user.name,
      email: user.email,
      role: user.role,
      department: user.department,
      position: user.position,
      createdAt: user.createdAt
    };

    res.status(201).json({
      success: true,
      token,
      user: userWithoutPassword
    });
  } catch (err) {
    console.error('Error in register:', err);
    res.status(500).json({
      success: false,
      message: err.message || 'Server error'
    });
  }
};

/**
 * @desc    Login user & get token
 * @route   POST /api/auth/login
 * @access  Public
 */
exports.login = async (req, res) => {
  try {
    const { email, password } = req.body;

    // Validate email & password
    if (!email || !password) {
      return res.status(400).json({
        success: false,
        message: 'Please provide email and password'
      });
    }

    // Check for user
    const user = await User.findOne({ email }).select('+password');

    if (!user) {
      return res.status(401).json({
        success: false,
        message: 'Invalid credentials'
      });
    }

    // Check if password matches
    const isMatch = await user.matchPassword(password);

    if (!isMatch) {
      return res.status(401).json({
        success: false,
        message: 'Invalid credentials'
      });
    }

    // Generate JWT token
    const token = user.getSignedJwtToken();
    
    // Get the user object without the password
    const userWithoutPassword = {
      _id: user._id,
      name: user.name,
      email: user.email,
      role: user.role,
      department: user.department,
      position: user.position,
      bio: user.bio,
      avatar: user.avatar,
      createdAt: user.createdAt
    };

    res.status(200).json({
      success: true,
      token,
      user: userWithoutPassword
    });
  } catch (err) {
    console.error('Error in login:', err);
    res.status(500).json({
      success: false,
      message: err.message || 'Server error'
    });
  }
};

/**
 * @desc    Get current user profile
 * @route   GET /api/auth/profile
 * @access  Private
 */
exports.getProfile = async (req, res) => {
  try {
    // req.user is set from the auth middleware
    const user = await User.findById(req.user.id);

    res.status(200).json({
      success: true,
      user
    });
  } catch (err) {
    console.error('Error in getProfile:', err);
    res.status(500).json({
      success: false,
      message: err.message || 'Server error'
    });
  }
};

/**
 * @desc    Update user profile
 * @route   PUT /api/auth/profile
 * @access  Private
 */
exports.updateProfile = async (req, res) => {
  try {
    const { name, email, department, position, bio } = req.body;

    // Build update object
    const updateData = {};
    if (name) updateData.name = name;
    if (email) updateData.email = email;
    if (department) updateData.department = department;
    if (position) updateData.position = position;
    if (bio) updateData.bio = bio;

    // Update user profile
    const updatedUser = await User.findByIdAndUpdate(
      req.user.id,
      updateData,
      { new: true, runValidators: true }
    );

    res.status(200).json({
      success: true,
      user: updatedUser
    });
  } catch (err) {
    console.error('Error in updateProfile:', err);
    res.status(500).json({
      success: false,
      message: err.message || 'Server error'
    });
  }
};

/**
 * @desc    Change user password
 * @route   PUT /api/auth/change-password
 * @access  Private
 */
exports.changePassword = async (req, res) => {
  try {
    const { currentPassword, newPassword } = req.body;

    // Get user with password
    const user = await User.findById(req.user.id).select('+password');

    // Check current password
    const isMatch = await user.matchPassword(currentPassword);

    if (!isMatch) {
      return res.status(401).json({
        success: false,
        message: 'Current password is incorrect'
      });
    }

    // Set new password
    user.password = newPassword;
    await user.save();

    res.status(200).json({
      success: true,
      message: 'Password updated successfully'
    });
  } catch (err) {
    console.error('Error in changePassword:', err);
    res.status(500).json({
      success: false,
      message: err.message || 'Server error'
    });
  }
};

/**
 * @desc    Log user out / clear cookie
 * @route   GET /api/auth/logout
 * @access  Private
 */
exports.logout = async (req, res) => {
  try {
    // On client-side, remove token from local storage or memory
    // Here we just send success response
    res.status(200).json({
      success: true,
      message: 'Logged out successfully'
    });
  } catch (err) {
    console.error('Error in logout:', err);
    res.status(500).json({
      success: false,
      message: err.message || 'Server error'
    });
  }
};