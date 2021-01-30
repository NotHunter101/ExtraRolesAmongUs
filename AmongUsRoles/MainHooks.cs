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

/*
Hex colors for extra roles
Engineer: 972e00
Joker: 838383
Medic: 24b720
Officer: 0028c6

Yes, I know the code is bad. I'm going to clean up the rpc handler soon, and the engineer's sabotage clearing ability doesn't work right now because i'm in the middle of rewriting that section. 
Also, the medic shield doesn't break when the medic dies.

I can comment the code more extensively if you need me to.
*/

namespace ExtraRolesMod
{
    //Class to log who has vented and their recent times. this is used to ratelimit vent entries/exits (fixes desync bug)
    public static class PlayerVentTimeExtension
    {
        public static void SetLastVent(byte player)
        {
            if (MainHooks.allVentTimes.ContainsKey(player))
            {
                MainHooks.allVentTimes[player] = DateTime.UtcNow;
            }
            else
            {
                MainHooks.allVentTimes.Add(player, DateTime.UtcNow);
            }
        }

        public static DateTime GetLastVent(byte player)
        {
            if (MainHooks.allVentTimes.ContainsKey(player))
            {
                return MainHooks.allVentTimes[player];
            }
            else
                return new DateTime(0);
        }
    }

    //body report class for when medic reports a body
    public class BodyReport
    {
        public byte DeathReason { get; set; }
        public FFGALNAPKCD Killer { get; set; }
        public FFGALNAPKCD Reporter { get; set; }
        public float KillAge { get; set; }

        public static string ParseBodyReport(BodyReport br)
        {
            if (br.KillAge > MainHooks.MedicSettings.medicKillerColorDuration * 1000)
            {
                return $"Body Report: The body is too old to gain information from. (Killed {Math.Round(br.KillAge / 1000)}s ago)";
            }
            else if (br.DeathReason == 3)
            {
                return $"Body Report (Officer): The cause of death appears to be suicide by shooting an Innocent! (Killed {Math.Round(br.KillAge / 1000)}s ago)";

            }
            else if (br.KillAge < MainHooks.MedicSettings.medicKillerNameDuration * 1000)
            {
                return $"Body Report: The murderer appears to be {br.Killer.name}! (Killed {Math.Round(br.KillAge / 1000)}s ago)";
            }
            else
            {
                //TODO (make the type of color be written to chat
                var colors = new Dictionary<string, byte>()
                {

                };
                var typeOfColor = "darker";
                return $"Body Report: The murder appears to be a {typeOfColor} color. (Killed {Math.Round(br.KillAge / 1000)}s ago)";
            }
        }
    }
    [HarmonyPatch]
    public static class MainHooks
    {
        //list of all entries/exits of vents for each player and their times (used by VentPlayerExtension)
        public static IDictionary<byte, DateTime> allVentTimes = new Dictionary<byte, DateTime>() { };
        //array of config settings
        public static string[] configSettingsKeys =
        {
            "Show Medic",
            "Show Shielded Player",
            "Murder Attempt Indicator For Shielded Player",
            "Show Officer",
            "Officer Kill Cooldown",
            "Show Engineer",
            "Show Joker",
            "Joker Can Die To Officer",
            "Duration In Which Medic Report Will Contain The Killers Name",
            "Duration In Which Medic Report Will Contain The Killers Color Type"
        };
        //array of all config settings and their values. these will be loaded by a config and sent to all clients
        public static IDictionary<string, byte> configSettings = new Dictionary<string, byte>()
        {
            { "Show Medic", 0 },
            { "Show Shielded Player", 0 },
            { "Murder Attempt Indicator For Shielded Player", 0 }, //TODO
            { "Show Officer", 0 },
            { "Officer Kill Cooldown", 0 },
            { "Show Engineer", 0 },
            { "Show Joker", 0 },
            { "Joker Can Die To Officer", 0 },
            { "Duration In Which Medic Report Will Contain The Killers Name", 0 },
            { "Duration In Which Medic Report Will Contain The Killers Color Type", 0 }
        };
        //rudimentary array to convert a byte setting from config into true/false
        public static bool[] byteBool =
        {
            false,
            true
        };
        //global rng
        public static System.Random rng = new System.Random();
        //the kill button in the bottom right
        public static MLPJGKEACMM KillButton;
        //the id of the targeted player
        public static int KBTarget;
        //distance between the local player and closest player
        public static double DistLocalClosest;
        //shield indicator sprite (placeholder)
        public static GameObject shieldIndicator = null;
        //renderer for the shield indicator
        public static SpriteRenderer shieldRenderer = null;
        //medic settings and values
        public static class MedicSettings
        {
            public static FFGALNAPKCD Medic;
            public static FFGALNAPKCD Protected;
            public static bool shieldUsed = false;

            public static int medicKillerNameDuration = 0;
            public static int medicKillerColorDuration = 0;
            public static bool showMedic = false;
            public static bool showProtected = false;
            public static bool shieldKillAttemptIndicator = false;
            public static Color medicColor = new Color(36f / 255f, 183f / 255f, 32f / 255f, 1);
            public static Color protectedColor = new Color(0, 1, 1, 1);

            public static void ClearSettings()
            {
                Medic = null;
                Protected = null;
                shieldUsed = false;
            }

            public static void SetConfigSettings()
            {
                showMedic = byteBool[configSettings["Show Medic"]];
                showProtected = byteBool[configSettings["Show Shielded Player"]];
                shieldKillAttemptIndicator = byteBool[configSettings["Murder Attempt Indicator For Shielded Player"]];
                medicKillerNameDuration = configSettings["Duration In Which Medic Report Will Contain The Killers Name"];
                medicKillerColorDuration = configSettings["Duration In Which Medic Report Will Contain The Killers Color Type"];
            }
        }
        //officer settings and values
        public static class OfficerSettings
        {
            public static FFGALNAPKCD Officer;

            public static float OfficerCD = 10f;
            public static bool showOfficer = false;
            public static Color officerColor = new Color(0, 40f / 255f, 198f / 255f, 1);
            public static DateTime? lastKilled = null;
            public static bool firstKill = true;
            public static void ClearSettings()
            {
                Officer = null;
                lastKilled = null;
            }

            public static void SetConfigSettings()
            {
                showOfficer = byteBool[configSettings["Show Officer"]];
                OfficerCD = configSettings["Officer Kill Cooldown"];
            }
        }
        //engineer settings and values
        public static class EngineerSettings
        {
            public static FFGALNAPKCD Engineer;
            public static bool repairUsed = false;

            public static bool showEngineer = false;
            public static Color engineerColor = new Color(151f / 255f, 46f / 255f, 0, 1);

            public static void ClearSettings()
            {
                Engineer = null;
                repairUsed = false;
            }

            public static void SetConfigSettings()
            {
                showEngineer = byteBool[configSettings["Show Engineer"]];
            }
        }

        //joker settings and values
        public static class JokerSettings
        {
            public static FFGALNAPKCD Joker;
            public static bool showJoker = false;
            public static bool jokerCanDieToOfficer = false;
            public static Color jokerColor = new Color(138f / 255f, 138f / 255f, 138f / 255f, 1);

            public static void ClearSettings()
            {
                Joker = null;
            }

            public static void SetConfigSettings()
            {
                showJoker = byteBool[configSettings["Show Joker"]];
                jokerCanDieToOfficer = byteBool[configSettings["Joker Can Die To Officer"]];
            }
        }
        //function called on start of game. write version text on menu
        [HarmonyPatch(typeof(BOCOFLHKCOJ), "Start")]
        public static void Postfix(BOCOFLHKCOJ __instance)
        {
            __instance.text.Text = __instance.text.Text + "   Extra Roles V0.8 Loaded. (https://github.com/NotHunter101/ExtraRolesAmongUs/)";
        }

        //function called when the game starts and impostors are chosen. this is where we choose all the roles and send the packets
        [HarmonyPatch(typeof(FFGALNAPKCD), "RpcSetInfected")]
        public static void Postfix(Il2CppReferenceArray<EGLJNOMOGNP.DCJMABDDJCF> JPGEIBIBJPJ)
        {
            ConsoleTools.Error("SETTING INFECTED! IF YOU AREN'T THE HOST, COPY YOUR CONSOLE AND SEND!");
            MedicSettings.ClearSettings();
            OfficerSettings.ClearSettings();
            EngineerSettings.ClearSettings();
            JokerSettings.ClearSettings();
            MedicSettings.SetConfigSettings();
            OfficerSettings.SetConfigSettings();
            EngineerSettings.SetConfigSettings();
            JokerSettings.SetConfigSettings();
            MessageWriter writer = FMLLKEACGIO.Instance.StartRpcImmediately(FFGALNAPKCD.LocalPlayer.NetId, (byte)CustomRPC.ResetVaribles, Hazel.SendOption.None, -1);
            Console.WriteLine(String.Join(",", configSettings.Values));
            writer.WriteBytesAndSize(configSettings.Values.ToArray<byte>());
            FMLLKEACGIO.Instance.FinishRpcImmediately(writer);

            List<FFGALNAPKCD> crewmates = PlayerTools.getCrewMates(JPGEIBIBJPJ);

            if (crewmates.Count > 0)
            {
                writer = FMLLKEACGIO.Instance.StartRpcImmediately(FFGALNAPKCD.LocalPlayer.NetId, (byte)CustomRPC.SetMedic, Hazel.SendOption.None, -1);
                var MedicRandom = rng.Next(0, crewmates.Count);
                MedicSettings.Medic = crewmates[MedicRandom];
                crewmates.RemoveAt(MedicRandom);
                byte MedicId = MedicSettings.Medic.PlayerId;

                writer.Write(MedicId);
                FMLLKEACGIO.Instance.FinishRpcImmediately(writer);
            }

            if (crewmates.Count > 0)
            {
                writer = FMLLKEACGIO.Instance.StartRpcImmediately(FFGALNAPKCD.LocalPlayer.NetId, (byte)CustomRPC.SetOfficer, Hazel.SendOption.None, -1);

                var OfficerRandom = rng.Next(0, crewmates.Count);
                OfficerSettings.Officer = crewmates[OfficerRandom];
                crewmates.RemoveAt(OfficerRandom);
                byte OfficerId = OfficerSettings.Officer.PlayerId;

                writer.Write(OfficerId);
                FMLLKEACGIO.Instance.FinishRpcImmediately(writer);
            }

            if (crewmates.Count > 0)
            {
                writer = FMLLKEACGIO.Instance.StartRpcImmediately(FFGALNAPKCD.LocalPlayer.NetId, (byte)CustomRPC.SetEngineer, Hazel.SendOption.None, -1);
                var EngineerRandom = rng.Next(0, crewmates.Count);
                EngineerSettings.Engineer = crewmates[EngineerRandom];
                crewmates.RemoveAt(EngineerRandom);
                byte EngineerId = EngineerSettings.Engineer.PlayerId;

                writer.Write(EngineerId);
                FMLLKEACGIO.Instance.FinishRpcImmediately(writer);
            }

            if (crewmates.Count > 0)
            {
                writer = FMLLKEACGIO.Instance.StartRpcImmediately(FFGALNAPKCD.LocalPlayer.NetId, (byte)CustomRPC.SetJoker, Hazel.SendOption.None, -1);
                var JokerRandom = rng.Next(0, crewmates.Count);
                Console.WriteLine(JokerRandom);
                JokerSettings.Joker = crewmates[JokerRandom];
                JokerSettings.Joker.myTasks.Clear();
                crewmates.RemoveAt(JokerRandom);
                byte JokerId = JokerSettings.Joker.PlayerId;

                writer.Write(JokerId);
                FMLLKEACGIO.Instance.FinishRpcImmediately(writer);
            }
        }

        //function that handles all packets from other clients and server. if you need comments to understand this just ask and i'll write them
        //i'll also send the server rpc code if you want
        [HarmonyPatch(typeof(FFGALNAPKCD), "HandleRpc")]
        public static void Postfix(byte HKHMBLJFLMC, MessageReader ALMCIJKELCP)
        {
            try
            {
                switch (HKHMBLJFLMC)
                {
                    case (byte)CustomRPC.ResetVaribles:
                        {
                            var configSettingsValues = ALMCIJKELCP.ReadBytesAndSize().ToArray();
                            for (var i = 0; i < configSettingsValues.Length; i++)
                            {
                                configSettings[configSettingsKeys[i]] = configSettingsValues[i];
                            }
                            Console.WriteLine(String.Join(",", configSettings));
                            MedicSettings.ClearSettings();
                            OfficerSettings.ClearSettings();
                            EngineerSettings.ClearSettings();
                            JokerSettings.ClearSettings();
                            MedicSettings.SetConfigSettings();
                            OfficerSettings.SetConfigSettings();
                            EngineerSettings.SetConfigSettings();
                            JokerSettings.SetConfigSettings();
                            break;
                        }
                    case (byte)CustomRPC.SetMedic:
                        {
                            ConsoleTools.Info("Medic Set Through RPC!");
                            byte MedicId = ALMCIJKELCP.ReadByte();
                            foreach (FFGALNAPKCD player in FFGALNAPKCD.AllPlayerControls)
                            {
                                if (player.PlayerId == MedicId)
                                {
                                    MedicSettings.Medic = player;
                                }
                            }
                            break;
                        }
                    case (byte)CustomRPC.SetProtected:
                        {
                            byte ProtectedId = ALMCIJKELCP.ReadByte();
                            foreach (FFGALNAPKCD player in FFGALNAPKCD.AllPlayerControls)
                            {
                                if (player.PlayerId == ProtectedId)
                                {
                                    MedicSettings.Protected = player;
                                }
                            }
                            break;
                        }
                    case (byte)CustomRPC.MedicDead:
                        {
                            MedicSettings.Protected = null;
                            break;
                        }
                    case (byte)CustomRPC.SetOfficer:
                        {
                            ConsoleTools.Info("Officer Set Through RPC!");
                            byte OfficerId = ALMCIJKELCP.ReadByte();
                            foreach (FFGALNAPKCD player in FFGALNAPKCD.AllPlayerControls)
                            {
                                if (player.PlayerId == OfficerId)
                                {
                                    OfficerSettings.Officer = player;
                                }
                            }
                            break;
                        }
                    case (byte)CustomRPC.OfficerKill:
                        {
                            FFGALNAPKCD killer = PlayerTools.getPlayerById(ALMCIJKELCP.ReadByte());
                            FFGALNAPKCD target = PlayerTools.getPlayerById(ALMCIJKELCP.ReadByte());
                            killer.MurderPlayer(target);
                            break;
                        }
                    case (byte)CustomRPC.SetEngineer:
                        {
                            ConsoleTools.Info("Engineer Set Through RPC!");
                            byte EngineerId = ALMCIJKELCP.ReadByte();
                            foreach (FFGALNAPKCD player in FFGALNAPKCD.AllPlayerControls)
                            {
                                if (player.PlayerId == EngineerId)
                                {
                                    EngineerSettings.Engineer = player;
                                }
                            }
                            break;
                        }
                    case (byte)CustomRPC.SetJoker:
                        {
                            ConsoleTools.Info("Joker Set Through RPC!");
                            byte JokerId = ALMCIJKELCP.ReadByte();
                            foreach (FFGALNAPKCD player in FFGALNAPKCD.AllPlayerControls)
                            {
                                if (player.PlayerId == JokerId)
                                {
                                    JokerSettings.Joker = player;
                                }
                            }
                            break;
                        }
                    case (byte)CustomRPC.MedicReport:
                        {
                            Console.WriteLine("Body Reported RPC!");
                            byte reporterId = ALMCIJKELCP.ReadByte();
                            byte killerId = ALMCIJKELCP.ReadByte();
                            byte deathReason = ALMCIJKELCP.ReadByte();
                            float killAge = ALMCIJKELCP.ReadSingle();
                            if (reporterId == MedicSettings.Medic.PlayerId)
                            {
                                if (MedicSettings.Medic.PlayerId == FFGALNAPKCD.LocalPlayer.PlayerId)
                                {
                                    BodyReport br = new BodyReport();
                                    br.Killer = PlayerTools.getPlayerById(killerId);
                                    br.Reporter = br.Killer = PlayerTools.getPlayerById(killerId);
                                    br.KillAge = killAge;
                                    br.DeathReason = deathReason;
                                    var reportMsg = BodyReport.ParseBodyReport(br);

                                    MessageWriter writer = FMLLKEACGIO.Instance.StartRpcImmediately(FFGALNAPKCD.LocalPlayer.NetId, (byte)CustomRPC.SendMeAMessage, Hazel.SendOption.None, -1);
                                    writer.Write(reportMsg);
                                    FMLLKEACGIO.Instance.FinishRpcImmediately(writer);
                                }
                            }
                            break;
                        }
                    //player exiled
                    case (byte)CustomRPC.JokerWin:
                        {
                            //Console.WriteLine("Joker won!");
                            var exiledId = ALMCIJKELCP.ReadByte();
                            if (JokerSettings.Joker != null)
                            {
                                if (exiledId == JokerSettings.Joker.PlayerId)
                                {
                                    foreach (FFGALNAPKCD player in FFGALNAPKCD.AllPlayerControls)
                                    {
                                        if (player != JokerSettings.Joker)
                                        {
                                            player.RemoveInfected();
                                            player.Die(DBLJKMDLJIF.Exile);
                                            player.NDGFFHMFGIG.DLPCKPBIJOE = true;
                                        }
                                        else
                                        {
                                            player.Revive();
                                            player.NDGFFHMFGIG.DAPKNDBLKIA = true;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case (byte)CustomRPC.MeetingEnded:
                        {
                            OfficerSettings.lastKilled = DateTime.UtcNow;
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                ConsoleTools.Error("Error during handling of RPC packets.");
            }
        }

        //called in the intro function
        [HarmonyPatch(typeof(PENEIDJGGAF.CKACLKCOJFO), "MoveNext")]
        public static void Postfix(PENEIDJGGAF.CKACLKCOJFO __instance)
        {
            //change the name and titles accordingly
            if (FFGALNAPKCD.LocalPlayer == MedicSettings.Medic)
            {
                __instance.field_Public_PENEIDJGGAF_0.Title.Text = "Medic";
                __instance.field_Public_PENEIDJGGAF_0.Title.Color = MedicSettings.medicColor;
                __instance.field_Public_PENEIDJGGAF_0.ImpostorText.Text = "Create a shield to protect a [8DFFFF]Crewmate";
                __instance.field_Public_PENEIDJGGAF_0.BackgroundBar.material.color = MedicSettings.medicColor;
            }
            if (FFGALNAPKCD.LocalPlayer == OfficerSettings.Officer)
            {
                __instance.field_Public_PENEIDJGGAF_0.Title.Text = "Officer";
                __instance.field_Public_PENEIDJGGAF_0.Title.Color = OfficerSettings.officerColor;
                __instance.field_Public_PENEIDJGGAF_0.ImpostorText.Text = "Shoot the [FF0000FF]Impostor";
                __instance.field_Public_PENEIDJGGAF_0.BackgroundBar.material.color = OfficerSettings.officerColor;
            }
            if (FFGALNAPKCD.LocalPlayer == EngineerSettings.Engineer)
            {
                __instance.field_Public_PENEIDJGGAF_0.Title.Text = "Engineer";
                __instance.field_Public_PENEIDJGGAF_0.Title.Color = EngineerSettings.engineerColor;
                __instance.field_Public_PENEIDJGGAF_0.ImpostorText.Text = "Maintain important systems on the ship";
                __instance.field_Public_PENEIDJGGAF_0.BackgroundBar.material.color = EngineerSettings.engineerColor;
            }
            if (FFGALNAPKCD.LocalPlayer == JokerSettings.Joker)
            {
                __instance.field_Public_PENEIDJGGAF_0.Title.Text = "Joker";
                __instance.field_Public_PENEIDJGGAF_0.Title.Color = JokerSettings.jokerColor;
                __instance.field_Public_PENEIDJGGAF_0.ImpostorText.Text = "Get voted off of the ship to win";
                __instance.field_Public_PENEIDJGGAF_0.BackgroundBar.material.color = JokerSettings.jokerColor;
            }
        }


        [HarmonyPatch(typeof(MLPJGKEACMM), "PerformKill")]
        static bool Prefix(MethodBase __originalMethod)
        {
            if (FFGALNAPKCD.LocalPlayer.NDGFFHMFGIG.DLPCKPBIJOE)
                return false; 
            //code that handles the ability button presses
            if (FFGALNAPKCD.LocalPlayer == OfficerSettings.Officer)
            {
                Console.WriteLine("Player is Officer.");
                Console.WriteLine(KBTarget);
                var target = PlayerTools.getPlayerById((byte)KBTarget);
                if (KBTarget != -1 && KBTarget != -2)
                {
                    if (PlayerTools.GetOfficerKD() == 0)
                    {
                        Console.WriteLine("KillButton has defined Target.");
                        //check if they're shielded by medic
                        if (MedicSettings.Protected != null && target.PlayerId == MedicSettings.Protected.PlayerId)
                        {
                            Console.WriteLine("The target is Protected.");
                            //officer suicide packet
                            MessageWriter writer = FMLLKEACGIO.Instance.StartRpcImmediately(FFGALNAPKCD.LocalPlayer.NetId, (byte)CustomRPC.OfficerKill, Hazel.SendOption.None, -1);
                            writer.Write(FFGALNAPKCD.LocalPlayer.PlayerId);
                            writer.Write(FFGALNAPKCD.LocalPlayer.PlayerId);
                            FMLLKEACGIO.Instance.FinishRpcImmediately(writer);
                            FFGALNAPKCD.LocalPlayer.MurderPlayer(FFGALNAPKCD.LocalPlayer);
                            OfficerSettings.lastKilled = DateTime.UtcNow;
                            return false;
                        }
                        //check if they're joker and the setting is configured
                        else if (JokerSettings.jokerCanDieToOfficer && (JokerSettings.Joker != null && target.PlayerId == JokerSettings.Joker.PlayerId))
                        {
                            Console.WriteLine("The target is an Joker. (jokerCanDieToOfficer = 1)");
                            //officer joker murder packet
                            MessageWriter writer = FMLLKEACGIO.Instance.StartRpcImmediately(FFGALNAPKCD.LocalPlayer.NetId, (byte)CustomRPC.OfficerKill, Hazel.SendOption.None, -1);
                            writer.Write(FFGALNAPKCD.LocalPlayer.PlayerId);
                            writer.Write(target.PlayerId);
                            FMLLKEACGIO.Instance.FinishRpcImmediately(writer);
                            FFGALNAPKCD.LocalPlayer.MurderPlayer(target);
                            OfficerSettings.lastKilled = DateTime.UtcNow;
                        }
                        //check if they're an impostor
                        else if (target.NDGFFHMFGIG.DAPKNDBLKIA)
                        {
                            Console.WriteLine("The target is an Impostor.");
                            //officer impostor murder packet
                            MessageWriter writer = FMLLKEACGIO.Instance.StartRpcImmediately(FFGALNAPKCD.LocalPlayer.NetId, (byte)CustomRPC.OfficerKill, Hazel.SendOption.None, -1);
                            writer.Write(FFGALNAPKCD.LocalPlayer.PlayerId);
                            writer.Write(target.PlayerId);
                            FMLLKEACGIO.Instance.FinishRpcImmediately(writer);
                            FFGALNAPKCD.LocalPlayer.MurderPlayer(target);
                            OfficerSettings.lastKilled = DateTime.UtcNow;
                            return false;
                        }
                        //else, they're innocent and not shielded
                        else
                        {
                            Console.WriteLine("The target is Innocent.");
                            //officer suicide packet
                            MessageWriter writer = FMLLKEACGIO.Instance.StartRpcImmediately(FFGALNAPKCD.LocalPlayer.NetId, (byte)CustomRPC.OfficerKill, Hazel.SendOption.None, -1);
                            writer.Write(FFGALNAPKCD.LocalPlayer.PlayerId);
                            writer.Write(FFGALNAPKCD.LocalPlayer.PlayerId);
                            FMLLKEACGIO.Instance.FinishRpcImmediately(writer);
                            FFGALNAPKCD.LocalPlayer.MurderPlayer(FFGALNAPKCD.LocalPlayer);
                            OfficerSettings.lastKilled = DateTime.UtcNow;
                            return false;
                        }
                        return false;
                    }
                    return false;
                }
            }
            //check if they're medic
            else if (FFGALNAPKCD.LocalPlayer == MedicSettings.Medic)
            {
                //if the target is defined
                if (KBTarget != -1 && KBTarget != -2)
                {
                    ConsoleTools.Info("Medic Creating Shield");
                    //create a shield for the targeted player
                    MessageWriter writer = FMLLKEACGIO.Instance.StartRpcImmediately(FFGALNAPKCD.LocalPlayer.NetId, (byte)CustomRPC.SetProtected, Hazel.SendOption.None, -1);
                    MedicSettings.Protected = PlayerTools.closestPlayer;
                    MedicSettings.shieldUsed = true;
                    byte ProtectedId = MedicSettings.Protected.PlayerId;
                    writer.Write(ProtectedId);
                    FMLLKEACGIO.Instance.FinishRpcImmediately(writer);
                    return false;
                }
                return false;
            }
            //check if they're engineer
            else if (FFGALNAPKCD.LocalPlayer == EngineerSettings.Engineer)
            {
                //INFO
                //this code is finished, but not implemented yet. It was working in a previous version, but I rewrote this whole section because of bugs
                if (KBTarget == -2 && EngineerSettings.repairUsed == false)
                {
                    MessageWriter writer = FMLLKEACGIO.Instance.StartRpcImmediately(FFGALNAPKCD.LocalPlayer.NetId, (byte)CustomRPC.RepairAllEmergencies, Hazel.SendOption.None, -1);
                    FMLLKEACGIO.Instance.FinishRpcImmediately(writer);
                    EngineerSettings.repairUsed = true;
                    return false;
                }
                return false;
            }
            //finally, check if the target is protected.
            if (MedicSettings.Protected != null && PlayerTools.closestPlayer.PlayerId == MedicSettings.Protected.PlayerId)
            {
                //cancel the kill
                return false;
            }
            //otherwise, continue the murder as normal
            return true;
        }

        //catch the murder before it is ran
        [HarmonyPatch(typeof(FFGALNAPKCD), "MurderPlayer")]
        public static bool Prefix(FFGALNAPKCD __instance, FFGALNAPKCD CAKODNGLPDF)
        {
            if (OfficerSettings.Officer != null)
            {
                //check if the player is an officer
                if (__instance == OfficerSettings.Officer)
                {
                    OfficerSettings.firstKill = false;
                    //if so, set them to impostor for one frame so they aren't banned for anti-cheat
                    __instance.NDGFFHMFGIG.DAPKNDBLKIA = true;
                }
            }
            return true;
        }

        //handle the murder after it's ran
        [HarmonyPatch(typeof(FFGALNAPKCD), "MurderPlayer")]
        public static void Postfix(FFGALNAPKCD __instance, FFGALNAPKCD CAKODNGLPDF)
        {
            if (MedicSettings.Medic != null)
            {
                if (CAKODNGLPDF.PlayerId == MedicSettings.Medic.PlayerId)
                {
                    //medic was just killed for sure.
                    MessageWriter writer = FMLLKEACGIO.Instance.StartRpcImmediately(FFGALNAPKCD.LocalPlayer.NetId, (byte)CustomRPC.MedicDead, Hazel.SendOption.None, -1);
                    FMLLKEACGIO.Instance.FinishRpcImmediately(writer);
                    //simply set the protected player to null to break the shield
                    MedicSettings.Protected = null;
                }
            }
            if (OfficerSettings.Officer != null)
            {
                //check if killer is officer
                if (__instance == OfficerSettings.Officer)
                {
                    //finally, set them back to normal
                    __instance.NDGFFHMFGIG.DAPKNDBLKIA = false;
                }
            }
        }

        //handle update. this will activate the buttons for the other roles. I can give you comments for this if it's required
        [HarmonyPatch(typeof(PIEFJFEOGOL), "Update")]
        public static void Postfix(PIEFJFEOGOL __instance)
        {
            if (FFGALNAPKCD.AllPlayerControls.Count > 3)
            {
                KillButton = __instance.KillButton;
                PlayerTools.closestPlayer = PlayerTools.getClosestPlayer(FFGALNAPKCD.LocalPlayer);
                DistLocalClosest = PlayerTools.getDistBetweenPlayers(FFGALNAPKCD.LocalPlayer, PlayerTools.closestPlayer);
                KBTarget = -1;

                if (JokerSettings.Joker != null && JokerSettings.Joker.myTasks.Count > 0)
                {
                    while (JokerSettings.Joker.myTasks.Count > 0)
                    {
                        JokerSettings.Joker.RemoveTask(JokerSettings.Joker.myTasks[0]);
                    }
                }
                
                foreach (FFGALNAPKCD player in FFGALNAPKCD.AllPlayerControls)
                {
                    player.nameText.Color = Color.white;
                    player.MKIDFJAEBGH.color = Color.white;
                    //once I find the color id object in PlayerControl, i'll set some sort of dictionary and update it with color id's for all players at the beginning of the game.
                    //then, i'll set it back to default here so I can modify it for the shielded player.
                    //player.SetColor(originalcolor);
                }

                if (FFGALNAPKCD.LocalPlayer.NDGFFHMFGIG.DAPKNDBLKIA)
                {
                    FFGALNAPKCD.LocalPlayer.nameText.Color = Color.red;
                }
                if (MedicSettings.Protected != null)
                {
                    if (MedicSettings.Protected == FFGALNAPKCD.LocalPlayer || MedicSettings.showProtected)
                    {
                        MedicSettings.Protected.nameText.Color = MedicSettings.protectedColor;
                        //this changes the player color and visor color. it's a cool effect to make it obvious a player is shielded.
                        //once again, I have to save the original color id and set it back once the shield is broken, though.
                        //MedicSettings.Protected.SetColor(7);
                        //MedicSettings.Protected.MKIDFJAEBGH.color = MedicSettings.protectedColor;
                    }
                }
                if (MedicSettings.Medic != null)
                {
                    if (MedicSettings.Medic == FFGALNAPKCD.LocalPlayer || MedicSettings.showMedic)
                    {
                        MedicSettings.Medic.nameText.Color = MedicSettings.medicColor;
                    }
                }
                if (OfficerSettings.Officer != null)
                {
                    if (OfficerSettings.Officer == FFGALNAPKCD.LocalPlayer || OfficerSettings.showOfficer)
                    {
                        OfficerSettings.Officer.nameText.Color = OfficerSettings.officerColor;
                    }
                }
                if (EngineerSettings.Engineer != null)
                {
                    if (EngineerSettings.Engineer == FFGALNAPKCD.LocalPlayer || EngineerSettings.showEngineer)
                    {
                        EngineerSettings.Engineer.nameText.Color = EngineerSettings.engineerColor;
                    }
                }
                if (JokerSettings.Joker != null)
                {
                    if (JokerSettings.Joker == FFGALNAPKCD.LocalPlayer || JokerSettings.showJoker)
                    {
                        JokerSettings.Joker.nameText.Color = JokerSettings.jokerColor;
                    }
                }

                if (FFGALNAPKCD.LocalPlayer == MedicSettings.Medic)
                {
                    Texture2D tex = rotateTexture(CustomSpriteArr.shieldImgArr.Reverse().ToArray(), false, 106, 106);
                    tex.Apply(false, false);
                    if (tex != KillButton.renderer.sprite.texture)
                    {
                        KillButton.renderer.sprite = Sprite.Create(tex, new Rect(0, 0, 106, 106), Vector2.one / 2f);
                    }
                    KillButton.gameObject.SetActive(true);
                    KillButton.isActive = true;
                    KillButton.SetCoolDown(0f, FFGALNAPKCD.GameOptions.IGHCIKIDAMO + 15.0f);
                    if (DistLocalClosest < KMOGFLPJLLK.JMLGACIOLIK[FFGALNAPKCD.GameOptions.DLIBONBKPKL] && MedicSettings.shieldUsed == false)
                    {
                        KillButton.SetTarget(PlayerTools.closestPlayer);
                        KBTarget = PlayerTools.closestPlayer.PlayerId;
                    }
                }
                if (FFGALNAPKCD.LocalPlayer == OfficerSettings.Officer)
                {
                    KillButton.gameObject.SetActive(true);
                    KillButton.isActive = true;

                    KillButton.SetCoolDown(PlayerTools.GetOfficerKD(), FFGALNAPKCD.GameOptions.IGHCIKIDAMO + 15.0f);
                    if (DistLocalClosest < KMOGFLPJLLK.JMLGACIOLIK[FFGALNAPKCD.GameOptions.DLIBONBKPKL])
                    {
                        KillButton.SetTarget(PlayerTools.closestPlayer);
                        KBTarget = PlayerTools.closestPlayer.PlayerId;
                    }
                }
                if (FFGALNAPKCD.LocalPlayer == MedicSettings.Protected)
                {
                    //shield indicator
                    //TODO
                }
                if (FFGALNAPKCD.LocalPlayer == EngineerSettings.Engineer)
                {
                    Texture2D tex = rotateTexture(CustomSpriteArr.repairImgArr.Reverse().ToArray(), false, 106, 106);
                    tex.Apply(false, false);
                    if (tex != KillButton.renderer.sprite.texture)
                    {
                        KillButton.renderer.sprite = Sprite.Create(tex, new Rect(0, 0, 106, 106), Vector2.one / 2f);
                    }
                    KillButton.gameObject.SetActive(true);
                    KillButton.isActive = true;
                    KillButton.SetCoolDown(0f, FFGALNAPKCD.GameOptions.IGHCIKIDAMO + 15.0f);
                    var allTasks = FFGALNAPKCD.LocalPlayer.myTasks;
                    if (allTasks.Count > 0)
                    {
                        var lastTaskType = allTasks.ToArray().Last().TaskType;
                        var sabotageActive = false;
                        if (lastTaskType == BOOMIBKNGPP.FixLights || lastTaskType == BOOMIBKNGPP.FixComms || lastTaskType == BOOMIBKNGPP.ResetReactor || lastTaskType == BOOMIBKNGPP.ResetSeismic || lastTaskType == BOOMIBKNGPP.RestoreOxy || lastTaskType == BOOMIBKNGPP.RebootWifi)
                        {
                            sabotageActive = true;
                        }
                        if (EngineerSettings.repairUsed == false && sabotageActive)
                        {
                            KillButton.SetTarget(FFGALNAPKCD.LocalPlayer);
                            KBTarget = -2;
                        }
                    }
                }
                if (FFGALNAPKCD.LocalPlayer.NDGFFHMFGIG.DLPCKPBIJOE)
                {
                    KillButton.gameObject.SetActive(false);
                    KillButton.isActive = false;
                }
                KillButton = __instance.KillButton;
            }

            //function to rotate textures
            Texture2D rotateTexture(Color[] originalTexture, bool clockwise, int w, int h)
            {
                Color[] original = originalTexture;
                Color[] rotated = new Color[original.Length];

                int iRotated, iOriginal;

                for (int j = 0; j < h; ++j)
                {
                    for (int i = 0; i < w; ++i)
                    {
                        iRotated = (i + 1) * h - j - 1;
                        iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                        rotated[iRotated] = original[iOriginal];
                    }
                }

                Texture2D rotatedTexture = new Texture2D(h, w, TextureFormat.ARGB32, true);
                rotatedTexture.SetPixels(rotated);
                rotatedTexture.Apply();
                return rotatedTexture;
            }
        }

        //the code that activates venting for engineers
        [HarmonyPatch]
        public static class VentPatch
        {
            [HarmonyPatch(typeof(OPPMFCFACJB), "CanUse")]
            public static bool Prefix(OPPMFCFACJB __instance, ref float __result, [HarmonyArgument(0)] EGLJNOMOGNP.DCJMABDDJCF pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
            {
                float num = float.MaxValue;
                FFGALNAPKCD localPlayer = pc.LAOEJKHLKAI;
                couldUse = (pc.DAPKNDBLKIA || localPlayer == EngineerSettings.Engineer) && !pc.DLPCKPBIJOE && (localPlayer.GEBLLBHGHLD || localPlayer.inVent);
                canUse = couldUse && (DateTime.UtcNow - PlayerVentTimeExtension.GetLastVent(pc.LAOEJKHLKAI.PlayerId)).TotalMilliseconds > 1000;
                if (canUse)
                {
                    num = Vector2.Distance(localPlayer.GetTruePosition(), __instance.transform.position);
                    canUse &= num <= __instance.ILPBJHPGNBJ;
                }
                __result = num;
                return false;
            }
        }
    }

    //handles a vent exit and sets it in the rate-limiter array
    [HarmonyPatch(typeof(OPPMFCFACJB), "ENCPOOAFILD")]
    public static class VentExitPatch
    {
        public static bool Prefix(FFGALNAPKCD NMEAPOJFNKA)
        {
            Console.WriteLine("ENCPOOAFILD! " + NMEAPOJFNKA.name);
            PlayerVentTimeExtension.SetLastVent(NMEAPOJFNKA.PlayerId);
            return true;
        }
    }

    //handles a vent enter and sets it in the rate-limiter array
    [HarmonyPatch(typeof(OPPMFCFACJB), "JBNFMBNNPJB")]
    public static class VentEnterPatch
    {
        public static bool Prefix(FFGALNAPKCD NMEAPOJFNKA)
        {
            Console.WriteLine("ENTER! " + NMEAPOJFNKA.name);
            PlayerVentTimeExtension.SetLastVent(NMEAPOJFNKA.PlayerId);
            return true;
        }
    }
}