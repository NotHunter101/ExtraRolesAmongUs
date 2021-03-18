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

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
    class EngineerMapUpdate
    {
        static void Postfix(MapBehaviour __instance)
        {
            if (!PlayerControl.LocalPlayer.isPlayerRole(Role.Engineer))
                return;
            if (!__instance.IsOpen || !__instance.infectedOverlay.gameObject.active)
                return;
            __instance.ColorControl.baseColor =
                !Main.Logic.sabotageActive ? Color.gray : Main.Palette.engineerColor;

            var perc = Main.Logic.getRolePlayer(Role.Engineer).UsedAbility ? 1f : 0f;

            foreach (var room in __instance.infectedOverlay.rooms)
            {
                if (room.special == null)
                    continue;
                room.special.material.SetFloat("_Desat", !Main.Logic.sabotageActive ? 1f : 0f);

                room.special.enabled = true;
                room.special.gameObject.SetActive(true);
                room.special.gameObject.active = true;
                room.special.material.SetFloat("_Percent", !PlayerControl.LocalPlayer.Data.IsDead ? perc : 1f);
            }
        }
    }

}
