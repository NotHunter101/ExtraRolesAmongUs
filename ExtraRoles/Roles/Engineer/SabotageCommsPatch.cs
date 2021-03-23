using HarmonyLib;

namespace ExtraRolesMod.Roles.Engineer
{
    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageComms))]
    class SabotageCommsPatch
    {
        static bool Prefix(MapRoom __instance)
        {
            if (!PlayerControl.LocalPlayer.HasRole(Role.Engineer))
                return true;
            if (!PlayerTools.CanEngineerUseAbility())
                return false;

            PlayerControl.LocalPlayer.GetModdedControl().UsedAbility = true;
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 1);

            return false;
        }
    }

}
