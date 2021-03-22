using ExtraRolesMod.Roles;
using ExtraRolesMod.Rpc;
using HarmonyLib;
using Reactor;
using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;


namespace ExtraRolesMod.Roles
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
    class SetInfectedPatch
    {
        public static void Postfix()
        {
            var roles = new List<(Role roleName, float spawnChance)>()
            {
                (Role.Medic, ExtraRolesPlugin.medicSpawnChance.GetValue()),
                (Role.Officer, ExtraRolesPlugin.officerSpawnChance.GetValue()),
                (Role.Engineer, ExtraRolesPlugin.engineerSpawnChance.GetValue()),
                (Role.Joker, ExtraRolesPlugin.jokerSpawnChance.GetValue()),
            };

            var crewmates = PlayerControl.AllPlayerControls
                .ToArray()
                .Where(x => !x.Data.IsImpostor)
                .ToList();

            var data = new InitializeRoundData();

            foreach (var (roleName, spawnChance) in roles)
            {
                var shouldSpawn = crewmates.Count > 0 && ExtraRoles.rng.Next(0, 100) <= spawnChance;
                if (!shouldSpawn)
                    continue;
                var player = crewmates[ExtraRoles.rng.Next(0, crewmates.Count)];
                crewmates.Remove(player);
                data.Roles[player.PlayerId] = roleName;
            }
            Rpc<InitializeRoundRpc>.Instance.Send(data, immediately: true);
        }
    }
}