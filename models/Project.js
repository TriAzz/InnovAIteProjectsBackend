const mongoose = require('mongoose');

const TaskSchema = new mongoose.Schema({
  title: {
    type: String,
    required: [true, 'Please add a task title'],
    trim: true,
    maxlength: [100, 'Task title cannot be more than 100 characters']
  },
  description: {
    type: String,
    maxlength: [500, 'Task description cannot be more than 500 characters']
  },
  status: {
    type: String,
    enum: ['To Do', 'In Progress', 'Review', 'Completed'],
    default: 'To Do'
  },
  priority: {
    type: String,
    enum: ['Low', 'Medium', 'High', 'Urgent'],
    default: 'Medium'
  },
  assignedTo: {
    type: mongoose.Schema.Types.ObjectId,
    ref: 'User'
  },
  dueDate: {
    type: Date
  },
  createdAt: {
    type: Date,
    default: Date.now
  },
  createdBy: {
    type: mongoose.Schema.Types.ObjectId,
    ref: 'User'
  }
});

const ProjectSchema = new mongoose.Schema({
  title: {
    type: String,
    required: [true, 'Please add a project title'],
    trim: true,
    maxlength: [100, 'Project title cannot be more than 100 characters']
  },
  description: {
    type: String,
    required: [true, 'Please add a project description'],
    maxlength: [1000, 'Description cannot be more than 1000 characters']
  },
  status: {
    type: String,
    enum: ['Not Started', 'In Progress', 'Completed', 'On Hold'],
    default: 'Not Started'
  },
  category: {
    type: String,
    required: [true, 'Please add a project category'],
    enum: [
      'Web Development',
      'Mobile Development',
      'Data Science',
      'Machine Learning',
      'UI/UX Design',
      'DevOps',
      'Research',
      'Other'
    ]
  },
  priority: {
    type: String,
    enum: ['Low', 'Medium', 'High', 'Urgent'],
    default: 'Medium'
  },
  creator: {
    type: mongoose.Schema.Types.ObjectId,
    ref: 'User',
    required: true
  },
  teamMembers: [{
    type: mongoose.Schema.Types.ObjectId,
    ref: 'User'
  }],
  tasks: [TaskSchema],
  technologies: {
    type: [String],
    required: [true, 'Please add at least one tool']
  },
  githubLink: {
    type: String
  },
  deadline: {
    type: Date
  },
  progress: {
    type: Number,
    min: [0, 'Progress cannot be less than 0'],
    max: [100, 'Progress cannot be more than 100'],
    default: 0
  },
  createdAt: {
    type: Date,
    default: Date.now
  }
}, {
  toJSON: { virtuals: true },
  toObject: { virtuals: true }
});

// Calculate project progress based on task completion
ProjectSchema.methods.calculateProgress = function() {
  if (!this.tasks || this.tasks.length === 0) {
    return 0;
  }
  
  const completedTasks = this.tasks.filter(task => task.status === 'Completed').length;
  const totalTasks = this.tasks.length;
  
  return Math.round((completedTasks / totalTasks) * 100);
};

// Update project status based on tasks
ProjectSchema.pre('save', function(next) {
  // Calculate progress
  if (this.tasks && this.tasks.length > 0) {
    this.progress = this.calculateProgress();
  }
  
  // Update status based on progress
  if (this.progress === 100) {
    this.status = 'Completed';
  } else if (this.progress > 0) {
    this.status = 'In Progress';
  }
  
  next();
});

// Cascade delete tasks when a project is deleted
ProjectSchema.pre('deleteOne', { document: true }, async function(next) {
  // Tasks are embedded, so they'll be deleted with the project
  next();
});

module.exports = mongoose.model('Project', ProjectSchema);