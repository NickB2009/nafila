#!/bin/bash

# Authentication Integration Test Suite
# This script runs comprehensive tests for the new phone number authentication system

echo "🧪 Running Phone Number Authentication Test Suite"
echo "================================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to run a test and capture results
run_test() {
    local test_file=$1
    local test_name=$2
    
    echo -e "\n${BLUE}🔍 Running: $test_name${NC}"
    echo "----------------------------------------"
    
    if flutter test "$test_file" --reporter=expanded; then
        echo -e "${GREEN}✅ $test_name: PASSED${NC}"
        return 0
    else
        echo -e "${RED}❌ $test_name: FAILED${NC}"
        return 1
    fi
}

# Counter for test results
total_tests=0
passed_tests=0

# Run unit tests for auth models
echo -e "${YELLOW}📋 Testing Authentication Models...${NC}"
if run_test "test/models/auth_models_test.dart" "Auth Models Unit Tests"; then
    ((passed_tests++))
fi
((total_tests++))

# Run integration tests for auth service
echo -e "\n${YELLOW}🔧 Testing Authentication Service Integration...${NC}"
if run_test "test/integration/auth_integration_test.dart" "Auth Service Integration Tests"; then
    ((passed_tests++))
fi
((total_tests++))

# Run widget tests for auth screens
echo -e "\n${YELLOW}🎨 Testing Authentication UI Screens...${NC}"
if run_test "test/screens/auth_screens_integration_test.dart" "Auth Screens Widget Tests"; then
    ((passed_tests++))
fi
((total_tests++))

# Run existing auth service tests (if they exist)
if [ -f "test/services/auth_service_test.dart" ]; then
    echo -e "\n${YELLOW}⚙️ Testing Authentication Service Units...${NC}"
    if run_test "test/services/auth_service_test.dart" "Auth Service Unit Tests"; then
        ((passed_tests++))
    fi
    ((total_tests++))
fi

# Summary
echo -e "\n${BLUE}📊 Test Results Summary${NC}"
echo "======================================="
echo -e "Total Tests: $total_tests"
echo -e "Passed: ${GREEN}$passed_tests${NC}"
echo -e "Failed: ${RED}$((total_tests - passed_tests))${NC}"

if [ $passed_tests -eq $total_tests ]; then
    echo -e "\n${GREEN}🎉 All authentication tests passed!${NC}"
    echo -e "${GREEN}✅ Phone number authentication system is working correctly${NC}"
    exit 0
else
    echo -e "\n${RED}⚠️ Some tests failed. Please review the output above.${NC}"
    echo -e "${YELLOW}💡 Common issues to check:${NC}"
    echo "   - Backend API compatibility"
    echo "   - Network connectivity"
    echo "   - Request/response format mismatches"
    echo "   - Database schema alignment"
    exit 1
fi
