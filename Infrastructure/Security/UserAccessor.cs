using System.Security.Claims;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Security
{
    public class UserAccessor : IUserAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Constructor injection for IHttpContextAccessor
        public UserAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserName()
        {
            // Using null-conditional operator to prevent NullReferenceException
            // and returning the Name claim from the JWT token
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
        }

        // Optional: If you need user ID as int (since your DB uses int keys)
        public int GetUserId()
        {
            var idClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(
                ClaimTypes.NameIdentifier
            );
            return int.TryParse(idClaim, out var id) ? id : 0;
        }
    }
}
