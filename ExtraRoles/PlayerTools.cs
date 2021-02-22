using HarmonyLib;
using System;
using System.Collections.Generic;

namespace ExtraRolesMod
{
    [HarmonyPatch]
    public static class PlayerTools
    {
        public static PlayerControl closestPlayer = null;
        
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
            var lastAbilityTime = ExtraRoles.Main.Logic.getRolePlayer("Officer").LastAbilityTime;
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

        public static PlayerControl getClosestPlayer(PlayerControl refplayer)
        {
            var mindist = double.MaxValue;
            PlayerControl closestplayer = null;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsDead)
                    continue;
                if (player == refplayer)
                    continue;
                var dist = getDistBetweenPlayers(player, refplayer);
                if (dist >= mindist)
                    continue;

                mindist = dist;
                closestplayer = player;
            }

            return closestplayer;
        }

        public static double getDistBetweenPlayers(PlayerControl player, PlayerControl refplayer)
        {
            var refpos = refplayer.GetTruePosition();
            var playerpos = player.GetTruePosition();

            return Math.Sqrt((refpos[0] - playerpos[0]) * (refpos[0] - playerpos[0]) +
                             (refpos[1] - playerpos[1]) * (refpos[1] - playerpos[1]));
        }
    }
}