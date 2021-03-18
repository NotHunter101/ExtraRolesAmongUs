using ExtraRoles.Medic;
using ExtraRoles.Officer;
using ExtraRoles.Rpc;
using ExtraRolesMod;
using HarmonyLib;
using Hazel;
using Reactor;
using System;
using UnityEngine;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRoles.Roles.Engineer
{

    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageOxygen))]
    class SabotageOxyPatch
    {
        static bool Prefix(MapRoom __instance)
        {
            if (!PlayerControl.LocalPlayer.isPlayerRole(Role.Engineer))
                return true;

            if (!PlayerTools.canEngineerUseAbility())
                return false;

            PlayerControl.LocalPlayer.getModdedControl().UsedAbility = true;
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
            ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);

            return false;
        }
    }

}
