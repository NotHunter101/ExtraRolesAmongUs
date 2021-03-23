using HarmonyLib;

namespace ExtraRolesMod.Roles.Engineer
{
    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageReactor))]
    class SabotageReactorPatch
    {
        static bool Prefix(MapRoom __instance)
        {
            if (!PlayerControl.LocalPlayer.HasRole(Role.Engineer))
                return true;

            if (!PlayerTools.CanEngineerUseAbility())
                return false;

            PlayerControl.LocalPlayer.GetModdedControl().UsedAbility = true;
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 16);

            return false;
        }
    }
}
