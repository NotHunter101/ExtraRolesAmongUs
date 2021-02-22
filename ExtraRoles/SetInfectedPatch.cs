using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
    class SetInfectedPatch
    {
        public static void Postfix(Il2CppReferenceArray<GameData.PlayerInfo> JPGEIBIBJPJ)
        {
            Main.Config.SetConfigSettings();
            Main.Logic.AllModPlayerControl.Clear();
            killedPlayers.Clear();
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte) CustomRPC.ResetVaribles, Hazel.SendOption.None, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            var crewmates = PlayerControl.AllPlayerControls.ToArray().ToList();
            foreach (var plr in crewmates)
                Main.Logic.AllModPlayerControl.Add(new ModPlayerControl
                {
                    PlayerControl = plr, Role = "Impostor", UsedAbility = false, LastAbilityTime = null,
                    Immortal = false
                });
            crewmates.RemoveAll(x => x.Data.IsImpostor);
            foreach (var plr in crewmates)
                plr.getModdedControl().Role = "Crewmate";

            System.Console.WriteLine(HarmonyMain.medicSpawnChance.GetValue());
            System.Console.WriteLine(HarmonyMain.officerSpawnChance.GetValue());
            System.Console.WriteLine(HarmonyMain.engineerSpawnChance.GetValue());
            System.Console.WriteLine(HarmonyMain.jokerSpawnChance.GetValue());

            if (crewmates.Count > 0 && rng.Next(0, 100) <= HarmonyMain.medicSpawnChance.GetValue())
            {
                var medicRandom = rng.Next(0, crewmates.Count);
                crewmates[medicRandom].getModdedControl().Role = "Medic";
                var medicId = crewmates[medicRandom].PlayerId;
                crewmates.RemoveAt(medicRandom);

                System.Console.WriteLine("Spawning Medic with PlayerID = " + medicId);

                writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.SetMedic, Hazel.SendOption.None, -1);
                writer.Write(medicId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }

            if (crewmates.Count > 0 && rng.Next(0, 100) <= HarmonyMain.officerSpawnChance.GetValue())
            {
                var OfficerRandom = rng.Next(0, crewmates.Count);
                crewmates[OfficerRandom].getModdedControl().Role = "Officer";
                var OfficerId = crewmates[OfficerRandom].PlayerId;
                crewmates.RemoveAt(OfficerRandom);

                System.Console.WriteLine("Spawning Officer with PlayerID = " + OfficerId);

                writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.SetOfficer, Hazel.SendOption.None, -1);
                writer.Write(OfficerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }

            if (crewmates.Count > 0 && rng.Next(0, 100) <= HarmonyMain.engineerSpawnChance.GetValue())
            {
                var EngineerRandom = rng.Next(0, crewmates.Count);
                crewmates[EngineerRandom].getModdedControl().Role = "Engineer";
                var EngineerId = crewmates[EngineerRandom].PlayerId;
                crewmates.RemoveAt(EngineerRandom);

                System.Console.WriteLine("Spawning Engineer with PlayerID = " + EngineerId);

                writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.SetEngineer, Hazel.SendOption.None, -1);
                writer.Write(EngineerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }

            if (crewmates.Count > 0 && rng.Next(0, 100) <= HarmonyMain.jokerSpawnChance.GetValue())
            {
                var JokerRandom = rng.Next(0, crewmates.Count);
                crewmates[JokerRandom].getModdedControl().Role = "Joker";
                var JokerId = crewmates[JokerRandom].PlayerId;
                crewmates.RemoveAt(JokerRandom);

                System.Console.WriteLine("Spawning Joker with PlayerID = " + JokerId);

                writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRPC.SetJoker, Hazel.SendOption.None, -1);
                writer.Write(JokerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }

            localPlayers.Clear();
            localPlayer = PlayerControl.LocalPlayer;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                var shouldAddPlayer = !player.Data.IsImpostor && !player.isPlayerRole("Joker");
                if (shouldAddPlayer)
                {
                    localPlayers.Add(player);
                }
            }

            writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte) CustomRPC.SetLocalPlayers, Hazel.SendOption.None, -1);
            writer.WriteBytesAndSize(localPlayers.Select(player => player.PlayerId).ToArray());
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }
}