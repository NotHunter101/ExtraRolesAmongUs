﻿using HarmonyLib;


namespace ExtraRolesMod.Utility
{
    //function called on start of game. write version text on menu
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public static class VersionStartPatch
    {
        static void Postfix(VersionShower __instance)
        {
            __instance.text.Text = __instance.text.Text + "   Extra Roles " + ExtraRoles.Version + " Loaded. (http://www.extraroles.net/)";
        }
    }
}
