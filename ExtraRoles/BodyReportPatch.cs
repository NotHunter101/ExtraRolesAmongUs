using ExtraRolesMod;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRoles
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.LocalPlayer.CmdReportDeadBody))]
    class BodyReportPatch
    {
        static void Postfix(PlayerControl __instance, GameData.PlayerInfo CAKODNGLPDF)
        {
            System.Console.WriteLine("Report Body!");
            byte reporterId = __instance.PlayerId;
            DeadPlayer killer = killedPlayers.Where(x => x.PlayerId == CAKODNGLPDF.PlayerId).FirstOrDefault();
            if (killer != null)
            {
                // If there is a Medic alive and Medic reported and reports are enabled
                if (MedicSettings.Medic != null && reporterId == MedicSettings.Medic.PlayerId && MedicSettings.showReport)
                {
                    // If the user is the medic
                    if (MedicSettings.Medic.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        // Create Body Report
                        BodyReport br = new BodyReport();
                        br.Killer = PlayerTools.getPlayerById(killer.KillerId);
                        br.Reporter = br.Killer = PlayerTools.getPlayerById(killer.KillerId);
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
