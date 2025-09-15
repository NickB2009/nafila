# Security Model Documentation

## Consolidated Role Structure

After analyzing the use cases and tenant architecture, we've consolidated the security model to eliminate unnecessary roles:

### ✅ **Final Roles**

1. **PlatformAdmin** - Platform-level administrator (cross-tenant)
2. **Admin** - Organization administrator (combined Admin + Owner)
3. **Barber** - Staff member at a location
4. **Client** - End user/customer
5. **ServiceAccount** - Background processes/system operations

### ❌ **Eliminated Roles**

- **~~Kiosk~~** - This is an interface, not a user role. Kiosk operations are either:
  - Public (no auth): UC-INPUTDATA, UC-KIOSKCALL, UC-KIOSKCANCEL
  - Anonymous client actions: UC-QRJOIN
- **~~System~~** - Background processes use ServiceAccount tokens, not user roles
- **~~Owner~~** - Merged with Admin role to simplify tenant management

## Security Implementation

### Tenant-Aware Claims Structure

```csharp
// Standard claims
sub: user_id
role: PlatformAdmin|Admin|Barber|Client|ServiceAccount
email: user@example.com
preferred_username: username

// Tenant-specific claims
org_id: organization_guid
loc_id: location_guid (for location-scoped users)
tenant_slug: location-slug
permissions: ["read:queue", "write:staff"]
is_service_account: true|false
```

### Authorization Attributes

```csharp
[RequirePlatformAdmin]  // Cross-organization operations
[RequireAdmin]         // Organization-level operations
[RequireBarber]        // Location-level operations
[RequireClient]        // Authenticated user operations
[AllowPublicAccess]    // No authentication required
[RequireServiceAccount] // Background processes
```

## Use Case → Role Mapping

### Platform Administration (PlatformAdmin)
- **UC-ANALYTICS**: View cross-barbershop analytics
- **UC-APPLYUPDT**: Apply system updates
- **UC-EDITSHOP**: Edit barbershop settings
- **UC-REDIRECTRULE**: Set redirect rules
- **UC-SUBPLAN**: Manage subscription plans

### Organization Management (Admin)
- **UC-CREATEBARBER**: Create new barbershop tenant
- **UC-ADDCOUPON**: Add coupons
- **UC-CHANGECAP**: Change late cap settings
- **UC-DISABLEQ**: Disable queue temporarily
- **UC-LOCALADS**: Local ads on kiosk
- **UC-ADDBARBER**: Add barbers to system
- **UC-EDITBARBER**: Edit barber details
- **UC-BRANDING**: Customize branding
- **UC-MANAGESERV**: Manage services
- **UC-SETDURATION**: Set service durations
- **UC-TRACKQ**: Track live activity
- **UC-METRICS**: View metrics
- **UC-MULTILOC**: Manage multiple locations

### Location Operations (Barber)
- **UC-BARBERLOGIN**: Login to barber panel
- **UC-BARBERQUEUE**: View current queue
- **UC-CALLNEXT**: Call next client
- **UC-BARBERADD**: Add client to queue
- **UC-STAFFSTATUS**: Change status
- **UC-STARTBREAK**: Start break
- **UC-ENDBREAK**: End break
- **UC-FINISH**: Finish appointment
- **UC-SAVEHAIRCUT**: Save haircut details

### Client Operations (Client/Authenticated)
- **UC-ENTRY**: Enter queue
- **UC-CANCEL**: Cancel queue spot
- **UC-CHECKIN**: Check-in client
- **UC-QUEUELISTCLI**: View live queue
- **UC-WAITTIME**: View wait time
- **UC-LOGINCLIENT**: Client login

### Public Operations (No Auth Required)
- **UC-INPUTDATA**: Input basic data (kiosk)
- **UC-KIOSKCALL**: Display queue on kiosk
- **UC-KIOSKCANCEL**: Cancel via kiosk
- **UC-QRJOIN**: QR code join
- **UC-LOGINWEB**: Web login page
- **UC-ASKPROFILE**: Offer profile option

### Background Processes (ServiceAccount)
- **UC-CALCWAIT**: Calculate estimated wait
- **UC-UPDATECACHE**: Update cache
- **UC-RESETAVG**: Reset averages
- **UC-CAPLATE**: Cap late clients
- **UC-DETAINACTIVE**: Detect inactive barbers
- **UC-SMSNOTIF**: SMS notifications
- **UC-TURNREM**: Turn reminders
- **UC-COUPONNOTIF**: Coupon notifications
- **UC-RETURNREM**: Return reminders

## Tenant Isolation

### Organization Level
- **PlatformAdmin**: Can access all organizations
- **Admin**: Can only access their assigned organization
- JWT must contain `org_id` claim for organization-scoped operations

### Location Level
- **Barber**: Can only access their assigned location
- JWT must contain both `org_id` and `loc_id` claims
- Location must belong to the user's organization

### Security Validation Flow

1. **Authentication**: Validate JWT token
2. **Role Check**: Verify user has required role
3. **Tenant Context**: Extract org_id/loc_id from claims
4. **Resource Access**: Ensure resource belongs to user's tenant scope
5. **Permission Check**: Validate specific permissions if needed

## Controller Security Examples

```csharp
[RequirePlatformAdmin]
public async Task<IActionResult> GetAllOrganizations() { }

[RequireAdmin] 
public async Task<IActionResult> CreateLocation(CreateLocationRequest request) { }

[RequireBarber]
public async Task<IActionResult> CallNextClient(Guid queueId) { }

[RequireClient]
public async Task<IActionResult> JoinQueue(JoinQueueRequest request) { }

[AllowPublicAccess]
public async Task<IActionResult> GetQueueStatus(string locationSlug) { }
```

## Benefits of This Model

1. **Simplified Roles**: Eliminated confusing Kiosk/System roles
2. **Clear Hierarchy**: PlatformAdmin > Admin > Barber > Client
3. **Tenant Isolation**: Strong separation between organizations
4. **Flexible Permissions**: Can add fine-grained permissions later
5. **Secure Defaults**: Most operations require appropriate authentication
6. **Public Access**: Clear separation of public vs private operations
