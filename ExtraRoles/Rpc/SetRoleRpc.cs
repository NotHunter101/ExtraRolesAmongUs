using ExtraRolesMod.Roles;
using ExtraRolesMod;
using Hazel;
using Reactor;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod.Rpc
{
    [RegisterCustomRpc]
    public class SetRoleRpc : PlayerCustomRpc<HarmonyMain, SetRoleRpc.SetRoleData>
    {
        public SetRoleRpc(HarmonyMain plugin) : base(plugin)
        {

        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

        public override void Handle(PlayerControl innerNetObject, SetRoleData role)
        {
            var player = PlayerTools.getPlayerById(role.Player);
            player.getModdedControl().Role = role.Role;
            if (!player.AmOwner)
                return;

            switch (role.Role)
            {
                case Role.Officer:
                    Roles.Officer.HudUpdatePatch.OfficerKillButton.Visible = true;
                    break;
                case Role.Engineer:
                    Roles.Engineer.HudUpdatePatch.EngineerButton.Visible = true;
                    break;
                case Role.Medic:
                    Roles.Medic.HudUpdatePatch.MedicShieldButton.Visible = true;
                    break;
            }
        }

        public override SetRoleData Read(MessageReader reader)
        {
            return new SetRoleData(player: reader.ReadByte(), role: (Role)reader.ReadByte());
        }

        public override void Write(MessageWriter writer, SetRoleData data)
        {
            writer.Write(data.Player);
            writer.Write((byte)data.Role);
        }

        public struct SetRoleData
        {
            public SetRoleData(byte player, Role role)
            {
                Player = player;
                Role = role;
            }

            public byte Player { get; }
            public Role Role { get; }

            public static implicit operator SetRoleData((byte PlayerId, Role Role) data) => new SetRoleData(data.PlayerId, data.Role);
        }
    }

}
