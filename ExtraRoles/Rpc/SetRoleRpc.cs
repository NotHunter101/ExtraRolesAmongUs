using ExtraRoles.Roles;
using ExtraRolesMod;
using Hazel;
using Reactor;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRoles.Rpc
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
            PlayerTools.getPlayerById(role.Player).getModdedControl().Role = role.Role;
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
