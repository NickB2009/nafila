#!/usr/bin/env bash

# Flutter App Deployment Script for KingHost
# Usage: ./deploy_to_kinghost_improved.sh

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
HOST="ftp.eutonafila.com.br"
PORT="21"
REMOTE_DIR="/public_html"
LOCAL_BUILD_DIR="build/web"
DOMAIN="eutonafila.com.br"

# Function to print colored output
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_header() {
    echo -e "${BLUE}========================================"
    echo -e "   Deploying Flutter App to KingHost"
    echo -e "========================================${NC}"
}

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Function to verify DNS
verify_dns() {
    print_status "Verifying DNS resolution for $HOST..."
    
    if command_exists dig; then
        DNS_RESULT=$(dig +short "$HOST" 2>/dev/null | head -1)
        if [ -n "$DNS_RESULT" ]; then
            print_status "DNS resolved: $HOST -> $DNS_RESULT"
        else
            print_error "DNS resolution failed for $HOST"
            exit 1
        fi
    else
        print_warning "dig command not found, skipping DNS verification"
    fi
}

# Function to get FTP credentials
get_credentials() {
    print_status "Please enter your KingHost FTP credentials:"
    
    read -p "FTP Username: " USER
    read -s -p "FTP Password: " PASS
    echo
    
    if [ -z "$USER" ] || [ -z "$PASS" ]; then
        print_error "Username and password are required"
        exit 1
    fi
}

# Function to build Flutter app
build_app() {
    print_status "Building Flutter web app..."
    
    # Clean previous build
    print_status "Cleaning previous build..."
    flutter clean
    
    # Build for web
    print_status "Building for web release..."
    flutter build web --release
    
    if [ $? -eq 0 ]; then
        print_status "Build completed successfully!"
    else
        print_error "Flutter build failed!"
        exit 1
    fi
    
    # Verify build directory exists
    if [ ! -d "$LOCAL_BUILD_DIR" ]; then
        print_error "Build directory $LOCAL_BUILD_DIR not found!"
        exit 1
    fi
    
    print_status "Build files ready in $LOCAL_BUILD_DIR"
}

# Function to create FTP script
create_ftp_script() {
    print_status "Creating FTP upload script..."
    
    cat > ftpscript.txt << EOF
open $HOST $PORT
user $USER $PASS
binary
cd $REMOTE_DIR
lcd $LOCAL_BUILD_DIR
mput *
bye
EOF
    
    print_status "FTP script created"
}

# Function to upload files
upload_files() {
    print_status "Starting FTP upload to $HOST..."
    
    if command_exists ftp; then
        ftp -inv < ftpscript.txt
        UPLOAD_RESULT=$?
        
        if [ $UPLOAD_RESULT -eq 0 ]; then
            print_status "Upload completed successfully!"
        else
            print_error "FTP upload failed with exit code $UPLOAD_RESULT"
            exit 1
        fi
    else
        print_error "ftp command not found. Please install an FTP client."
        exit 1
    fi
}

# Function to cleanup
cleanup() {
    print_status "Cleaning up temporary files..."
    rm -f ftpscript.txt
}

# Function to test connection
test_connection() {
    print_status "Testing FTP connection..."
    
    # Create a simple test script
    cat > test_ftp.txt << EOF
open $HOST $PORT
user $USER $PASS
pwd
bye
EOF
    
    if ftp -inv < test_ftp.txt > /dev/null 2>&1; then
        print_status "FTP connection test successful"
        rm -f test_ftp.txt
    else
        print_error "FTP connection test failed"
        rm -f test_ftp.txt
        exit 1
    fi
}

# Function to verify Flutter installation
verify_flutter() {
    print_status "Verifying Flutter installation..."
    
    if ! command_exists flutter; then
        print_error "Flutter is not installed or not in PATH"
        print_error "Please install Flutter: https://docs.flutter.dev/get-started/install"
        exit 1
    fi
    
    FLUTTER_VERSION=$(flutter --version | head -1)
    print_status "Flutter version: $FLUTTER_VERSION"
}

# Main deployment function
main() {
    print_header
    
    # Verify Flutter installation
    verify_flutter
    
    # Verify DNS
    verify_dns
    
    # Get credentials
    get_credentials
    
    # Test connection
    test_connection
    
    # Build the app
    build_app
    
    # Create FTP script
    create_ftp_script
    
    # Upload files
    upload_files
    
    # Cleanup
    cleanup
    
    # Success message
    echo
    print_status "========================================"
    print_status "   Deployment completed successfully!"
    print_status "========================================"
    echo
    print_status "Your app should be available at: https://$DOMAIN"
    print_status "Please allow a few minutes for changes to propagate."
    echo
}

# Handle script interruption
trap cleanup EXIT

# Run main function
main "$@" 