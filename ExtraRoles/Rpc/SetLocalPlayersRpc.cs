using ExtraRolesMod;
using Hazel;
using Reactor;
using UnhollowerBaseLib;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRoles.Rpc
{
    [RegisterCustomRpc]
    public class SetLocalPlayersRpc : PlayerCustomRpc<HarmonyMain, byte[]>
    {
        public SetLocalPlayersRpc(HarmonyMain plugin) : base(plugin)
        {

        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

        public override void Handle(PlayerControl innerNetObject, byte[] localPlayerBytes)
        {
            localPlayers.Clear();
            localPlayer = PlayerControl.LocalPlayer;
            foreach (var id in localPlayerBytes)
                foreach (var player in PlayerControl.AllPlayerControls)
                    if (player.PlayerId == id)
                        localPlayers.Add(player);
        }

        public override byte[] Read(MessageReader reader)
        {
            return reader.ReadBytesAndSize();
        }

        public override void Write(MessageWriter writer, byte[] data)
        {
            writer.WriteBytesAndSize(data);
        }
    }
}
