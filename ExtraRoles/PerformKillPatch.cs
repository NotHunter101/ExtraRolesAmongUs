using ExtraRoles.Medic;
﻿using ExtraRoles.Officer;
using HarmonyLib;
using Hazel;
using System;
using UnityEngine;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod
{
    [HarmonyPatch(typeof(KillButtonManager), nameof(KillButtonManager.PerformKill))]
    class PerformKillPatch
    {
        private static void WriteKillRpc(PlayerControl killer, PlayerControl target)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                      (byte)CustomRPC.OfficerKill, Hazel.SendOption.None, -1);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            writer.Write(target.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            PlayerControl.LocalPlayer.MurderPlayer(target);
            PlayerControl.LocalPlayer.getModdedControl().LastAbilityTime = DateTime.UtcNow;
            writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte)CustomRPC.AttemptSound, Hazel.SendOption.None, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        private static void WriteGiveShieldRpc(PlayerControl medic, PlayerControl target)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte)CustomRPC.SetProtected, Hazel.SendOption.None, -1);
            target.getModdedControl().Immortal = true;
            PlayerControl.LocalPlayer.getModdedControl().UsedAbility = true;
            writer.Write(target.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static bool Prefix()
        {
            if (PlayerControl.LocalPlayer.isPlayerRole("Engineer"))
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
                if (PlayerControl.LocalPlayer.isPlayerRole("Officer"))
                {
                    if (PlayerTools.getOfficerCD() > 0)
                        return false;

                    var isTargetJoker = target.isPlayerRole("Joker");
                    var isTargetImpostor = target.Data.IsImpostor;
                    var officerKillSetting = Main.Config.officerKillBehaviour;
                    if (target.isPlayerImmortal())
                    {
                        // suicide packet
                        WriteKillRpc(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer);
                        BreakShield(false);
                    }
                    else if (officerKillSetting == OfficerKillBehaviour.OfficerSurvives // don't care who it is, kill them
                        || isTargetImpostor // impostors always die
                        || officerKillSetting == OfficerKillBehaviour.Joker && isTargetJoker) // joker can die and target is joker
                    {
                        // kill target
                        WriteKillRpc(PlayerControl.LocalPlayer, target);
                    }
                    else // officer dies
                    {
                        if (officerKillSetting == OfficerKillBehaviour.CrewDie)
                        {
                            // kill target too
                            WriteKillRpc(PlayerControl.LocalPlayer, target);
                        }
                        // kill officer
                        WriteKillRpc(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer);
                    }

                    return false;
                }

                if (PlayerControl.LocalPlayer.isPlayerRole("Medic"))
                {
                    WriteGiveShieldRpc(PlayerControl.LocalPlayer, target);
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
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte) CustomRPC.AttemptSound, Hazel.SendOption.None, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            BreakShield(false);

            return false;
        }

        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowInfectedMap))]
        class EngineerMapOpen
        {
            static void Postfix(MapBehaviour __instance)
            {
                if (!PlayerControl.LocalPlayer.isPlayerRole("Engineer"))
                    return;
                if (!__instance.IsOpen)
                    return;

                __instance.ColorControl.baseColor = Main.Palette.engineerColor;
                foreach (var room in __instance.infectedOverlay.rooms)
                {
                    if (room.door == null)
                        continue;

                    room.door.enabled = false;
                    room.door.gameObject.SetActive(false);
                    room.door.gameObject.active = false;
                }
            }
        }

        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
        class EngineerMapUpdate
        {
            static void Postfix(MapBehaviour __instance)
            {
                if (!PlayerControl.LocalPlayer.isPlayerRole("Engineer"))
                    return;
                if (!__instance.IsOpen || !__instance.infectedOverlay.gameObject.active)
                    return;
                __instance.ColorControl.baseColor =
                    !Main.Logic.sabotageActive ? Color.gray : Main.Palette.engineerColor;

                var perc = Main.Logic.getRolePlayer("Engineer").UsedAbility ? 1f : 0f;

                foreach (var room in __instance.infectedOverlay.rooms)
                {
                    if (room.special == null)
                        continue;
                    room.special.material.SetFloat("_Desat", !Main.Logic.sabotageActive ? 1f : 0f);

                    room.special.enabled = true;
                    room.special.gameObject.SetActive(true);
                    room.special.gameObject.active = true;
                    room.special.material.SetFloat("_Percent", !PlayerControl.LocalPlayer.Data.IsDead ? perc : 1f);
                }
            }
        }

        [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.Method_41))]
        class SabotageButtonDeactivatePatch
        {
            static bool Prefix(MapRoom __instance, float DCEFKAOFGOG)
            {
                return !PlayerControl.LocalPlayer.isPlayerRole("Engineer");
            }
        }

        [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageReactor))]
        class SabotageReactorPatch
        {
            static bool Prefix(MapRoom __instance)
            {
                if (!PlayerControl.LocalPlayer.isPlayerRole("Engineer"))
                    return true;

                if (!PlayerTools.canEngineerUseAbility())
                    return false;

                PlayerControl.LocalPlayer.getModdedControl().UsedAbility = true;
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 16);

                return false;
            }
        }

        [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageLights))]
        class SabotageLightsPatch
        {
            static bool Prefix(MapRoom __instance)
            {
                if (!PlayerControl.LocalPlayer.isPlayerRole("Engineer"))
                    return true;
                if (!PlayerTools.canEngineerUseAbility())
                    return false;

                PlayerControl.LocalPlayer.getModdedControl().UsedAbility = true;
                var switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.FixLights, Hazel.SendOption.None, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);

                return false;
            }
        }

        [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageComms))]
        class SabotageCommsPatch
        {
            static bool Prefix(MapRoom __instance)
            {
                if (!PlayerControl.LocalPlayer.isPlayerRole("Engineer"))
                    return true;
                if (!PlayerTools.canEngineerUseAbility())
                    return false;

                PlayerControl.LocalPlayer.getModdedControl().UsedAbility = true;
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 1);

                return false;
            }
        }

        [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageOxygen))]
        class SabotageOxyPatch
        {
            static bool Prefix(MapRoom __instance)
            {
                if (!PlayerControl.LocalPlayer.isPlayerRole("Engineer"))
                    return true;

                if (!PlayerTools.canEngineerUseAbility())
                    return false;

                PlayerControl.LocalPlayer.getModdedControl().UsedAbility = true;
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);

                return false;
            }
        }

        [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageSeismic))]
        class SabotageSeismicPatch
        {
            static bool Prefix(MapRoom __instance)
            {
                if (!PlayerControl.LocalPlayer.isPlayerRole("Engineer"))
                    return true;

                if (!PlayerTools.canEngineerUseAbility())
                    return false;

                PlayerControl.LocalPlayer.getModdedControl().UsedAbility = true;
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Laboratory, 16);

                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        public static class MurderPlayerPatch
        {
            public static bool Prefix(PlayerControl __instance, PlayerControl CAKODNGLPDF)
            {
                //check if the player is an officer
                if (__instance.isPlayerRole("Officer"))
                {
                    //if so, set them to impostor for one frame so they aren't banned for anti-cheat
                    __instance.Data.IsImpostor = true;
                }

                return true;
            }

            //handle the murder after it's ran
            public static void Postfix(PlayerControl __instance, PlayerControl CAKODNGLPDF)
            {
                var deadBody = new DeadPlayer
                {
                    PlayerId = CAKODNGLPDF.PlayerId,
                    KillerId = __instance.PlayerId,
                    KillTime = DateTime.UtcNow,
                    DeathReason = DeathReason.Kill
                };

                if (__instance.isPlayerRole("Officer"))
                {
                    __instance.Data.IsImpostor = false;
                }

                if (__instance.PlayerId == CAKODNGLPDF.PlayerId)
                {
                    deadBody.DeathReason = (DeathReason) 3;
                }

                killedPlayers.Add(deadBody);
            }
        }
    }
}
