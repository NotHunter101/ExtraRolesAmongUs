using ExtraRolesMod.Roles;
using ExtraRolesMod.Roles.Medic;
using ExtraRolesMod;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRoles
{

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.LocalPlayer.CmdReportDeadBody))]
    class BodyReportPatch
    {
        static void Postfix(PlayerControl __instance, GameData.PlayerInfo __0)
        {
            System.Console.WriteLine("Report Body!");
            byte reporterId = __instance.PlayerId;
            DeadPlayer killer = killedPlayers.Where(x => x.PlayerId == __0.PlayerId).FirstOrDefault();
            if (killer != null)
            {
                // If there is a Medic alive and Medic reported and reports are enabled
                if (PlayerControl.LocalPlayer.isPlayerRole(Role.Medic) && Main.Config.showReport)
                {
                    // If the user is the medic
                    if (PlayerControl.LocalPlayer.isPlayerRole(Role.Medic))
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
                        }
                    }
                }
            }
        }
    }
}
