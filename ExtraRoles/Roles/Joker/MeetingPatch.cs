using ExtraRolesMod;
using HarmonyLib;
using Hazel;
using System;

using UnhollowerBaseLib;
using ExtraRolesMod.Roles;
using ExtraRolesMod.Rpc;
using Reactor;

namespace ExtraRolesMod
{
    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy),
        new[] {typeof(UnityEngine.Object)})]
    class MeetingExiledEnd
    {
        static void Prefix(UnityEngine.Object obj)
        {
            if (ExileController.Instance == null || obj != ExileController.Instance.gameObject)
                return;

            var Officer = ExtraRoles.Logic.GetRolePlayer(Role.Joker);
            if (Officer != null)
                Officer.LastAbilityTime = DateTime.UtcNow;
            if (ExileController.Instance.exiled == null ||
                !ExileController.Instance.exiled._object.IsPlayerRole(Role.Joker))
                return;

            Rpc<JokerWinRpc>.Instance.Send(data: true, immediately: true);

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.IsPlayerRole(Role.Joker))
                    continue;
                player.RemoveInfected();
                player.Die(DeathReason.Exile);
                player.Data.IsDead = true;
                player.Data.IsImpostor = false;
            }

            var joker = ExtraRoles.Logic.GetRolePlayer(Role.Joker).PlayerControl;
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
                    if (ExileController.Instance.exiled.Object.IsPlayerRole(Role.Medic))
                        __result = ExileController.Instance.exiled.PlayerName + " was The Medic.";
                    else if (ExileController.Instance.exiled.Object.IsPlayerRole(Role.Engineer))
                        __result = ExileController.Instance.exiled.PlayerName + " was The Engineer.";
                    else if (ExileController.Instance.exiled.Object.IsPlayerRole(Role.Officer))
                        __result = ExileController.Instance.exiled.PlayerName + " was The Officer.";
                    else if (ExileController.Instance.exiled.Object.IsPlayerRole(Role.Joker))
                        __result = ExileController.Instance.exiled.PlayerName + " was The Joker.";
                    else
                        __result = ExileController.Instance.exiled.PlayerName + " was not The Impostor.";
                    break;
                }
                case StringNames.ImpostorsRemainP:
                case StringNames.ImpostorsRemainS:
                {
                    if (ExileController.Instance.exiled.Object.IsPlayerRole(Role.Joker))
                        __result = "";
                    break;
                }
            }
        }
    }
}