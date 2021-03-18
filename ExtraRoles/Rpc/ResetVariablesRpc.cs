using ExtraRolesMod.Medic;
using ExtraRolesMod.Roles;
using ExtraRolesMod;
using Hazel;
using Reactor;
using System.Linq;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod.Rpc
{
    [RegisterCustomRpc]
    public class ResetVariablesRpc : PlayerCustomRpc<HarmonyMain, bool>
    {
        public ResetVariablesRpc(HarmonyMain plugin) : base(plugin)
        {

        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

        public override void Handle(PlayerControl innerNetObject, bool data)
        {
            Main.Config.SetConfigSettings();
            Main.Logic.AllModPlayerControl.Clear();
            killedPlayers.Clear();
            var crewmates = PlayerControl.AllPlayerControls.ToArray().ToList();
            foreach (var plr in crewmates)
            {
                Main.Logic.AllModPlayerControl.Add(new ModPlayerControl
                {
                    PlayerControl = plr,
                    Role = Role.Impostor,
                    UsedAbility = false,
                    LastAbilityTime = null,
                    Immortal = ShieldState.None
                });
            }

            crewmates.RemoveAll(x => x.Data.IsImpostor);
            foreach (var plr in crewmates)
                plr.getModdedControl().Role = Role.Crewmate;

            Roles.Engineer.HudUpdatePatch.AddEngineerButton();
            Roles.Officer.HudUpdatePatch.AddOfficerKillButton();
            Roles.Medic.HudUpdatePatch.AddMedicShieldButton();
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
