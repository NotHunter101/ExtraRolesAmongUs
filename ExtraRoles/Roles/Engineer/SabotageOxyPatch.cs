using HarmonyLib;

namespace ExtraRolesMod.Roles.Engineer
{

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageOxygen))]
    class SabotageOxyPatch
    {
        static bool Prefix(MapRoom __instance)
        {
            if (!PlayerControl.LocalPlayer.HasRole(Role.Engineer))
                return true;

            if (!PlayerTools.CanEngineerUseAbility())
                return false;

            PlayerControl.LocalPlayer.GetModdedControl().UsedAbility = true;
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);

            return false;
        }
    }

}
