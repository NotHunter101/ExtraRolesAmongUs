using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.IL2CPP.UnityEngine;
using HarmonyLib;
using Hazel;
using Il2CppDumper;
using InnerNet;
using Steamworks;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using System.IO;
using static ExtraRolesMod.MainHooks;
using Reactor;
using ExtraRolesMod;
using Essentials;
using Essentials.CustomOptions;

namespace ExtraRolesMod
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class HarmonyMain : BasePlugin
    {
        public const string Id = "gg.reactor.extraroles";

        public Harmony Harmony { get; } = new Harmony(Id);
        public ConfigEntry<string> Ip { get; set; }
        public ConfigEntry<ushort> Port { get; set; }

        //This section uses the https://github.com/DorCoMaNdO/Reactor-Essentials framework, but I disabled the watermark.
        //The code said that you were allowed, as long as you provided credit elsewhere. 
        //I added a link in the Credits of the GitHub page, and I'm also mentioning it here.
        //If the owner of this library has any problems with this, just message me on discord and we'll find a solution

        //Hunter101#1337

        public static CustomToggleOption showMedic = CustomOption.AddToggle("Show Medic", false);
        public static CustomToggleOption showShieldedPlayer = CustomOption.AddToggle("Show Shielded Player", true);
        public static CustomToggleOption playerMurderIndicator = CustomOption.AddToggle("Murder Attempt Indicator for Shielded Player", true);
        public static CustomToggleOption showOfficer = CustomOption.AddToggle("Show Officer", false);
        public static CustomNumberOption OfficerKillCooldown = CustomOption.AddNumber("Officer Kill Cooldown", 30f, 10f, 60f, 2.5f);
        public static CustomToggleOption showEngineer = CustomOption.AddToggle("Show Engineer", false);
        public static CustomToggleOption showJoker = CustomOption.AddToggle("Show Joker", false);
        public static CustomToggleOption jokerCanDieToOfficer = CustomOption.AddToggle("Joker Can Die To Officer", true);
        public static CustomNumberOption medicReportNameDuration = CustomOption.AddNumber("Time Where Medic Reports Will Have Name", 5, 0, 60, 2.5f);
        public static CustomNumberOption medicReportColorDuration = CustomOption.AddNumber("Time Where Medic Reports Will Have Color Type", 20, 0, 120, 2.5f);
        public static CustomNumberOption medicSpawnChance = CustomOption.AddNumber("Medic Spawn Chance", 100, 1, 100, 5);
        public static CustomNumberOption officerSpawnChance = CustomOption.AddNumber("Officer Spawn Chance", 100, 1, 100, 5);
        public static CustomNumberOption engineerSpawnChance = CustomOption.AddNumber("Engineer Spawn Chance", 100, 1, 100, 5);
        public static CustomNumberOption jokerSpawnChance = CustomOption.AddNumber("Joker Spawn Chance", 100, 1, 100, 5);

        public override void Load()
        {
            System.Console.WriteLine(Path.GetFileName(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\..\\LocalLow\\InnerSloth\\Among Us\\regionInfo.dat") );
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\..\\LocalLow\\InnerSloth\\Among Us\\regionInfo.dat"))
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\..\\LocalLow\\InnerSloth\\Among Us\\regionInfo.dat");

            Ip = Config.Bind("Custom Server", "IP", "24.57.85.224");
            Port = Config.Bind("Custom Server", "Port", (ushort)22023);

            var defaultRegions = ServerManager.DefaultRegions.ToList();
            var ip = Ip.Value;
            if (Uri.CheckHostName(Ip.Value).ToString() == "Dns")
            {
                System.Console.WriteLine("Resolving " + ip + " ...");
                try
                {
                    foreach (IPAddress address in Dns.GetHostAddresses(Ip.Value))
                    {
                        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            ip = address.ToString(); break;
                        }
                    }
                }
                catch
                {
                    ConsoleTools.Error("Hostname could not be resolved" + ip);
                }
                ConsoleTools.Info("IP is " + ip);
            }


            var port = Port.Value;


            var region = new RegionInfo(
                "Custom", ip, new[]
                {
                    new ServerInfo($"Custom-Master-1", ip, port)
                });

            defaultRegions.Clear();
            defaultRegions.Insert(0, region);

            ServerManager.DefaultRegions = defaultRegions.ToArray();

            ServerManager.Instance.ReselectRegion();
            ServerManager.Instance.CurrentRegion = region;
            Harmony.PatchAll();
        }
    }
}
