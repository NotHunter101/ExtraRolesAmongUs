using ExtraRoles.Roles;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod
{

    [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
    public static class VentPatch
    {
        public static bool Prefix(Vent __instance, ref float __result, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            float num = float.MaxValue;
            PlayerControl localPlayer = pc.Object;
            couldUse = !localPlayer.Data.IsDead;
            couldUse &= localPlayer.Data.IsImpostor || localPlayer.isPlayerRole(Role.Engineer);
            canUse = couldUse;
            num = Vector2.Distance(localPlayer.GetTruePosition(), __instance.transform.position);
            canUse &= num <= __instance.UsableDistance;
            __result = num;
            return false;
        }
    }
}
