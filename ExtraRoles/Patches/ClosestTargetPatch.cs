using System.Collections.Generic;
using System.Linq;
using ExtraRoles2.Classes;
using HarmonyLib;
using UnityEngine;

namespace ExtraRoles2.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FindClosestTarget))]
    public class ClosestTargetPatch
    {
        static bool Prefix(PlayerControl __instance, ref PlayerControl __result)
        {
            float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            if (!ShipStatus.Instance) return true;
            Vector2 truePosition = __instance.GetTruePosition();
            List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers.ToArray().ToList();
            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = allPlayers[i];
                if (!playerInfo.Disconnected && playerInfo.PlayerId != __instance.PlayerId && !playerInfo.IsDead && (!playerInfo.IsImpostor || !__instance.Data.IsImpostor))
                {
                    PlayerControl @object = playerInfo.Object;
                    if (@object)
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                        {
                            __result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            
            return false;
        }
    }
}