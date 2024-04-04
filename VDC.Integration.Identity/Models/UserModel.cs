using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace VDC.Integration.Identity.Models
{
    public class UserModel : IdentityUser
    {
        public double ExpiresMinutes { get; set; }
        public List<UserRoleModel> Roles { get; set; }
        public int? TenantId { get; set; }
    }

    public class UserRoleModel
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }

        public UserRoleModel(string id, string name, bool selected)
        {
            Id = id;
            Name = name;
            Selected = selected;
        }

    }
}
