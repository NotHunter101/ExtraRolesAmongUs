using ExtraRolesMod.Roles;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ExtraRolesMod
{
    public static class Extensions
    {
        public static bool IsPlayerRole(this PlayerControl plr, Role roleName)
        {
            return plr.GetModdedControl() != null && plr.GetModdedControl().Role == roleName;
        }

        /// <summary>
        /// Returns true if the player is shielded by the medic, false otherwise
        /// </summary>
        public static bool IsPlayerImmortal(this PlayerControl plr)
        {
            return plr.GetModdedControl() != null && plr.GetModdedControl().Immortal;
        }

        public static ModPlayerControl GetModdedControl(this PlayerControl plr)
        {
            return ExtraRoles.Logic.AllModPlayerControl.Find(x => x.PlayerControl == plr);
        }
        public static Color ToColor(this Vector3 vec)
        {
            return new Color(vec.x, vec.y, vec.z);
        }

        public static Vector3 ToVector(this Color color)
        {
            return new Vector3(color.r, color.g, color.b);
        }
    }
}
