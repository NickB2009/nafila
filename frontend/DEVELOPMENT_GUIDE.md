
# Frontend Development Guide

## 🌐 System Context

This frontend is for a SaaS queue management platform with a multi-tenant architecture structured around **Organizations** and **Locations**. Access control is enforced through tenant-aware RBAC, with roles as follows:

- **Platform-level operations** (`PlatformAdmin`)
- **Organization-level operations** (`Admin`)
- **Location-level operations** (`Barber`)
- **Client-level operations** (`Client`)
- **Service operations** (`ServiceAccount`)

The frontend is scoped for **Client**, **Barber**, and partial **Admin** interfaces in the MVP.

## 🧱 Development Context & Use Cases

We're in the **MVP Core Features (Priority 1)** phase. Focused use cases:

- **UC-ENTRY**: Client joins queue
- **UC-QUEUELISTCLI**: Client-side real-time queue viewer
- **UC-BARBERQUEUE**: Barber's queue management view
- **UC-CALLNEXT**: Interface for calling next client
- **UC-CANCEL**: Client cancelation workflow

📄 Reference: `USE_CASES.md` for detailed logic.

---

## 🧰 Prerequisites Setup

### Required Tools
- **VS Code** with:
  - Dart
  - Flutter
  - Flutter Widget Snippets (recommended)
- **Flutter SDK** with web enabled

### Initial Configuration
```bash
flutter config --enable-web
flutter devices
````

### Repository Setup

```bash
git checkout -b feature/ui-YYYYMMDD
# Example:
# git checkout -b feature/ui-20250611

# After your changes:
git add .
git commit -m "feat: implement [feature] UI components"
git push origin feature/ui-YYYYMMDD
```

---

## 🧩 Project Structure

```plaintext
lib/
├── main.dart
└── ui/
    ├── screens/
    │   └── queue_screen.dart
    ├── widgets/
    │   ├── queue_card.dart
    │   └── status_panel.dart
    ├── view_models/
    │   └── mock_queue_notifier.dart
    └── data/
        └── mock_data.dart
```

---

## 📦 State Management

### Provider Setup

In `pubspec.yaml`:

```yaml
dependencies:
  flutter:
    sdk: flutter
  provider: ^6.1.1
```

### App Entry

```dart
void main() {
  runApp(
    ChangeNotifierProvider(
      create: (_) => MockQueueNotifier(),
      child: MyApp(),
    ),
  );
}

class MyApp extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Eutonafila Queue Manager',
      theme: ThemeData(
        primarySwatch: Colors.blue,
        visualDensity: VisualDensity.adaptivePlatformDensity,
      ),
      home: QueueScreen(),
      debugShowCheckedModeBanner: false,
    );
  }
}
```

---

## 🖼️ UI Layer Implementation Priorities

### Phase 1: Core Components

* **QueueScreen**: Full layout
* **QueueCard**: Single client entry
* **StatusPanel**: Queue state indicator

### Constraints

* Target width: `390–430dp`
* Responsive: Use `LayoutBuilder`
* Mobile-first & touch optimized
* Accessibility: semantic labeling

### Component Props

| Component     | Props                         |
| ------------- | ----------------------------- |
| `QueueCard`   | `name`, `status`, `width`     |
| `StatusPanel` | `status`, `iconData`, `color` |

---

## 🧪 Testing Strategy

### Widget Tests

```dart
testWidgets('QueueCard displays name and status correctly', (WidgetTester tester) async {
  // Test logic
});
```

### Test File Structure

```plaintext
test/
├── widget_test.dart
└── widgets/
    ├── queue_card_test.dart
    └── status_panel_test.dart
```

---

## 🤖 AI Workflow for UI

### Prompt Strategy

```txt
Goal: Generate a QueueCard widget matching this screenshot  
Inputs: name (String), status (String), width (double)  
Constraints:
- Mobile-first design
- Provider for state management
- Minimal mocking
- Material Design components
```

### Screenshot Usage

Attach PNG/JPG mockups
Refer to component parts visually
Clarify responsive behavior in prompt

---

## 📅 Development Phases

### ✅ Current Phase: Presentation Layer

* Project structure ready
* QueueScreen in progress
* QueueCard + StatusPanel pending
* Initial responsiveness testing

### 🔜 Next Phase: ViewModel Layer

* Replace mocks with real data
* Business logic integration
* Error + state handling

### 📡 Future Phase: Data Layer

* API integration
* HTTP client & repositories

---

## 🧼 Quality Standards

### Code

* Descriptive naming
* Follow Dart style guide
* Public methods = documented
* Handle errors gracefully

### UI/UX

* Consistent layout and type
* Loading and error states
* Accessibility compliance
* Smooth transitions

### Testing

* Widget test for all custom components
* Integration tests for flows
* Golden tests for major screens
* Performance checks on target devices
