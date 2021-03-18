using System;

namespace ExtraRolesMod.Roles.Medic
{
    public class DeadPlayer
    {
        public byte KillerId { get; set; }
        public byte PlayerId { get; set; }
        public DateTime KillTime { get; set; }
        public DeathReason DeathReason { get; set; }
    }
}
