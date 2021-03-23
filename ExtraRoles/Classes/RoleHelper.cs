using System.Collections.Generic;
using System.Linq;
using ExtraRoles2.Classes.Roles;
using Hazel;
using Reactor.Extensions;

namespace ExtraRoles2.Classes
{
    public class RoleHelper
    {
        public static RoleHelper Instance { get; set; }
        
        public void RpcAssignRoles()
        {
            List<PlayerControl> allCrew = PlayerControl.AllPlayerControls.ToArray().ToList().FindAll(x => !x.Data.IsImpostor);
            List<RoleId> rolesToAssign =
                new List<RoleId>(new[] {RoleId.Engineer, RoleId.Joker, RoleId.Medic, RoleId.Officer});
            
            List<RpcRole> assignedRoles = new List<RpcRole>();
            MessageWriter writer = RpcHelper.Instance.GeneratePacket(CustomRPC.SetRoles);
            
            while (rolesToAssign.Count > 0 && allCrew.Count > 0)
            {
                RoleId assignedRole = rolesToAssign.Random();
                rolesToAssign.Remove(assignedRole);

                PlayerControl rolePlayer = allCrew.Random();
                allCrew.Remove(rolePlayer);
                
                AssignRole(assignedRole, rolePlayer);
                assignedRoles.Add(new RpcRole(assignedRole, rolePlayer));
            }

            writer.Write(assignedRoles.Count);
            foreach (RpcRole role in assignedRoles)
            {
                writer.Write((byte)role.RoleId);
                writer.Write(role.RolePlayer.PlayerId);
            }

            writer.EndMessage();
        }

        public void AssignRole(RoleId id, PlayerControl player)
        {
            Player moddedPlayer = player.GetModdedPlayer();
            if (moddedPlayer == null) return;
            switch (id)
            {
                case RoleId.Engineer:
                    moddedPlayer.Role = new Engineer(moddedPlayer);
                    break;
                case RoleId.Joker:
                    moddedPlayer.Role = new Joker(moddedPlayer);
                    break;
                case RoleId.Medic:
                    moddedPlayer.Role = new Medic(moddedPlayer);
                    break;
                case RoleId.Officer:
                    moddedPlayer.Role = new Officer(moddedPlayer);
                    break;
            }
        }
    }
}