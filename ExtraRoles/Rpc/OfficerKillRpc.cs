using ExtraRolesMod;
using Hazel;
using Reactor;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRoles.Rpc
{
    [RegisterCustomRpc]
    public class OfficerKillRpc : PlayerCustomRpc<HarmonyMain, OfficerKillRpc.OfficerKillData>
    {
        public OfficerKillRpc(HarmonyMain plugin) : base(plugin)
        {

        }

        // Handle this rpc localy first then send it to other clients
        public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

        public override void Handle(PlayerControl innerNetObject, OfficerKillData data)
        {
            var attacker = PlayerTools.getPlayerById(data.Attacker);

            var target = PlayerTools.getPlayerById(data.Target);

            attacker.MurderPlayer(target);
            if (target.isPlayerImmortal())
                BreakShield(false);
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
