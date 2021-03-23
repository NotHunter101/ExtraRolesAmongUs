using ExtraRolesMod.Rpc;
using HarmonyLib;
using Reactor;

namespace ExtraRolesMod.Roles.Engineer
{

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageLights))]
    class SabotageLightsPatch
    {
        static bool Prefix(MapRoom __instance)
        {
            if (!PlayerControl.LocalPlayer.HasRole(Role.Engineer))
                return true;
            if (!PlayerTools.CanEngineerUseAbility())
                return false;

            PlayerControl.LocalPlayer.GetModdedControl().UsedAbility = true;
            Rpc<FixLightsRpc>.Instance.Send(data: true, immediately: true);

            return false;
        }
    }

}
