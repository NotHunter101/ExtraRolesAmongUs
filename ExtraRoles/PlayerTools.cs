using ExtraRoles.Roles;
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

        public static PlayerControl getPlayerById(byte id)
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

        /// <summary>
        /// Returns the cooldown of the officer in seconds. Zero means the officer can kill again.
        /// </summary>
        public static float getOfficerCD()
        {
            var lastAbilityTime = ExtraRoles.Main.Logic.getRolePlayer(Role.Officer).LastAbilityTime;
            if (lastAbilityTime == null)
            {
                return ExtraRoles.Main.Config.OfficerCD;
            }

            var now = DateTime.UtcNow;
            var diff = (TimeSpan) (now - lastAbilityTime);

            var killCooldown = ExtraRoles.Main.Config.OfficerCD * 1000.0f;
            if (killCooldown - (float) diff.TotalMilliseconds < 0)
                return 0;

            return (killCooldown - (float) diff.TotalMilliseconds) / 1000.0f;
        }

        public static bool canEngineerUseAbility()
        {
            if (PlayerControl.LocalPlayer.getModdedControl().UsedAbility)
            {
                return false;
            }

            if (!ExtraRoles.Main.Logic.sabotageActive)
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
            if (!ShipStatus.Instance)
            {
                return null;
            }

            PlayerControl result = null;
            float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            Vector2 truePosition = player.GetTruePosition();
            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                PlayerControl control = playerInfo.Object;
                if (!layerInfo.Disconnected && playerInfo.PlayerId != player.PlayerId && !playerInfo.IsDead && !control.inVent) // Add InVent Check
                {
                    Vector2 vector = control.GetTruePosition() - truePosition;
                    float magnitude = vector.magnitude;
                    if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                    {
                        result = control;
                        num = magnitude;
                    }
                }
            }
            return result;
        }
    }
}
