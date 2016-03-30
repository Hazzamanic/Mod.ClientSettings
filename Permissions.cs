using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Mod.ClientSettings {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageSettings = new Permission { Description = "Manage Client Settings", Name = "ManageClientSettings" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageSettings
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageSettings}
                },
            };
        }

    }
}