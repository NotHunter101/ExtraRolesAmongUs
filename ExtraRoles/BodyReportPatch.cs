using ExtraRolesMod;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRoles
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
    class BodyReportPatch
    {
        static void Postfix(PlayerControl __instance, GameData.PlayerInfo CAKODNGLPDF)
        {
            System.Console.WriteLine("Report Body!");
            DeadPlayer[] matches = killedPlayers.Where(x => x.PlayerId == CAKODNGLPDF.PlayerId).ToArray();
            DeadPlayer killer = null;

            if (matches.Length > 0)
                killer = matches[0];

            if (killer != null)
            {
                // If there is a Medic alive and Medic reported and reports are enabled
                if (__instance.isPlayerRole("Medic") && Main.Config.showReport)
                {
                    // If the user is the medic
                    if (PlayerControl.LocalPlayer.isPlayerRole("Medic"))
                    {
                        // Create Body Report
                        BodyReport br = new BodyReport();
                        br.Killer = PlayerTools.getPlayerById(killer.KillerId);
                        br.Reporter = __instance;
                        br.KillAge = (float)(DateTime.UtcNow - killer.KillTime).TotalMilliseconds;
                        br.DeathReason = killer.DeathReason;
                        // Generate message
                        var reportMsg = BodyReport.ParseBodyReport(br);

                        // If message is not empty
                        if (!string.IsNullOrWhiteSpace(reportMsg))
                        {   
                            
                            if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
                            {
                                // Send the message through chat only visible to the medic
                                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, reportMsg);
                            }
                            if (reportMsg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                // Really did not understand this
                                DestroyableSingleton<Telemetry>.Instance.SendWho();
                            }
                        }
                    }
                }
            }       
        }
    }
}
