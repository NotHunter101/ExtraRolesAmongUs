using HarmonyLib;
using System;
using System.Collections.Generic;

namespace ExtraRolesMod
{
    [HarmonyPatch]
    public static class PlayerTools
    {
        public static PlayerControl closestPlayer = null;
        
        public static List<PlayerControl> getCrewMates()
        {
            List<PlayerControl> CrewmateIds = new List<PlayerControl>();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                bool isInfected = false;
                if (player.Data.IsImpostor)
                {
                    isInfected = true;
                    break;
                }
                if (!isInfected)
                {
                    CrewmateIds.Add(player);
                }
            }
            return CrewmateIds;
        }

        public static PlayerControl getPlayerById(byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == id)
                {
                    return player;
                }
            }
            return null;
        }

        public static float GetOfficerKD()
        {
            if (ExtraRoles.OfficerSettings.lastKilled == null)
            {
                return ExtraRoles.OfficerSettings.OfficerCD;
            }
            DateTime now = DateTime.UtcNow;
            TimeSpan diff = (TimeSpan)(now - ExtraRoles.OfficerSettings.lastKilled);

            var KillCoolDown = ExtraRoles.OfficerSettings.OfficerCD * 1000.0f;
            if (KillCoolDown - (float)diff.TotalMilliseconds < 0) return 0;
            else
            {
                return (KillCoolDown - (float)diff.TotalMilliseconds) / 1000.0f;
            }
        }

        public static PlayerControl getClosestPlayer(PlayerControl refplayer)
        {
            double mindist = double.MaxValue;
            PlayerControl closestplayer = null;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsDead) continue;
                if (player != refplayer)
                {

                    double dist = getDistBetweenPlayers(player, refplayer);
                    if (dist < mindist)
                    {
                        mindist = dist;
                        closestplayer = player;
                    }

                }

            }
            return closestplayer;
        }

        public static PlayerControl getPlayerFromId(byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == id)
                    return player;
            return null;
        }

        public static double getDistBetweenPlayers(PlayerControl player, PlayerControl refplayer)
        {
            var refpos = refplayer.GetTruePosition();
            var playerpos = player.GetTruePosition();

            return Math.Sqrt((refpos[0] - playerpos[0]) * (refpos[0] - playerpos[0]) + (refpos[1] - playerpos[1]) * (refpos[1] - playerpos[1]));
        }
    }
}