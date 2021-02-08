using ExtraRolesMod;
using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Text;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRoles
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.HandleProceed))]
    class MeetingPatch
    {
        static void Postfix(MeetingHud __instance)
        {
            if (JokerSettings.Joker != null)
            {
                if (__instance.exiledPlayer.PlayerId == JokerSettings.Joker.PlayerId)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.JokerWin, Hazel.SendOption.None, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);

                    localPlayers.Clear();
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        if (player != JokerSettings.Joker)
                        {
                            player.RemoveInfected();
                            player.MurderPlayer(player);
                            player.Data.IsDead = true;
                            player.Data.IsImpostor = false;
                        }
                        else
                        {
                            localPlayers.Add(player);
                            player.Revive();
                            player.Data.IsDead = false;
                            player.Data.IsImpostor = true;
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    class MeetingEnd
    {
        static void Postfix(ExileController __instance)
        {
            System.Console.WriteLine("Meeting Ended!");
            OfficerSettings.lastKilled = DateTime.UtcNow.AddMilliseconds(__instance.Duration);
        }
    }
}
