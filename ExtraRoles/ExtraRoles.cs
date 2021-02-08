using HarmonyLib;
using Hazel;
using QRCoder;
using QRCoder.Unity;
using System;
using System.Collections.Generic;
using draw = System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.IL2CPP.UnityEngine;
using Il2CppDumper;
using InnerNet;
using Steamworks;
using System.CodeDom;
using System.Net;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using Reactor;
using ExtraRolesMod;
using Reactor.Unstrip;
using UnityEngine.Networking;
using Reactor.Extensions;

/*
Hex colors for extra roles
Engineer: 972e00
Joker: 838383
Medic: 24b720
Officer: 0028c6
*/

namespace ExtraRolesMod
{
    public class DeadPlayer
    {
        public byte KillerId { get; set; }
        public byte PlayerId { get; set; }
        public DateTime KillTime { get; set; }
        public DeathReason DeathReason { get; set; }
    }
    //body report class for when medic reports a body
    public class BodyReport
    {
        public DeathReason DeathReason { get; set; }
        public PlayerControl Killer { get; set; }
        public PlayerControl Reporter { get; set; }
        public float KillAge { get; set; }

        public static string ParseBodyReport(BodyReport br)
        {
            System.Console.WriteLine(br.KillAge);
            if (br.KillAge > ExtraRoles.MedicSettings.medicKillerColorDuration * 1000)
            {
                return $"Body Report: The corpse is too old to gain information from. (Killed {Math.Round(br.KillAge / 1000)}s ago)";
            }
            else if (br.DeathReason == (DeathReason)3)
            {
                return $"Body Report (Officer): The cause of death appears to be suicide! (Killed {Math.Round(br.KillAge / 1000)}s ago)";

            }
            else if (br.KillAge < ExtraRoles.MedicSettings.medicKillerNameDuration * 1000)
            {
                return $"Body Report: The killer appears to be {br.Killer.name}! (Killed {Math.Round(br.KillAge / 1000)}s ago)";
            }
            else
            {
                //TODO (make the type of color be written to chat
                var colors = new Dictionary<byte, string>()
                {
                    {0, "darker"},
                    {1, "darker"},
                    {2, "darker"},
                    {3, "lighter"},
                    {4, "lighter"},
                    {5, "lighter"},
                    {6, "darker"},
                    {7, "lighter"},
                    {8, "darker"},
                    {9, "darker"},
                    {10, "lighter"},
                    {11, "lighter"},
                };
                var typeOfColor = colors[br.Killer.Data.ColorId];
                return $"Body Report: The killer appears to be a {typeOfColor} color. (Killed {Math.Round(br.KillAge / 1000)}s ago)";
            }
        }
    }

    [HarmonyPatch]
    public static class ExtraRoles
    {
        public static AssetBundle bundle = AssetBundle.LoadFromFile(Directory.GetCurrentDirectory() + "\\Assets\\bundle");
        public static AudioClip breakClip = bundle.LoadAsset<AudioClip>("SB").DontUnload();
        public static Sprite repairIco = bundle.LoadAsset<Sprite>("RE").DontUnload();
        public static Sprite shieldIco = bundle.LoadAsset<Sprite>("SA").DontUnload();
        public static Sprite smallShieldIco = bundle.LoadAsset<Sprite>("RESmall").DontUnload();

        public static void BreakShield(bool flag)
        {
            if (flag)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShieldBreak, Hazel.SendOption.None, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                MedicSettings.Protected.myRend.material.SetColor("_VisorColor", Palette.VisorColor);
                MedicSettings.Protected.myRend.material.SetFloat("_Outline", 0f);
                MedicSettings.Protected = null;
            }
        }

        public static GameObject rend;
        //rudimentary array to convert a byte setting from config into true/false
        public static bool[] byteBool =
        {
            false,
            true
        };
        public static List<DeadPlayer> killedPlayers = new List<DeadPlayer>();
        public static PlayerControl CurrentTarget = null;
        public static PlayerControl localPlayer = null;
        public static List<PlayerControl> localPlayers = new List<PlayerControl>();
        //global rng
        public static System.Random rng = new System.Random();
        //the kill button in the bottom right
        public static KillButtonManager KillButton;
        //the id of the targeted player
        public static int KBTarget;
        //distance between the local player and closest player
        public static double DistLocalClosest;
        //shield indicator sprite (placeholder)
        public static GameObject shieldIndicator = null;
        //renderer for the shield indicator
        public static SpriteRenderer shieldRenderer = null;
        //medic settings and values
        public static string versionString = "v1.2.4";
        public static class ModdedPalette
        {
            public static Color medicColor = new Color(36f / 255f, 183f / 255f, 32f / 255f, 1);
            public static Color officerColor = new Color(0, 40f / 255f, 198f / 255f, 1);
            public static Color engineerColor = new Color(151f / 255f, 46f / 255f, 0, 1);
            public static Color jokerColor = new Color(138f / 255f, 138f / 255f, 138f / 255f, 1);
            public static Color protectedColor = new Color(0, 1, 1, 1);
        }
        public static class MedicSettings 
        {
            public static PlayerControl Medic { get; set; }
            public static PlayerControl Protected { get; set; }
            public static bool shieldUsed { get; set; }
            public static int medicKillerNameDuration { get; set; }
            public static int medicKillerColorDuration { get; set; }
            public static bool showMedic { get; set; }
            public static bool showReport {get; set;}
            public static bool showProtected { get; set; }
            public static bool shieldKillAttemptIndicator { get; set; }
            public static void ClearSettings()
            {
                Medic = null;
                Protected = null;
                shieldUsed = false;
            }

            public static void SetConfigSettings()
            {
                showMedic = HarmonyMain.showMedic.GetValue();
                showProtected = HarmonyMain.showShieldedPlayer.GetValue();
                showReport = HarmonyMain.medicReportSwitch.GetValue();
                shieldKillAttemptIndicator = HarmonyMain.playerMurderIndicator.GetValue();
                medicKillerNameDuration = (int)HarmonyMain.medicReportNameDuration.GetValue();
                medicKillerColorDuration = (int)HarmonyMain.medicReportColorDuration.GetValue();
            }
        }
        //officer settings and values
        public static class OfficerSettings
        {
            public static PlayerControl Officer { get; set; }
            public static float OfficerCD { get; set; }
            public static bool showOfficer { get; set; }
            public static DateTime? lastKilled { get; set; }

            public static void ClearSettings()
            {
                Officer = null;
                lastKilled = null;
            }

            public static void SetConfigSettings()
            {
                showOfficer = HarmonyMain.showOfficer.GetValue();
                OfficerCD = HarmonyMain.OfficerKillCooldown.GetValue();
            }
        }
        //engineer settings and values
        public static class EngineerSettings
        {
            public static PlayerControl Engineer;
            public static bool repairUsed = false;
            public static bool showEngineer = false;
            public static bool sabotageActive { get; set; }
            public static void ClearSettings()
            {
                Engineer = null;
                repairUsed = false;
            }

            public static void SetConfigSettings()
            {
                showEngineer = HarmonyMain.showEngineer.GetValue();
            }
        }

        //joker settings and values
        public static class JokerSettings
        {
            public static PlayerControl Joker;
            public static bool showJoker = false;
            public static bool jokerCanDieToOfficer = false;

            public static void ClearSettings()
            {
                Joker = null;
            }

            public static void ClearTasks()
            {
                var removeTask = new List<PlayerTask>();
                foreach (PlayerTask task in JokerSettings.Joker.myTasks)
                    if (task.TaskType != TaskTypes.FixComms && task.TaskType != TaskTypes.FixLights && task.TaskType != TaskTypes.ResetReactor && task.TaskType != TaskTypes.ResetSeismic && task.TaskType != TaskTypes.RestoreOxy)
                        removeTask.Add(task);
                foreach (PlayerTask task in removeTask)
                    JokerSettings.Joker.RemoveTask(task);
            }

            public static void SetConfigSettings()
            {
                showJoker = HarmonyMain.showJoker.GetValue();
                jokerCanDieToOfficer = HarmonyMain.jokerCanDieToOfficer.GetValue();
            }
        }

        //function called on start of game. write version text on menu
        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        public static class VersionStartPatch
        {
            static void Postfix(VersionShower __instance)
            {
                __instance.text.Text = __instance.text.Text + "   Extra Roles " + versionString + " Loaded. (http://www.extraroles.net/)";
            }
        }

        [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
        public static class AmBannedPatch
        {
            public static void Postfix(out bool __result)
            {
                __result = false;
            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        public static class PingPatch
        {
            public static void Postfix(PingTracker __instance)
            {
                __instance.text.Text += "\nextraroles.net";
                __instance.text.Text += "\nExtraRoles " + versionString;
            }
        }

        [HarmonyPatch(typeof(ShipStatus), "GetSpawnLocation")]
        public static class StartGamePatch
        {
            public static void Postfix(ShipStatus __instance)
            {
                ConsoleTools.Info("Game Started!");
            }
        }
    }
}