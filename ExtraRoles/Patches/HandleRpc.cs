using ExtraRoles2.Classes;
using HarmonyLib;
using Hazel;

namespace ExtraRoles2.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    public class HandleRpc
    {
        static void Postfix(byte __0, MessageReader __1)
        {
            switch (__0)
            {
                case RolesPlugin.DataPacket:
                    RpcHelper.Instance.HandlePacket(__1);
                    break;
            }
        }
    }
}