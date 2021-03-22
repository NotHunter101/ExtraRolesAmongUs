using HarmonyLib;

namespace ExtraRolesMod.Roles.Engineer
{
    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.Method_0))]
    class SabotageButtonDeactivatePatch
    {
        static bool Prefix(MapRoom __instance)
        {
            return !PlayerControl.LocalPlayer.IsPlayerRole(Role.Engineer);
        }
    }
}
