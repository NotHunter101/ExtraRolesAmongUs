using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using ExtraRolesMod.Roles.Medic;

namespace ExtraRolesMod
{
    [HarmonyPatch]
    public class ExtraRoles
    {
        public static Assets Assets = new Assets();
        public static ModdedConfig Config = new ModdedConfig();
        public static ModdedLogic Logic = new ModdedLogic();

        public static List<DeadPlayer> KilledPlayers = new List<DeadPlayer>();
        public static System.Random rng = new System.Random();
        public const string Version = "v1.4.3b";

    }
}