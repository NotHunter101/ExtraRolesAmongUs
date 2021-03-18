using ExtraRolesMod;
using Hazel;
using Reactor;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod.Rpc
{
    [RegisterCustomRpc]
    public class AttemptKillShieldedPlayerRpc : PlayerCustomRpc<HarmonyMain, byte>
    {
        public AttemptKillShieldedPlayerRpc(HarmonyMain plugin) : base(plugin)
        {

        }

        // Do not handle this locally
        public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;

        public override void Handle(PlayerControl innerNetObject, byte data)
        {
            BreakShield(false);
        }

        public override byte Read(MessageReader reader)
        {
            return reader.ReadByte();
        }

        public override void Write(MessageWriter writer, byte data)
        {
            writer.Write(data);
        }
    }
}
