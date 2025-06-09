@echo off
echo Starting Flutter Web Development Server...
echo.
echo This will start the Flutter development server with hot reload
echo Navigate to the URL shown below in your browser
echo Press Ctrl+C to stop the server
echo.
flutter run -d chrome --web-renderer html --web-port 8080
