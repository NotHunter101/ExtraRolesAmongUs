using ExtraRoles.Medic;
using ExtraRoles.Officer;
using ExtraRoles.Rpc;
using ExtraRolesMod;
using HarmonyLib;
using Hazel;
using Reactor;
using System;
using UnityEngine;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRoles.Roles.Officer
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static class MurderPlayerPatch
    {
        public static bool Prefix(PlayerControl __instance, PlayerControl PAIBDFDMIGK)
        {
            //check if the player is an officer
            if (__instance.isPlayerRole(Role.Officer))
            {
                //if so, set them to impostor for one frame so they aren't banned for anti-cheat
                __instance.Data.IsImpostor = true;
            }

            return true;
        }

        //handle the murder after it's ran
        public static void Postfix(PlayerControl __instance, PlayerControl PAIBDFDMIGK)
        {
            var deadBody = new DeadPlayer
            {
                PlayerId = PAIBDFDMIGK.PlayerId,
                KillerId = __instance.PlayerId,
                KillTime = DateTime.UtcNow,
                DeathReason = DeathReason.Kill
            };

            if (__instance.isPlayerRole(Role.Officer))
            {
                __instance.Data.IsImpostor = false;
            }

            if (__instance.PlayerId == PAIBDFDMIGK.PlayerId)
            {
                deadBody.DeathReason = (DeathReason)3;
            }

            killedPlayers.Add(deadBody);
        }
    }
}
