using ExtraRolesMod.Medic;
using ExtraRolesMod.Roles;
using ExtraRolesMod.Rpc;
using HarmonyLib;
using Hazel;
using Reactor;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
    class SetInfectedPatch
    {
        public static void Postfix(Il2CppReferenceArray<GameData.PlayerInfo> FMAOEJEHPAO)
        {
            Rpc<ResetVariablesRpc>.Instance.Send(data: true, immediately: true);

            System.Console.WriteLine(HarmonyMain.medicSpawnChance.GetValue());
            System.Console.WriteLine(HarmonyMain.officerSpawnChance.GetValue());
            System.Console.WriteLine(HarmonyMain.engineerSpawnChance.GetValue());
            System.Console.WriteLine(HarmonyMain.jokerSpawnChance.GetValue());

            var roles = new List<(Role roleName, float spawnChance)>()
            {
                (Role.Medic, HarmonyMain.medicSpawnChance.GetValue()),
                (Role.Officer, HarmonyMain.officerSpawnChance.GetValue()),
                (Role.Engineer, HarmonyMain.engineerSpawnChance.GetValue()),
                (Role.Joker, HarmonyMain.jokerSpawnChance.GetValue()),
            };

            var crewmates = PlayerControl.AllPlayerControls
                .ToArray()
                .Where(x => !x.Data.IsImpostor)
                .ToList();

            foreach (var (roleName, spawnChance) in roles)
            {
                var shouldSpawn = crewmates.Count > 0 && rng.Next(0, 100) <= spawnChance;
                if (!shouldSpawn)
                    continue;
                
                var randomCrewmateIndex = rng.Next(0, crewmates.Count);
                crewmates[randomCrewmateIndex].getModdedControl().Role = roleName;
                var playerIdForRole = crewmates[randomCrewmateIndex].PlayerId;
                crewmates.RemoveAt(randomCrewmateIndex);

                Rpc<SetRoleRpc>.Instance.Send(data: (PlayerId: playerIdForRole, Role: roleName), immediately: true);
            }

            localPlayers.Clear();
            localPlayer = PlayerControl.LocalPlayer;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                var shouldAddPlayer = !player.Data.IsImpostor && !player.isPlayerRole(Role.Joker);
                if (shouldAddPlayer)
                {
                    localPlayers.Add(player);
                }
            }
            var players = localPlayers
                .Select(player => player.PlayerId)
                .ToArray();
            Rpc<SetLocalPlayersRpc>.Instance.Send(data: players, immediately: true);

        }
    }
}