const Project = require('../models/Project');
const User = require('../models/User');

/**
 * @desc    Get all projects
 * @route   GET /api/projects
 * @access  Private
 */
exports.getProjects = async (req, res) => {
  try {
    let query;
    
    // Copy req.query
    const reqQuery = { ...req.query };
    
    // Fields to exclude
    const removeFields = ['select', 'sort', 'page', 'limit'];
    
    // Delete excluded fields from reqQuery
    removeFields.forEach(param => delete reqQuery[param]);
    
    // Removed user filter to allow all users to see all projects
    // Now any user can view all projects in the system
    
    // Create query string
    let queryStr = JSON.stringify(reqQuery);
    
    // Replace operators ($gt, $gte, etc)
    queryStr = queryStr.replace(/\b(gt|gte|lt|lte|in)\b/g, match => `$${match}`);
    
    // Finding projects
    query = Project.find(JSON.parse(queryStr))
      .populate('creator', 'name email')
      .populate('teamMembers', 'name email');
    
    // Select fields
    if (req.query.select) {
      const fields = req.query.select.split(',').join(' ');
      query = query.select(fields);
    }
    
    // Sort
    if (req.query.sort) {
      const sortBy = req.query.sort.split(',').join(' ');
      query = query.sort(sortBy);
    } else {
      query = query.sort('-createdAt');
    }
    
    // Pagination
    const page = parseInt(req.query.page, 10) || 1;
    const limit = parseInt(req.query.limit, 10) || 10;
    const startIndex = (page - 1) * limit;
    const endIndex = page * limit;
    const total = await Project.countDocuments(JSON.parse(queryStr));
    
    query = query.skip(startIndex).limit(limit);
    
    // Execute query
    const projects = await query;
    
    // Pagination result
    const pagination = {};
    
    if (endIndex < total) {
      pagination.next = {
        page: page + 1,
        limit
      };
    }
    
    if (startIndex > 0) {
      pagination.prev = {
        page: page - 1,
        limit
      };
    }
    
    res.status(200).json({
      success: true,
      count: projects.length,
      pagination,
      data: projects
    });
  } catch (err) {
    console.error('Error in getProjects:', err);
    res.status(500).json({
      success: false,
      message: err.message || 'Server error'
    });
  }
};

/**
 * @desc    Get single project
 * @route   GET /api/projects/:id
 * @access  Private
 */
exports.getProject = async (req, res) => {
  try {
    const project = await Project.findById(req.params.id)
      .populate('creator', 'name email')
      .populate('teamMembers', 'name email')
      .populate('tasks.assignedTo', 'name email');
    
    if (!project) {
      return res.status(404).json({
        success: false,
        message: 'Project not found'
      });
    }
    
    // Removed permission check - any authenticated user can now view project details
    // Edit/delete permissions are still restricted in their respective handlers
    
    res.status(200).json({
      success: true,
      data: project
    });
  } catch (err) {
    console.error('Error in getProject:', err);
    res.status(500).json({
      success: false,
      message: err.message || 'Server error'
    });
  }
};

/**
 * @desc    Create new project
 * @route   POST /api/projects
 * @access  Private
 */
exports.createProject = async (req, res) => {
  try {
    // Add user to req.body
    req.body.creator = req.user._id;
    
    // Create project
    const project = await Project.create(req.body);
    
    res.status(201).json({
      success: true,
      data: project
    });
  } catch (err) {
    console.error('Error in createProject:', err);
    
    // Handle validation errors
    if (err.name === 'ValidationError') {
      const messages = Object.values(err.errors).map(val => val.message);
      return res.status(400).json({
        success: false,
        message: messages.join(', ')
      });
    }
    
    res.status(500).json({
      success: false,
      message: err.message || 'Server error'
    });
  }
};

/**
 * @desc    Update project
 * @route   PUT /api/projects/:id
 * @access  Private
 */
exports.updateProject = async (req, res) => {
  try {
    let project = await Project.findById(req.params.id);
    
    if (!project) {
      return res.status(404).json({
        success: false,
        message: 'Project not found'
      });
    }
    
    // Make sure user is project owner or admin
    if (
      project.creator.toString() !== req.user._id.toString() && 
      req.user.role !== 'admin'
    ) {
      return res.status(403).json({
        success: false,
        message: 'Not authorized to update this project'
      });
    }
    
    // Update project
    project = await Project.findByIdAndUpdate(req.params.id, req.body, {
      new: true,
      runValidators: true
    });
    
    res.status(200).json({
      success: true,
      data: project
    });
  } catch (err) {
    console.error('Error in updateProject:', err);
    
    // Handle validation errors
    if (err.name === 'ValidationError') {
      const messages = Object.values(err.errors).map(val => val.message);
      return res.status(400).json({
        success: false,
        message: messages.join(', ')
      });
    }
    
    res.status(500).json({
      success: false,
      message: err.message || 'Server error'
    });
  }
};

/**
 * @desc    Delete project
 * @route   DELETE /api/projects/:id
 * @access  Private
 */
exports.deleteProject = async (req, res) => {
  try {
    const project = await Project.findById(req.params.id);
    
    if (!project) {
      return res.status(404).json({
        success: false,
        message: 'Project not found'
      });
    }
    
    // Make sure user is project owner or admin
    if (
      project.creator.toString() !== req.user._id.toString() && 
      req.user.role !== 'admin'
    ) {
      return res.status(403).json({
        success: false,
        message: 'Not authorized to delete this project'
      });
    }
    
    await project.deleteOne();
    
    res.status(200).json({
      success: true,
      data: {}
    });
  } catch (err) {
    console.error('Error in deleteProject:', err);
    res.status(500).json({
      success: false,
      message: err.message || 'Server error'
    });
  }
};

/**
 * @desc    Add team member to project
 * @route   PUT /api/projects/:id/team
 * @access  Private
 */
exports.addTeamMember = async (req, res) => {
  try {
    const { email } = req.body;
    
    // Find user by email
    const user = await User.findOne({ email });
    
    if (!user) {
      return res.status(404).json({
        success: false,
        message: 'User not found'
      });
    }
    
    // Find project
    const project = await Project.findById(req.params.id);
    
    if (!project) {
      return res.status(404).json({
        success: false,
        message: 'Project not found'
      });
    }
    
    // Check if user is project owner or admin
    if (
      project.creator.toString() !== req.user._id.toString() && 
      req.user.role !== 'admin'
    ) {
      return res.status(403).json({
        success: false,
        message: 'Not authorized to modify this project'
      });
    }
    
    // Check if user is already a team member
    if (project.teamMembers.includes(user._id)) {
      return res.status(400).json({
        success: false,
        message: 'User is already a team member'
      });
    }
    
    // Add user to team members
    project.teamMembers.push(user._id);
    await project.save();
    
    res.status(200).json({
      success: true,
      data: project
    });
  } catch (err) {
    console.error('Error in addTeamMember:', err);
    res.status(500).json({
      success: false,
      message: err.message || 'Server error'
    });
  }
};

/**
 * @desc    Remove team member from project
 * @route   DELETE /api/projects/:id/team/:userId
 * @access  Private
 */
exports.removeTeamMember = async (req, res) => {
  try {
    const project = await Project.findById(req.params.id);
    
    if (!project) {
      return res.status(404).json({
        success: false,
        message: 'Project not found'
      });
    }
    
    // Check if user is project owner or admin
    if (
      project.creator.toString() !== req.user._id.toString() && 
      req.user.role !== 'admin'
    ) {
      return res.status(403).json({
        success: false,
        message: 'Not authorized to modify this project'
      });
    }
    
    // Check if user is a team member
    if (!project.teamMembers.includes(req.params.userId)) {
      return res.status(400).json({
        success: false,
        message: 'User is not a team member'
      });
    }
    
    // Remove user from team members
    project.teamMembers = project.teamMembers.filter(
      member => member.toString() !== req.params.userId
    );
    
    await project.save();
    
    res.status(200).json({
      success: true,
      data: project
    });
  } catch (err) {
    console.error('Error in removeTeamMember:', err);
    res.status(500).json({
      success: false,
      message: err.message || 'Server error'
    });
  }
};

/**
 * @desc    Add task to project
 * @route   POST /api/projects/:id/tasks
 * @access  Private
 */
exports.addTask = async (req, res) => {
  try {
    const project = await Project.findById(req.params.id);
    
    if (!project) {
      return res.status(404).json({
        success: false,
        message: 'Project not found'
      });
    }
    
    // Check if user is project owner, team member, or admin
    if (
      project.creator.toString() !== req.user._id.toString() && 
      !project.teamMembers.includes(req.user._id) &&
      req.user.role !== 'admin'
    ) {
      return res.status(403).json({
        success: false,
        message: 'Not authorized to modify this project'
      });
    }
    
    // Add created by and date
    req.body.createdBy = req.user._id;
    req.body.createdAt = Date.now();
    
    // Add task to project
    project.tasks.push(req.body);
    
    // Update project status based on tasks
    if (project.status === 'Not Started') {
      project.status = 'In Progress';
    }
    
    await project.save();
    
    res.status(201).json({
      success: true,
      data: project
    });
  } catch (err) {
    console.error('Error in addTask:', err);
    res.status(500).json({
      success: false,
      message: err.message || 'Server error'
    });
  }
};

/**
 * @desc    Update task in project
 * @route   PUT /api/projects/:id/tasks/:taskId
 * @access  Private
 */
exports.updateTask = async (req, res) => {
  try {
    const project = await Project.findById(req.params.id);
    
    if (!project) {
      return res.status(404).json({
        success: false,
        message: 'Project not found'
      });
    }
    
    // Find task
    const task = project.tasks.id(req.params.taskId);
    
    if (!task) {
      return res.status(404).json({
        success: false,
        message: 'Task not found'
      });
    }
    
    // Check if user is project owner, task creator, assignee, or admin
    if (
      project.creator.toString() !== req.user._id.toString() && 
      task.createdBy.toString() !== req.user._id.toString() &&
      (task.assignedTo && task.assignedTo.toString() !== req.user._id.toString()) &&
      !project.teamMembers.some(member => member.toString() === req.user._id.toString()) &&
      req.user.role !== 'admin'
    ) {
      return res.status(403).json({
        success: false,
        message: 'Not authorized to modify this task'
      });
    }
    
    // Update task fields
    Object.keys(req.body).forEach(key => {
      task[key] = req.body[key];
    });
    
    await project.save();
    
    res.status(200).json({
      success: true,
      data: project
    });
  } catch (err) {
    console.error('Error in updateTask:', err);
    res.status(500).json({
      success: false,
      message: err.message || 'Server error'
    });
  }
};

/**
 * @desc    Delete task from project
 * @route   DELETE /api/projects/:id/tasks/:taskId
 * @access  Private
 */
exports.deleteTask = async (req, res) => {
  try {
    const project = await Project.findById(req.params.id);
    
    if (!project) {
      return res.status(404).json({
        success: false,
        message: 'Project not found'
      });
    }
    
    // Find task
    const task = project.tasks.id(req.params.taskId);
    
    if (!task) {
      return res.status(404).json({
        success: false,
        message: 'Task not found'
      });
    }
    
    // Check if user is project owner, task creator, or admin
    if (
      project.creator.toString() !== req.user._id.toString() && 
      task.createdBy.toString() !== req.user._id.toString() &&
      req.user.role !== 'admin'
    ) {
      return res.status(403).json({
        success: false,
        message: 'Not authorized to delete this task'
      });
    }
    
    // Remove task
    project.tasks.pull(req.params.taskId);
    await project.save();
    
    res.status(200).json({
      success: true,
      data: project
    });
  } catch (err) {
    console.error('Error in deleteTask:', err);
    res.status(500).json({
      success: false,
      message: err.message || 'Server error'
    });
  }
};