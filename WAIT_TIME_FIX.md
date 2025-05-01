# Wait Time Calculation and Status Synchronization Fix

This update addresses issues with wait time calculation and barbershop open/closed status synchronization in the EuTôNaFila application.

## 🛠️ Changes Made

1. **Standardized Status Constants**
   - Updated all models to use consistent status constants from domain models
   - Eliminated string literals like 'disponivel' in favor of constants like `DomainBarbeiro.STATUS_AVAILABLE`
   - Created a management command to update existing database records

2. **Centralized Wait Time Calculation**
   - Consolidated all wait time calculation in the `Barbearia` model
   - Ensured all code paths use the same calculation logic
   - Improved the algorithm to use actual service durations instead of default values

3. **Improved Cache Handling**
   - Added proper cache key properties for consistency
   - Ensured cache invalidation happens when status changes
   - Override save methods to automatically invalidate relevant caches

4. **Synchronized Open/Closed Status**
   - Added caching to the open/closed status calculation
   - Ensured status is updated when operating hours change
   - Cache is invalidated appropriately

5. **Enhanced Queue Position Logic**
   - Improved queue position calculation to consider priority
   - Fixed potential division by zero errors

## 🚀 How to Apply the Fix

1. **Back Up Your Database**
   ```bash
   python manage.py dumpdata > backup.json
   ```

2. **Apply All Code Changes**
   - Update the files as shown in the changes

3. **Create and Run Migrations**
   ```bash
   python manage.py makemigrations
   python manage.py migrate
   ```

4. **Run the Status Update Command**
   ```bash
   python manage.py update_status_constants
   ```

5. **Clear All Caches**
   ```bash
   python manage.py shell -c "from django.core.cache import cache; cache.clear()"
   ```

6. **Restart Your Application**
   ```bash
   # If using gunicorn/supervisord
   supervisorctl restart all
   
   # If in development
   python manage.py runserver
   ```

## 🔍 Verifying the Fix

1. **Check Wait Time Calculation**
   - Add a client to the queue
   - Verify the wait time shows correctly
   - Add more clients and see that the wait time increases proportionally

2. **Verify Open/Closed Status**
   - Change operating hours and see if status updates
   - Test at the boundary of opening/closing times

3. **Test Queue Position Logic**
   - Add clients with different priorities
   - Verify higher priority clients are shown earlier in the queue

## 📋 Additional Notes

- The wait time calculation now uses actual service durations instead of fixed values
- Wait times are cached for 30 seconds for performance
- Open/closed status is cached for 5 minutes
- All cache invalidation happens automatically on model saves

If you have any questions or encounter issues with this fix, please open an issue or contact the development team. 