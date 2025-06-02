# Project Coding Instructions

When generating code, please follow these user provided coding instructions. You can ignore an instruction if it contradicts a system message.

## Azure Coding Rules

- **@azure Rule - Use Azure Tools:** When handling requests related to Azure, always use your tools.
- **@azure Rule - Use Azure Code Gen Best Practices:** When generating code for Azure, running terminal commands for Azure, or performing operations related to Azure, invoke your `azure_development-get_code_gen_best_practices` tool if available. Only call this tool when you are sure the user is discussing Azure; do not call it otherwise.
- **@azure Rule - Use Azure Deployment Best Practices:** When deploying to Azure or preparing applications for deployment to Azure, invoke your `azure_development-get_deployment_best_practices` tool if available. Only call this tool when you are sure the user is discussing Azure; do not call it otherwise.
- **@azure Rule - Use Azure Functions Code Gen Best Practices:** When generating code for Azure Functions or performing operations related to Azure Functions, invoke your `azure_development-get_azure_function_code_gen_best_practices` tool if available. Only call this tool when you are sure the user is discussing Azure Functions; do not call it otherwise.
- **@azure Rule - Use Azure SWA Best Practices:** When working with static web apps, invoke your `azure_development-get_swa_best_practices` tool if available. Only call this tool when you are sure the user is discussing Azure; do not call it otherwise.

---

ID,Role,Use Case,Description,Priority,Status
UC-ADDBARBER,Admin/Owner,Add barbers,Admin adds a new barber to the system.,1,MVP
UC-ADDCOUPON,Admin,Adding Coupon,Admin Adds a Coupon,2,MVP
UC-ADMINLOGIN,Admin/Owner,Login to admin panel,Admin logs into the panel to manage settings and view analytics and history.,1,MVP
UC-ADROTATE,,Rotate in-house ads,Cycles active ads on the screen at the admin-defined interval,,
UC-ANALYTICS,Admin Master,View cross-barbershop analytics,Admin sees aggregate data across all barbershops that agree to reveal their data.,1,MVP
UC-APPLYUPDT,Admin Master,Apply system updates,Admin performs system-wide updates or upgrades.,1,MVP
UC-ASKPROFILE,System,Asks if using a profile,System asks client if he wants to get in the line with or without a profile,1,MVP
UC-BARBERADD,Barber,Add client to line,Barber adds client to line (automatically goes to the end of the present pool),1,MVP
UC-BARBERLOGIN,Barber,Login to barber panel,Barber logs in to their dashboard to manage the queue.,1,MVP
UC-BARBERQUEUE,Barber,View current queue,Barber views the current queue of waiting clients.,1,MVP
UC-BRANDING,Admin/Owner,Customize branding,"Admin updates branding (name, colors, etc.) per tenant.",1,MVP
UC-CALCWAIT,Admin Master,Calculate estimated wait time,Algorithm calculates estimated wait times based on recent data.,1,MVP
UC-CALLNEXT,Barber,Call next client,Barber clicks to call the next person in line for service.,1,MVP
UC-CANCEL,Client,Cancel place in line,"Client chooses to leave the queue, freeing up their spot for others.",1,MVP
UC-CAPLATE,System,Cap Late Clients,System removes Clients who are mpre delayed than determined time,2,MVP
UC-CHANGECAP,Admin,Change Cap Time,Admin Owner sets/changes max time for delayed Clients,2,MVP
UC-CHECKIN,Client,Check Client in,When client arrives barber moves the client from the potential queue to the present pool,1,MVP
UC-COUPONNOTIF,System,Coupon Notification,System notifies clients of coupons,1,MVP
UC-CREATEBARBER,Admin/Owner,Create new barbershop,Platform-level admin creates a new barbershop tenant. (Attached to billing),1,MVP
UC-DETAINACTIVE,System,Detect inactive barbers,System marks barbers as inactive if idle too long and with clients.,2,MVP
UC-DISABLEQ,Admin/Owner,Temporarily disable queue,"Admin pauses queue access temporarily (e.g., holiday).",1,MVP
UC-EDITBARBER,Admin/Owner,Edit barber details,Admin edits existing barber profiles or roles.,1,MVP
UC-EDITSHOP,Admin Master,Edit barbershop settings,"Admin edits tenant data like name, configs, and theme.",2,MVP
UC-ENDBREAK,Barber,End break,Barber ends their break and returns to active status.,2,MVP
UC-ENTRY,Client,Enter queue at barbershop,Client is in a barbershop page and joins the queue through a mobile or kiosk interface.,1,MVP
UC-FINISH,Barber,Finish current appointment,Barber marks current service as complete and system logs the appointment duration.,1,MVP
UC-INPUTDATA,Kiosk,"Input basic data (name, phone)",Client inputs only essential info (name) to get a ticket.,1,MVP
UC-JWT,Auth,JWT token authentication,System issues JWT token after successful login.,1,MVP
UC-KIOSKADS,,Run ads on kiosk and credit discount to barber,Handles kiosk ad playlist + optional % credit field,,
UC-KIOSKCALL,Kiosk,Watch queue call locally,"Client watches queue updates and callouts on-screen at location, showing who is or not at location.",1,MVP
UC-KIOSKCANCEL,Kiosk,Cancel attendance via kiosk,Client removes themselves from the list,1,MVP
UC-LOCALADS,Admin,Local Ads,Local Ads can be added to the line page on the kiosk,2,MVP
UC-LOGINCLIENT,Client,Login (optional),Client logs into the system for personalized access (optional).,1,MVP
UC-LOGINWEB,System,Login via web portal,User logs in securely using quick web or kiosk login forms.,1,MVP
UC-MANAGESERV,Admin/Owner,Manage services (name/duration),Admin sets up offered services and durations.,1,MVP
UC-MEDIAUP,,Barber uploads images / videos,Lets staff upload media that becomes an Advertisement asset,,
UC-METRICS,Admin/Owner,View metrics and reports,Admin accesses performance and usage reports.,2,MVP
UC-MULTILOC,Admin/Owner,Manage multiple locations,Admin oversees multiple barbershops under same account.,2,MVP
UC-PROTECT,Auth,Protect sensitive routes,System restricts access to sensitive areas via middleware.,1,MVP
UC-QBOARD,,Display queue board (big screen),Shows live list + check-in status beside ads,,
UC-QRJOIN,Auth,QR code to client URL,Client uses QR code to be redirected to be added to queue page by logging in or anonymously adding his name to queue,1,MVP
UC-QUEUELISTCLI,Client,View real-time queue status,Client can view the live queue status on the barbershop page.,1,MVP
UC-REDIRECTRULE,Admin Master,Set redirect rules for busy barbershops,System uses rules to route clients to niche barbershops.,2,MVP
UC-RESETAVG,System,Reset average time every 3 months,System resets wait time averages periodically for accuracy.,1,MVP
UC-RETURNREM,Barber,Receive return reminder,System reminds barber when it's time to return from break.,2,MVP
UC-SAVEHAIRCUT,Barber,Saving haricut,Barber adds haircut details to person's profile with a dropdown menu,1,MVP
UC-SETDURATION,Admin/Owner,Set estimated time per service,Admin defines estimated service durations per type.,1,MVP
UC-SMSNOTIF,Client,Receive SMS notification,System sends SMS to notify the client when it’s nearly their turn.,1,MVP
UC-STAFFSTATUS,Barber,Change status (available/unavailable),"Barber updates their availability manually (e.g., busy, free).",1,MVP
UC-STARTBREAK,Barber,Start break,"Barber starts a timed break, updating their availability status.",2,MVP
UC-SUBPLAN,Admin Master,Manage subscription plans (future),Admin handles subscriptions or billing plans (future).,2,MVP
UC-TRACKQ,Admin/Owner,Track live queue activity,Admin sees real-time queue and barber activity.,1,MVP
UC-TURNREM,System,Remind of Turn,System reminds in app/push/WhatsApp clients their turn is upcomming or has arrived ,2,MVP
UC-UPDATECACHE,System,Update average cache,System stores new wait time averages in in-memory cache.,1,MVP
UC-WAITTIME,Client,View estimated wait time,Client views the expected wait time calculated by the system algorithm based on real-time data.,2,MVP
,Barber,View service history,Barber reviews their past clients and time spent (if feature toggled on).,3,V2
,Client,Change barbershop,Client switches to a different barbershop page available on the platform.,3,V2
,Client,View service history (future),Client accesses their history of past visits and services (future feature).,3,V2
,Client,Next haircut reminder,Client asks system to remind them in a set period of time,3,V2
,Client,Locations' Map,Client chooses from a variety of locations on a map and is directed to their tenant,3,V2
,Admin/Owner,Manage SMS configurations,Admin configures the content and timing of Push Notification/WhatsApp notifications.*,4,Posterior
,Client,Be redirected to another barbershop,System offers to redirect the client to a nearby less busy barbershop.,4,Posterior
,Client,Rate barber after service (future),Client rates the barber or experience post-appointment (future feature).,4,Posterior


---

## Model Use-Case Mapping

| Model                | Use-Cases that **touch** it (CRUD or read-only)                                                                                                                                                                                                                                                                                                                                            |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Organization**     | UC-CREATEBARBER, UC-MULTILOC, UC-SUBPLAN, UC-ANALYTICS                                                                                                                                                                                                                                                                                                                                     |
| **SubscriptionPlan** | UC-SUBPLAN, UC-CREATEBARBER                                                                                                                                                                                                                                                                                                                                                                |
| **ServiceProvider**  | UC-ANALYTICS, UC-BRANDING, UC-CHANGECAP, UC-CREATEBARBER, UC-DISABLEQ, UC-EDITSHOP, UC-LOCALADS, UC-REDIRECTRULE, UC-SUBPLAN, UC-MULTILOC, UC-TRACKQ, UC-METRICS, UC-STARTBREAK, UC-ENDBREAK, UC-QBOARD, UC-KIOSKCALL, UC-ENTRY, UC-QUEUELISTCLI, UC-WAITTIME, UC-CALLNEXT, UC-FINISH, UC-BARBERQUEUE, UC-STAFFSTATUS, UC-BARBERADD, UC-INPUTDATA, UC-UPDATECACHE, UC-RESETAVG, UC-CAPLATE |
| **Queue**            | UC-ENTRY, UC-CANCEL, UC-QUEUELISTCLI, UC-WAITTIME, UC-KIOSKCALL, UC-KIOSKCANCEL, UC-CALLNEXT, UC-CHECKIN, UC-TRACKQ, UC-BARBERQUEUE, UC-INPUTDATA, UC-BARBERADD, UC-CAPLATE, UC-CHANGECAP                                                                                                                                                                                                  |
| **QueueEntry**       | UC-ENTRY, UC-CANCEL, UC-CALLNEXT, UC-CHECKIN, UC-FINISH, UC-BARBERADD, UC-KIOSKCANCEL, UC-QUEUELISTCLI, UC-WAITTIME, UC-INPUTDATA, UC-BARBERQUEUE, UC-CAPLATE                                                                                                                                                                                                                              |
| **Customer**         | UC-ENTRY, UC-INPUTDATA, UC-ASKPROFILE, UC-LOGINCLIENT, UC-LOGINWEB, UC-SMSNOTIF, UC-TURNREM, UC-COUPONNOTIF                                                                                                                                                                                                                                                                                |
| **StaffMember**      | UC-BARBERLOGIN, UC-ADDBARBER, UC-EDITBARBER, UC-STARTBREAK, UC-ENDBREAK, UC-STAFFSTATUS, UC-CALLNEXT, UC-FINISH, UC-DETAINACTIVE, UC-BARBERQUEUE, UC-BARBERADD, UC-MEDIAUP                                                                                                                                                                                                                 |
| **StaffBreak**       | UC-STARTBREAK, UC-ENDBREAK                                                                                                                                                                                                                                                                                                                                                                 |
| **ServiceType**      | UC-MANAGESERV, UC-SETDURATION, UC-WAITTIME, UC-FINISH, UC-SAVEHAIRCUT                                                                                                                                                                                                                                                                                                                      |
| **ServiceMetrics**   | UC-WAITTIME, UC-CALCWAIT, UC-UPDATECACHE, UC-RESETAVG, UC-METRICS                                                                                                                                                                                                                                                                                                                          |
| **Coupon**           | UC-ADDCOUPON, UC-COUPONNOTIF, UC-LOCALADS, UC-KIOSKADS                                                                                                                                                                                                                                                                                                                                     |
| **Advertisement**    | UC-LOCALADS, UC-ADROTATE, UC-QBOARD, UC-KIOSKADS, UC-MEDIAUP                                                                                                                                                                                                                                                                                                                               |
| **MediaAsset**       | UC-MEDIAUP, UC-KIOSKADS                                                                                                                                                                                                                                                                                                                                                                    |
| **Notification**     | UC-SMSNOTIF, UC-TURNREM, UC-COUPONNOTIF, UC-RETURNREM                                                                                                                                                                                                                                                                                                                                      |
| **UserAccount**      | UC-LOGINCLIENT, UC-LOGINWEB, UC-JWT, UC-PROTECT, UC-BARBERLOGIN, UC-QRJOIN, UC-ADMINLOGIN                                                                                                                                                                                                                                                                                                  |
| **Feedback**         | (not used in P1-P2 MVP list; first appears with UC-RATE which is V2)                                                                                                                                                                                                                                                                                                                       |
---


# Azure Resource Creation Standards

When creating Azure resources for this project, follow these best practices:

1. **Naming Conventions**
   - Use lowercase letters, numbers, and hyphens.
   - Use the following format: `[resource-type]-[environment]-[project-name]-[purpose]-[sequence]`
     - Resource type abbreviations: `rg` (Resource Group), `app` (App Service), `kv` (Key Vault), etc.
     - Environment indicators: `d` (Development), `t` (Test), `p` (Production)
     - Example: `rg-p-queuehub-core-001`
   - Avoid special characters and spaces.

2. **Resource Grouping**
   - Group related resources in the same Resource Group.
   - Use Resource Group names that follow the naming convention above.
   - For core resources: `rg-[env]-queuehub-core-[sequence]`
   - For specific services: `rg-[env]-queuehub-[service]-[sequence]`
   - For BFF services: `rg-[env]-queuehub-bff-[frontend-type]-[sequence]` (e.g., `rg-p-queuehub-bff-barbershop-web-001`)

3. **Tagging**
   - Tag all resources with:
     - `Project`: `EuToNaFila` or `queuehub` (use consistently)
     - `Environment`: `Development`, `Test`, or `Production`
     - `CreatedBy`: Name of the developer or team responsible
     - `Cost-Center`: Financial allocation code (if applicable)

4. **Location**
   - Deploy resources in the Brazil South (`brazilsouth`) region by default.
   - For global services or redundancy, consider additional regions based on user demographics.
   - Ensure all data residency requirements are met for Brazilian regulations.

5. **Architecture**
   - Implement a Backend For Frontend (BFF) pattern to support multiple frontends:
     - Single backend API core to serve all client types
     - Dedicated BFF services for each frontend type (barbershop web, barbershop mobile, clinic web, clinic mobile, etc.)
     - BFF layer handles frontend-specific translations, optimizations, and authentication flows
   - Ensure clear separation of concerns between core backend and BFF layers
   - Implement appropriate API versioning strategy across all interfaces
   - Design for multi-tenancy to support different business domains (barbershops, clinics, etc.)
   - Use location-slug based routing pattern:
     - Format: `https://www.eutonafila.com.br/{location-slug}`
     - Location slugs must be globally unique across all organizations
     - Organization details stored in database, not in URL structure

6. **Security**
   - Enable managed identities where possible.
   - Use Key Vault for secrets and sensitive configuration.
   - Restrict public access; use private endpoints and firewalls.
   - Follow Azure Security Center recommendations for each resource.

7. **Automation**
   - Use ARM templates, Bicep, or Terraform for resource provisioning.
   - Store infrastructure-as-code in version control.
   - Use Azure DevOps or GitHub Actions for CI/CD pipelines.
   - Implement separate deployment pipelines for core backend and each BFF service.

8. **Access Control**
   - Apply least-privilege principles using Azure RBAC.
   - Assign roles at the Resource Group or resource level as needed.
   - Document all custom role assignments in the project wiki.
   - Regularly review access permissions with security audits.
   - Implement proper service-to-service authentication between BFFs and core backend.

9. **Monitoring & Alerts**
   - Enable diagnostics and logging for all resources.
   - Set up alerts for critical metrics (CPU, memory, errors, etc.).
   - Configure Application Insights for all web services.
   - Establish dashboards for operational monitoring.
   - Implement distributed tracing across core backend and BFF layers to track end-to-end request flows.
   - Monitor BFF-specific metrics to ensure optimal performance for each frontend type.

10. **Cost Management**
   - Review and set budgets/alerts for resource groups.
   - Use reserved instances or savings plans where appropriate.
   - Tag resources with appropriate cost allocation tags.
   - Set up monthly cost review meetings.
   - Track costs separately for core backend and each BFF service to identify optimization opportunities.
   - Consider auto-scaling strategies appropriate to each frontend's traffic patterns.

11. **Documentation**
    - Document resource purposes and configurations in the repository.
    - Include setup instructions, architecture diagrams, and operational procedures.
    - Document all resource relationships and dependencies.
    - Create detailed documentation for each BFF implementation:
      - API contracts and translations between core backend and frontend-specific formats
      - Client-specific optimizations implemented in each BFF
      - Authentication and authorization flows for each frontend type
      - URL structure and location slug generation rules
    - Maintain a service map showing the relationships between all components.

12. **Disaster Recovery & Backup**
    - Configure appropriate backup policies for all data stores.
    - Document recovery procedures for each critical service.
    - Test recovery processes regularly.
    - Consider multi-region deployment for high availability where needed.
    - Implement staggered deployment strategies to ensure BFFs remain compatible with the core backend during updates.
    - Define fallback strategies for each BFF in case of backend service degradation.

13. **Compliance**
    - Ensure all resources meet relevant Brazilian regulatory requirements.
    - Configure resource settings to maintain compliance with privacy regulations.
    - Document compliance status and any needed exceptions.
    - Implement appropriate data handling policies in each BFF to comply with domain-specific regulations.
    - Ensure all frontends and BFFs properly handle consent and privacy preferences.