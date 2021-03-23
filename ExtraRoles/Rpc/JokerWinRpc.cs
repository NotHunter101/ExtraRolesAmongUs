using ExtraRolesMod.Roles;
using ExtraRolesMod;
using Hazel;
using Reactor;


namespace ExtraRolesMod.Rpc
{
    [RegisterCustomRpc]
    public class JokerWinRpc : PlayerCustomRpc<ExtraRolesPlugin, bool>
    {
        public JokerWinRpc(ExtraRolesPlugin plugin) : base(plugin)
        {

        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

        public override void Handle(PlayerControl innerNetObject, bool data)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.HasRole(Role.Joker))
                {
                    player.Revive();
                    player.Data.IsDead = false;
                    player.Data.IsImpostor = true;
                }
                else
                {

                    player.RemoveInfected();
                    player.Die(DeathReason.Exile);
                    player.Data.IsDead = true;
                    player.Data.IsImpostor = false;
                }
            }
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