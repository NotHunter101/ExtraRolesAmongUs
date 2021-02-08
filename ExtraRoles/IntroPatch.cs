using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod
{
    [HarmonyPatch(typeof(IntroCutscene.CoBegin__d), nameof(IntroCutscene.CoBegin__d.MoveNext))]
    class IntroCutscenePath
    {
        static bool Prefix(IntroCutscene.CoBegin__d __instance)
        {
            if (PlayerControl.LocalPlayer == JokerSettings.Joker)
            {
                var jokerTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                jokerTeam.Add(PlayerControl.LocalPlayer);
                __instance.yourTeam = jokerTeam;
                return true;
            }
            return true;
        }

        static void Postfix(IntroCutscene.CoBegin__d __instance)
        {
            OfficerSettings.lastKilled = DateTime.UtcNow.AddSeconds((OfficerSettings.OfficerCD * -1) + 10 + __instance.timer_0);
            //change the name and titles accordingly
            if (PlayerControl.LocalPlayer == MedicSettings.Medic)
            {
                __instance.__this.Title.Text = "Medic";
                __instance.__this.Title.Color = ModdedPalette.medicColor;
                __instance.__this.ImpostorText.Text = "Create a shield to protect a [8DFFFF]Crewmate";
                __instance.__this.BackgroundBar.material.color = ModdedPalette.medicColor;
            }
            if (PlayerControl.LocalPlayer == OfficerSettings.Officer)
            {
                __instance.__this.Title.Text = "Officer";
                __instance.__this.Title.Color = ModdedPalette.officerColor;
                __instance.__this.ImpostorText.Text = "Shoot the [FF0000FF]Impostor";
                __instance.__this.BackgroundBar.material.color = ModdedPalette.officerColor;
            }
            if (PlayerControl.LocalPlayer == EngineerSettings.Engineer)
            {
                __instance.__this.Title.Text = "Engineer";
                __instance.__this.Title.Color = ModdedPalette.engineerColor;
                __instance.__this.ImpostorText.Text = "Maintain important systems on the ship";
                __instance.__this.BackgroundBar.material.color = ModdedPalette.engineerColor;
            }
            if (PlayerControl.LocalPlayer == JokerSettings.Joker)
            {
                __instance.__this.Title.Text = "Joker";
                __instance.__this.Title.Color = ModdedPalette.jokerColor;
                __instance.__this.ImpostorText.Text = "Get voted off of the ship to win";
                __instance.__this.BackgroundBar.material.color = ModdedPalette.jokerColor;
            }
        }
    }
}
