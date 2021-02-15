using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod
{
    [HarmonyPatch(typeof(EndGameManager), "SetEverythingUp")]
    public static class EndGamePatch
    {
        public static bool Prefix()
        {
            if (TempData.winners.Count <= 1 || !TempData.DidHumansWin(TempData.EndReason))
                return true;

            TempData.winners.Clear();
            
            var orderLocalPlayers = localPlayers.Where(player => player.PlayerId == localPlayer.PlayerId).ToList();
            orderLocalPlayers.AddRange(localPlayers.Where(player => player.PlayerId != localPlayer.PlayerId));
            
            foreach (var winner in orderLocalPlayers)
            {
                TempData.winners.Add(new WinningPlayerData(winner.Data));
            }

            return true;
        }

        public static void Postfix(EndGameManager __instance)
        {
            if (!TempData.DidHumansWin(TempData.EndReason))
                return;

            var flag = localPlayers.Count(player => player.PlayerId == localPlayer.PlayerId) == 0;
           
            if (!flag)
                return;
            
            __instance.WinText.Text = "Defeat";
            __instance.WinText.Color = Palette.ImpostorRed;
            __instance.BackgroundBar.material.color = new Color(1, 0, 0);
        }
    }
}