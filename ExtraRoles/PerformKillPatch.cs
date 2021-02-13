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
        public static bool Prefix()
        {
            if (PlayerControl.LocalPlayer == EngineerSettings.Engineer)
            {
                DestroyableSingleton<HudManager>.Instance.ShowMap((Action<MapBehaviour>)delegate (MapBehaviour m)
                {
                    m.ShowInfectedMap();
                    m.ColorControl.baseColor = EngineerSettings.sabotageActive ? Color.gray : ModdedPalette.engineerColor;
                });
                return false;
            }
            if (PlayerControl.LocalPlayer.Data.IsDead)
                return false;
            if (CurrentTarget != null)
            {
                PlayerControl target = CurrentTarget;
                //code that handles the ability button presses
                if (OfficerSettings.Officer != null && PlayerControl.LocalPlayer.PlayerId == OfficerSettings.Officer.PlayerId)
                {
                    if (PlayerTools.GetOfficerKD() == 0)
                    {
                        //check if they're shielded by medic
                        if (MedicSettings.Protected != null && target.PlayerId == MedicSettings.Protected.PlayerId)
                        {
                            //officer suicide packet
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.OfficerKill, Hazel.SendOption.None, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            PlayerControl.LocalPlayer.MurderPlayer(PlayerControl.LocalPlayer);
                            OfficerSettings.lastKilled = DateTime.UtcNow;
                            return false;
                        }
                        //check if they're joker and the setting is configured
                        else if (JokerSettings.jokerCanDieToOfficer && (JokerSettings.Joker != null && target.PlayerId == JokerSettings.Joker.PlayerId))
                        {
                            //officer joker murder packet
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.OfficerKill, Hazel.SendOption.None, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write(target.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            PlayerControl.LocalPlayer.MurderPlayer(target);
                            OfficerSettings.lastKilled = DateTime.UtcNow;
                        }
                        //check if they're an impostor
                        else if (target.Data.IsImpostor)
                        {
                            //officer impostor murder packet
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.OfficerKill, Hazel.SendOption.None, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write(target.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            PlayerControl.LocalPlayer.MurderPlayer(target);
                            OfficerSettings.lastKilled = DateTime.UtcNow;
                            return false;
                        }
                        //else, they're innocent and not shielded
                        else
                        {
                            //officer suicide packet
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.OfficerKill, Hazel.SendOption.None, -1);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            PlayerControl.LocalPlayer.MurderPlayer(PlayerControl.LocalPlayer);
                            OfficerSettings.lastKilled = DateTime.UtcNow;
                            return false;
                        }
                        return false;
                    }
                    return false;
                }
                else if (MedicSettings.Medic != null && PlayerControl.LocalPlayer == MedicSettings.Medic)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetProtected, Hazel.SendOption.None, -1);
                    MedicSettings.Protected = target;
                    MedicSettings.shieldUsed = true;
                    byte ProtectedId = MedicSettings.Protected.PlayerId;
                    writer.Write(ProtectedId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    return false;
                }
            }

            if (MedicSettings.Protected != null && PlayerTools.closestPlayer.PlayerId == MedicSettings.Protected.PlayerId)
            {
                //cancel the kill
                return false;
            }
            //otherwise, continue the murder as normal
            return true;
        }

        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowInfectedMap))]
        class EngineerMapOpen
        {
            static void Postfix(MapBehaviour __instance)
            {
                if (EngineerSettings.Engineer != null)
                {
                    if (EngineerSettings.Engineer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        if (__instance.IsOpen)
                        {
                            __instance.ColorControl.baseColor = ModdedPalette.engineerColor;
                            foreach (MapRoom room in __instance.infectedOverlay.rooms)
                            {
                                if (room.door != null)
                                {
                                    room.door.enabled = false;
                                    room.door.gameObject.SetActive(false);
                                    room.door.gameObject.active = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
        class EngineerMapUpdate
        {
            static void Postfix(MapBehaviour __instance)
            {
                if (EngineerSettings.Engineer != null)
                {
                    if (EngineerSettings.Engineer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        if (__instance.IsOpen && __instance.infectedOverlay.gameObject.active)
                        {
                            if (!EngineerSettings.sabotageActive)
                                __instance.ColorControl.baseColor = Color.gray;
                            else
                                __instance.ColorControl.baseColor = ModdedPalette.engineerColor;
                            float perc = EngineerSettings.repairUsed ? 1f : 0f;
                            foreach (MapRoom room in __instance.infectedOverlay.rooms)
                            {
                                if (room.special != null)
                                {
                                    if (!EngineerSettings.sabotageActive)
                                        room.special.material.SetFloat("_Desat", 1f);
                                    else
                                        room.special.material.SetFloat("_Desat", 0f);
                                    room.special.enabled = true;
                                    room.special.gameObject.SetActive(true);
                                    room.special.gameObject.active = true;
                                    if (!PlayerControl.LocalPlayer.Data.IsDead)
                                        room.special.material.SetFloat("_Percent", perc);
                                    else
                                        room.special.material.SetFloat("_Percent", 1f);
                                }
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.Method_41))]
        class SabotageButtonDeactivatePatch
        {
            static bool Prefix(MapRoom __instance, float DCEFKAOFGOG)
            {
                if (EngineerSettings.Engineer != null)
                {
                    if (EngineerSettings.Engineer != null && EngineerSettings.Engineer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageReactor))]
        class SabotageReactorPatch
        {
            static bool Prefix(MapRoom __instance)
            {
                if (EngineerSettings.Engineer != null && EngineerSettings.Engineer.PlayerId == PlayerControl.LocalPlayer.PlayerId && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    if (!EngineerSettings.repairUsed && EngineerSettings.sabotageActive)
                    {
                        EngineerSettings.repairUsed = true;
                        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 16);
                    }
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageLights))]
        class SabotageLightsPatch
        {
            static bool Prefix(MapRoom __instance)
            {
                if (EngineerSettings.Engineer != null && EngineerSettings.Engineer.PlayerId == PlayerControl.LocalPlayer.PlayerId && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    if (!EngineerSettings.repairUsed && EngineerSettings.sabotageActive)
                    {
                        EngineerSettings.repairUsed = true;
                        SwitchSystem switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                        switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.FixLights, Hazel.SendOption.None, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                    }
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageComms))]
        class SabotageCommsPatch
        {
            static bool Prefix(MapRoom __instance)
            {
                if (EngineerSettings.Engineer != null && EngineerSettings.Engineer.PlayerId == PlayerControl.LocalPlayer.PlayerId && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    if (!EngineerSettings.repairUsed && EngineerSettings.sabotageActive)
                    {
                        EngineerSettings.repairUsed = true;
                        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                    }
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageOxygen))]
        class SabotageOxyPatch
        {
            static bool Prefix(MapRoom __instance)
            {
                if (EngineerSettings.Engineer != null && EngineerSettings.Engineer.PlayerId == PlayerControl.LocalPlayer.PlayerId && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    if (!EngineerSettings.repairUsed && EngineerSettings.sabotageActive)
                    {
                        EngineerSettings.repairUsed = true;
                        ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                        ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                    }
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.SabotageSeismic))]
        class SabotageSeismicPatch
        {
            static bool Prefix(MapRoom __instance)
            {
                if (EngineerSettings.Engineer != null && EngineerSettings.Engineer.PlayerId == PlayerControl.LocalPlayer.PlayerId && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    if (!EngineerSettings.repairUsed && EngineerSettings.sabotageActive)
                    {
                        EngineerSettings.repairUsed = true;
                        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Laboratory, 16);
                    }
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        public static class MurderPlayerPatch
        {
            public static bool Prefix(PlayerControl __instance, PlayerControl CAKODNGLPDF)
            {
                if (OfficerSettings.Officer != null)
                {
                    //check if the player is an officer
                    if (__instance == OfficerSettings.Officer)
                    {
                        //if so, set them to impostor for one frame so they aren't banned for anti-cheat
                        __instance.Data.IsImpostor = true;
                    }
                }
                return true;
            }

            //handle the murder after it's ran
            public static void Postfix(PlayerControl __instance, PlayerControl CAKODNGLPDF)
            {
                var deadBody = new DeadPlayer();
                deadBody.PlayerId = CAKODNGLPDF.PlayerId;
                deadBody.KillerId = __instance.PlayerId;
                deadBody.KillTime = DateTime.UtcNow;
                deadBody.DeathReason = DeathReason.Kill;
                if (OfficerSettings.Officer != null)
                {
                    //check if killer is officer
                    if (__instance == OfficerSettings.Officer)
                    {
                        //finally, set them back to normal
                        __instance.Data.IsImpostor = false;
                    }
                    if (__instance.PlayerId == CAKODNGLPDF.PlayerId)
                    {
                        deadBody.DeathReason = (DeathReason)3;
                    }
                }
                killedPlayers.Add(deadBody);
            }
        }
    }
}
