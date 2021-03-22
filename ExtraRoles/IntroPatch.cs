using ExtraRolesMod.Roles;
using HarmonyLib;
using System;


namespace ExtraRolesMod
{
    [HarmonyPatch(typeof(IntroCutscene.CoBegin__d), nameof(IntroCutscene.CoBegin__d.MoveNext))]
    class IntroCutscenePath
    {
        static bool Prefix(IntroCutscene.CoBegin__d __instance)
        {
            if (!PlayerControl.LocalPlayer.IsPlayerRole(Role.Joker))
                return true;

            var jokerTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            jokerTeam.Add(PlayerControl.LocalPlayer);
            __instance.yourTeam = jokerTeam;
            return true;
        }

        static void Postfix(IntroCutscene.CoBegin__d __instance)
        {
            if (PlayerControl.LocalPlayer.IsPlayerRole(Role.Medic))
            {
                __instance.__this.Title.Text = "Medic";
                __instance.__this.Title.Color = Colors.medicColor;
                __instance.__this.ImpostorText.Text = "Create a shield to protect a [8DFFFF]Crewmate";
                __instance.__this.BackgroundBar.material.color = Colors.medicColor;
                return;
            }

            if (PlayerControl.LocalPlayer.IsPlayerRole(Role.Officer))
            {
                __instance.__this.Title.Text = "Officer";
                __instance.__this.Title.Color = Colors.officerColor;
                __instance.__this.ImpostorText.Text = "Shoot the [FF0000FF]Impostor";
                __instance.__this.BackgroundBar.material.color = Colors.officerColor;
                return;
            }

            if (PlayerControl.LocalPlayer.IsPlayerRole(Role.Engineer))
            {
                __instance.__this.Title.Text = "Engineer";
                __instance.__this.Title.Color = Colors.engineerColor;
                __instance.__this.ImpostorText.Text = "Maintain important systems on the ship";
                __instance.__this.BackgroundBar.material.color = Colors.engineerColor;
                return;
            }

            if (PlayerControl.LocalPlayer.IsPlayerRole(Role.Joker))
            {
                __instance.__this.Title.Text = "Joker";
                __instance.__this.Title.Color = Colors.jokerColor;
                __instance.__this.ImpostorText.Text = "Get voted off of the ship to win";
                __instance.__this.BackgroundBar.material.color = Colors.jokerColor;
            }
        }
    }
}