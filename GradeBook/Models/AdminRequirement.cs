using GradeBook.Storage;
using GradeBook.Storage.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GradeBook.Models
{
    public class AdminRequirement : IAuthorizationRequirement
    {

    }

    public class AdminHandler : AuthorizationHandler<AdminRequirement>
    {
        IGradeBookContext _context;

        public AdminHandler(IGradeBookContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, AdminRequirement requirement)
        {
            if (context.User.Identity is not null && context.User.Identity.IsAuthenticated)
                if ((await _context.Users.FirstOrDefaultAsync(u => u.Login == context.User.Identity.Name))
                    ?.IsAdmin ?? false)
                    context.Succeed(requirement);
        }
    }
}
