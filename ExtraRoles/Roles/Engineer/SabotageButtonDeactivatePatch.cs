using ExtraRolesMod.Medic;
using ExtraRolesMod.Officer;
using ExtraRolesMod.Rpc;
using ExtraRolesMod;
using HarmonyLib;
using Hazel;
using Reactor;
using System;
using UnityEngine;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod.Roles.Engineer
{
    [HarmonyPatch(typeof(MapRoom), nameof(MapRoom.Method_0))]
    class SabotageButtonDeactivatePatch
    {
        static bool Prefix(MapRoom __instance)
        {
            return !PlayerControl.LocalPlayer.isPlayerRole(Role.Engineer);
        }
    }
}
