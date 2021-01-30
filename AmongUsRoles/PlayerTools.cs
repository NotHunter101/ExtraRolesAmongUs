using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnityEngine;

namespace ExtraRolesMod
{
    enum CustomRPC
    {
        SetMedic = 43,
        SetProtected = 44,
        MedicDead = 45,
        SetOfficer = 46,
        OfficerKill = 47,
        SetEngineer = 48,
        RepairAllEmergencies = 49,
        SetJoker = 50,
        ResetVaribles = 51,
        MedicReport = 52,
        JokerWin = 53,
        SendMeAMessage = 54
    }
    enum RPC
    {
        PlayAnimation = 0,
        CompleteTask = 1,
        SyncSettings = 2,
        SetInfected = 3,
        Exiled = 4,
        CheckName = 5,
        SetName = 6,
        CheckColor = 7,
        SetColor = 8,
        SetHat = 9,
        SetSkin = 10,
        ReportDeadBody = 11,
        MurderPlayer = 12,
        SendChat = 13,
        StartMeeting = 14,
        SetScanner = 15,
        SendChatNote = 16,
        SetPet = 17,
        SetStartCounter = 18,
        EnterVent = 19,
        ExitVent = 20,
        SnapTo = 21,
        Close = 22,
        VotingComplete = 23,
        CastVote = 24,
        ClearVote = 25,
        AddVote = 26,
        CloseDoorsOfType = 27,
        RepairSystem = 28,
        SetTasks = 29,
        UpdateGameData = 30,
    }

    [HarmonyPatch]
    public static class PlayerTools
    {
        public static FFGALNAPKCD closestPlayer = null;
        
        public static List<FFGALNAPKCD> getCrewMates(Il2CppReferenceArray<EGLJNOMOGNP.DCJMABDDJCF> infection)
        {
            List<FFGALNAPKCD> CrewmateIds = new List<FFGALNAPKCD>();
            foreach (FFGALNAPKCD player in FFGALNAPKCD.AllPlayerControls)
            {
                bool isInfected = false;
                foreach (EGLJNOMOGNP.DCJMABDDJCF infected in infection)
                {
                    if (player == infected.LAOEJKHLKAI)
                    {
                        isInfected = true;
                        break;
                    }
                }
                if (!isInfected)
                {
                    CrewmateIds.Add(player);
                }
            }
            return CrewmateIds;
        }

        public static FFGALNAPKCD getPlayerById(byte id)
        {
            foreach (FFGALNAPKCD player in FFGALNAPKCD.AllPlayerControls)
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
            if (MainHooks.OfficerSettings.lastKilled == null)
                return 0;
            DateTime now = DateTime.UtcNow;
            TimeSpan diff = (TimeSpan)(now - MainHooks.OfficerSettings.lastKilled);

            var KillCoolDown = MainHooks.OfficerSettings.OfficerCD * 1000.0f;
            if (KillCoolDown - (float)diff.TotalMilliseconds < 0) return 0;
            else
            {
                return (KillCoolDown - (float)diff.TotalMilliseconds) / 1000.0f;
            }
        }
        public static FFGALNAPKCD getClosestPlayer(FFGALNAPKCD refplayer)
        {
            double mindist = double.MaxValue;
            FFGALNAPKCD closestplayer = null;
            foreach (FFGALNAPKCD player in FFGALNAPKCD.AllPlayerControls)
            {
                if (player.NDGFFHMFGIG.DLPCKPBIJOE) continue;
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

        public static double getDistBetweenPlayers(FFGALNAPKCD player, FFGALNAPKCD refplayer)
        {
            var refpos = refplayer.GetTruePosition();
            var playerpos = player.GetTruePosition();

            return Math.Sqrt((refpos[0] - playerpos[0]) * (refpos[0] - playerpos[0]) + (refpos[1] - playerpos[1]) * (refpos[1] - playerpos[1]));
        }
    }
}
