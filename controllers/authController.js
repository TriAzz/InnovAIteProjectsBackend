const User = require('../models/User');

/**
 * @desc    Register a new user
 * @route   POST /api/auth/register
 * @access  Public
 */
exports.register = async (req, res) => {
  try {
    console.log('Register request received:', req.body);

    // Validate required fields
    if (!req.body) {
      console.log('Registration failed: No request body');
      return res.status(400).json({
        success: false,
        message: 'No data provided'
      });
    }

    const { name, email, password, role, department, position } = req.body;

    // Validate required fields
    if (!name) {
      console.log('Registration failed: Name is required');
      return res.status(400).json({
        success: false,
        message: 'Name is required'
      });
    }

    if (!email) {
      console.log('Registration failed: Email is required');
      return res.status(400).json({
        success: false,
        message: 'Email is required'
      });
    }

    if (!password) {
      console.log('Registration failed: Password is required');
      return res.status(400).json({
        success: false,
        message: 'Password is required'
      });
    }

    // Check if user already exists
    const existingUser = await User.findOne({ email });

    if (existingUser) {
      console.log('Registration failed: User already exists:', email);
      return res.status(400).json({
        success: false,
        message: 'User with this email already exists'
      });
    }

    // Create user
    console.log('Creating new user with email:', email);
    const user = await User.create({
      name,
      email,
      password,
      role: role || 'user',
      department: department || '',
      position: position || ''
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
    console.log('Login request received:', req.body);

    // Validate request body
    if (!req.body) {
      console.log('Login failed: No request body');
      return res.status(400).json({
        success: false,
        message: 'No data provided'
      });
    }

    const { email, password } = req.body;

    // Validate email & password
    if (!email) {
      console.log('Login failed: Missing email');
      return res.status(400).json({
        success: false,
        message: 'Please provide an email address'
      });
    }

    if (!password) {
      console.log('Login failed: Missing password');
      return res.status(400).json({
        success: false,
        message: 'Please provide a password'
      });
    }

    // Check for user
    console.log('Looking up user with email:', email);
    const user = await User.findOne({ email }).select('+password');

    if (!user) {
      console.log('Login failed: User not found with email:', email);
      return res.status(401).json({
        success: false,
        message: 'Invalid credentials'
      });
    }

    // Check if password matches
    console.log('Checking password for user:', email);
    const isMatch = await user.matchPassword(password);

    if (!isMatch) {
      console.log('Login failed: Password does not match for user:', email);
      return res.status(401).json({
        success: false,
        message: 'Invalid credentials'
      });
    }

    console.log('Login successful for user:', email);

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