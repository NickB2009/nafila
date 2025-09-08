# Backend Registration Debug Checklist

## 🚨 500 Internal Server Error Analysis

The frontend is sending the correct request format, but the backend is returning a 500 error. Here are the most likely causes:

### 1. **Database Schema Issues**
- Check if the User table has the new columns: `FullName`, `PhoneNumber`
- Verify that `Username` column constraints have been updated/removed
- Ensure phone number field allows the format being sent

### 2. **Backend Model Validation**
The backend might be expecting:
```json
{
  "FullName": "John Doe",
  "Email": "john@example.com", 
  "PhoneNumber": "+5511999999999",
  "Password": "StrongPass123!"
}
```

Check if:
- Field names match exactly (case-sensitive)
- Phone number validation accepts the format being sent
- Password validation rules match frontend requirements
- Email validation is working correctly

### 3. **Backend Controller Issues**
- Verify the RegisterRequest model in the backend matches the frontend
- Check if the registration endpoint is properly handling the new fields
- Ensure password hashing is working with the new structure

### 4. **Database Constraints**
- Check for unique constraints on PhoneNumber field
- Verify that the database can handle the phone number format
- Ensure no foreign key constraints are failing

### 5. **Backend Logging**
Check the backend logs for:
- Detailed exception information
- Database query errors
- Validation failures
- Model binding issues

## 🔧 Quick Backend Fixes to Try

1. **Temporarily disable phone number validation** to see if that's the issue
2. **Add more detailed logging** in the backend registration endpoint
3. **Check if the database migration** for the new user fields was applied correctly
4. **Verify the User entity model** matches the request structure

## 📞 Frontend Phone Number Formats to Test

Try registering with these different formats:
- `+5511999999999` (international)
- `5511999999999` (country code without +)
- `11999999999` (area code + number)
- `(11) 99999-9999` (formatted)

The enhanced frontend logging will show exactly what format is being sent.
