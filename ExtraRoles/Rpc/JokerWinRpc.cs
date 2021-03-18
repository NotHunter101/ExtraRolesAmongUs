using ExtraRolesMod.Roles;
using ExtraRolesMod;
using Hazel;
using Reactor;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod.Rpc
{
    [RegisterCustomRpc]
    public class JokerWinRpc : PlayerCustomRpc<HarmonyMain, bool>
    {
        public JokerWinRpc(HarmonyMain plugin) : base(plugin)
        {

        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

        public override void Handle(PlayerControl innerNetObject, bool data)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.isPlayerRole(Role.Joker))
                    continue;

                player.RemoveInfected();
                player.Die(DeathReason.Exile);
                player.Data.IsDead = true;
                player.Data.IsImpostor = false;
            }

            var joker = Main.Logic.getRolePlayer(Role.Joker).PlayerControl;
            joker.Revive();
            joker.Data.IsDead = false;
            joker.Data.IsImpostor = true;
        }

        public override bool Read(MessageReader reader)
        {
            return reader.ReadBoolean();
        }

        public override void Write(MessageWriter writer, bool data)
        {
            writer.Write(data);
        }
    }
}