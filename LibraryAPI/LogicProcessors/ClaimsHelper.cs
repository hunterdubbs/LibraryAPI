using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LibraryAPI.LogicProcessors
{
    public static class ClaimsHelper
    {
        public static string GetUserIDFromClaim(ClaimsPrincipal user)
        {
            return user.Claims.First(c => c.Type == ClaimTypes.PrimarySid)?.Value;
        }

        public static string GetUserNameFromClaim(ClaimsPrincipal user)
        {
            return user.Claims.First(c => c.Type == ClaimTypes.Name)?.Value;
        }
    }
}
