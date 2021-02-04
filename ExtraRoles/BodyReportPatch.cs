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

            if (MedicSettings.Medic != null && reporterId == MedicSettings.Medic.PlayerId)
            {
                if (MedicSettings.Medic.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                {
                    BodyReport br = new BodyReport();
                    br.Killer = PlayerTools.getPlayerById(killer.KillerId);
                    br.Reporter = br.Killer = PlayerTools.getPlayerById(killer.KillerId);
                    br.KillAge = (float)(DateTime.UtcNow - killer.KillTime).TotalMilliseconds;
                    br.DeathReason = killer.DeathReason;
                    var reportMsg = BodyReport.ParseBodyReport(br);

                    if (!string.IsNullOrWhiteSpace(reportMsg))
                    {
                        if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
                        {
                            DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, reportMsg);
                        }
                        if (reportMsg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            DestroyableSingleton<Telemetry>.Instance.SendWho();
                        }
                    }
                }
            }
        }
    }
}
