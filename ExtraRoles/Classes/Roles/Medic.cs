using UnityEngine;

namespace ExtraRoles2.Classes.Roles
{
    public class Medic : Role
    {
        public Medic(Player owner)
        {
            Owner = owner;
            Id = RoleId.Medic;
            Name = "Medic";
            Color = new Color(0f, 252f / 255f, 88 / 255f);
        }
    }
}