## ✅ Step 1: Role Terminology Integration (PlatformAdmin, Admin, Barber, Client, ServiceAccount)

I’ll update **descriptions and phrasing** throughout the checklist to reflect your architectural hierarchy. Here's the **fully updated checklist** with your **original structure preserved**, and **future-proofing hooks** added at the appropriate spots:

---

# Implementation Checklist

## Use Case Implementation Priority 📋

### Phase 1 – Core Queue Management (Current Focus)

Target use cases for UI development based on role-specific operations:

* [ ] UC-ENTRY (`Client`): Client queue entry interface
* [ ] UC-QUEUELISTCLI (`Client`): Real-time queue status display
* [ ] UC-BARBERQUEUE (`Barber`): Barber queue management view
* [ ] UC-CALLNEXT (`Barber`): Next client call interface
* [ ] UC-CANCEL (`Client`): Queue cancellation functionality

Refer to `USE_CASES.md` for detailed specifications.

---

## Project Setup Phase ✅

### Prerequisites

* [ ] VS Code with Dart & Flutter extensions installed
* [ ] Flutter Web enabled (`flutter config --enable-web`)
* [ ] Repository cloned locally
* [ ] Feature branch created: `feature/ui-20250608`

### Project Configuration

* [ ] `pubspec.yaml` configured with required dependencies

  * [ ] `provider: ^6.1.1` for state management
  * [ ] `flutter_test` for testing
  * [ ] Dev dependencies for testing tools
* [ ] Flutter web support verified (`flutter devices` shows web)
* [ ] Project runs without errors (`flutter run -d web-server`)

---

## Phase 1: UI Layer Development 🚧

### Core Directory Structure

* [ ] `lib/ui/screens/` – Screen widgets (mapped to use cases and roles)
* [ ] `lib/ui/widgets/` – Shared and component widgets
* [ ] `lib/ui/view_models/` – ViewModel logic per feature
* [ ] `lib/ui/data/` – Mock/local/fake data layer
* [ ] `test/` – Mirrored testing structure for all above layers

### Design System Implementation

* [ ] `app_colors.dart` – Centralized color palette
* [ ] `app_text_styles.dart` – Typography and scaling
* [ ] `app_spacing.dart` – Padding, margins, gutters
* [ ] `app_theme.dart` – Base theme configuration
* [ ] **Dark mode toggle support added** (test coverage pending)

### State Management Setup

* [ ] `mock_queue_notifier.dart` (`ChangeNotifier` subclass)

  * [ ] Handles queue entry, update, removal
  * [ ] Loading state and listener notification
* [ ] `mock_data.dart` – Temporary hardcoded entries
* [ ] `main.dart` – `MultiProvider` setup

---

## Core Widgets Development

### QueueScreen (for Clients & Barbers)

* [ ] `Scaffold` and `SafeArea`
* [ ] `AppBar`: Title = “Queue Management”
* [ ] `LayoutBuilder`: Responsive columns
* [ ] `ListView.builder`: Queue display
* [ ] Empty and loading state UI
* [ ] Pull-to-refresh
* [ ] Padding for viewports

### QueueCard Widget (Client Representation)

* [ ] `Card` with elevation
* [ ] `CircleAvatar` or initials
* [ ] Name, ETA, and priority status
* [ ] `StatusPanel` integration
* [ ] Tappable with semantic feedback
* [ ] Adaptive width, layout responsiveness

### StatusPanel Widget

* [ ] Status-to-icon mapping
* [ ] Color-coded status types
* [ ] Rounded containers
* [ ] Icon + Label in compact mode
* [ ] Animated state transitions
* [ ] Sufficient contrast ratio

---

## Responsive Design

* [ ] Optimized for iPhone 15 web (390–430dp)
* [ ] Width breakpoints mapped and tested
* [ ] Padding adjusted by device class
* [ ] Font size scales for readability
* [ ] Minimum 48dp touch target area

---

## Phase 2: Testing Implementation 🧪

### Widget Tests

* [ ] `queue_card_test.dart`

  * [ ] Name & status verification
  * [ ] Tap behavior
  * [ ] Responsive layout
  * [ ] Accessibility tags

* [ ] `status_panel_test.dart`

  * [ ] Icon/color mapping
  * [ ] Compact mode

* [ ] `queue_screen_test.dart`

  * [ ] Loading/empty states
  * [ ] Entry list rendering
  * [ ] Layout validation

### State Management Tests

* [ ] `mock_queue_notifier_test.dart`

  * [ ] Initial state
  * [ ] Add/update/remove
  * [ ] `notifyListeners` integrity

### Integration Tests

* [ ] `queue_management_test.dart`

  * [ ] Entry-to-call workflow
  * [ ] State persistence
  * [ ] Cross-role navigation

### Accessibility Tests

* [ ] Semantic labeling
* [ ] Screen reader compatibility
* [ ] Minimum touch target sizing
* [ ] Color contrast compliance

---

## Phase 3: Quality Assurance 🔍

### Code Quality

* [ ] Analyzer pass (`flutter analyze`)
* [ ] No lint violations
* [ ] Doc comments for public APIs
* [ ] `dart format` consistency

### Performance Validation

* [ ] Smooth scroll, no jank
* [ ] Widget render < 100ms
* [ ] Memory usage stable
* [ ] Animations drop zero frames

### Visual Testing

* [ ] Golden tests for widgets
* [ ] Cross-browser testing

  * [ ] **Device/browser matrix defined**
* [ ] Responsive breakpoint validation
* [ ] Color fidelity across screens

### User Experience

* [ ] Clear loading indicators
* [ ] Friendly error messages
* [ ] Predictable animations
* [ ] Intuitive navigation per role

---

## Phase 4: Advanced Features ⚡

### Enhanced UI

* [ ] Swipe actions for queue cards
* [ ] Animated status transitions
* [ ] Filter by status/time/role
* [ ] Text-based search
* [ ] Real-time updates simulation

### State Management Evolution

* [ ] Replace mocks with simulated API layer
* [ ] Optimistic updates
* [ ] Error propagation handling
* [ ] Local storage or cache logic

### Performance Optimization

* [ ] Lazy widget building
* [ ] Static image compression
* [ ] Bundle size reduction
* [ ] **Offline fallback (PWA support)**

---

## Deployment Preparation 🚀

### Build Configuration

* [ ] `flutter build web --release` tested
* [ ] Asset compression and image optimization
* [ ] `flavors` or env config setup
* [ ] Crash reporting and Sentry or similar

### Documentation

* [ ] `README.md` with install/setup
* [ ] API usage & contract
* [ ] Deployment how-to
* [ ] Common issue guide

### Final Testing

* [ ] Full end-to-end flow
* [ ] Performance benchmarks logged
* [ ] Accessibility audit
* [ ] Basic security checklist

---

## Next Phase Planning 📋

### Backend Integration Ready

* [ ] Platform-level: `PlatformAdmin` operations
* [ ] Org-level: `Admin` APIs
* [ ] Location-level: `Barber` and schedule endpoints
* [ ] Client-level: booking + queue access
* [ ] ServiceAccount integration (e.g., bot flows, alerts)
* [ ] Auth + token exchange
* [ ] Consistent error codes + messages

### Production Considerations

* [ ] Monitoring pipeline
* [ ] Feedback loops
* [ ] A/B test scaffolding
* [ ] Maintenance scheduling

---

## Daily Progress Tracking

### Session Goals Template

**Date**: `YYYY-MM-DD`
**Branch**: `feature/ui-YYYYMMDD`
**Focus**: \[Key goal for the session]

**Completed**:

* [ ] Task A
* [ ] Task B

**In Progress**:

* [ ] Task being worked on

**Blocked**:

* [ ] Dependency or decision

**Next Session**:

* [ ] High-priority next steps

**AI Agent Collaboration**:

* [ ] Prompt-to-code roundtrip
* [ ] Generated tests alongside logic
* [ ] Chat context continuity ensured

---

### Weekly Review Points

* [ ] All high-priority widgets functional
* [ ] Tests pass (>90% coverage)
* [ ] Performance baseline stable
* [ ] Accessibility passes WCAG
* [ ] PR/code review complete
* [ ] Docs current & dev-ready

---

## Success Criteria

### Phase 1 Metrics

* ✅ All required UI components render
* ✅ Fully responsive layout across breakpoints
* ✅ Queue state managed via Provider
* ✅ Widget test suite stable
* ✅ Accessibility and contrast requirements met

### Phase Transition Gate

* ✅ UI layer complete
* ✅ Clear separation of roles and logic
* ✅ Mock data decoupled
* ✅ Tests + code quality aligned
* ✅ Team/onboarding docs complete