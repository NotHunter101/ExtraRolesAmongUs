using ExtraRolesMod;
using ExtraRolesMod.Officer;
using ExtraRolesMod.Roles;
using Hazel;
using Reactor;


namespace ExtraRolesMod.Rpc
{
    [RegisterCustomRpc]
    public class OfficerKillRpc : PlayerCustomRpc<ExtraRolesPlugin, OfficerKillRpc.OfficerKillData>
    {
        public OfficerKillRpc(ExtraRolesPlugin plugin) : base(plugin)
        {

        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

        public override void Handle(PlayerControl innerNetObject, OfficerKillData data)
        {
            var attacker = PlayerTools.GetPlayerById(data.Attacker);
            var target = PlayerTools.GetPlayerById(data.Target);
            
            var isTargetJoker = target.HasRole(Role.Joker);
            var isTargetImpostor = target.Data.IsImpostor;
            var officerKillSetting = ExtraRoles.Config.officerKillBehaviour;
            if (target.IsPlayerImmortal())
            {
                if (ExtraRoles.Config.officerShouldDieToShieldedPlayers)
                {
                    // suicide packet
                    attacker.MurderPlayer(attacker);
                }

            }
            else if (officerKillSetting == OfficerKillBehaviour.OfficerSurvives // don't care who it is, kill them
                || isTargetImpostor // impostors always die
                || (officerKillSetting != OfficerKillBehaviour.Impostor && isTargetJoker)) // joker can die and target is joker
            {
                // kill target
                attacker.MurderPlayer(target);
            }
            else // officer dies
            {
                if (officerKillSetting == OfficerKillBehaviour.CrewDie)
                {
                    // kill target too
                    attacker.MurderPlayer(target);
                }
                // kill officer
                attacker.MurderPlayer(attacker);
            }

        }

        public override OfficerKillData Read(MessageReader reader)
        {
            return new OfficerKillData(attacker: reader.ReadByte(), target: reader.ReadByte());
        }

        public override void Write(MessageWriter writer, OfficerKillData data)
        {
            writer.Write(data.Attacker);
            writer.Write(data.Target);
        }
        public struct OfficerKillData
        {
            public OfficerKillData(byte attacker, byte target)
            {
                Attacker = attacker;
                Target = target;
            }

            public byte Attacker { get; }
            public byte Target { get; }
            public static implicit operator OfficerKillData((PlayerControl Attacker, PlayerControl Target) data) => new OfficerKillData(data.Attacker.PlayerId, data.Target.PlayerId);
        }
    }

}
