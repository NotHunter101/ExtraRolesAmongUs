using ExtraRolesMod.Roles;
using HarmonyLib;
using System;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod
{
    [HarmonyPatch(typeof(IntroCutscene.CoBegin__d), nameof(IntroCutscene.CoBegin__d.MoveNext))]
    class IntroCutscenePath
    {
        static bool Prefix(IntroCutscene.CoBegin__d __instance)
        {
            if (!PlayerControl.LocalPlayer.isPlayerRole(Role.Joker))
                return true;

            var jokerTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            jokerTeam.Add(PlayerControl.LocalPlayer);
            __instance.yourTeam = jokerTeam;
            return true;
        }

        static void Postfix(IntroCutscene.CoBegin__d __instance)
        {
            var officer = Main.Logic.getRolePlayer(Role.Officer);
            if (officer != null)
                officer.LastAbilityTime = DateTime.UtcNow;

            if (PlayerControl.LocalPlayer.isPlayerRole(Role.Medic))
            {
                __instance.__this.Title.Text = "Medic";
                __instance.__this.Title.Color = Main.Palette.medicColor;
                __instance.__this.ImpostorText.Text = "Create a shield to protect a [8DFFFF]Crewmate";
                __instance.__this.BackgroundBar.material.color = Main.Palette.medicColor;
                return;
            }

            if (PlayerControl.LocalPlayer.isPlayerRole(Role.Officer))
            {
                __instance.__this.Title.Text = "Officer";
                __instance.__this.Title.Color = Main.Palette.officerColor;
                __instance.__this.ImpostorText.Text = "Shoot the [FF0000FF]Impostor";
                __instance.__this.BackgroundBar.material.color = Main.Palette.officerColor;
                return;
            }

            if (PlayerControl.LocalPlayer.isPlayerRole(Role.Engineer))
            {
                __instance.__this.Title.Text = "Engineer";
                __instance.__this.Title.Color = Main.Palette.engineerColor;
                __instance.__this.ImpostorText.Text = "Maintain important systems on the ship";
                __instance.__this.BackgroundBar.material.color = Main.Palette.engineerColor;
                return;
            }

            if (PlayerControl.LocalPlayer.isPlayerRole(Role.Joker))
            {
                __instance.__this.Title.Text = "Joker";
                __instance.__this.Title.Color = Main.Palette.jokerColor;
                __instance.__this.ImpostorText.Text = "Get voted off of the ship to win";
                __instance.__this.BackgroundBar.material.color = Main.Palette.jokerColor;
            }
        }
    }
}