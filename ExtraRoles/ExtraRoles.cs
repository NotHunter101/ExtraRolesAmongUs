using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using Reactor.Unstrip;
using static ExtraRolesMod.ExtraRoles;
using ExtraRoles.Medic;
using ExtraRoles.Officer;
using ExtraRoles.Roles;
using ExtraRoles.Roles.Medic;

namespace ExtraRolesMod
{
    public static class Extensions
    {
        public static bool isPlayerRole(this PlayerControl plr, Role roleName)
        {
            return plr.getModdedControl() != null && plr.getModdedControl().Role == roleName;
        }

        /// <summary>
        /// Returns true if the player is shielded by the medic, false otherwise
        /// </summary>
        public static bool isPlayerImmortal(this PlayerControl plr)
        {
            return plr.getModdedControl() != null && plr.getModdedControl().Immortal != ShieldState.None;
        }

        public static ModPlayerControl getModdedControl(this PlayerControl plr)
        {
            return Main.Logic.AllModPlayerControl.Find(x => x.PlayerControl == plr);
        }
    }

    [HarmonyPatch]
    public class ExtraRoles
    {
        public static class Main
        {
            public static Assets Assets = new Assets();
            public static ModdedPalette Palette = new ModdedPalette();
            public static ModdedConfig Config = new ModdedConfig();
            public static ModdedLogic Logic = new ModdedLogic();
        }

        public class ModdedLogic
        {
            public ModPlayerControl getRolePlayer(Role roleName)
            {
                return Main.Logic.AllModPlayerControl.Find(x => x.Role == roleName);
            }

            public ModPlayerControl getImmortalPlayer()
            {
                return Main.Logic.AllModPlayerControl.Find(x => x.Immortal != ShieldState.None);
            }

            public bool anyPlayerImmortal()
            {
                return Main.Logic.AllModPlayerControl.FindAll(x => x.Immortal != ShieldState.None).Count > 0;
            }

            public void clearJokerTasks()
            {
                var joker = Main.Logic.getRolePlayer(Role.Joker);
                if (joker == null)
                    return;
                var jokerControl = joker.PlayerControl;
                var removeTask = new List<PlayerTask>();

                foreach (var task in jokerControl.myTasks)
                    if (!PlayerTools.sabotageTasks.Contains(task.TaskType))
                        removeTask.Add(task);

                foreach (var task in removeTask)
                    jokerControl.RemoveTask(task);
            }

            public List<ModPlayerControl> AllModPlayerControl = new List<ModPlayerControl>();
            public bool sabotageActive { get; set; }
        }

        public class Assets
        {
            public AssetBundle bundle;
            public AudioClip breakClip;
            public Sprite repairIco;
            public Sprite shieldIco;
            public Sprite smallShieldIco;
        }

        public static Color VecToColor(Vector3 vec)
        {
            return new Color(vec.x, vec.y, vec.z);
        }

        public static Vector3 ColorToVec(Color color)
        {
            return new Vector3(color.r, color.g, color.b);
        }

        public static void BreakShield(bool flag)
        {
            if (PlayerControl.LocalPlayer.getModdedControl()?.Immortal == ShieldState.Broken)
            {
                SoundManager.Instance.PlaySound(Main.Assets.breakClip, false, 100f);
                PlayerControl.LocalPlayer.myRend.material.SetColor("_VisorColor", Palette.VisorColor);
                PlayerControl.LocalPlayer.myRend.material.SetFloat("_Outline", 0f);
                PlayerControl.LocalPlayer.getModdedControl().Immortal = ShieldState.Broken;
            }

            if (!flag)
                return;

            if (!Main.Logic.anyPlayerImmortal())
                return;

            var immortal = Main.Logic.getImmortalPlayer();
            immortal.PlayerControl.myRend.material.SetColor("_VisorColor", Palette.VisorColor);
            immortal.PlayerControl.myRend.material.SetFloat("_Outline", 0f);
            immortal.Immortal = ShieldState.None;
        }

        public static GameObject rend;
        public static List<DeadPlayer> killedPlayers = new List<DeadPlayer>();
        public static PlayerControl localPlayer = null;
        public static List<PlayerControl> localPlayers = new List<PlayerControl>();
        public static System.Random rng = new System.Random();
        public static KillButtonManager KillButton;
        public static int KBTarget;
        public static GameObject shieldIndicator = null;
        public static SpriteRenderer shieldRenderer = null;
        public static string versionString = "v1.4.3b";

        public class ModdedPalette
        {
            public Color medicColor = new Color(36f / 255f, 183f / 255f, 32f / 255f, 1);
            public Color officerColor = new Color(0, 40f / 255f, 198f / 255f, 1);
            public Color engineerColor = new Color(255f / 255f, 165f / 255f, 10f / 255f, 1);
            public Color jokerColor = new Color(138f / 255f, 138f / 255f, 138f / 255f, 1);
            public Color protectedColor = new Color(0, 1, 1, 1);
        }

        public class ModPlayerControl
        {
            public PlayerControl PlayerControl { get; set; }
            public Role Role { get; set; }
            public DateTime? LastAbilityTime { get; set; }
            public bool UsedAbility { get; set; }
            public ShieldState Immortal { get; set; } = ShieldState.None;
        }

        public class ModdedConfig
        {
            public float medicKillerNameDuration { get; set; }
            public float medicKillerColorDuration { get; set; }
            public bool showMedic { get; set; }
            public bool showReport { get; set; }
            public int showProtected { get; set; }
            public bool shieldKillAttemptIndicator { get; set; }
            public float OfficerCD { get; set; }
            public bool showOfficer { get; set; }
            public bool showEngineer { get; set; }
            public bool showJoker { get; set; }
            public float minimumCrewmateCount { get; set; }
            public OfficerKillBehaviour officerKillBehaviour { get; set; }
            public bool officerShouldDieToShieldedPlayers { get; set; }
            public float medicSpawnChance { get; set; }
            public float engineerSpawnChance { get; set; }
            public float officerSpawnChance { get; set; }
            public float jokerSpawnChance { get; set; }

            public void SetConfigSettings()
            {
                this.showMedic = HarmonyMain.showMedic.GetValue();
                this.showProtected = HarmonyMain.showShieldedPlayer.GetValue();
                this.showReport = HarmonyMain.medicReportSwitch.GetValue();
                this.shieldKillAttemptIndicator = HarmonyMain.playerMurderIndicator.GetValue();
                this.medicKillerNameDuration = HarmonyMain.medicReportNameDuration.GetValue();
                this.medicKillerColorDuration = HarmonyMain.medicReportColorDuration.GetValue();
                this.showOfficer = HarmonyMain.showOfficer.GetValue();
                this.OfficerCD = HarmonyMain.OfficerKillCooldown.GetValue();
                this.showEngineer = HarmonyMain.showEngineer.GetValue();
                this.showJoker = HarmonyMain.showJoker.GetValue();
                this.minimumCrewmateCount = HarmonyMain.minimumCrewmateCount.GetValue();
                this.officerKillBehaviour = (OfficerKillBehaviour) HarmonyMain.officerKillBehaviour.GetValue();
                this.officerShouldDieToShieldedPlayers = HarmonyMain.officerShouldDieToShieldedPlayers.GetValue();
                this.medicSpawnChance = HarmonyMain.medicSpawnChance.GetValue();
                this.engineerSpawnChance = HarmonyMain.engineerSpawnChance.GetValue();
                this.officerSpawnChance = HarmonyMain.officerSpawnChance.GetValue();
                this.jokerSpawnChance = HarmonyMain.jokerSpawnChance.GetValue();
            }
        }
    }
}