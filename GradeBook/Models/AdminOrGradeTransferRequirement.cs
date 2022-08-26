using GradeBook.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GradeBook.Models
{
    public class AdminOrGradeTransferRequirement : IAuthorizationRequirement
    {
    }

    public class AdminOrGradeTransferHandler : AuthorizationHandler<AdminOrGradeTransferRequirement>
    {
        IGradeBookContext _context;

        public AdminOrGradeTransferHandler(IGradeBookContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, AdminOrGradeTransferRequirement requirement)
        {
            if (context.User.Identity is not null && context.User.Identity.IsAuthenticated)
                if ((await _context.Users.FirstOrDefaultAsync(u => u.Login == context.User.Identity.Name))
                    ?.IsAdminOrGradeTransfer ?? false)
                    context.Succeed(requirement);
        }
    }
}
