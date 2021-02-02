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

        public override void Load()
        {
            Ip = Config.Bind("Custom Server", "IP", "24.57.85.224");
            Port = Config.Bind("Custom Server", "Port", (ushort)22023);

            /* "Show Medic",
            "Show Shielded Player",
            "Murder Attempt Indicator For Shielded Player",
            "Show Officer",
            "Officer Kill Cooldown",
            "Show Engineer",
            "Engineer Repair Cooldown",
            "Show Joker",
            "Joker Can Die To Officer",
            "Duration In Which Medic Report Will Contain The Killer's Name",
            "Duration In Which Medic Report Will Contain The Killer's Color Type" */

            configSettings["Show Medic"] = (byte)Config.Bind("Game Options", "Show Medic", 0).Value;
            configSettings["Show Shielded Player"] = (byte)Config.Bind("Game Options", "Show Shielded Player", 1).Value;
            configSettings["Murder Attempt Indicator For Shielded Player"] = (byte)Config.Bind("Game Options", "Murder Attempt Indicator For Shielded Player", 1).Value;
            configSettings["Show Officer"] = (byte)Config.Bind("Game Options", "Show Officer", 0).Value;
            configSettings["Officer Kill Cooldown"] = (byte)Config.Bind("Game Options", "Officer Kill Cooldown", 30).Value;
            configSettings["Show Engineer"] = (byte)Config.Bind("Game Options", "Show Engineer", 0).Value;
            configSettings["Show Joker"] = (byte)Config.Bind("Game Options", "Show Joker", 0).Value;
            configSettings["Joker Can Die To Officer"] = (byte)Config.Bind("Game Options", "Joker Can Die To Officer", 1).Value;
            configSettings["Duration In Which Medic Report Will Contain The Killers Name"] = (byte)Config.Bind("Game Options", "Duration In Which Medic Report Will Contain The Killers Name", 5).Value;
            configSettings["Duration In Which Medic Report Will Contain The Killers Color Type"] = (byte)Config.Bind("Game Options", "Duration In Which Medic Report Will Contain The Killers Color Type", 20).Value;
            configSettings["Medic Spawn Chance"] = (byte)Config.Bind("Game Options", "Medic Spawn Chance", 100).Value;
            configSettings["Officer Spawn Chance"] = (byte)Config.Bind("Game Options", "Officer Spawn Chance", 100).Value;
            configSettings["Engineer Spawn Chance"] = (byte)Config.Bind("Game Options", "Engineer Spawn Chance", 100).Value;
            configSettings["Joker Spawn Chance"] = (byte)Config.Bind("Game Options", "Joker Spawn Chance", 100).Value;
            var defaultRegions = AOBNFCIHAJL.DefaultRegions.ToList();
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

            defaultRegions.Clear();
            defaultRegions.Insert(0, new OIBMKGDLGOG(
                "Custom", ip, new[]
                {
                    new PLFDMKKDEMI($"Custom-Master-1", ip, port)
                })
            );

            AOBNFCIHAJL.DefaultRegions = defaultRegions.ToArray();
            Harmony.PatchAll();
        }
    }
}
