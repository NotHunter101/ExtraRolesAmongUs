using Hazel;

namespace ExtraRoles2.Classes
{
    public class RpcHelper
    {
        public static RpcHelper Instance { get; set; }
        
        public MessageWriter GeneratePacket(CustomRPC packetId)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, RolesPlugin.DataPacket, SendOption.Reliable);
            writer.Write((byte)packetId);

            return writer;
        }

        public void HandlePacket(MessageReader reader)
        {
            CustomRPC packet = (CustomRPC)reader.ReadByte();

            switch (packet)
            {
                case CustomRPC.SetRoles:
                    var roleCount = reader.ReadInt32();

                    for (var i = 0; i < roleCount; i++)
                    {
                        RoleId roleId = (RoleId)reader.ReadByte();
                        byte playerId = reader.ReadByte();

                        RoleHelper.Instance.AssignRole(roleId, GameData.Instance.GetPlayerById(playerId).Object);
                    }
                    break;
                case CustomRPC.FixLights:
                    SwitchSystem switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                    switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
                    break;
                case CustomRPC.SetProtected:
                    byte protectedId = reader.ReadByte();
                    GameData.Instance.GetPlayerById(protectedId).Object.GetModdedPlayer().Immortal = true;
                    break;
            }
        }
    }
}