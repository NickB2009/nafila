# Project Coding Instructions

Follow these guidelines whenever generating or modifying code, infrastructure, or documentation for **EuTôNaFila / QueueHub**. If any instruction conflicts with an explicit system directive, the system directive prevails.

---

## 1  Azure‑Specific Coding Rules

*Replaced by core practices and local environment notes to reduce noise.*

## 1  Core Practices

- **TDD First**   Write the failing test before implementation.
- **DDD Layering**   Domain, Application, Infrastructure, API separation.
- **Thin Controllers**   Validate → delegate → format response.
- **Integration Tests**   `WebApplicationFactory<Program>` pattern.
- **Observability Hooks**   Structured logs, metrics, correlation IDs.
- **Security**   JWT for protected routes; authorization attributes.

### Local environment

- **Runtime**: .NET 8
- **Local DB**: SQL Server 2022 in Docker (container `queuehub-sqlserver`)
- **Production DB**: Azure SQL Database (`barberqueue` on `grande.database.windows.net`) ✅
- **API**: http://localhost:8080 (container listens on 80; mapped to host 8080)
- **Connection string key**: `ConnectionStrings:AzureSqlConnection`
- **Compose quick start**:
  ```powershell
  cd GrandeTech.QueueHub
  docker-compose up -d --build
  ```

---

## 2  Use‑Case Catalogue (CSV)

```csv
ID,Role,Use  Case,Description,Priority,Status
UC-ADDBARBER,Admin/Owner,Add barbers,Admin adds a new barber to the system.,1,MVP
UC-ADDCOUPON,Admin,Add coupon,Admin adds a coupon.,2,MVP
UC-ADMINLOGIN,Admin/Owner,Login to admin panel,Admin logs in to manage settings and view analytics.,1,MVP
UC-ADROTATE,,Rotate in‑house ads,Cycle active ads on screen at the admin‑defined interval,3,V2
UC-ANALYTICS,Admin  Master,View cross‑barbershop analytics,View aggregate data across consenting barbershops.,1,MVP
UC-APPLYUPDT,Admin  Master,Apply system updates,Perform system‑wide updates or upgrades.,1,MVP
UC-ASKPROFILE,System,Offer profile option,Ask client whether to join queue with or without a profile.,1,MVP
UC-BARBERADD,Barber,Add client to queue,Add client to end of present pool.,1,MVP
UC-BARBERLOGIN,Barber,Login to barber panel,Access dashboard to manage queue.,1,MVP
UC-BARBERQUEUE,Barber,View current queue,Display current waiting list.,1,MVP
UC-BRANDING,Admin/Owner,Customize branding,Update tenant branding (name, colours, etc.).,1,MVP
UC-CALCWAIT,Admin  Master,Calculate estimated wait,Algorithm calculates wait based on recent data.,1,MVP
UC-CALLNEXT,Barber,Call next client,Advance queue to next client.,1,MVP ✅
UC-CANCEL,Client,Cancel queue spot,Client leaves the queue.,1,MVP ✅
UC-CAPLATE,System,Cap late clients,Remove clients delayed beyond max time.,2,MVP
UC-CHANGECAP,Admin,Change late cap,Set or modify max late‑time threshold.,2,MVP
UC-CHECKIN,Client,Check‑in client,Move client from potential queue to present pool.,1,MVP ✅
UC-COUPONNOTIF,System,Send coupon notification,Notify clients of coupons.,1,MVP
UC-CREATEBARBER,Admin/Owner,Create barbershop,Create new barbershop tenant (with billing).,1,MVP
UC-DETAINACTIVE,System,Detect inactive barbers,Mark barbers inactive if idle too long with clients.,2,MVP
UC-DISABLEQ,Admin/Owner,Disable queue temporarily,Pause queue access (e.g., holiday).,1,MVP
UC-EDITBARBER,Admin/Owner,Edit barber,Edit barber profiles or roles.,1,MVP
UC-EDITSHOP,Admin  Master,Edit barbershop settings,Edit tenant data, configs, theme.,2,MVP
UC-ENDBREAK,Barber,End break,Return barber to active status.,2,MVP
UC-ENTRY,Client,Enter queue,Join queue via mobile or kiosk.,1,MVP ✅
UC-FINISH,Barber,Finish appointment,Mark service complete and log duration.,1,MVP ✅
UC-INPUTDATA,Kiosk,Input basic data,Client enters minimal info (name).,1,MVP
UC-JWT,Auth,Issue JWT,System issues JWT after login.,1,MVP
UC-KIOSKADS,,Run kiosk ads,Play ad playlist and credit optional % to barber,3,V2
UC-KIOSKCALL,Kiosk,Display queue on kiosk,Show queue updates at location.,1,MVP
UC-KIOSKCANCEL,Kiosk,Cancel via kiosk,Client removes themselves from list.,1,MVP
UC-LOCALADS,Admin,Local ads on kiosk,Add local ads to kiosk line page.,2,MVP
UC-LOGINCLIENT,Client,Client login,Optional personalised access.,1,MVP
UC-LOGINWEB,System,Web login,Secure quick login via web or kiosk.,1,MVP
UC-MANAGESERV,Admin/Owner,Manage services,Define offered services and durations.,1,MVP
UC-MEDIAUP,,Upload media asset,Staff upload images/videos used in ads,3,V2
UC-METRICS,Admin/Owner,View metrics,Access performance and usage reports.,2,MVP
UC-MULTILOC,Admin/Owner,Manage multiple locations,Oversee multiple barbershops.,2,MVP
UC-PROTECT,Auth,Protect routes,Restrict access via middleware.,1,MVP
UC-QBOARD,,Display queue board,Show live list beside ads,3,V2
UC-QRJOIN,Auth,QR code join,Redirect via QR to queue join page.,1,MVP
UC-QUEUELISTCLI,Client,View live queue,View queue status on barbershop page.,1,MVP
UC-REDIRECTRULE,Admin  Master,Set redirect rules,Route clients to less busy shops.,2,MVP
UC-RESETAVG,System,Reset wait averages,Reset averages every 3  months.,1,MVP
UC-RETURNREM,Barber,Return reminder,Remind barber to return from break.,2,MVP
UC-SAVEHAIRCUT,Barber,Save haircut,Attach haircut details to client profile.,1,MVP
UC-SETDURATION,Admin/Owner,Set service duration,Define estimated durations per service.,1,MVP
UC-SMSNOTIF,Client,SMS notification,Notify client when turn is near.,1,MVP
UC-STAFFSTATUS,Barber,Change status,Update availability (busy/free).,1,MVP
UC-STARTBREAK,Barber,Start break,Begin timed break.,2,MVP
UC-SUBPLAN,Admin  Master,Manage subscription plans,Handle plans/billing.,2,MVP
UC-TRACKQ,Admin/Owner,Track live activity,View real‑time queue and barber activity.,1,MVP
UC-TURNREM,System,Turn reminder,Send in‑app/WhatsApp reminders.,2,MVP
UC-UPDATECACHE,System,Update cache,Store new wait averages in memory.,1,MVP
UC-WAITTIME,Client,View wait time,Display expected wait time.,2,MVP
UC-HISTORYBARBER,Barber,View service history,Review past clients & durations.,3,V2
UC-CHANGELOCATION,Client,Change barbershop,Switch to another shop.,3,V2
UC-HISTORYCLIENT,Client,View service history (future),Review past visits and services.,3,V2
UC-NEXTHAIRCUT,Client,Next‑haircut reminder,Schedule future reminder.,3,V2
UC-MAPLOCATIONS,Client,Locations map,Select locations on map to reach tenant.,3,V2
UC-SMSCONFIG,Admin/Owner,Manage SMS configs,Set push/WhatsApp content & timing.,4,Posterior
UC-REDIRECTCLIENT,Client,Redirect to less busy shop,Offer redirect to nearby shop.,4,Posterior
UC-RATE,Client,Rate barber (future),Rate experience post‑service.,4,Posterior
```

---

## 3  Model ↔︎ Use‑Case Mapping

| **Model**            | Related Use‑Cases                                                                                                                                                                                                                                                                                                                                                                                                     |
| -------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Organization**     | UC-CREATEBARBER · UC-MULTILOC · UC-SUBPLAN · UC-ANALYTICS                                                                                                                                                                                                                                                                                                                                                             |
| **SubscriptionPlan** | UC-SUBPLAN · UC-CREATEBARBER                                                                                                                                                                                                                                                                                                                                                                                          |
| **ServiceProvider**  | UC-ANALYTICS · UC-BRANDING · UC-CHANGECAP · UC-CREATEBARBER · UC-DISABLEQ · UC-EDITSHOP · UC-LOCALADS · UC-REDIRECTRULE · UC-SUBPLAN · UC-MULTILOC · UC-TRACKQ · UC-METRICS · UC-STARTBREAK · UC-ENDBREAK · UC-QBOARD · UC-KIOSKCALL · UC-ENTRY · UC-QUEUELISTCLI · UC-WAITTIME · UC-CALLNEXT · UC-FINISH · UC-BARBERQUEUE · UC-STAFFSTATUS · UC-BARBERADD · UC-INPUTDATA · UC-UPDATECACHE · UC-RESETAVG · UC-CAPLATE |
| **Queue**            | UC-ENTRY · UC-CANCEL · UC-QUEUELISTCLI · UC-WAITTIME · UC-KIOSKCALL · UC-KIOSKCANCEL · UC-CALLNEXT · UC-CHECKIN · UC-TRACKQ · UC-BARBERQUEUE · UC-INPUTDATA · UC-BARBERADD · UC-CAPLATE · UC-CHANGECAP                                                                                                                                                                                                                |
| **QueueEntry**       | UC-ENTRY · UC-CANCEL · UC-CALLNEXT · UC-CHECKIN · UC-FINISH · UC-BARBERADD · UC-KIOSKCANCEL · UC-QUEUELISTCLI · UC-WAITTIME · UC-INPUTDATA · UC-BARBERQUEUE · UC-CAPLATE                                                                                                                                                                                                                                              |
| **Customer**         | UC-ENTRY · UC-INPUTDATA · UC-ASKPROFILE · UC-LOGINCLIENT · UC-LOGINWEB · UC-SMSNOTIF · UC-TURNREM · UC-COUPONNOTIF                                                                                                                                                                                                                                                                                                    |
| **StaffMember**      | UC-BARBERLOGIN · UC-ADDBARBER · UC-EDITBARBER · UC-STARTBREAK · UC-ENDBREAK · UC-STAFFSTATUS · UC-CALLNEXT · UC-FINISH · UC-DETAINACTIVE · UC-BARBERQUEUE · UC-BARBERADD · UC-MEDIAUP                                                                                                                                                                                                                                 |
| **StaffBreak**       | UC-STARTBREAK · UC-ENDBREAK                                                                                                                                                                                                                                                                                                                                                                                           |
| **ServiceType**      | UC-MANAGESERV · UC-SETDURATION · UC-WAITTIME · UC-FINISH · UC-SAVEHAIRCUT                                                                                                                                                                                                                                                                                                                                             |
| **ServiceMetrics**   | UC-WAITTIME · UC-CALCWAIT · UC-UPDATECACHE · UC-RESETAVG · UC-METRICS                                                                                                                                                                                                                                                                                                                                                 |
| **Coupon**           | UC-ADDCOUPON · UC-COUPONNOTIF · UC-LOCALADS · UC-KIOSKADS                                                                                                                                                                                                                                                                                                                                                             |
| **Advertisement**    | UC-LOCALADS · UC-ADROTATE · UC-QBOARD · UC-KIOSKADS · UC-MEDIAUP                                                                                                                                                                                                                                                                                                                                                      |
| **MediaAsset**       | UC-MEDIAUP · UC-KIOSKADS                                                                                                                                                                                                                                                                                                                                                                                              |
| **Notification**     | UC-SMSNOTIF · UC-TURNREM · UC-COUPONNOTIF · UC-RETURNREM                                                                                                                                                                                                                                                                                                                                                              |
| **UserAccount**      | UC-LOGINCLIENT · UC-LOGINWEB · UC-JWT · UC-PROTECT · UC-BARBERLOGIN · UC-QRJOIN · UC-ADMINLOGIN                                                                                                                                                                                                                                                                                                                       |
| **Feedback**         | UC-RATE (V2)                                                                                                                                                                                                                                                                                                                                                                                                          |

---

## 4  Azure Resource Creation Standards

### 4.1  Naming Conventions

* Lower‑case, numbers, hyphens only.
* Format: `[resource-type]-[env]-queuehub-[purpose]-[seq]` e.g. `rg-p-queuehub-core-001`.

### 4.2  Resource Grouping

* Core resources: `rg-[env]-queuehub-core-[seq]`.
* Service‑specific: `rg-[env]-queuehub-[service]-[seq]`.
* BFFs: `rg-[env]-queuehub-bff-[frontend]-[seq]`.

### 4.3  Tagging (mandatory)

| Tag         | Value                           |       |
| ----------- | ------------------------------- | ----- |
| Project     | EuToNaFila / queuehub           |       |
| Environment | Development / Test / Production |       |
| CreatedBy   | \<name                          | team> |
| Cost-Center | <code> (optional)               |       |

### 4.4  Region

* Default: **Brazil South** (`brazilsouth`).
* Use additional regions only for latency/DR requirements.

### 4.5  Architecture & Security Highlights

* **BFF pattern** per frontend; single core backend.
* **Multi‑tenancy** via location slug routing (`https://www.eutonafila.com.br/{location-slug}`).
* Managed identities, Key  Vault, private endpoints.

### 4.6  Automation & CI/CD

* IaC: Bicep/ARM/Terraform, stored in VCS.
* Pipelines: GitHub  Actions or Azure DevOps; separate for core vs each BFF.

### 4.7  Monitoring, Cost, DR, Compliance

* Application  Insights, dashboards, distributed tracing.
* Budgets & alerts; monthly cost reviews; per‑BFF cost tracking.
* Backup policies, tested DR; multi‑region where required.
* Brazilian regulatory compliance; data residency; consent handling.

---

## 5  EuTôNaFila Development Guidelines

1. **TDD First**   Write the failing test before implementation.
2. **DDD Layering**   Domain, Application, Infrastructure, API separation.
3. **Dependency Injection & Mocks**   Inject repos/clients; use in‑memory doubles in tests.
4. **Thin Controllers**   Validate → delegate → format response.
5. **Integration Tests**   `WebApplicationFactory<Program>` pattern.
6. **Progressive Implementation**   Stub → real adapter; build foundational entities first.
7. **Observability Hooks**   Structured logs, metrics, correlation IDs.
8. **Domain Events**   `ClientJoinedQueueEvent`, `BarberCalledNextEvent`, etc.
9. **Safety Gates**   Validate queue capacity, availability, etc.; tenant isolation.
10. **Multi‑Tenant Documentation**   Update architectural notes after each change.
11. **Implementation Sequence**   For every new use-case or feature, strictly follow this order:
   a) Begin with failing unit tests (TDD) that describe the application-layer behavior.
   b) Implement or update the application layer until all unit tests pass.
   c) Add integration tests that validate end-to-end behavior through the API surface.
   d) Finally, expose the feature through the Controller layer (routes/DTOs) once all tests are green.

---

## 6  API DTO Guidelines

- **Never return domain entities directly from controllers.**
- **Always use DTOs (Data Transfer Objects) for API input and output.**
- **Map between domain models and DTOs in the controller or a dedicated mapping/service layer.**
- **DTOs should be designed for the API contract, not for internal persistence or domain logic.**
- **This prevents serialization issues, decouples the API from internal changes, and allows for versioning and security.**

---

## 7  Implementation Progress

### ✅ Completed Use Cases (MVP Priority 1 & 2)
- **UC-ENTRY** (Client enters queue) - Complete with application service, DTOs, and integration tests
- **UC-CALLNEXT** (Barber calls next client) - Complete with application service, DTOs, and integration tests  
- **UC-CANCEL** (Client cancels queue entry) - Complete with application service, DTOs, and integration tests
- **UC-FINISH** (Client finishes service) - Complete with application service, DTOs, and integration tests
- **UC-CHECKIN** (Client check-in) - Complete with application service, DTOs, and integration tests
- **UC-BARBERADD** (Barber adds client to queue) - Complete with application service and controller endpoint
- **UC-BARBERQUEUE** (Barber view current queue) - Complete with GET /api/queues/{id}/entries endpoint
- **UC-QUEUELISTCLI** (Client view live queue) - Complete with GET /api/queues/{id}/public endpoint  
- **UC-WAITTIME** (View estimated wait time) - Complete with GET /api/queues/{id}/wait-time endpoint
- **UC-INPUTDATA** (Kiosk input basic data) - Complete with KioskController and integration tests
- **UC-KIOSKCANCEL** (Kiosk cancellation) - Complete with KioskController
- **UC-STAFFSTATUS** (Barber change status) - Complete with application service, DTOs, and integration tests
- **UC-ADMINLOGIN, UC-BARBERLOGIN, UC-LOGINCLIENT** (Authentication) - Complete with AuthController and JWT
- **UC-BRANDING** (Admin/Owner customize branding) - Complete with OrganizationService and controller endpoint
- **UC-TRACKQ** (Admin/Owner track live activity) - Complete with TrackLiveActivityService and integration tests
- **UC-ADDBARBER** (Admin/Owner adds barbers) - Complete with POST /api/staff/barbers endpoint
- **UC-EDITBARBER** (Admin/Owner edit barber) - Complete with PUT /api/staff/barbers/{id} endpoint and integration tests
- **UC-MANAGESERV** (Admin/Owner manage services) - Complete with ServicesOfferedController CRUD operations
- **UC-SETDURATION** (Set service duration) - Complete as part of ServicesOffered management
- **UC-CREATEBARBER** (Create new barbershop tenant) - Complete as CreateLocation in LocationsController
- **UC-DISABLEQ** (Admin/Owner disable queue temporarily) - Complete with PUT /api/locations/{id}/queue-status endpoint
- **UC-SAVEHAIRCUT** (Barber save haircut details) - Complete with POST /api/queues/entries/{id}/haircut-details endpoint
- **UC-JWT** (Issue JWT) - Complete with AuthController
- **UC-PROTECT** (Protect routes) - Complete with authorization attributes and middleware
- **UC-SUBPLAN** (Manage subscription plans) - Complete with CreateSubscriptionPlanService, SubscriptionPlanService, SubscriptionPlansController, and full CRUD operations
- **UC-ANALYTICS** (View cross-barbershop analytics) - Complete with AnalyticsService, comprehensive unit/integration tests, and cross-barbershop data aggregation

### 🔄 Remaining High Priority Use Cases
Based on the use case catalogue, these high-priority MVP use cases still need implementation:
1. **UC-APPLYUPDT** (Apply system updates) - Priority 1  
2. **UC-ASKPROFILE** (Offer profile option) - Priority 1
3. **UC-CALCWAIT** (Calculate estimated wait) - Priority 1
4. **UC-COUPONNOTIF** (Send coupon notification) - Priority 1
5. **UC-KIOSKCALL** (Display queue on kiosk) - Priority 1
6. **UC-LOGINWEB** (Web login) - Priority 1
7. **UC-QRJOIN** (QR code join) - Priority 1
8. **UC-RESETAVG** (Reset wait averages) - Priority 1
9. **UC-SMSNOTIF** (SMS notification) - Priority 1
10. **UC-UPDATECACHE** (Update cache) - Priority 1

### 📋 New API Endpoints Added
- **GET /api/queues/{id}/entries** - Barbers can view current queue with all entries
- **GET /api/queues/{id}/public** - Public endpoint for clients to view live queue status
- **POST /api/queues/{id}/barber-add** - Barbers can add walk-in clients to queue
- **GET /api/queues/{id}/wait-time** - Anyone can check estimated wait time for queue
- **POST /api/kiosk/join** - Kiosk users can join queue with basic data input
- **POST /api/kiosk/cancel** - Kiosk users can cancel their queue entry
- **PUT /api/organizations/{id}/branding** - Admin/Owner can update organization branding
- **GET /api/organizations/{id}/live-activity** - Admin/Owner can track real-time queue and staff activity
- **PUT /api/staff/barbers/{id}** - Admin/Owner can edit barber details
- **PUT /api/locations/{id}/queue-status** - Admin/Owner can enable/disable queue
- **POST /api/queues/entries/{id}/haircut-details** - Barbers can save haircut details
- **POST /api/subscriptionplans** - PlatformAdmin can create subscription plans
- **GET /api/subscriptionplans** - PlatformAdmin can view all subscription plans
- **GET /api/subscriptionplans/{id}** - PlatformAdmin can view specific subscription plan
- **PUT /api/subscriptionplans/{id}** - PlatformAdmin can update subscription plan details
- **PUT /api/subscriptionplans/{id}/activate** - PlatformAdmin can activate subscription plan
- **PUT /api/subscriptionplans/{id}/deactivate** - PlatformAdmin can deactivate subscription plan
- **GET /api/subscriptionplans/default** - Public endpoint to get default subscription plan
- **POST /api/analytics/cross-barbershop** - PlatformAdmin can view aggregate analytics across consenting organizations
- **POST /api/analytics/organization** - Owner/Admin can view organization-specific analytics with detailed breakdowns
- **POST /api/analytics/top-organizations** - PlatformAdmin can view top performing organizations by various metrics

### 📋 Implementation Notes
- All completed use cases follow TDD approach with comprehensive unit and integration tests
- Application services are properly layered with domain-driven design
- DTOs are used consistently for API contracts
- Integration tests cover end-to-end scenarios with proper authentication
- KioskController provides anonymous access for kiosk functionality
- Queue viewing endpoints support different authorization levels (barber vs public)
- Service history functionality enables tracking customer haircut preferences over time
- Location-level queue control allows temporary disabling for holidays/maintenance

---

## 8  MySQL-Optimized Flattening Plan

### 8.1  Goals and Principles

**Primary Goal**: Eliminate complex value object mappings that cause EF Core runtime exceptions with MySQL, replacing them with simple, flat column mappings for better stability and performance.

**Core Principles**:
- **Simplicity First**: Replace complex owned types and JSON conversions with direct column mappings
- **MySQL Compatibility**: Use native MySQL data types and avoid EF Core features that don't work well with MySQL
- **Zero Downtime**: Implement changes without requiring immediate database schema migrations
- **Backward Compatibility**: Maintain existing API contracts and DTOs
- **Performance**: Improve query performance by eliminating complex materialization

### 8.2  Scope and Phased Rollout

**Phase 1 (Immediate - High Priority)**:
- `Location` entity: Flatten `CustomBranding` and `WeeklyHours`
- `ServiceOffered` entity: Flatten `Price` value object
- `SubscriptionPlan` entity: Flatten `Price` value object

**Phase 2 (Next - Medium Priority)**:
- `Organization` entity: Flatten `CustomBranding`
- `User` entity: Flatten `Email` and `PhoneNumber` value objects
- `Customer` entity: Flatten `Email` and `PhoneNumber` value objects

**Phase 3 (Future - Low Priority)**:
- `Coupon` entity: Flatten `FixedDiscountAmount` (currently ignored)
- `Advertisement` entity: Flatten any complex value objects
- `Notification` entity: Flatten any complex value objects

### 8.3  Entity-by-Entity Changes

#### 8.3.1  Location Entity

**Current Value Objects**:
- `CustomBranding` (BrandingConfig)
- `WeeklyHours` (WeeklyBusinessHours)

**Flattened Columns**:
```sql
-- Branding fields
CustomBranding_PrimaryColor VARCHAR(7) NULL,
CustomBranding_SecondaryColor VARCHAR(7) NULL,
CustomBranding_LogoUrl VARCHAR(500) NULL,
CustomBranding_FontFamily VARCHAR(100) NULL,

-- Weekly hours fields (JSON stored as TEXT for simplicity)
WeeklyHours_Monday_OpenTime TIME NULL,
WeeklyHours_Monday_CloseTime TIME NULL,
WeeklyHours_Monday_IsClosed BIT NULL,
-- ... repeat for each day of the week
```

**Domain Model Changes**:
```csharp
public class Location : Entity
{
    // Replace CustomBranding property with individual properties
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? LogoUrl { get; set; }
    public string? FontFamily { get; set; }
    
    // Replace WeeklyHours with individual day properties
    public TimeSpan? MondayOpenTime { get; set; }
    public TimeSpan? MondayCloseTime { get; set; }
    public bool MondayIsClosed { get; set; }
    // ... repeat for each day
    
    // Helper methods to maintain domain logic
    public BrandingConfig GetBrandingConfig() => new(PrimaryColor, SecondaryColor, LogoUrl, FontFamily);
    public WeeklyBusinessHours GetWeeklyHours() => new(/* construct from individual properties */);
}
```

#### 8.3.2  ServiceOffered Entity

**Current Value Objects**:
- `Price` (Money)

**Flattened Columns**:
```sql
Price_Amount DECIMAL(10,2) NOT NULL,
Price_Currency VARCHAR(3) NOT NULL DEFAULT 'BRL'
```

**Domain Model Changes**:
```csharp
public class ServiceOffered : Entity
{
    // Replace Price property with individual properties
    public decimal PriceAmount { get; set; }
    public string PriceCurrency { get; set; } = "BRL";
    
    // Helper method to maintain domain logic
    public Money GetPrice() => new(PriceAmount, PriceCurrency);
}
```

#### 8.3.3  SubscriptionPlan Entity

**Current Value Objects**:
- `Price` (Money)

**Flattened Columns**:
```sql
Price_Amount DECIMAL(10,2) NOT NULL,
Price_Currency VARCHAR(3) NOT NULL DEFAULT 'BRL'
```

**Domain Model Changes**:
```csharp
public class SubscriptionPlan : Entity
{
    // Replace Price property with individual properties
    public decimal PriceAmount { get; set; }
    public string PriceCurrency { get; set; } = "BRL";
    
    // Helper method to maintain domain logic
    public Money GetPrice() => new(PriceAmount, PriceCurrency);
}
```

#### 8.3.4  Organization Entity

**Current Value Objects**:
- `CustomBranding` (BrandingConfig)

**Flattened Columns**:
```sql
CustomBranding_PrimaryColor VARCHAR(7) NULL,
CustomBranding_SecondaryColor VARCHAR(7) NULL,
CustomBranding_LogoUrl VARCHAR(500) NULL,
CustomBranding_FontFamily VARCHAR(100) NULL
```

**Domain Model Changes**:
```csharp
public class Organization : Entity
{
    // Replace CustomBranding property with individual properties
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? LogoUrl { get; set; }
    public string? FontFamily { get; set; }
    
    // Helper method to maintain domain logic
    public BrandingConfig GetCustomBranding() => new(PrimaryColor, SecondaryColor, LogoUrl, FontFamily);
}
```

#### 8.3.5  User Entity

**Current Value Objects**:
- `Email` (Email)
- `PhoneNumber` (PhoneNumber)

**Flattened Columns**:
```sql
Email_Value VARCHAR(255) NOT NULL,
PhoneNumber_Value VARCHAR(20) NULL
```

**Domain Model Changes**:
```csharp
public class User : Entity
{
    // Replace Email property with direct string
    public string Email { get; set; } = string.Empty;
    
    // Replace PhoneNumber property with direct string
    public string? PhoneNumber { get; set; }
    
    // Helper methods to maintain domain logic
    public Email GetEmailValue() => new(Email);
    public PhoneNumber? GetPhoneNumberValue() => PhoneNumber != null ? new PhoneNumber(PhoneNumber) : null;
}
```

#### 8.3.6  Customer Entity

**Current Value Objects**:
- `Email` (Email)
- `PhoneNumber` (PhoneNumber)

**Flattened Columns**:
```sql
Email_Value VARCHAR(255) NULL,
PhoneNumber_Value VARCHAR(20) NULL
```

**Domain Model Changes**:
```csharp
public class Customer : Entity
{
    // Replace Email property with direct string
    public string? Email { get; set; }
    
    // Replace PhoneNumber property with direct string
    public string? PhoneNumber { get; set; }
    
    // Helper methods to maintain domain logic
    public Email? GetEmailValue() => Email != null ? new Email(Email) : null;
    public PhoneNumber? GetPhoneNumberValue() => PhoneNumber != null ? new PhoneNumber(PhoneNumber) : null;
}
```

### 8.4  EF Core Mapping Updates

#### 8.4.1  Remove Owned Type Mappings

**Current Configuration** (to be removed):
```csharp
modelBuilder.Entity<Location>()
    .OwnsOne(e => e.CustomBranding, branding =>
    {
        branding.Property(b => b.PrimaryColor).HasColumnName("CustomBranding_PrimaryColor");
        branding.Property(b => b.SecondaryColor).HasColumnName("CustomBranding_SecondaryColor");
        branding.Property(b => b.LogoUrl).HasColumnName("CustomBranding_LogoUrl");
        branding.Property(b => b.FontFamily).HasColumnName("CustomBranding_FontFamily");
    });
```

**New Configuration**:
```csharp
modelBuilder.Entity<Location>()
    .Property(e => e.PrimaryColor)
        .HasColumnName("CustomBranding_PrimaryColor")
        .HasMaxLength(7);
    
    modelBuilder.Entity<Location>()
    .Property(e => e.SecondaryColor)
        .HasColumnName("CustomBranding_SecondaryColor")
        .HasMaxLength(7);
    
    modelBuilder.Entity<Location>()
    .Property(e => e.LogoUrl)
        .HasColumnName("CustomBranding_LogoUrl")
        .HasMaxLength(500);
    
    modelBuilder.Entity<Location>()
    .Property(e => e.FontFamily)
        .HasColumnName("CustomBranding_FontFamily")
        .HasMaxLength(100);
```

#### 8.4.2  Remove JSON Conversions

**Current Configuration** (to be removed):
```csharp
modelBuilder.Entity<Location>()
    .Property(e => e.WeeklyHours)
    .HasConversion(
        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
        v => !string.IsNullOrEmpty(v) ? System.Text.Json.JsonSerializer.Deserialize<WeeklyBusinessHours>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? WeeklyBusinessHours.CreateUniform(TimeSpan.FromHours(9), TimeSpan.FromHours(17)) : WeeklyBusinessHours.CreateUniform(TimeSpan.FromHours(9), TimeSpan.FromHours(17)))
    .HasColumnName("WeeklyBusinessHours")
    .HasColumnType("LONGTEXT");
```

**New Configuration**:
```csharp
// Monday
modelBuilder.Entity<Location>()
    .Property(e => e.MondayOpenTime)
        .HasColumnName("WeeklyHours_Monday_OpenTime")
        .HasColumnType("TIME");
    
modelBuilder.Entity<Location>()
    .Property(e => e.MondayCloseTime)
        .HasColumnName("WeeklyHours_Monday_CloseTime")
        .HasColumnType("TIME");
    
modelBuilder.Entity<Location>()
    .Property(e => e.MondayIsClosed)
        .HasColumnName("WeeklyHours_Monday_IsClosed")
        .HasColumnType("BIT");
    
// ... repeat for each day of the week
```

#### 8.4.3  Remove Value Object Ignore Rules

**Current Configuration** (to be removed):
```csharp
modelBuilder.Entity<Location>().Ignore(e => e.CustomBranding);
modelBuilder.Entity<Location>().Ignore(e => e.WeeklyHours);
```

**New Configuration**: No ignore rules needed - properties are now directly mapped.

### 8.5  Migration and Data Considerations

#### 8.5.1  Database Schema Changes

**No Immediate Schema Changes Required**:
- Current database schema already has the flattened columns
- EF Core will map to existing columns
- No data migration needed initially

**Future Schema Optimization** (Phase 4):
- Add proper indexes on frequently queried columns
- Optimize column types for MySQL
- Add constraints for data validation

#### 8.5.2  Data Validation

**Application-Level Validation**:
- Add validation attributes to flattened properties
- Implement business rule validation in domain methods
- Use helper methods to maintain value object behavior

**Database-Level Validation**:
- Add CHECK constraints for data integrity
- Use appropriate MySQL data types
- Add foreign key constraints where needed

### 8.6  Refactoring Checklist

#### 8.6.1  Phase 1 Implementation Steps

1. **Update Domain Models**:
   - [ ] Replace value object properties with primitive properties in `Location`
   - [ ] Replace value object properties with primitive properties in `ServiceOffered`
   - [ ] Replace value object properties with primitive properties in `SubscriptionPlan`
   - [ ] Add helper methods to maintain domain logic

2. **Update EF Core Mappings**:
   - [ ] Remove owned type configurations for `Location.CustomBranding`
   - [ ] Remove JSON conversion for `Location.WeeklyHours`
   - [ ] Add direct column mappings for all flattened properties
   - [ ] Remove ignore rules for value objects

3. **Update Application Services**:
   - [ ] Update mapping logic in DTOs to use flattened properties
   - [ ] Update validation logic to work with primitive properties
   - [ ] Update business logic to use helper methods

4. **Update Tests**:
   - [ ] Update unit tests to work with flattened properties
   - [ ] Update integration tests to verify EF Core mappings
   - [ ] Update API tests to verify DTO mapping

#### 8.6.2  Phase 2 Implementation Steps

1. **Update Domain Models**:
   - [ ] Flatten `Organization.CustomBranding`
   - [ ] Flatten `User.Email` and `User.PhoneNumber`
   - [ ] Flatten `Customer.Email` and `Customer.PhoneNumber`

2. **Update EF Core Mappings**:
   - [ ] Add direct column mappings for flattened properties
   - [ ] Remove owned type configurations

3. **Update Application Services**:
   - [ ] Update mapping and validation logic
   - [ ] Update business logic to use helper methods

4. **Update Tests**:
   - [ ] Update all affected tests
   - [ ] Verify end-to-end functionality

### 8.7  Testing Strategy

#### 8.7.1  Unit Tests

**Domain Model Tests**:
- Test helper methods return correct value objects
- Test business logic works with flattened properties
- Test validation rules are maintained

**Application Service Tests**:
- Test mapping between domain models and DTOs
- Test business logic with flattened properties
- Test error handling and validation

#### 8.7.2  Integration Tests

**EF Core Mapping Tests**:
- Test that entities can be saved and retrieved
- Test that queries work correctly
- Test that relationships are maintained

**API Tests**:
- Test that endpoints return correct data
- Test that DTOs are properly mapped
- Test that validation works end-to-end

#### 8.7.3  Performance Tests

**Query Performance**:
- Test query performance with flattened properties
- Compare performance before and after changes
- Test with realistic data volumes

**Memory Usage**:
- Test memory usage with flattened properties
- Verify no memory leaks in EF Core materialization
- Test with large result sets

### 8.8  Rollback Strategy

#### 8.8.1  Code Rollback

**Git Branching**:
- Create feature branch for each phase
- Implement changes incrementally
- Keep original value object code in separate branch

**Deployment Strategy**:
- Deploy changes to staging environment first
- Test thoroughly before production deployment
- Have rollback plan ready

#### 8.8.2  Database Rollback

**Schema Compatibility**:
- Ensure new code works with existing database schema
- No immediate schema changes required
- Can rollback code without database changes

**Data Integrity**:
- Verify data integrity after rollback
- Test that existing data still works
- Ensure no data loss during rollback

### 8.9  Acceptance Criteria

#### 8.9.1  Functional Requirements

**API Functionality**:
- [ ] All existing API endpoints work correctly
- [ ] All DTOs return expected data
- [ ] All validation rules are maintained
- [ ] All business logic works as expected

**Database Operations**:
- [ ] Entities can be saved and retrieved
- [ ] Queries work correctly
- [ ] Relationships are maintained
- [ ] No EF Core runtime exceptions

#### 8.9.2  Non-Functional Requirements

**Performance**:
- [ ] Query performance is maintained or improved
- [ ] Memory usage is stable
- [ ] No memory leaks in EF Core materialization
- [ ] Application startup time is not significantly impacted

**Reliability**:
- [ ] No runtime exceptions during entity materialization
- [ ] All existing tests pass
- [ ] Health checks pass
- [ ] Application is stable under load

**Maintainability**:
- [ ] Code is easier to understand and maintain
- [ ] EF Core mappings are simpler
- [ ] Debugging is easier
- [ ] Future changes are easier to implement

### 8.10  Success Metrics

#### 8.10.1  Technical Metrics

**Error Reduction**:
- Zero EF Core runtime exceptions related to value object materialization
- Zero database health check failures
- Reduced complexity in EF Core mappings

**Performance Metrics**:
- Query performance maintained or improved
- Memory usage stable or reduced
- Application startup time maintained

#### 8.10.2  Business Metrics

**Stability**:
- Increased application uptime
- Reduced support tickets related to database issues
- Improved developer productivity

**Maintainability**:
- Reduced time to implement new features
- Easier debugging and troubleshooting
- Improved code quality

---

*Last updated: 2025-01-28*