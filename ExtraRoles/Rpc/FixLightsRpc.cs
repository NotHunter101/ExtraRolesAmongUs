using ExtraRolesMod;
using Hazel;
using Reactor;


namespace ExtraRolesMod.Rpc
{
    [RegisterCustomRpc]
    public class FixLightsRpc : PlayerCustomRpc<ExtraRolesPlugin, bool>
    {
        public FixLightsRpc(ExtraRolesPlugin plugin) : base(plugin)
        {

        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

        public override void Handle(PlayerControl innerNetObject, bool data)
        {
            var switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
            switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
        }

        public override bool Read(MessageReader reader)
        {
            return reader.ReadBoolean();
        }

        public override void Write(MessageWriter writer, bool data)
        {
            writer.Write(data);
        }
    }
}
