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
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowInfectedMap))]
    class EngineerMapOpen
    {
        static void Postfix(MapBehaviour __instance)
        {
            if (!PlayerControl.LocalPlayer.isPlayerRole(Role.Engineer))
                return;
            if (!__instance.IsOpen)
                return;

            __instance.ColorControl.baseColor = Main.Palette.engineerColor;
            foreach (var room in __instance.infectedOverlay.rooms)
            {
                if (room.door == null)
                    continue;

                room.door.enabled = false;
                room.door.gameObject.SetActive(false);
                room.door.gameObject.active = false;
            }
        }
    }
}
