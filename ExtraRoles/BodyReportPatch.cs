using ExtraRolesMod;
using HarmonyLib;
using System;
using System.Linq;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRoles
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.LocalPlayer.CmdReportDeadBody))]
    class BodyReportPatch
    {
        static void Postfix(PlayerControl __instance, GameData.PlayerInfo CAKODNGLPDF)
        {
            System.Console.WriteLine("Report Body!");
            var reporterId = __instance.PlayerId;
            var killer = killedPlayers.FirstOrDefault(x => x.PlayerId == CAKODNGLPDF.PlayerId);
            if (killer == null)
                return;

            var hasMedicReported = MedicSettings.Medic != null && reporterId == MedicSettings.Medic.PlayerId;
            if (!hasMedicReported || !MedicSettings.showReport)
                return;

            var isUserMedic = MedicSettings.Medic.PlayerId == PlayerControl.LocalPlayer.PlayerId;
            if (!isUserMedic)
                return;

            // Create Body Report
            var bodyReport = new BodyReport();
            bodyReport.Killer = PlayerTools.getPlayerById(killer.KillerId);
            bodyReport.Reporter = bodyReport.Killer = PlayerTools.getPlayerById(killer.KillerId);
            bodyReport.KillAge = (float) (DateTime.UtcNow - killer.KillTime).TotalMilliseconds;
            bodyReport.DeathReason = killer.DeathReason;
            // Generate message
            var reportMsg = BodyReport.ParseBodyReport(bodyReport);

            // If message is not empty
            var isMessageEmpty = string.IsNullOrWhiteSpace(reportMsg);
            if (isMessageEmpty)
                return;

            if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
            {
                // Send the message through chat only visible to the medic
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer,
                    reportMsg);
            }

            if (reportMsg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // Really did not understand this
                DestroyableSingleton<Telemetry>.Instance.SendWho();
            }
        }
    }
}