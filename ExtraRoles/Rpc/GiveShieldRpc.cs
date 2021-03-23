using ExtraRolesMod.Roles;
using ExtraRolesMod.Roles.Medic;
using Hazel;
using Reactor;

namespace ExtraRolesMod.Rpc
{

    [RegisterCustomRpc]
    public class GiveShieldRpc : PlayerCustomRpc<ExtraRolesPlugin, byte>
    {
        public GiveShieldRpc(ExtraRolesPlugin plugin) : base(plugin)
        {

        }

        // Do not handle this locally
        public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

        public override void Handle(PlayerControl innerNetObject, byte protectedId)
        {
            var shieldedPlayer = PlayerTools.GetPlayerById(protectedId);
            shieldedPlayer.GetModdedControl().Immortal = true;

            var showShielded = ExtraRoles.Config.showProtected;
            bool flag = showShielded == ShieldOptions.Everyone;
            flag |= showShielded == ShieldOptions.SelfAndMedic && (shieldedPlayer.AmOwner || PlayerControl.LocalPlayer.HasRole(Role.Medic));
            flag |= showShielded == ShieldOptions.Medic && shieldedPlayer.HasRole(Role.Medic);

            if (flag)
            {
                shieldedPlayer.gameObject.AddComponent<PlayerShield>();
            }
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
