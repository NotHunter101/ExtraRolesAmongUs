using ExtraRolesMod.Medic;
using ExtraRolesMod.Officer;
using ExtraRolesMod.Rpc;
using ExtraRolesMod;
using HarmonyLib;
using Hazel;
using Reactor;
using System;
using UnityEngine;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod.Roles.Engineer
{

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageLights))]
    class SabotageLightsPatch
    {
        static bool Prefix(MapRoom __instance)
        {
            if (!PlayerControl.LocalPlayer.isPlayerRole(Role.Engineer))
                return true;
            if (!PlayerTools.canEngineerUseAbility())
                return false;

            PlayerControl.LocalPlayer.getModdedControl().UsedAbility = true;
            Rpc<FixLightsRpc>.Instance.Send(data: true, immediately: true);

            return false;
        }
    }

}
