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

            var roles = new List<(string roleName, float spawnChance, CustomRPC rpc)>()
            {
                ("Medic", HarmonyMain.medicSpawnChance.GetValue(), CustomRPC.SetMedic),
                ("Officer", HarmonyMain.officerSpawnChance.GetValue(), CustomRPC.SetOfficer),
                ("Engineer", HarmonyMain.engineerSpawnChance.GetValue(), CustomRPC.SetEngineer),
                ("Joker", HarmonyMain.jokerSpawnChance.GetValue(), CustomRPC.SetJoker),
            };

            while (roles.Count > 0 && crewmates.Count > HarmonyMain.minimumCrewmateCount.GetValue())
            {
                // Randomize order of role setting
                var roleIndex = rng.Next(0, roles.Count);
                var (roleName, spawnChance, rpc) = roles[roleIndex];
                var shouldSpawn = rng.Next(0, 100) <= spawnChance;

                if (shouldSpawn)
                {
                    var randomCrewmateIndex = rng.Next(0, crewmates.Count);
                    crewmates[randomCrewmateIndex].getModdedControl().Role = roleName;
                    var playerIdForRole = crewmates[randomCrewmateIndex].PlayerId;
                    crewmates.RemoveAt(randomCrewmateIndex);

                    System.Console.WriteLine($"Spawning {roleName} with PlayerID = {playerIdForRole}");

                    writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                        (byte)rpc, Hazel.SendOption.None, -1);
                    writer.Write(playerIdForRole);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
                roles.RemoveAt(roleIndex);
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