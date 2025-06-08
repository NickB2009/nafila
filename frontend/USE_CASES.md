# Use Cases Documentation

## Overview
This document contains all the use cases for the Eutonafila queue management system, organized by priority and implementation phase.

## Use Case Classification

### Priority Levels
- **Priority 1**: MVP Core Features
- **Priority 2**: MVP Enhanced Features  
- **Priority 3**: Version 2 Features
- **Priority 4**: Future/Posterior Features

### Implementation Status
- **MVP**: Minimum Viable Product
- **V2**: Version 2 Features
- **Posterior**: Future Implementation

---

## MVP Core Features (Priority 1)

### Authentication & Access Control
| ID | Role | Use Case | Description |
|---|---|---|---|
| UC-ADMINLOGIN | Admin/Owner | Login to admin panel | Admin logs into the panel to manage settings and view analytics and history. |
| UC-BARBERLOGIN | Barber | Login to barber panel | Barber logs in to their dashboard to manage the queue. |
| UC-LOGINCLIENT | Client | Login (optional) | Client logs into the system for personalized access (optional). |
| UC-LOGINWEB | System | Login via web portal | User logs in securely using quick web or kiosk login forms. |
| UC-JWT | Auth | JWT token authentication | System issues JWT token after successful login. |
| UC-PROTECT | Auth | Protect sensitive routes | System restricts access to sensitive areas via middleware. |
| UC-QRJOIN | Auth | QR code to client URL | Client uses QR code to be redirected to be added to queue page by logging in or anonymously adding his name to queue. |

### Queue Management
| ID | Role | Use Case | Description |
|---|---|---|---|
| UC-ENTRY | Client | Enter queue at barbershop | Client is in a barbershop page and joins the queue through a mobile or kiosk interface. |
| UC-INPUTDATA | Kiosk | Input basic data (name, phone) | Client inputs only essential info (name) to get a ticket. |
| UC-ASKPROFILE | System | Asks if using a profile | System asks client if he wants to get in the line with or without a profile. |
| UC-BARBERADD | Barber | Add client to line | Barber adds client to line (automatically goes to the end of the present pool). |
| UC-CHECKIN | Client | Check Client in | When client arrives barber moves the client from the potential queue to the present pool. |
| UC-CALLNEXT | Barber | Call next client | Barber clicks to call the next person in line for service. |
| UC-CANCEL | Client | Cancel place in line | Client chooses to leave the queue, freeing up their spot for others. |
| UC-KIOSKCANCEL | Kiosk | Cancel attendance via kiosk | Client removes themselves from the list. |
| UC-FINISH | Barber | Finish current appointment | Barber marks current service as complete and system logs the appointment duration. |

### Queue Viewing & Status
| ID | Role | Use Case | Description |
|---|---|---|---|
| UC-BARBERQUEUE | Barber | View current queue | Barber views the current queue of waiting clients. |
| UC-QUEUELISTCLI | Client | View real-time queue status | Client can view the live queue status on the barbershop page. |
| UC-KIOSKCALL | Kiosk | Watch queue call locally | Client watches queue updates and callouts on-screen at location, showing who is or not at location. |
| UC-TRACKQ | Admin/Owner | Track live queue activity | Admin sees real-time queue and barber activity. |

### Staff Management
| ID | Role | Use Case | Description |
|---|---|---|---|
| UC-ADDBARBER | Admin/Owner | Add barbers | Admin adds a new barber to the system. |
| UC-EDITBARBER | Admin/Owner | Edit barber details | Admin edits existing barber profiles or roles. |
| UC-STAFFSTATUS | Barber | Change status (available/unavailable) | Barber updates their availability manually (e.g., busy, free). |
| UC-SAVEHAIRCUT | Barber | Saving haircut | Barber adds haircut details to person's profile with a dropdown menu. |

### Barbershop Management
| ID | Role | Use Case | Description |
|---|---|---|---|
| UC-CREATEBARBER | Admin/Owner | Create new barbershop | Platform-level admin creates a new barbershop tenant. (Attached to billing) |
| UC-BRANDING | Admin/Owner | Customize branding | Admin updates branding (name, colors, etc.) per tenant. |
| UC-DISABLEQ | Admin/Owner | Temporarily disable queue | Admin pauses queue access temporarily (e.g., holiday). |
| UC-MANAGESERV | Admin/Owner | Manage services (name/duration) | Admin sets up offered services and durations. |
| UC-SETDURATION | Admin/Owner | Set estimated time per service | Admin defines estimated service durations per type. |

### System Core Functions
| ID | Role | Use Case | Description |
|---|---|---|---|
| UC-CALCWAIT | Admin Master | Calculate estimated wait time | Algorithm calculates estimated wait times based on recent data. |
| UC-UPDATECACHE | System | Update average cache | System stores new wait time averages in in-memory cache. |
| UC-RESETAVG | System | Reset average time every 3 months | System resets wait time averages periodically for accuracy. |
| UC-ANALYTICS | Admin Master | View cross-barbershop analytics | Admin sees aggregate data across all barbershops that agree to reveal their data. |
| UC-APPLYUPDT | Admin Master | Apply system updates | Admin performs system-wide updates or upgrades. |

---

## MVP Enhanced Features (Priority 2)

### Advanced Queue Features
| ID | Role | Use Case | Description |
|---|---|---|---|
| UC-CAPLATE | System | Cap Late Clients | System removes Clients who are more delayed than determined time. |
| UC-CHANGECAP | Admin | Change Cap Time | Admin Owner sets/changes max time for delayed Clients. |
| UC-WAITTIME | Client | View estimated wait time | Client views the expected wait time calculated by the system algorithm based on real-time data. |

### Staff Break Management
| ID | Role | Use Case | Description |
|---|---|---|---|
| UC-STARTBREAK | Barber | Start break | Barber starts a timed break, updating their availability status. |
| UC-ENDBREAK | Barber | End break | Barber ends their break and returns to active status. |
| UC-RETURNREM | Barber | Receive return reminder | System reminds barber when it's time to return from break. |
| UC-DETAINACTIVE | System | Detect inactive barbers | System marks barbers as inactive if idle too long and with clients. |

### Notifications & Communication
| ID | Role | Use Case | Description |
|---|---|---|---|
| UC-SMSNOTIF | Client | Receive SMS notification | System sends SMS to notify the client when it's nearly their turn. |
| UC-TURNREM | System | Remind of Turn | System reminds in app/push/WhatsApp clients their turn is upcoming or has arrived. |
| UC-COUPONNOTIF | System | Coupon Notification | System notifies clients of coupons. |

### Marketing & Promotions
| ID | Role | Use Case | Description |
|---|---|---|---|
| UC-ADDCOUPON | Admin | Adding Coupon | Admin Adds a Coupon. |
| UC-LOCALADS | Admin | Local Ads | Local Ads can be added to the line page on the kiosk. |

### Analytics & Reporting
| ID | Role | Use Case | Description |
|---|---|---|---|
| UC-METRICS | Admin/Owner | View metrics and reports | Admin accesses performance and usage reports. |
| UC-EDITSHOP | Admin Master | Edit barbershop settings | Admin edits tenant data like name, configs, and theme. |
| UC-MULTILOC | Admin/Owner | Manage multiple locations | Admin oversees multiple barbershops under same account. |
| UC-REDIRECTRULE | Admin Master | Set redirect rules for busy barbershops | System uses rules to route clients to niche barbershops. |
| UC-SUBPLAN | Admin Master | Manage subscription plans (future) | Admin handles subscriptions or billing plans (future). |

---

## Future Features (Version 2+)

### Enhanced Client Features (Priority 3)
| ID | Role | Use Case | Description |
|---|---|---|---|
| - | Barber | View service history | Barber reviews their past clients and time spent (if feature toggled on). |
| - | Client | Change barbershop | Client switches to a different barbershop page available on the platform. |
| - | Client | View service history (future) | Client accesses their history of past visits and services (future feature). |
| - | Client | Next haircut reminder | Client asks system to remind them in a set period of time. |
| - | Client | Locations' Map | Client chooses from a variety of locations on a map and is directed to their tenant. |

### Advanced Features (Priority 4 - Posterior)
| ID | Role | Use Case | Description |
|---|---|---|---|
| - | Admin/Owner | Manage SMS configurations | Admin configures the content and timing of Push Notification/WhatsApp notifications. |
| - | Client | Be redirected to another barbershop | System offers to redirect the client to a nearby less busy barbershop. |
| - | Client | Rate barber after service (future) | Client rates the barber or experience post-appointment (future feature). |

### Advertisement System (No Priority Assigned)
| ID | Role | Use Case | Description |
|---|---|---|---|
| UC-ADROTATE | - | Rotate in-house ads | Cycles active ads on the screen at the admin-defined interval. |
| UC-KIOSKADS | - | Run ads on kiosk and credit discount to barber | Handles kiosk ad playlist + optional % credit field. |
| UC-MEDIAUP | - | Barber uploads images / videos | Lets staff upload media that becomes an Advertisement asset. |
| UC-QBOARD | - | Display queue board (big screen) | Shows live list + check-in status beside ads. |

---

## Model Use-Case Mapping

| Model | Use-Cases that **touch** it (CRUD or read-only) |
|---|---|
| **Organization** | UC-CREATEBARBER, UC-MULTILOC, UC-SUBPLAN, UC-ANALYTICS |
| **SubscriptionPlan** | UC-SUBPLAN, UC-CREATEBARBER |
| **ServiceProvider** | UC-ANALYTICS, UC-BRANDING, UC-CHANGECAP, UC-CREATEBARBER, UC-DISABLEQ, UC-EDITSHOP, UC-LOCALADS, UC-REDIRECTRULE, UC-SUBPLAN, UC-MULTILOC, UC-TRACKQ, UC-METRICS, UC-STARTBREAK, UC-ENDBREAK, UC-QBOARD, UC-KIOSKCALL, UC-ENTRY, UC-QUEUELISTCLI, UC-WAITTIME, UC-CALLNEXT, UC-FINISH, UC-BARBERQUEUE, UC-STAFFSTATUS, UC-BARBERADD, UC-INPUTDATA, UC-UPDATECACHE, UC-RESETAVG, UC-CAPLATE |
| **Queue** | UC-ENTRY, UC-CANCEL, UC-QUEUELISTCLI, UC-WAITTIME, UC-KIOSKCALL, UC-KIOSKCANCEL, UC-CALLNEXT, UC-CHECKIN, UC-TRACKQ, UC-BARBERQUEUE, UC-INPUTDATA, UC-BARBERADD, UC-CAPLATE, UC-CHANGECAP |
| **QueueEntry** | UC-ENTRY, UC-CANCEL, UC-CALLNEXT, UC-CHECKIN, UC-FINISH, UC-BARBERADD, UC-KIOSKCANCEL, UC-QUEUELISTCLI, UC-WAITTIME, UC-INPUTDATA, UC-BARBERQUEUE, UC-CAPLATE |
| **Customer** | UC-ENTRY, UC-INPUTDATA, UC-ASKPROFILE, UC-LOGINCLIENT, UC-LOGINWEB, UC-SMSNOTIF, UC-TURNREM, UC-COUPONNOTIF |
| **StaffMember** | UC-BARBERLOGIN, UC-ADDBARBER, UC-EDITBARBER, UC-STARTBREAK, UC-ENDBREAK, UC-STAFFSTATUS, UC-CALLNEXT, UC-FINISH, UC-DETAINACTIVE, UC-BARBERQUEUE, UC-BARBERADD, UC-MEDIAUP |
| **StaffBreak** | UC-STARTBREAK, UC-ENDBREAK |
| **ServiceType** | UC-MANAGESERV, UC-SETDURATION, UC-WAITTIME, UC-FINISH, UC-SAVEHAIRCUT |
| **ServiceMetrics** | UC-WAITTIME, UC-CALCWAIT, UC-UPDATECACHE, UC-RESETAVG, UC-METRICS |
| **Coupon** | UC-ADDCOUPON, UC-COUPONNOTIF, UC-LOCALADS, UC-KIOSKADS |
| **Advertisement** | UC-LOCALADS, UC-ADROTATE, UC-QBOARD, UC-KIOSKADS, UC-MEDIAUP |
| **MediaAsset** | UC-MEDIAUP, UC-KIOSKADS |
| **Notification** | UC-SMSNOTIF, UC-TURNREM, UC-COUPONNOTIF, UC-RETURNREM |
| **UserAccount** | UC-LOGINCLIENT, UC-LOGINWEB, UC-JWT, UC-PROTECT, UC-BARBERLOGIN, UC-QRJOIN, UC-ADMINLOGIN |
| **Feedback** | (not used in P1-P2 MVP list; first appears with UC-RATE which is V2) |

---

## Frontend Implementation Priority

### Phase 1 - Core Queue Management (Current Focus)
**Target Use Cases for UI Development:**
- UC-ENTRY: Client queue entry interface
- UC-QUEUELISTCLI: Real-time queue status display
- UC-BARBERQUEUE: Barber queue management view
- UC-CALLNEXT: Next client call interface
- UC-CANCEL: Queue cancellation functionality

### Phase 2 - Staff Management
- UC-BARBERLOGIN: Barber authentication
- UC-STAFFSTATUS: Status management interface
- UC-ADDBARBER: Admin staff management

### Phase 3 - Enhanced Features
- UC-WAITTIME: Wait time estimation display
- UC-SMSNOTIF: Notification preferences
- UC-METRICS: Analytics dashboard
