using System.Security.Claims;

namespace Aimachine.Extensions
{
    public static class ClaimsExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var idString = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(idString))
                throw new Exception("User ID not found in token"); // หรือ return 0 ตามชอบ

            return int.Parse(idString);
        }
    }
}
