using ExtraRolesMod;
using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ExtraRolesMod.ExtraRoles;
using UnhollowerBaseLib;
using System.Collections;

namespace ExtraRoles
{

    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), new Type[] { typeof(UnityEngine.Object) })]
    class MeetingExiledEnd
    {
        static void Prefix(UnityEngine.Object obj)
        {
            if (ExileController.Instance != null && obj == ExileController.Instance.gameObject)
            {
                if (JokerSettings.Joker != null)
                {
                    if (ExileController.Instance.exiled != null && ExileController.Instance.exiled.PlayerId == JokerSettings.Joker.PlayerId)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.JokerWin, Hazel.SendOption.None, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);

                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            if (player != JokerSettings.Joker)
                            {
                                player.RemoveInfected();
                                player.Die(DeathReason.Exile);
                                player.Data.IsDead = true;
                                player.Data.IsImpostor = false;
                            }
                        }
                        JokerSettings.Joker.Revive();
                        JokerSettings.Joker.Data.IsDead = false;
                        JokerSettings.Joker.Data.IsImpostor = true;
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
            OfficerSettings.lastKilled = DateTime.UtcNow.AddMilliseconds(__instance.Duration);
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    class TranslationPatch
    {
        static void Postfix(ref string __result, StringNames __0)
        {
            if (ExileController.Instance != null && ExileController.Instance.exiled != null)
            {
                if (__0 == StringNames.ExileTextPN || __0 == StringNames.ExileTextSN)
                {
                    if (MedicSettings.Medic != null && ExileController.Instance.exiled.Object.PlayerId == MedicSettings.Medic.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Medic.";
                    else if (EngineerSettings.Engineer != null && ExileController.Instance.exiled.Object.PlayerId == EngineerSettings.Engineer.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Engineer.";
                    else if (OfficerSettings.Officer != null && ExileController.Instance.exiled.Object.PlayerId == OfficerSettings.Officer.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Officer.";
                    else if (JokerSettings.Joker != null && ExileController.Instance.exiled.Object.PlayerId == JokerSettings.Joker.PlayerId)
                        __result = ExileController.Instance.exiled.PlayerName + " was The Joker.";
                    else
                        __result = ExileController.Instance.exiled.PlayerName + " was not The Impostor.";
                }
                if (__0 == StringNames.ImpostorsRemainP || __0 == StringNames.ImpostorsRemainS)
                {
                    if (JokerSettings.Joker != null && ExileController.Instance.exiled.Object.PlayerId == JokerSettings.Joker.PlayerId)
                        __result = "";
                }
            }
        }
    }
}
