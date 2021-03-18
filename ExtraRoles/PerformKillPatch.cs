using ExtraRolesMod.Medic;
﻿using ExtraRolesMod.Officer;
using ExtraRolesMod.Roles;
using ExtraRolesMod.Rpc;
using HarmonyLib;
using Hazel;
using Reactor;
using System;
using UnityEngine;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod
{
    [HarmonyPatch(typeof(KillButtonManager), nameof(KillButtonManager.PerformKill))]
    class PerformKillPatch
    {
        public static bool Prefix(KillButtonManager __instance)
        {
            // continue the murder as normal
            if (!KillButton.CurrentTarget.isPlayerImmortal())
                return true;

            // play shield break sound
            var shouldPlayShieldBreakSound = KillButton.CurrentTarget.isPlayerImmortal() &&
                                             KillButton.isActiveAndEnabled &&
                                             !KillButton.isCoolingDown && Main.Config.shieldKillAttemptIndicator;
            if (!shouldPlayShieldBreakSound)
                return false;

            // Send Play Shield Break RPC
            Rpc<AttemptKillShieldedPlayerRpc>.Instance.Send(data: PlayerControl.LocalPlayer.PlayerId, immediately: true);

            return false;
        }
    }
}
