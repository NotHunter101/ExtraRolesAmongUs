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
            var matches = killedPlayers.Where(x => x.PlayerId == CAKODNGLPDF.PlayerId).ToArray();
            DeadPlayer killer = null;

            if (matches.Length > 0)
                killer = matches[0];

            if (killer == null)
                return;
            var isMedicAlive = __instance.isPlayerRole("Medic");
            var areReportsEnabled = Main.Config.showReport;

            if (!isMedicAlive || !areReportsEnabled)
                return;
            
            var isUserMedic = PlayerControl.LocalPlayer.isPlayerRole("Medic");
            if (!isUserMedic)
                return;

            var br = new BodyReport
            {
                Killer = PlayerTools.getPlayerById(killer.KillerId),
                Reporter = __instance,
                KillAge = (float) (DateTime.UtcNow - killer.KillTime).TotalMilliseconds,
                DeathReason = killer.DeathReason
            };

            var reportMsg = BodyReport.ParseBodyReport(br);

            if (string.IsNullOrWhiteSpace(reportMsg))
                return;

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