# AI Agent Prompts & Workflows

## Overview
This document provides structured prompts and workflows for effectively collaborating with AI agents (like Cursor, GitHub Copilot, or ChatGPT) to develop the Eutonafila Flutter application.

## General AI Prompt Structure

### Template Format
```
GOAL: [Specific objective]
CONTEXT: [Current state, files involved]
INPUTS: [Required parameters, props, data]
CONSTRAINTS: [Technical limitations, design requirements]
OUTPUT: [Expected deliverable format]
VALIDATION: [How to verify success]
```

## Phase 1: UI Component Development

### 1. QueueScreen Implementation

**Prompt:**
```
GOAL: Create the main QueueScreen widget for a Flutter queue management app

CONTEXT: 
- Flutter web app targeting iPhone 15 (390-430dp width)
- Using Provider for state management with MockQueueNotifier
- Material Design with custom theme

INPUTS:
- MockQueueNotifier providing List<QueueEntry> entries
- Responsive width constraints
- AppBar with title "Queue Management"

CONSTRAINTS:
- Use Scaffold + SafeArea structure
- LayoutBuilder for responsive design
- ListView.builder for queue entries
- Mobile-first design approach
- Follow Material Design guidelines

OUTPUT:
- Complete QueueScreen widget code
- Responsive behavior for different screen widths
- Integration with Provider state management
- Basic widget test template

VALIDATION:
- Screen renders on web without errors
- List displays mock queue entries
- Responsive layout adapts to width changes
- Provider integration works correctly
```

### 2. QueueCard Widget

**Prompt:**
```
GOAL: Create a QueueCard widget displaying individual queue entries

CONTEXT:
- Part of Flutter queue management app
- Used in ListView.builder within QueueScreen
- Material Design card component

INPUTS:
- String name (person's name)
- String status ('waiting', 'in_service', 'completed')
- double width (for responsive design)
- Optional VoidCallback onTap

CONSTRAINTS:
- Material Card with elevation 2
- Height minimum 80dp, max 88dp
- Rounded corners 12dp
- Include CircleAvatar, name text, StatusPanel
- Responsive padding based on width
- Accessibility labels

OUTPUT:
- Complete QueueCard widget code
- Responsive design implementation
- Integration with StatusPanel widget
- Widget test covering name/status display

VALIDATION:
- Card displays name and status correctly
- Responsive behavior works at different widths
- Tap interaction triggers callback
- Accessibility labels are properly set
```

### 3. StatusPanel Widget

**Prompt:**
```
GOAL: Create a StatusPanel widget showing queue status with icon and styling

CONTEXT:
- Used within QueueCard widget
- Displays queue status with color-coded styling
- Material Design chip-like appearance

INPUTS:
- String status ('waiting', 'in_service', 'completed')
- bool compact (optional, default false)

CONSTRAINTS:
- Map status to appropriate icon and color
- Container with rounded background (20dp radius)
- Status color background with 0.1 opacity
- Icon + text in status color
- Padding 8x12dp
- Support for compact mode

OUTPUT:
- Complete StatusPanel widget code
- Status-to-style mapping implementation
- Compact mode variation
- Widget test for status display

VALIDATION:
- Correct icon and color for each status
- Proper styling and spacing
- Compact mode works correctly
- Text is readable with proper contrast
```

## AI Agent Image Analysis Prompts

### UI Screenshot Analysis
```
I'm attaching a screenshot of the target UI design for a queue management app. Please analyze this image and:

1. Identify the key UI components and their layout
2. Describe the color scheme and typography
3. Note any specific spacing, sizing, or styling details
4. Suggest Flutter widget structure to achieve this design
5. Identify responsive design considerations

Focus on creating a clean, Material Design implementation that works well on mobile web browsers.
```

### Design System Extraction
```
Based on this UI screenshot, help me create a Flutter design system including:

1. Color palette (primary, secondary, status colors)
2. Typography scale (heading, body, caption styles)
3. Spacing constants (margins, padding, gaps)
4. Component sizing (heights, widths, radius values)
5. Elevation and shadow specifications

Format the output as Dart constants that I can use throughout the app.
```

## Code Generation Workflows

### 1. Widget Development Workflow

**Step 1: Context Setting**
```
I'm building a Flutter queue management app. Current structure:
- lib/ui/screens/queue_screen.dart
- lib/ui/widgets/queue_card.dart
- lib/ui/widgets/status_panel.dart
- Using Provider for state management

Next, I need to implement [SPECIFIC_WIDGET].
```

**Step 2: Detailed Requirements**
```
For [WIDGET_NAME], I need:
- Properties: [list props with types]
- Layout: [describe visual structure]
- Behavior: [interactions, state changes]
- Constraints: [responsive, accessibility, performance]
```

**Step 3: Implementation Request**
```
Please generate the complete widget code following these specifications:
[paste UI_SPECIFICATIONS.md relevant section]

Include proper documentation and basic widget test.
```

### 2. State Management Integration

**Provider Setup Prompt:**
```
GOAL: Create MockQueueNotifier for Flutter Provider state management

CONTEXT:
- Managing list of queue entries for UI development
- Using ChangeNotifier pattern
- Mock data for development phase

INPUTS:
- QueueEntry model with id, name, status, joinTime, position
- Basic CRUD operations (add, update, remove)
- Loading state management

OUTPUT:
- Complete MockQueueNotifier class
- Sample mock data
- Methods for state updates with notifyListeners()
- Integration example for main.dart

Include proper error handling and documentation.
```

### 3. Testing Code Generation

**Widget Test Prompt:**
```
GOAL: Generate comprehensive widget tests for [WIDGET_NAME]

CONTEXT:
- Flutter widget testing using flutter_test
- Testing [specific widget] with props: [list properties]
- Need to verify rendering, interactions, and state changes

TEST SCENARIOS:
1. Widget renders with correct initial state
2. Props are displayed correctly
3. User interactions trigger expected behavior
4. Error states are handled properly
5. Accessibility labels are present

OUTPUT:
- Complete test file with all scenarios
- Proper test setup and teardown
- Mock data and dependencies
- Descriptive test names and assertions
```

## Debugging and Troubleshooting Prompts

### Error Analysis
```
I'm getting this error in my Flutter app:
[paste error message]

Context:
- Widget: [widget name]
- Code: [relevant code snippet]
- Expected behavior: [what should happen]

Please help me:
1. Understand what's causing this error
2. Provide a fix with explanation
3. Suggest how to prevent similar issues
4. Recommend any testing improvements
```

### Performance Optimization
```
My Flutter app is experiencing [performance issue]. 

Current implementation:
[paste relevant code]

Please analyze and suggest:
1. Performance bottlenecks
2. Optimization strategies
3. Best practices for Flutter web
4. Monitoring and debugging techniques
```

## Code Review Prompts

### Quality Assessment
```
Please review this Flutter widget code for:
1. Code quality and best practices
2. Performance considerations
3. Accessibility compliance
4. Material Design adherence
5. Maintainability and readability

Code:
[paste widget code]

Provide specific suggestions for improvement.
```

### Architecture Validation
```
Review my Flutter app structure:
[paste file structure and key code snippets]

Validate:
1. Separation of concerns
2. State management approach
3. Code organization
4. Scalability considerations
5. Testing strategy

Suggest improvements for production readiness.
```

## Tips for Effective AI Collaboration

### 1. Context Preparation
- Always provide current file structure
- Share relevant existing code
- Include error messages or screenshots
- Specify target platform and constraints

### 2. Iterative Development
- Start with basic implementation
- Request specific improvements
- Ask for code reviews and optimizations
- Build complexity gradually

### 3. Quality Assurance
- Always request tests with implementation
- Ask for accessibility considerations
- Verify responsive design requirements
- Request documentation and comments

### 4. Learning Integration
- Ask for explanations of design decisions
- Request alternative approaches
- Seek best practices recommendations
- Learn Flutter-specific patterns
