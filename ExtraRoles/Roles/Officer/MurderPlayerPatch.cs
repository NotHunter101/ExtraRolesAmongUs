using ExtraRolesMod.Officer;
using ExtraRolesMod.Roles.Medic;
using ExtraRolesMod.Rpc;
using ExtraRolesMod;
using HarmonyLib;
using Hazel;
using Reactor;
using System;
using UnityEngine;


namespace ExtraRolesMod.Roles.Officer
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static class MurderPlayerPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            __instance.Data.IsImpostor = true;
            
            if (__instance.AmOwner && target.AmOwner)
            {
                target.GetModdedControl().Immortal = false;
                return true;
            }

            if (target.IsPlayerImmortal())
            {
                var comp = target.GetComponent<PlayerShield>();
                if (comp && target.AmOwner)
                {
                    if (ExtraRoles.Config.ShieldMurderAttemptIndicator)
                    {
                        comp.GlowShield();
                    }
                }
                return false;
            }

            return true;
        }

        public static void Postfix(PlayerControl __instance, PlayerControl __0)
        {
            if (__instance.HasRole(Role.Officer))
            {
                __instance.Data.IsImpostor = false;
            }
            var deadBody = new DeadPlayer
            {
                PlayerId = __0.PlayerId,
                KillerId = __instance.PlayerId,
                KillTime = DateTime.UtcNow,
                DeathReason = DeathReason.Kill
            };

            if (__instance.HasRole(Role.Officer))
            {
                __instance.Data.IsImpostor = false;
            }

            if (__instance.PlayerId == __0.PlayerId)
            {
                deadBody.DeathReason = (DeathReason)3;
            }

            ExtraRoles.KilledPlayers.Add(deadBody);
        }

    }
}
