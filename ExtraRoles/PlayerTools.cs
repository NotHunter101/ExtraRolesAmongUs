using ExtraRolesMod.Roles;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExtraRolesMod
{
    [HarmonyPatch]
    public static class PlayerTools
    {
        public static List<TaskTypes> sabotageTasks = new List<TaskTypes>
        {
            TaskTypes.FixComms,
            TaskTypes.FixLights,
            TaskTypes.ResetReactor,
            TaskTypes.ResetSeismic,
            TaskTypes.RestoreOxy
        };

        public static PlayerControl GetPlayerById(byte id)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == id)
                {
                    return player;
                }
            }

            return null;
        }
        public static bool CanEngineerUseAbility()
        {
            if (PlayerControl.LocalPlayer.GetModdedControl().UsedAbility)
            {
                return false;
            }

            if (ExtraRoles.Logic.CurrentSabotage == null)
            {
                return false;
            }

            if (PlayerControl.LocalPlayer.Data.IsDead)
            {
                return false;
            }

            return true;
        }

        public static PlayerControl FindClosestPlayer(this PlayerControl player)
        {
            PlayerControl result = null;
            float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            if (!ShipStatus.Instance)
            {
                return null;
            }
            Vector2 truePosition = player.GetTruePosition();
            List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers.ToArray().ToList();
            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = allPlayers[i];
                if (!playerInfo.Disconnected && playerInfo.PlayerId != player.PlayerId && !playerInfo.IsDead)
                {
                    PlayerControl @object = playerInfo.Object;
                    if (@object)
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                        {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            return result;
        }
    }
}