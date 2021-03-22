using ExtraRolesMod.Officer;

namespace ExtraRolesMod
{

    public class ModdedConfig
    {
        public float medicKillerNameDuration { get; set; }
        public float medicKillerColorDuration { get; set; }
        public bool showReport { get; set; }
        public ShieldOptions showProtected { get; set; }
        public bool ShieldMurderAttemptIndicator { get; set; }
        public float OfficerCD { get; set; }
        public OfficerKillBehaviour officerKillBehaviour { get; set; }
        public bool officerShouldDieToShieldedPlayers { get; set; }
        public float medicSpawnChance { get; set; }
        public float engineerSpawnChance { get; set; }
        public float officerSpawnChance { get; set; }
        public float jokerSpawnChance { get; set; }

        public void SetConfigSettings()
        {
            this.showProtected = (ShieldOptions)ExtraRolesPlugin.showShieldedPlayer.GetValue();
            this.showReport = ExtraRolesPlugin.medicReportSwitch.GetValue();
            this.ShieldMurderAttemptIndicator = ExtraRolesPlugin.playerMurderIndicator.GetValue();
            this.medicKillerNameDuration = ExtraRolesPlugin.medicReportNameDuration.GetValue();
            this.medicKillerColorDuration = ExtraRolesPlugin.medicReportColorDuration.GetValue();
            this.OfficerCD = ExtraRolesPlugin.OfficerKillCooldown.GetValue();
            this.officerKillBehaviour = (OfficerKillBehaviour)ExtraRolesPlugin.officerKillBehaviour.GetValue();
            this.officerShouldDieToShieldedPlayers = ExtraRolesPlugin.officerShouldDieToShieldedPlayers.GetValue();
            this.medicSpawnChance = ExtraRolesPlugin.medicSpawnChance.GetValue();
            this.engineerSpawnChance = ExtraRolesPlugin.engineerSpawnChance.GetValue();
            this.officerSpawnChance = ExtraRolesPlugin.officerSpawnChance.GetValue();
            this.jokerSpawnChance = ExtraRolesPlugin.jokerSpawnChance.GetValue();
        }
    }
}
