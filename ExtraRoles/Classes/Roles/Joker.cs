using UnityEngine;

namespace ExtraRoles2.Classes.Roles
{
    public class Joker : Role
    {
        public Joker(Player owner)
        {
            Owner = owner;
            Id = RoleId.Joker;
            Name = "Joker";
            Teamless = true;
            Color = new Color(254f / 255f, 185f / 255f, 209f / 255f);
        }
    }
}