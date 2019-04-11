using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RF.ContentSearch.Api.AuthorizationRequirements
{
    public class IsMemberOfGroupHandler : AuthorizationHandler<IsMemberOfGroupRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, IsMemberOfGroupRequirement requirement)
        {
            var groupClaim = context.User.Claims
                 .FirstOrDefault(claim => claim.Type == "groups" &&
                     claim.Value.Equals(requirement.GroupId, StringComparison.InvariantCultureIgnoreCase));

            if (groupClaim != null)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}