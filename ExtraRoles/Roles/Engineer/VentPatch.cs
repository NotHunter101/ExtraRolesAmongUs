using ExtraRolesMod.Roles;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace ExtraRolesMod
{

    [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
    public static class VentPatch
    {
        public static bool Prefix(Vent __instance, ref float __result, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            var vent = __instance;
            var player = pc.Object;
            float num = float.MaxValue;
            couldUse = (pc.IsImpostor || player.HasRole(Role.Engineer)) && !pc.IsDead && (player.CanMove || player.inVent);
            canUse = couldUse;
            if (canUse)
            {
                Vector2 truePosition = player.GetTruePosition();
                Vector3 position = vent.transform.position;
                num = Vector2.Distance(truePosition, position);
                canUse &= (num <= vent.UsableDistance && !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShipAndObjectsMask, false));
            }
            __result = num;
            return false;
        }
    }
}
