using ExtraRoles2.Classes;
using HarmonyLib;

namespace ExtraRoles2.Patches
{
    [HarmonyPatch(typeof(KillButtonManager), nameof(KillButtonManager.PerformKill))]
    public class PressButtonPatch
    {
        static bool Prefix(KillButtonManager __instance)
        {
            if (__instance != HudManager.Instance.KillButton)
            {
                PlayerControl.LocalPlayer.GetModdedPlayer()?.Role?.PerformKill(__instance);
                return false;
            }

            return true;
        }
    }
}