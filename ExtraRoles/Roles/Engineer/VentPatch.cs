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
			PlayerControl @object = pc.Object;
			couldUse = ((pc.IsImpostor || @object.isPlayerRole(Role.Engineer)) && !pc.IsDead && (@object.CanMove || @object.inVent));
			canUse = couldUse;
			if (canUse)
			{
				Vector2 truePosition = @object.GetTruePosition();
				Vector3 position = __instance.transform.position;
				num = Vector2.Distance(truePosition, position);
				canUse &= (num <= __instance.UsableDistance && !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShipOnlyMask, false));
			}
			__result = num;
			return false;
		}
    }
}
