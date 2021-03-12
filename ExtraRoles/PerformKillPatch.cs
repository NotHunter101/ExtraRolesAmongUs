using ExtraRoles.Medic;
﻿using ExtraRoles.Officer;
using ExtraRoles.Roles;
using ExtraRoles.Rpc;
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
        private static void SendOfficerKillRpc(PlayerControl target)
        {
            var attacker = PlayerControl.LocalPlayer;
            Rpc<OfficerKillRpc>.Instance.Send(data: (Attacker: attacker, Target: target), immediately: true);
            Rpc<AttemptKillShieldedPlayerRpc>.Instance.Send(data: attacker.PlayerId, immediately: true);
        }

        private static void WriteGiveShieldRpc(PlayerControl target)
        {
            PlayerControl.LocalPlayer.getModdedControl().UsedAbility = true;
            Rpc<GiveShieldRpc>.Instance.Send(data: target.PlayerId, immediately: true);
        }

        public static bool Prefix()
        {
            if (PlayerControl.LocalPlayer.isPlayerRole(Role.Engineer))
            {
                DestroyableSingleton<HudManager>.Instance.ShowMap((Action<MapBehaviour>) delegate(MapBehaviour m)
                {
                    m.ShowInfectedMap();
                    m.ColorControl.baseColor = Main.Logic.sabotageActive ? Color.gray : Main.Palette.engineerColor;
                });
                return false;
            }

            if (PlayerControl.LocalPlayer.Data.IsDead)
                return false;

            if (CurrentTarget != null)
            {
                var target = CurrentTarget;
                //code that handles the ability button presses
                if (PlayerControl.LocalPlayer.isPlayerRole(Role.Officer))
                {
                    if (PlayerTools.getOfficerCD() > 0)
                        return false;

                    var isTargetJoker = target.isPlayerRole(Role.Joker);
                    var isTargetImpostor = target.Data.IsImpostor;
                    var officerKillSetting = Main.Config.officerKillBehaviour;
                    if (target.isPlayerImmortal())
                    {
                        // suicide packet
                        SendOfficerKillRpc(PlayerControl.LocalPlayer);
                        BreakShield(false);
                    }
                    else if (officerKillSetting == OfficerKillBehaviour.OfficerSurvives // don't care who it is, kill them
                        || isTargetImpostor // impostors always die
                        || (officerKillSetting  != OfficerKillBehaviour.Impostor && isTargetJoker)) // joker can die and target is joker
                    {
                        // kill target
                        SendOfficerKillRpc(target);
                    }
                    else // officer dies
                    {
                        if (officerKillSetting == OfficerKillBehaviour.CrewDie)
                        {
                            // kill target too
                            SendOfficerKillRpc(target);
                        }
                        // kill officer
                        SendOfficerKillRpc(PlayerControl.LocalPlayer);
                    }

                    return false;
                }

                if (PlayerControl.LocalPlayer.isPlayerRole(Role.Medic))
                {
                    WriteGiveShieldRpc(target);
                    return false;
                }
            }

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
