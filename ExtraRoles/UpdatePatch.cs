using ExtraRolesMod.Roles;
using HarmonyLib;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;


namespace ExtraRolesMod
{

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudUpdateManager
    {


        static void Postfix()
        {
            if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
                return;

            ExtraRoles.Logic.ClearJokerTasks();
            UpdateIsSabotageActive();
            UpdatePlayerNameColors();
        }

        private static void UpdatePlayerNameColors()
        {
            // TODO: this list could maybe find a better place?
            //       It is only meant for looping through role "name", "color" and "show" simultaneously
            var roles = new List<(Role roleName, Color roleColor)>()
            {
                (Role.Medic, Colors.medicColor),
                (Role.Officer, Colors.officerColor),
                (Role.Engineer, Colors.engineerColor),
                (Role.Joker, Colors.jokerColor),
            };
            // Color of imposters and crewmates
            foreach (var player in PlayerControl.AllPlayerControls)
                player.nameText.Color = player.Data.IsImpostor && (PlayerControl.LocalPlayer.Data.IsImpostor || PlayerControl.LocalPlayer.Data.IsDead)
                    ? Color.red
                    : Color.white;

            // Color of roles (always see yourself, and depending on setting, others may see the role too)
            foreach (var (roleName, roleColor) in roles)
            {
                var role = ExtraRoles.Logic.getRolePlayer(roleName);
                if (role == null)
                    continue;
                if (PlayerControl.LocalPlayer.IsPlayerRole(roleName) || PlayerControl.LocalPlayer.Data.IsDead)
                    role.PlayerControl.nameText.Color = roleColor;
            }
            //Color of name plates in the voting hub should be the same as in-game
            if (MeetingHud.Instance != null)
            {
                foreach (var playerVoteArea in MeetingHud.Instance.playerStates)
                {
                    if (playerVoteArea.NameText == null)
                        continue;

                    var player = PlayerTools.GetPlayerById((byte)playerVoteArea.TargetPlayerId);
                    playerVoteArea.NameText.Color = player.nameText.Color;
                }
            }
        }

        private static void UpdateIsSabotageActive()
        {
            var sabotageActive = false;
            foreach (var task in PlayerControl.LocalPlayer.myTasks)
                if (PlayerTools.sabotageTasks.Contains(task.TaskType))
                    sabotageActive = true;
            ExtraRoles.Logic.sabotageActive = sabotageActive;
        }
    }
}