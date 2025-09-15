# Project Coding Instructions

Follow these guidelines whenever generating or modifying code, infrastructure, or documentation for **EuTôNaFila / QueueHub**. If any instruction conflicts with an explicit system directive, the system directive prevails.

---

## 1  Core Practices

- **TDD First**   Write the failing test before implementation.
- **DDD Layering**   Domain, Application, Infrastructure, API separation.
- **Thin Controllers**   Validate → delegate → format response.
- **Integration Tests**   `WebApplicationFactory<Program>` pattern.
- **Observability Hooks**   Structured logs, metrics, correlation IDs.
- **Security**   JWT for protected routes; authorization attributes.
 - **No Value Objects**   Use primitives for persistence with MySQL; avoid owned types/JSON conversions.

### Local environment

- **Runtime**: .NET 8
- **Database**: MySQL 8.x (local instance)
- **API (dev)**: http://localhost:5098 (per `Properties/launchSettings.json`)
- **Connection string key**: `ConnectionStrings:MySqlConnection`
- **Dev workflow (no containers)**:
  - Install MySQL 8 locally and create database `QueueHubDb`
  - Set credentials in `appsettings.Development.json` or environment variables
  - From `GrandeTech.QueueHub/GrandeTech.QueueHub.API`, run: `dotnet run`

  Example `appsettings.Development.json` snippet:
  ```json
  {
    "ConnectionStrings": {
      "MySqlConnection": "Server=localhost;Database=QueueHubDb;User=root;Password=your_password;Port=3306;CharSet=utf8mb4;SslMode=None;ConnectionTimeout=30;"
    },
    "Database": { "Provider": "MySQL", "AutoMigrate": true }
  }
  ```

  Production Swagger: `https://api.eutonafila.com.br/swagger/index.html`

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

## 4  Hosting on BoaHost/Plesk (No Containers)

### 4.1  Overview
We deploy on BoaHost using Plesk. No Azure resources and no Docker/containers.

### 4.2  Publish
- Framework-dependent (server has .NET runtime):
  ```bash
  dotnet publish GrandeTech.QueueHub/GrandeTech.QueueHub.API -c Release -o publish
  ```
- Self-contained (server without .NET runtime), choose appropriate RID (e.g., `linux-x64`, `win-x64`):
  ```bash
  dotnet publish GrandeTech.QueueHub/GrandeTech.QueueHub.API -c Release -o publish -r linux-x64 --self-contained true
  ```

### 4.3  Deploy via Plesk
- Upload `publish/` to your domain path (e.g., `httpdocs/api`).
- Configure the app to run with Kestrel; set reverse proxy from domain to the internal Kestrel port.
- Set environment variables in Plesk:
  - `ASPNETCORE_ENVIRONMENT=Production`
  - `MYSQL_SERVER` (or `MYSQL_HOST`), `MYSQL_DATABASE`, `MYSQL_USER`, `MYSQL_PASSWORD`, `MYSQL_PORT=3306`
  - `JWT_KEY`
  The app reads `ConnectionStrings:MySqlConnection` from `appsettings.Production.json` with environment expansion.

### 4.4  MySQL and TLS
- Use BoaHost MySQL hostname and enable `SslMode=Required` in production.
- Ensure reverse proxy forwards `X-Forwarded-*` headers.

### 4.5  Health and Logs
- Health: `/api/Health`, `/api/Health/database`.
- Use Plesk logs; optionally configure rolling file logs.

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

## 9  User Stories and API Mapping

### 9.1  Client User Stories

#### 9.1.1  Anonymous Client Journey
**As an anonymous client**, I want to:
- **Browse available salons** → `GET /api/Public/salons`
- **View salon details** → `GET /api/Public/salons/{salonId}`
- **Check queue status** → `GET /api/Public/queue-status/{salonId}`
- **Join queue anonymously** → `POST /api/Public/queue/join`
- **Check my position** → `GET /api/Public/queue/entry-status/{entryId}`
- **Update my contact info** → `PUT /api/Public/queue/update/{entryId}`
- **Leave queue if needed** → `POST /api/Public/queue/leave/{entryId}`

#### 9.1.2  Registered Client Journey
**As a registered client**, I want to:
- **Login to my account** → `POST /api/Auth/login`
- **Register new account** → `POST /api/Auth/register`
- **Join queue with profile** → `POST /api/Queues/{id}/join`
- **Check my queue position** → `GET /api/Queues/{id}/entries`
- **Cancel my queue entry** → `POST /api/Queues/{id}/cancel`
- **Check in when called** → `POST /api/Queues/{id}/check-in`
- **View my notifications** → `GET /api/Notifications`
- **Transfer to different salon** → `POST /api/QueueTransfer/transfer`

### 9.2  Barber User Stories

#### 9.2.1  Barber Daily Operations
**As a barber**, I want to:
- **Login to barber panel** → `POST /api/Auth/login`
- **View current queue** → `GET /api/Queues/{id}/entries`
- **Add walk-in customer** → `POST /api/Queues/{id}/barber-add`
- **Call next customer** → `POST /api/Queues/{id}/call-next`
- **Mark service complete** → `POST /api/Queues/{id}/finish`
- **Save haircut details** → `POST /api/Queues/entries/{id}/haircut-details`
- **Update my status** → `PUT /api/Staff/status`
- **Start/end break** → `POST /api/Staff/break/start`, `POST /api/Staff/break/end`

### 9.3  Admin/Owner User Stories

#### 9.3.1  Salon Management
**As a salon owner**, I want to:
- **Login to admin panel** → `POST /api/Auth/login`
- **Create new salon** → `POST /api/Locations`
- **Update salon details** → `PUT /api/Locations/{id}`
- **Enable/disable queue** → `PUT /api/Locations/{id}/queue-status`
- **Manage services offered** → `POST/GET/PUT/DELETE /api/ServicesOffered`
- **Add/edit barbers** → `POST /api/Staff/barbers`, `PUT /api/Staff/barbers/{id}`
- **Customize branding** → `PUT /api/Organizations/{id}/branding`
- **Track live activity** → `GET /api/Organizations/{id}/live-activity`
- **View analytics** → `POST /api/Analytics/organization`

### 9.4  Platform Admin User Stories

#### 9.4.1  Platform Management
**As a platform admin**, I want to:
- **Create organizations** → `POST /api/Organizations`
- **Manage subscription plans** → `POST/GET/PUT /api/SubscriptionPlans`
- **View cross-barbershop analytics** → `POST /api/Analytics/cross-barbershop`
- **Apply system updates** → `POST /api/Maintenance/apply-updates`
- **Monitor system performance** → `GET /api/Performance`

### 9.5  Kiosk User Stories

#### 9.5.1  Kiosk Operations
**As a kiosk user**, I want to:
- **Join queue with basic info** → `POST /api/Kiosk/join`
- **Cancel my queue entry** → `POST /api/Kiosk/cancel`
- **View queue display** → `GET /api/Kiosk/display/{locationId}`

## 10  Complete API Reference

### 10.1  Public APIs (No Authentication Required)

#### 10.1.1  Public Controller (`/api/Public`)
| Method | Endpoint | Description | Use Case | Response Type |
|--------|----------|-------------|----------|---------------|
| `GET` | `/api/Public/salons` | Get all available salons with current status | UC-QUEUELISTCLI | `PublicSalonDto[]` |
| `GET` | `/api/Public/salons/{salonId}` | Get detailed information for specific salon | UC-QUEUELISTCLI | `PublicSalonDto` |
| `GET` | `/api/Public/queue-status/{salonId}` | Get current queue status for salon | UC-QUEUELISTCLI | `PublicQueueStatusDto` |
| `POST` | `/api/Public/queue/join` | Anonymous user joins queue | UC-ENTRY | `AnonymousJoinResult` |
| `GET` | `/api/Public/queue/entry-status/{entryId}` | Get status of specific queue entry | UC-QUEUELISTCLI | `QueueEntryStatusDto` |
| `POST` | `/api/Public/queue/leave/{entryId}` | Anonymous user leaves queue | UC-CANCEL | `LeaveQueueResult` |
| `PUT` | `/api/Public/queue/update/{entryId}` | Update contact info for queue entry | UC-ENTRY | `UpdateQueueEntryResult` |

#### 10.1.2  Kiosk Controller (`/api/Kiosk`)
| Method | Endpoint | Description | Use Case | Response Type |
|--------|----------|-------------|----------|---------------|
| `POST` | `/api/Kiosk/join` | Kiosk user joins queue with basic data | UC-INPUTDATA | `KioskJoinResult` |
| `POST` | `/api/Kiosk/cancel` | Kiosk user cancels queue entry | UC-KIOSKCANCEL | `KioskCancelResult` |
| `GET` | `/api/Kiosk/display/{locationId}` | Get kiosk display information | UC-KIOSKCALL | `KioskDisplayResult` |

#### 10.1.3  Health Controller (`/api/Health`)
| Method | Endpoint | Description | Use Case | Response Type |
|--------|----------|-------------|----------|---------------|
| `GET` | `/api/Health` | Overall system health check | System monitoring | `HealthStatus` |
| `GET` | `/api/Health/database` | Database connectivity check | System monitoring | `DatabaseHealth` |

### 10.2  Authenticated APIs

#### 10.2.1  Authentication Controller (`/api/Auth`)
| Method | Endpoint | Description | Use Case | Response Type | Authorization |
|--------|----------|-------------|----------|---------------|---------------|
| `POST` | `/api/Auth/login` | User login (admin/barber/client) | UC-ADMINLOGIN, UC-BARBERLOGIN, UC-LOGINCLIENT | `LoginResult` | None |
| `POST` | `/api/Auth/verify-2fa` | Verify two-factor authentication | UC-ADMINLOGIN, UC-BARBERLOGIN | `LoginResult` | None |
| `POST` | `/api/Auth/register` | User registration | UC-LOGINCLIENT | `RegisterResult` | None |

#### 10.2.2  Queues Controller (`/api/Queues`)
| Method | Endpoint | Description | Use Case | Response Type | Authorization |
|--------|----------|-------------|----------|---------------|---------------|
| `POST` | `/api/Queues` | Create new queue | UC-CREATEBARBER | `AddQueueResult` | Owner |
| `GET` | `/api/Queues/{id}` | Get queue details | UC-QUEUELISTCLI | `QueueDto` | Client |
| `POST` | `/api/Queues/{id}/join` | Join queue (authenticated) | UC-ENTRY | `JoinQueueResult` | Public |
| `POST` | `/api/Queues/{id}/barber-add` | Barber adds customer to queue | UC-BARBERADD | `BarberAddResult` | Barber |
| `POST` | `/api/Queues/{id}/call-next` | Barber calls next customer | UC-CALLNEXT | `CallNextResult` | Barber |
| `POST` | `/api/Queues/{id}/check-in` | Customer checks in | UC-CHECKIN | `CheckInResult` | Client |
| `POST` | `/api/Queues/{id}/finish` | Barber marks service complete | UC-FINISH | `FinishResult` | Barber |
| `POST` | `/api/Queues/{id}/cancel` | Cancel queue entry | UC-CANCEL | `CancelResult` | Client |
| `GET` | `/api/Queues/{id}/entries` | Get all queue entries | UC-BARBERQUEUE | `QueueEntryDto[]` | Barber |
| `GET` | `/api/Queues/{id}/public` | Get public queue status | UC-QUEUELISTCLI | `PublicQueueDto` | Public |
| `GET` | `/api/Queues/{id}/wait-time` | Get estimated wait time | UC-WAITTIME | `WaitTimeDto` | Public |
| `POST` | `/api/Queues/entries/{id}/haircut-details` | Save haircut details | UC-SAVEHAIRCUT | `SaveHaircutResult` | Barber |

#### 10.2.3  Staff Controller (`/api/Staff`)
| Method | Endpoint | Description | Use Case | Response Type | Authorization |
|--------|----------|-------------|----------|---------------|---------------|
| `POST` | `/api/Staff/barbers` | Add new barber | UC-ADDBARBER | `AddBarberResult` | Owner |
| `PUT` | `/api/Staff/barbers/{id}` | Edit barber details | UC-EDITBARBER | `EditBarberResult` | Owner |
| `PUT` | `/api/Staff/status` | Update staff status | UC-STAFFSTATUS | `UpdateStatusResult` | Barber |
| `POST` | `/api/Staff/break/start` | Start break | UC-STARTBREAK | `StartBreakResult` | Barber |
| `POST` | `/api/Staff/break/end` | End break | UC-ENDBREAK | `EndBreakResult` | Barber |

#### 10.2.4  Organizations Controller (`/api/Organizations`)
| Method | Endpoint | Description | Use Case | Response Type | Authorization |
|--------|----------|-------------|----------|---------------|---------------|
| `POST` | `/api/Organizations` | Create new organization | UC-CREATEBARBER | `CreateOrganizationResult` | PlatformAdmin |
| `GET` | `/api/Organizations/{id}` | Get organization details | UC-MULTILOC | `OrganizationDto` | Owner/Admin |
| `PUT` | `/api/Organizations/{id}` | Update organization | UC-EDITSHOP | `UpdateOrganizationResult` | Owner/Admin |
| `PUT` | `/api/Organizations/{id}/branding` | Update organization branding | UC-BRANDING | `UpdateBrandingResult` | Owner/Admin |
| `GET` | `/api/Organizations/{id}/live-activity` | Get live activity tracking | UC-TRACKQ | `LiveActivityDto` | Owner/Admin |

#### 10.2.5  Locations Controller (`/api/Locations`)
| Method | Endpoint | Description | Use Case | Response Type | Authorization |
|--------|----------|-------------|----------|---------------|---------------|
| `POST` | `/api/Locations` | Create new location | UC-CREATEBARBER | `CreateLocationResult` | Owner |
| `GET` | `/api/Locations/{id}` | Get location details | UC-MULTILOC | `LocationDto` | Owner/Admin |
| `PUT` | `/api/Locations/{id}` | Update location | UC-EDITSHOP | `UpdateLocationResult` | Owner/Admin |
| `PUT` | `/api/Locations/{id}/queue-status` | Enable/disable queue | UC-DISABLEQ | `UpdateQueueStatusResult` | Owner/Admin |

#### 10.2.6  Services Offered Controller (`/api/ServicesOffered`)
| Method | Endpoint | Description | Use Case | Response Type | Authorization |
|--------|----------|-------------|----------|---------------|---------------|
| `POST` | `/api/ServicesOffered` | Create new service | UC-MANAGESERV | `CreateServiceResult` | Owner/Admin |
| `GET` | `/api/ServicesOffered` | Get all services | UC-MANAGESERV | `ServiceOfferedDto[]` | Owner/Admin |
| `GET` | `/api/ServicesOffered/{id}` | Get service details | UC-MANAGESERV | `ServiceOfferedDto` | Owner/Admin |
| `PUT` | `/api/ServicesOffered/{id}` | Update service | UC-MANAGESERV | `UpdateServiceResult` | Owner/Admin |
| `DELETE` | `/api/ServicesOffered/{id}` | Delete service | UC-MANAGESERV | `DeleteServiceResult` | Owner/Admin |

#### 10.2.7  Subscription Plans Controller (`/api/SubscriptionPlans`)
| Method | Endpoint | Description | Use Case | Response Type | Authorization |
|--------|----------|-------------|----------|---------------|---------------|
| `POST` | `/api/SubscriptionPlans` | Create subscription plan | UC-SUBPLAN | `CreatePlanResult` | PlatformAdmin |
| `GET` | `/api/SubscriptionPlans` | Get all plans | UC-SUBPLAN | `SubscriptionPlanDto[]` | PlatformAdmin |
| `GET` | `/api/SubscriptionPlans/{id}` | Get plan details | UC-SUBPLAN | `SubscriptionPlanDto` | PlatformAdmin |
| `PUT` | `/api/SubscriptionPlans/{id}` | Update plan | UC-SUBPLAN | `UpdatePlanResult` | PlatformAdmin |
| `PUT` | `/api/SubscriptionPlans/{id}/activate` | Activate plan | UC-SUBPLAN | `ActivatePlanResult` | PlatformAdmin |
| `PUT` | `/api/SubscriptionPlans/{id}/deactivate` | Deactivate plan | UC-SUBPLAN | `DeactivatePlanResult` | PlatformAdmin |
| `GET` | `/api/SubscriptionPlans/default` | Get default plan | UC-SUBPLAN | `SubscriptionPlanDto` | Public |

#### 10.2.8  Analytics Controller (`/api/Analytics`)
| Method | Endpoint | Description | Use Case | Response Type | Authorization |
|--------|----------|-------------|----------|---------------|---------------|
| `POST` | `/api/Analytics/cross-barbershop` | Cross-barbershop analytics | UC-ANALYTICS | `CrossBarbershopAnalyticsDto` | PlatformAdmin |
| `POST` | `/api/Analytics/organization` | Organization analytics | UC-ANALYTICS | `OrganizationAnalyticsDto` | Owner/Admin |
| `POST` | `/api/Analytics/top-organizations` | Top performing organizations | UC-ANALYTICS | `TopOrganizationsDto` | PlatformAdmin |

#### 10.2.9  Queue Analytics Controller (`/api/QueueAnalytics`)
| Method | Endpoint | Description | Use Case | Response Type | Authorization |
|--------|----------|-------------|----------|---------------|---------------|
| `GET` | `/api/QueueAnalytics/wait-time/{salonId}` | Calculate estimated wait time | UC-CALCWAIT | `WaitTimeEstimate` | Client |

#### 10.2.10  Notifications Controller (`/api/Notifications`)
| Method | Endpoint | Description | Use Case | Response Type | Authorization |
|--------|----------|-------------|----------|---------------|---------------|
| `GET` | `/api/Notifications` | Get user notifications | UC-SMSNOTIF, UC-TURNREM | `NotificationDto[]` | Client |
| `POST` | `/api/Notifications/send` | Send notification | UC-COUPONNOTIF | `SendNotificationResult` | Admin |
| `PUT` | `/api/Notifications/{id}/read` | Mark notification as read | UC-SMSNOTIF | `MarkReadResult` | Client |

#### 10.2.11  Performance Controller (`/api/Performance`)
| Method | Endpoint | Description | Use Case | Response Type | Authorization |
|--------|----------|-------------|----------|---------------|---------------|
| `GET` | `/api/Performance` | Get system performance metrics | UC-METRICS | `PerformanceMetricsDto` | Admin |

#### 10.2.12  Queue Transfer Controller (`/api/QueueTransfer`)
| Method | Endpoint | Description | Use Case | Response Type | Authorization |
|--------|----------|-------------|----------|---------------|---------------|
| `POST` | `/api/QueueTransfer/transfer` | Transfer queue entry | UC-CHANGELOCATION | `TransferResult` | Client |

#### 10.2.13  Maintenance Controller (`/api/Maintenance`)
| Method | Endpoint | Description | Use Case | Response Type | Authorization |
|--------|----------|-------------|----------|---------------|---------------|
| `POST` | `/api/Maintenance/apply-updates` | Apply system updates | UC-APPLYUPDT | `ApplyUpdatesResult` | PlatformAdmin |
| `GET` | `/api/Maintenance/status` | Get maintenance status | UC-APPLYUPDT | `MaintenanceStatusDto` | PlatformAdmin |

### 10.3  Test Controller (`/api/Test`) - Development Only
| Method | Endpoint | Description | Use Case | Response Type | Authorization |
|--------|----------|-------------|----------|---------------|---------------|
| `GET` | `/api/Test/users` | Get test users | Development | `TestUsersResult` | None |
| `GET` | `/api/Test/organizations` | Get test organizations | Development | `TestOrganizationsResult` | None |
| `POST` | `/api/Test/clear` | Clear test data | Development | `ClearDataResult` | None |
| `POST` | `/api/Test/initialize` | Initialize test data | Development | `InitializeDataResult` | None |
| `GET` | `/api/Test/status` | Get test status | Development | `TestStatusResult` | None |

### 10.4  API Testing and Documentation

#### 10.4.1  Testing Strategy
- **Unit Tests**: All application services have comprehensive unit tests following TDD approach
- **Integration Tests**: End-to-end API testing with proper authentication and authorization
- **Controller Tests**: Each controller endpoint is tested with various scenarios
- **Mock Services**: Repository and external service dependencies are properly mocked
- **Test Data**: BogusDataStore provides consistent test data for development

#### 10.4.2  API Documentation
- **Swagger/OpenAPI**: Available at `/swagger/index.html` when running locally
- **Production Swagger**: `https://api.eutonafila.com.br/swagger/index.html`
- **HTTP Files**: Test files in `http/` directory for manual API testing
- **Response Types**: All endpoints have proper `ProducesResponseType` attributes
- **Error Handling**: Consistent error responses with proper HTTP status codes
- **Authentication**: JWT-based authentication with role-based authorization

#### 10.4.3  Development Tools
- **Test Controller**: `/api/Test` endpoints for development and testing
- **Health Checks**: `/api/Health` for system monitoring
- **Performance Metrics**: `/api/Performance` for system performance monitoring
- **Logging**: Comprehensive logging throughout the application

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
    
    // No value objects; business logic uses primitives
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
    
    // No value objects; pricing is represented by primitives only
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
    
    // No value objects; pricing is represented by primitives only
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
    
    // No value objects; branding handled via primitives only
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
    
    // No value objects; validate via attributes/services
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
    
    // No value objects; validate via attributes/services
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
- Test business logic works with flattened, primitive properties
- Test validation rules on primitive properties are enforced
- Test edge cases and invariants without value objects

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

*Last updated: 2025-09-15*