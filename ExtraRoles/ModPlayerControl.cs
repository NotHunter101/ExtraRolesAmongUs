using ExtraRolesMod.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExtraRolesMod
{
    public class ModPlayerControl
    {
        public PlayerControl PlayerControl { get; set; }
        public Role Role { get; set; }
        public DateTime? LastAbilityTime { get; set; }
        public bool UsedAbility { get; set; }
        public bool Immortal { get; set; }
    }
}
