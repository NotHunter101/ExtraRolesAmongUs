using ExtraRolesMod.Roles;
using ExtraRolesMod.Utility;
using Hazel;
using Reactor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtraRolesMod.Rpc
{
    [RegisterCustomRpc]
    public class InitializeRoundRpc : PlayerCustomRpc<ExtraRolesPlugin, InitializeRoundData>
    {
        public InitializeRoundRpc(ExtraRolesPlugin plugin) : base(plugin)
        {

        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

        public override void Handle(PlayerControl innerNetObject, InitializeRoundData data)
        {
            ResetVariables();
            foreach (var role in data.Roles)
            {
                var player = PlayerTools.GetPlayerById(role.Key);
                player.GetModdedControl().Role = role.Value;
            }
        }

        private void ResetVariables()
        {
            ExtraRoles.Config.SetConfigSettings();
            ExtraRoles.Logic.AllModPlayerControl.Clear();
            ExtraRoles.KilledPlayers.Clear();
            var crewmates = PlayerControl.AllPlayerControls.ToArray().ToList();
            foreach (var plr in crewmates)
            {
                ExtraRoles.Logic.AllModPlayerControl.Add(new ModPlayerControl
                {
                    PlayerControl = plr,
                    Role = Role.Crewmate,
                    UsedAbility = false,
                    Immortal = false
                });
            }
            foreach (var plr in crewmates.Where(x => x.Data.IsImpostor))
                plr.GetModdedControl().Role = Role.Impostor;
        }

        public override InitializeRoundData Read(MessageReader reader)
        {
            return BinarySerializer.Deserialize<InitializeRoundData>(reader.ReadBytesAndSize());
        }

        public override void Write(MessageWriter writer, InitializeRoundData data)
        {
            writer.WriteBytesAndSize(BinarySerializer.Serialize(data));
        }
    }
}
