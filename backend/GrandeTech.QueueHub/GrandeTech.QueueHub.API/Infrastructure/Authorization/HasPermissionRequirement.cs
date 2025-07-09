using Microsoft.AspNetCore.Authorization;

namespace Grande.Fila.API.Infrastructure.Authorization
{
    public class HasPermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public HasPermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
} 