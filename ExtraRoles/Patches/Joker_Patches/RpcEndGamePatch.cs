using System;
using Assets.CoreScripts;
using ExtraRoles2.Classes;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;

namespace ExtraRoles2.Patches.Joker_Patches
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public class RpcEndGamePatch
    {
        static bool Prefix(AmongUsClient __instance, GameOverReason __0, bool __1)
        {
            StatsManager.Instance.BanPoints -= 1.5f;
            StatsManager.Instance.LastGameStarted = Il2CppSystem.DateTime.MinValue;
            __instance.DisconnectHandlers.Clear();
            if (Minigame.Instance)
            {
                try
                {
                    Minigame.Instance.Close();
                    Minigame.Instance.Close();
                }
                catch
                {
                }
            }
            try
            {
                DestroyableSingleton<Telemetry>.Instance.EndGame(__0);
            }
            catch
            {
            }
            TempData.EndReason = __0;
            TempData.showAd = __1;
            bool flag = TempData.DidHumansWin(__0);
            TempData.winners = new List<WinningPlayerData>();
            for (int i = 0; i < GameData.Instance.PlayerCount; i++)
            {
                GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                if (playerInfo != null && (__0 == GameOverReason.HumansDisconnect || __0 == GameOverReason.ImpostorDisconnect || flag != playerInfo.IsImpostor))
                {
                    TempData.winners.Add(new WinningPlayerData(playerInfo));
                }
            }
            
            if (__0 == GameOverReason.HumansDisconnect)
            {
                Player joker = Main.Instance.Players.Find(x => x.Role?.Id == RoleId.Joker);
                joker.Owner.Revive();
                TempData.winners.Clear();
                TempData.winners.Add(new WinningPlayerData(joker.Owner.Data));
                TempData.EndReason = GameOverReason.ImpostorByVote;
            }
            
            __instance.StartCoroutine(__instance.CoEndGame());
            return false;
        }
    }
}