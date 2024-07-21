using System;
using System.Linq;
using System.Security.Claims;

namespace Utility.Extensions
{
    public static class PrincipalExtensions
    {
        public static string GetClaim(this ClaimsPrincipal claimsPrincipal, string claimType)
        {
            return claimsPrincipal.Claims.FirstOrDefault(x => x.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase))?.Value?.ToString();
        }
    }
}
