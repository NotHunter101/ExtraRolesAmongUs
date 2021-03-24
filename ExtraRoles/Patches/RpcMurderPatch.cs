using ExtraRoles2.Classes;
using HarmonyLib;

namespace ExtraRoles2.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcMurderPlayer))]
    public class RpcMurderPatch
    {
        static bool Prefix(PlayerControl __0)
        {
            if (__0.GetModdedPlayer().Immortal) return false;
            return true;
        }
    }
}