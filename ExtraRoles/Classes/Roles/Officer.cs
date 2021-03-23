using UnityEngine;

namespace ExtraRoles2.Classes.Roles
{
    public class Officer : Role
    {
        public Officer(Player owner)
        {
            Owner = owner;
            Id = RoleId.Officer;
            Name = "Officer";
            Color = new Color(0f, 97f / 255f, 252f / 255f);
        }
    }
}