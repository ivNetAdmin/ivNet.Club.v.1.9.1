
using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace ivNet.Club
{
    public class Permissions : IPermissionProvider
    {

        public static readonly Permission ivMemberTab = new Permission
        {
            Description = "Access member website tab",
            Name = "ivMemberTab"
        };


        public static readonly Permission ivAdminTab = new Permission
        {
            Description = "Access admin website tab",
            Name = "ivAdminTab"
        };

        public static readonly Permission ivSiteAdmin = new Permission
        {
            Description = "Access site admin tools",
            Name = "ivSiteAdmin"
        };

        public static readonly Permission ivMembershipAdmin = new Permission
        {
            Description = "Access membership admin tools",
            Name = "ivMembershipAdmin"
        };

        public Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions()
        {
            return new[]
            {
                ivMemberTab,
                ivAdminTab,
                ivSiteAdmin,
                ivMembershipAdmin
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return null;
        }
    }
}