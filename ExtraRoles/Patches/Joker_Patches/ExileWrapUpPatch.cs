using ExtraRoles2.Classes;
using HarmonyLib;

namespace ExtraRoles2.Patches.Joker_Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public class ExileWrapUp
    {
        static void Postfix(PlayerControl __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;

            if (__instance.GetModdedPlayer().Role?.Id == RoleId.Joker)
                ShipStatus.RpcEndGame(GameOverReason.HumansDisconnect, false);
        }
    }
}