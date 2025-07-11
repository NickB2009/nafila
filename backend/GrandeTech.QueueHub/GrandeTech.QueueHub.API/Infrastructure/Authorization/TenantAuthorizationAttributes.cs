using Microsoft.AspNetCore.Authorization;
using Grande.Fila.API.Domain.Users;

namespace Grande.Fila.API.Infrastructure.Authorization
{
    /// <summary>
    /// Custom authorization attributes for tenant-aware security
    /// </summary>
    /// 
    /// <summary>
    /// Requires Platform Admin role - can manage all organizations
    /// Use cases: UC-ANALYTICS, UC-APPLYUPDT, UC-EDITSHOP, UC-REDIRECTRULE, UC-SUBPLAN
    /// </summary>
    public class RequirePlatformAdminAttribute : AuthorizeAttribute
    {
        public RequirePlatformAdminAttribute()
        {
            Policy = "RequirePlatformAdmin";
        }
    }

    /// <summary>
    /// Requires Owner role within organization context
    /// Use cases: UC-CREATEBARBER, UC-ADDCOUPON, UC-CHANGECAP, UC-DISABLEQ, UC-LOCALADS
    /// </summary>
    public class RequireOwnerAttribute : AuthorizeAttribute
    {
        public RequireOwnerAttribute()
        {
            Policy = "RequireOwner";
        }
    }

    /// <summary>
    /// Legacy alias for RequireOwner - for backward compatibility
    /// </summary>
    public class RequireAdminAttribute : AuthorizeAttribute
    {
        public RequireAdminAttribute()
        {
            Policy = "RequireOwner"; // Map to Owner policy
        }
    }



    /// <summary>
    /// Requires Staff role within location context
    /// Use cases: UC-BARBERQUEUE, UC-CALLNEXT, UC-BARBERADD, UC-STAFFSTATUS, UC-STARTBREAK, UC-ENDBREAK, UC-FINISH, UC-SAVEHAIRCUT
    /// </summary>
    public class RequireStaffAttribute : AuthorizeAttribute
    {
        public RequireStaffAttribute()
        {
            Policy = "RequireStaff";
        }
    }

    /// <summary>
    /// Legacy alias for RequireStaff - for backward compatibility
    /// </summary>
    public class RequireBarberAttribute : AuthorizeAttribute
    {
        public RequireBarberAttribute()
        {
            Policy = "RequireStaff"; // Map to Staff policy
        }
    }

    /// <summary>
    /// Allows any authenticated user (Customer level access)
    /// Use cases: UC-ENTRY, UC-CANCEL, UC-CHECKIN, UC-QUEUELISTCLI, UC-WAITTIME, UC-LOGINCLIENT
    /// </summary>
    public class RequireCustomerAttribute : AuthorizeAttribute
    {
        public RequireCustomerAttribute()
        {
            Policy = "RequireCustomer";
        }
    }

    /// <summary>
    /// Legacy alias for RequireCustomer - for backward compatibility
    /// </summary>
    public class RequireClientAttribute : AuthorizeAttribute
    {
        public RequireClientAttribute()
        {
            Policy = "RequireCustomer"; // Map to Customer policy
        }
    }    /// <summary>
    /// Public access - no authentication required
    /// Use cases: UC-INPUTDATA (kiosk), UC-KIOSKCALL (public display), UC-KIOSKCANCEL (kiosk), UC-QRJOIN
    /// </summary>
    public class AllowPublicAccessAttribute : Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute
    {
    }

    /// <summary>
    /// Service account access for background processes
    /// Use cases: UC-CALCWAIT, UC-UPDATECACHE, UC-RESETAVG, UC-CAPLATE, UC-DETAINACTIVE
    /// </summary>
    public class RequireServiceAccountAttribute : AuthorizeAttribute
    {
        public RequireServiceAccountAttribute()
        {
            Policy = "RequireServiceAccount";
        }
    }
}
