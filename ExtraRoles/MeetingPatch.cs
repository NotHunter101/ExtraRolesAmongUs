using ExtraRolesMod;
using HarmonyLib;
using Hazel;
using System;
using static ExtraRolesMod.ExtraRoles;
using UnhollowerBaseLib;

namespace ExtraRoles
{
    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy),
        new Type[] {typeof(UnityEngine.Object)})]
    class MeetingExiledEnd
    {
        static void Prefix(UnityEngine.Object obj)
        {
            if (ExileController.Instance == null || obj != ExileController.Instance.gameObject)
                return;

            var Officer = Main.Logic.getRolePlayer("Officer");
            if (Officer != null)
                Officer.LastAbilityTime = DateTime.UtcNow;
            if (ExileController.Instance.Field_10 == null ||
                !ExileController.Instance.Field_10._object.isPlayerRole("Joker"))
                return;

            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte) CustomRPC.JokerWin, Hazel.SendOption.None, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.isPlayerRole("Joker"))
                    continue;
                player.RemoveInfected();
                player.Die(DeathReason.Exile);
                player.Data.IsDead = true;
                player.Data.IsImpostor = false;
            }

            var joker = Main.Logic.getRolePlayer("Joker").PlayerControl;
            joker.Revive();
            joker.Data.IsDead = false;
            joker.Data.IsImpostor = true;
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString),
        new Type[] {typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>)})]
    class TranslationPatch
    {
        static void Postfix(ref string __result, StringNames HKOIECMDOKL,
            Il2CppReferenceArray<Il2CppSystem.Object> EBKIKEILMLF)
        {
            if (ExileController.Instance == null || ExileController.Instance.Field_10 == null)
                return;

            switch (HKOIECMDOKL)
            {
                case StringNames.ExileTextPN:
                case StringNames.ExileTextSN:
                {
                    if (ExileController.Instance.Field_10.Object.isPlayerRole("Medic"))
                        __result = ExileController.Instance.Field_10.PlayerName + " was The Medic.";
                    else if (ExileController.Instance.Field_10.Object.isPlayerRole("Engineer"))
                        __result = ExileController.Instance.Field_10.PlayerName + " was The Engineer.";
                    else if (ExileController.Instance.Field_10.Object.isPlayerRole("Officer"))
                        __result = ExileController.Instance.Field_10.PlayerName + " was The Officer.";
                    else if (ExileController.Instance.Field_10.Object.isPlayerRole("Joker"))
                        __result = ExileController.Instance.Field_10.PlayerName + " was The Joker.";
                    else
                        __result = ExileController.Instance.Field_10.PlayerName + " was not The Impostor.";
                    break;
                }
                case StringNames.ImpostorsRemainP:
                case StringNames.ImpostorsRemainS:
                {
                    if (ExileController.Instance.Field_10.Object.isPlayerRole("Joker"))
                        __result = "";
                    break;
                }
            }
        }
    }
}