using ExtraRoles.Medic;
using ExtraRoles.Officer;
using ExtraRoles.Rpc;
using ExtraRolesMod;
using HarmonyLib;
using Hazel;
using Reactor;
using System;
using UnityEngine;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRoles.Roles.Engineer
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
