using ExtraRolesMod.Medic;
using ExtraRolesMod.Roles;
using HarmonyLib;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod
{
    enum ShieldOptions
    {
        Self = 0,
        Medic = 1,
        SelfAndMedic = 2,
        Everyone = 3,
    }

    [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.Method_5))]
    class GameOptionsData_ToHudString
    {
        static void Postfix()
        {
            HudManager.Instance.GameSettings.scale = 0.5f;
        }
    }

    //This is a class that sends a ping to my public api so people can see a player counter. Go to http://computable.us:5001/api/playercount to view the people currently playing.
    //No sensitive information is logged, viewed, or used in any way.
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.Update))]
    class GameUpdate
    {
        static readonly HttpClient client = new HttpClient();
        static DateTime? lastGuid = null;
        static Guid clientGuid = Guid.NewGuid();

        static void Postfix()
        {
            lastGuid ??= DateTime.UtcNow.AddSeconds(-20);

            if (lastGuid.Value.AddSeconds(20).Ticks >= DateTime.UtcNow.Ticks) 
                return;
            
            client.PostAsync("http://computable.us:5001/api/ping?guid=" + clientGuid, null);
            lastGuid = DateTime.UtcNow;
        }
    }

    [HarmonyPatch]
    class GameOptionsMenuManger
    {
        static float defaultBounds = 0f;

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        class Start
        {
            static void Postfix(ref GameOptionsMenu __instance)
            {
                defaultBounds = __instance.GetComponentInParent<Scroller>().YBounds.max;
            }
        }

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
        class Update
        {
            static void Postfix(ref GameOptionsMenu __instance)
            {
                __instance.GetComponentInParent<Scroller>().YBounds.max = 13.5f;
            }
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudUpdateManager
    {
        static bool defaultSet = false;
        static int currentColor = 0;
        static Color newColor;
        static Color nextColor;

        static Color[] colors =
        {
            Color.red, new Color(255f / 255f, 94f / 255f, 19f / 255f), Color.yellow, Color.green, Color.blue,
            new Color(120f / 255f, 7f / 255f, 188f / 255f)
        };

        static void Postfix(HudManager __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) 
                return;
            
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.PlayerName != "Hunter") continue;
                
                if (!defaultSet)
                {
                    System.Console.Write(currentColor);
                    defaultSet = true;
                    player.myRend.material.SetColor("_BackColor", colors[currentColor]);
                    player.myRend.material.SetColor("_BodyColor", colors[currentColor]);
                    newColor = colors[currentColor];
                    if (currentColor + 1 >= colors.Length)
                        currentColor = -1;
                    nextColor = colors[currentColor + 1];
                }

                newColor = VecToColor(Vector3.MoveTowards(ColorToVec(newColor), ColorToVec(nextColor), 0.02f));
                player.myRend.material.SetColor("_BackColor", newColor);
                player.myRend.material.SetColor("_BodyColor", newColor);
                
                if (newColor != nextColor) 
                    continue;
                
                currentColor++;
                defaultSet = false;
            }

            KillButton = __instance.KillButton;

            Main.Logic.clearJokerTasks();
       
            if (rend != null)
                rend.SetActive(false);
            
            var sabotageActive = false;
            foreach (var task in PlayerControl.LocalPlayer.myTasks)
                if (PlayerTools.sabotageTasks.Contains(task.TaskType))
                    sabotageActive = true;
            Main.Logic.sabotageActive = sabotageActive;
            
            if (Main.Logic.getImmortalPlayer() != null && Main.Logic.getImmortalPlayer().PlayerControl.Data.IsDead)
                BreakShield(true);
            if (Main.Logic.getImmortalPlayer() != null && Main.Logic.getRolePlayer(Role.Medic) != null &&
                Main.Logic.getRolePlayer(Role.Medic).PlayerControl.Data.IsDead)
                BreakShield(true);
            if (Main.Logic.getRolePlayer(Role.Medic) == null && Main.Logic.getImmortalPlayer() != null)
                BreakShield(true);

            // TODO: this list could maybe find a better place?
            //       It is only meant for looping through role "name", "color" and "show" simultaneously
            var roles = new List<(Role roleName, Color roleColor, bool showRole)>()
            {
                (Role.Medic, Main.Palette.medicColor, Main.Config.showMedic),
                (Role.Officer, Main.Palette.officerColor, Main.Config.showOfficer),
                (Role.Engineer, Main.Palette.engineerColor, Main.Config.showEngineer),
                (Role.Joker, Main.Palette.jokerColor, Main.Config.showJoker),
            };

            // Color of imposters and crewmates
            foreach (var player in PlayerControl.AllPlayerControls)
                player.nameText.Color = player.Data.IsImpostor && (PlayerControl.LocalPlayer.Data.IsImpostor || PlayerControl.LocalPlayer.Data.IsDead)
                    ? Color.red
                    : Color.white;

            // Color of roles (always see yourself, and depending on setting, others may see the role too)
            foreach (var (roleName, roleColor, showRole) in roles)
            {
                var role = Main.Logic.getRolePlayer(roleName);
                if (role == null)
                    continue;
                if (PlayerControl.LocalPlayer.isPlayerRole(roleName) || showRole || PlayerControl.LocalPlayer.Data.IsDead)
                    role.PlayerControl.nameText.Color = roleColor;
            }

            //Color of name plates in the voting hub should be the same as in-game
            if (MeetingHud.Instance != null)
            {
                foreach (var playerVoteArea in MeetingHud.Instance.playerStates)
                {
                    if (playerVoteArea.NameText == null) 
                        continue;

                    var player = PlayerTools.getPlayerById((byte)playerVoteArea.TargetPlayerId);
                    playerVoteArea.NameText.Color = player.nameText.Color;
                }
            }

            if (Main.Logic.anyPlayerImmortal())
            {
                var showShielded = Main.Config.showProtected;
                var shieldedPlayer = Main.Logic.getImmortalPlayer().PlayerControl;
                if (showShielded == (int) ShieldOptions.Everyone)
                {
                    GiveShieldedPlayerShield(shieldedPlayer);
                }
                else if (PlayerControl.LocalPlayer.isPlayerImmortal() && (showShielded == (int) ShieldOptions.Self || showShielded == (int) ShieldOptions.SelfAndMedic))
                {
                   
                    GiveShieldedPlayerShield(shieldedPlayer);

                }
                else if (PlayerControl.LocalPlayer.isPlayerRole(Role.Medic) &&
                         (showShielded == (int) ShieldOptions.Medic || showShielded == (int) ShieldOptions.SelfAndMedic))
                {
                    GiveShieldedPlayerShield(shieldedPlayer);
                }
            }
        }

        private static void GiveShieldedPlayerShield(PlayerControl shieldedPlayer)
        {
            if (shieldedPlayer.getModdedControl().Immortal == ShieldState.Broken)
            {
                shieldedPlayer.myRend.material.SetColor("_VisorColor", Color.white);
                shieldedPlayer.myRend.material.SetFloat("_Outline", 1f);
                shieldedPlayer.myRend.material.SetColor("_OutlineColor", Color.white);
            }
            else
            {
                shieldedPlayer.myRend.material.SetColor("_VisorColor", Main.Palette.protectedColor);
                shieldedPlayer.myRend.material.SetFloat("_Outline", 1f);
                shieldedPlayer.myRend.material.SetColor("_OutlineColor", Main.Palette.protectedColor);
            }
        }
    }
}