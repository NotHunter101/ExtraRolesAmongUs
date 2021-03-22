using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using System;
using System.Linq;
using System.Net;
using Reactor;
using Essentials.Options;

using Reactor.Unstrip;
using UnityEngine;
using System.IO;
using Reactor.Extensions;
using System.Collections.Generic;
using System.Net.Sockets;

namespace ExtraRolesMod
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public partial class ExtraRolesPlugin : BasePlugin
    {
        public const string Id = "net.extraroles";

        public Harmony Harmony { get; } = new Harmony(Id);

        //This section uses the https://github.com/DorCoMaNdO/Reactor-Essentials framework

        public static CustomStringOption showShieldedPlayer = CustomOption.AddString("Show Shielded Player",
            new[] {"Self", "Medic", "Self+Medic", "Everyone"});

        public static CustomNumberOption OfficerKillCooldown =
            CustomOption.AddNumber("Officer Kill Cooldown", 30f, 10f, 60f, 2.5f);

        public static CustomStringOption
            officerKillBehaviour = CustomOption.AddString("Officer Kill Behaviour", new[] { "Impostor", "Joker", "Crew Die", "Anyone" });

        public static CustomToggleOption officerShouldDieToShieldedPlayers =
            CustomOption.AddToggle("Officer Dies When Attacking Shielded Players", true);


        public static CustomToggleOption playerMurderIndicator =
            CustomOption.AddToggle("Murder Attempt Indicator for Shielded Player", false);

        public static CustomToggleOption medicReportSwitch = CustomOption.AddToggle("Show Medic Reports", true);

        public static CustomNumberOption medicReportNameDuration =
            CustomOption.AddNumber("Time Where Medic Reports Will Have Name", 0, 0, 60, 2.5f);

        public static CustomNumberOption medicReportColorDuration =
            CustomOption.AddNumber("Time Where Medic Reports Will Have Color Type", 15, 0, 120, 2.5f);

        public static CustomNumberOption
            medicSpawnChance = CustomOption.AddNumber("Medic Spawn Chance", 100, 0, 100, 5);

        public static CustomNumberOption officerSpawnChance =
            CustomOption.AddNumber("Officer Spawn Chance", 100, 0, 100, 5);

        public static CustomNumberOption engineerSpawnChance =
            CustomOption.AddNumber("Engineer Spawn Chance", 100, 0, 100, 5);

        public static CustomNumberOption
            jokerSpawnChance = CustomOption.AddNumber("Joker Spawn Chance", 100, 0, 100, 5);

        public override void Load()
        {
            ExtraRoles.Assets.bundle = AssetBundle.LoadFromFile(Directory.GetCurrentDirectory() + "\\Assets\\bundle");
            ExtraRoles.Assets.breakClip = ExtraRoles.Assets.bundle.LoadAsset<AudioClip>("SB").DontUnload();
            ExtraRoles.Assets.repairIco = ExtraRoles.Assets.bundle.LoadAsset<Sprite>("RE").DontUnload();
            ExtraRoles.Assets.shieldIco = ExtraRoles.Assets.bundle.LoadAsset<Sprite>("SA").DontUnload();
            ExtraRoles.Assets.smallShieldIco = ExtraRoles.Assets.bundle.LoadAsset<Sprite>("RESmall").DontUnload();

            //Disable the https://github.com/DorCoMaNdO/Reactor-Essentials watermark.
            //The code said that you were allowed, as long as you provided credit elsewhere. 
            //I added a link in the Credits of the GitHub page, and I'm also mentioning it here.
            //If the owner of this library has any problems with this, just message me on discord and we'll find a solution

            //Hunter101#1337
            CustomOption.ShamelessPlug = false;


            RegisterInIl2CppAttribute.Register();
            RegisterCustomRpcAttribute.Register(this);

            Roles.Engineer.EngineerRepairButton.AddEngineerButton();
            Roles.Officer.OfficerKillButton.AddOfficerKillButton();
            Roles.Medic.MedicShieldButton.AddMedicShieldButton();

            Harmony.PatchAll();
        }
    }
}