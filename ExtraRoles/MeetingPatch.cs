using ExtraRolesMod;
using HarmonyLib;
using Hazel;
using System;
using static ExtraRolesMod.ExtraRoles;
using UnhollowerBaseLib;

namespace ExtraRoles
{

    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), new Type[] { typeof(UnityEngine.Object) })]
    class MeetingExiledEnd
    {
        static void Prefix(UnityEngine.Object obj)
        {
            if (ExileController.Instance != null && obj == ExileController.Instance.gameObject)
            {
                ModPlayerControl Officer = Main.Logic.getRolePlayer("Officer");
                if (Officer != null)
                    Officer.LastAbilityTime = DateTime.UtcNow;
                if (ExileController.Instance.Field_10 != null && ExileController.Instance.Field_10._object.isPlayerRole("Joker"))
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.JokerWin, Hazel.SendOption.None, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);

                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        if (!player.isPlayerRole("Joker"))
                        {
                            player.RemoveInfected();
                            player.Die(DeathReason.Exile);
                            player.Data.IsDead = true;
                            player.Data.IsImpostor = false;
                        }
                    }
                    PlayerControl joker = Main.Logic.getRolePlayer("Joker").PlayerControl;
                    joker.Revive();
                    joker.Data.IsDead = false;
                    joker.Data.IsImpostor = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    class TranslationPatch
    {
        static void Postfix(ref string __result, StringNames HKOIECMDOKL, Il2CppReferenceArray<Il2CppSystem.Object> EBKIKEILMLF)
        {
            if (ExileController.Instance != null && ExileController.Instance.Field_10 != null)
            {
                if (HKOIECMDOKL == StringNames.ExileTextPN || HKOIECMDOKL == StringNames.ExileTextSN)
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
                }
                if (HKOIECMDOKL == StringNames.ImpostorsRemainP || HKOIECMDOKL == StringNames.ImpostorsRemainS)
                {
                    if (ExileController.Instance.Field_10.Object.isPlayerRole("Joker"))
                        __result = "";
                }
            }
        }
    }
}
