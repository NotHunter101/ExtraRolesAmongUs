using ExtraRoles.Medic;
using ExtraRolesMod;
using Hazel;
using Reactor;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRoles.Rpc
{

    [RegisterCustomRpc]
    public class GiveShieldRpc : PlayerCustomRpc<HarmonyMain, byte>
    {
        public GiveShieldRpc(HarmonyMain plugin) : base(plugin)
        {

        }

        // Do not handle this locally
        public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

        public override void Handle(PlayerControl innerNetObject, byte protectedId)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == protectedId)
                    player.getModdedControl().Immortal = ShieldState.Intact;
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
