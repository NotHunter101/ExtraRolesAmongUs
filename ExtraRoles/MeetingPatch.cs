using ExtraRolesMod;
using HarmonyLib;
using Hazel;
using System;
using static ExtraRolesMod.ExtraRoles;
using UnhollowerBaseLib;
using ExtraRolesMod.Roles;
using ExtraRolesMod.Rpc;
using Reactor;

namespace ExtraRoles
{
    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy),
        new[] {typeof(UnityEngine.Object)})]
    class MeetingExiledEnd
    {
        static void Prefix(UnityEngine.Object obj)
        {
            if (ExileController.Instance == null || obj != ExileController.Instance.gameObject)
                return;

            var Officer = Main.Logic.getRolePlayer(Role.Joker);
            if (Officer != null)
                Officer.LastAbilityTime = DateTime.UtcNow;
            if (ExileController.Instance.exiled == null ||
                !ExileController.Instance.exiled._object.isPlayerRole(Role.Joker))
                return;

            Rpc<JokerWinRpc>.Instance.Send(data: true, immediately: true);

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.isPlayerRole(Role.Joker))
                    continue;
                player.RemoveInfected();
                player.Die(DeathReason.Exile);
                player.Data.IsDead = true;
                player.Data.IsImpostor = false;
            }

            var joker = Main.Logic.getRolePlayer(Role.Joker).PlayerControl;
            joker.Revive();
            joker.Data.IsDead = false;
            joker.Data.IsImpostor = true;
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    class TranslationPatch
    {
        static void Postfix(ref string __result, StringNames __0)
        {
            if (ExileController.Instance == null || ExileController.Instance.exiled == null)
                return;

            switch (__0)
            {
                case StringNames.ExileTextPN:
                case StringNames.ExileTextSN:
                {
                    if (ExileController.Instance.exiled.Object.isPlayerRole(Role.Medic))
                        __result = ExileController.Instance.exiled.PlayerName + " was The Medic.";
                    else if (ExileController.Instance.exiled.Object.isPlayerRole(Role.Engineer))
                        __result = ExileController.Instance.exiled.PlayerName + " was The Engineer.";
                    else if (ExileController.Instance.exiled.Object.isPlayerRole(Role.Officer))
                        __result = ExileController.Instance.exiled.PlayerName + " was The Officer.";
                    else if (ExileController.Instance.exiled.Object.isPlayerRole(Role.Joker))
                        __result = ExileController.Instance.exiled.PlayerName + " was The Joker.";
                    else
                        __result = ExileController.Instance.exiled.PlayerName + " was not The Impostor.";
                    break;
                }
                case StringNames.ImpostorsRemainP:
                case StringNames.ImpostorsRemainS:
                {
                    if (ExileController.Instance.exiled.Object.isPlayerRole(Role.Joker))
                        __result = "";
                    break;
                }
            }
        }
    }
}