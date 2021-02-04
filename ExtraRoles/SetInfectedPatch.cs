using ExtraRolesMod;
using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnhollowerBaseLib;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
    class SetInfectedPatch
    {
        public static void Postfix(Il2CppReferenceArray<GameData.PlayerInfo> JPGEIBIBJPJ)
        {
            MedicSettings.ClearSettings();
            OfficerSettings.ClearSettings();
            EngineerSettings.ClearSettings();
            JokerSettings.ClearSettings();
            MedicSettings.SetConfigSettings();
            OfficerSettings.SetConfigSettings();
            EngineerSettings.SetConfigSettings();
            JokerSettings.SetConfigSettings();
            killedPlayers.Clear();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ResetVaribles, Hazel.SendOption.None, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList();
            crewmates.RemoveAll(x => x.Data.IsImpostor);

            if (crewmates.Count > 0 && (rng.Next(1, 101) <= HarmonyMain.medicSpawnChance.GetValue()))
            {
                writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetMedic, Hazel.SendOption.None, -1);
                var MedicRandom = rng.Next(0, crewmates.Count);
                MedicSettings.Medic = crewmates[MedicRandom];
                crewmates.RemoveAt(MedicRandom);
                byte MedicId = MedicSettings.Medic.PlayerId;

                writer.Write(MedicId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }

            if (crewmates.Count > 0 && (rng.Next(1, 101) <= HarmonyMain.officerSpawnChance.GetValue()))
            {
                writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetOfficer, Hazel.SendOption.None, -1);

                var OfficerRandom = rng.Next(0, crewmates.Count);
                OfficerSettings.Officer = crewmates[OfficerRandom];
                crewmates.RemoveAt(OfficerRandom);
                byte OfficerId = OfficerSettings.Officer.PlayerId;

                writer.Write(OfficerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }

            if (crewmates.Count > 0 && (rng.Next(1, 101) <= HarmonyMain.engineerSpawnChance.GetValue()))
            {
                writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetEngineer, Hazel.SendOption.None, -1);
                var EngineerRandom = rng.Next(0, crewmates.Count);
                EngineerSettings.Engineer = crewmates[EngineerRandom];
                crewmates.RemoveAt(EngineerRandom);
                byte EngineerId = EngineerSettings.Engineer.PlayerId;

                writer.Write(EngineerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }

            if (crewmates.Count > 0 && (rng.Next(1, 101) <= HarmonyMain.jokerSpawnChance.GetValue()))
            {
                writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetJoker, Hazel.SendOption.None, -1);
                var JokerRandom = rng.Next(0, crewmates.Count);
                ConsoleTools.Info(JokerRandom.ToString());
                JokerSettings.Joker = crewmates[JokerRandom];
                crewmates.RemoveAt(JokerRandom);
                byte JokerId = JokerSettings.Joker.PlayerId;

                writer.Write(JokerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }

            localPlayers.Clear();
            localPlayer = PlayerControl.LocalPlayer;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsImpostor)
                    continue;
                if (JokerSettings.Joker != null && player.PlayerId == JokerSettings.Joker.PlayerId)
                    continue;
                else
                    localPlayers.Add(player);
            }
            var localPlayerBytes = new List<byte>();
            foreach (PlayerControl player in localPlayers)
            {
                localPlayerBytes.Add(player.PlayerId);
            }
            writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetLocalPlayers, Hazel.SendOption.None, -1);
            writer.WriteBytesAndSize(localPlayerBytes.ToArray());
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static bool Prefix(Il2CppReferenceArray<GameData.PlayerInfo> JPGEIBIBJPJ)
        {
            var debugImpostors = false;
            if (debugImpostors)
            {
                var infected = new byte[] { 0, 0 };

                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.Data.PlayerName == "Impostor")
                    {
                        infected[0] = player.PlayerId;
                    }
                    if (player.Data.PlayerName == "Pretender")
                    {
                        infected[1] = player.PlayerId;
                    }
                }

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RPC.SetInfected, Hazel.SendOption.None, -1);
                writer.WritePacked((uint)2);
                writer.WriteBytesAndSize(infected);
                AmongUsClient.Instance.FinishRpcImmediately(writer);

                PlayerControl.LocalPlayer.SetInfected(infected);

                return false;
            }
            return true;
        }
    }
}
