using Microsoft.AspNetCore.Authorization;

namespace RF.Identity.Api.AuthorizationRequirements
{
    public class IsMemberOfGroupRequirement : IAuthorizationRequirement
    {
        public readonly string GroupId;
        public readonly string GroupName;

        public IsMemberOfGroupRequirement(string groupName, string groupId)
        {
            GroupName = groupName;
            GroupId = groupId;
        }
    }
}