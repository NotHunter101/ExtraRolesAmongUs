using HarmonyLib;

namespace ExtraRolesMod.Roles.Engineer
{

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageSeismic))]
    class SabotageSeismicPatch
    {
        static bool Prefix(MapRoom __instance)
        {
            if (!PlayerControl.LocalPlayer.HasRole(Role.Engineer))
                return true;

            if (!PlayerTools.CanEngineerUseAbility())
                return false;

            PlayerControl.LocalPlayer.GetModdedControl().UsedAbility = true;
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Laboratory, 16);

            return false;
        }
    }
}
