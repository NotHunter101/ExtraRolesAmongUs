using ExtraRoles.Medic;
using HarmonyLib;
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
    
    [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.Method_24))]
    class GameOptionsData_ToHudString
    {
        static void Postfix(ref string __result)
        {
            DestroyableSingleton<HudManager>.Instance.GameSettings.scale = 0.5f;
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
                __instance.GetComponentInParent<Scroller>().YBounds.max = 15.5f;
            }
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudUpdateManager
    {
        static bool defaultSet = false;
        static bool lastQ = false;
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

            lastQ = Input.GetKeyUp(KeyCode.Q);
            KillButton = __instance.KillButton;
            PlayerTools.closestPlayer = PlayerTools.getClosestPlayer(PlayerControl.LocalPlayer);
            if (PlayerTools.closestPlayer != null && PlayerControl.LocalPlayer != null)
                DistLocalClosest =
                    PlayerTools.getDistBetweenPlayers(PlayerControl.LocalPlayer, PlayerTools.closestPlayer);
            if (!PlayerControl.LocalPlayer.Data.IsImpostor && Input.GetKeyDown(KeyCode.Q) && !lastQ &&
                __instance.UseButton.isActiveAndEnabled)
                PerformKillPatch.Prefix();
            if (PlayerControl.LocalPlayer.isPlayerRole("Engineer") && __instance.UseButton.isActiveAndEnabled)
            {
                KillButton.gameObject.SetActive(true);
                KillButton.isActive = true;
                KillButton.SetCoolDown(0f, 1f);
                KillButton.renderer.sprite = Main.Assets.repairIco;
                KillButton.renderer.color = Palette.EnabledColor;
                KillButton.renderer.material.SetFloat("_Desat", 0f);
            }

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
            if (Main.Logic.getImmortalPlayer() != null && Main.Logic.getRolePlayer("Medic") != null &&
                Main.Logic.getRolePlayer("Medic").PlayerControl.Data.IsDead)
                BreakShield(true);
            if (Main.Logic.getRolePlayer("Medic") == null && Main.Logic.getImmortalPlayer() != null)
                BreakShield(true);

            // TODO: this list could maybe find a better place?
            //       It is only meant for looping through role "name", "color" and "show" simultaneously
            var roles = new List<(string roleName, Color roleColor, bool showRole)>()
            {
                ("Medic", Main.Palette.medicColor, Main.Config.showMedic),
                ("Officer", Main.Palette.officerColor, Main.Config.showOfficer),
                ("Engineer", Main.Palette.engineerColor, Main.Config.showEngineer),
                ("Joker", Main.Palette.jokerColor, Main.Config.showJoker),
            };

            // Color of imposters and crewmates
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                bool iAmImposter = PlayerControl.LocalPlayer.Data.IsImpostor;
                bool iAmJoker = PlayerControl.LocalPlayer.isPlayerRole("Joker");
                player.nameText.Color = player.Data.IsImpostor && (iAmImposter || iAmJoker && Main.Config.jokerCanSeeImpostors) ? Color.red : Color.white;
            }

            // Color of roles (always see yourself, and depending on setting, others may see the role too)
            foreach (var (roleName, roleColor, showRole) in roles)
            {
                var role = Main.Logic.getRolePlayer(roleName);
                if (role == null)
                    continue;
                if (PlayerControl.LocalPlayer.isPlayerRole(roleName) ||
                    showRole ||
                    Main.Config.jokerCanSeeRoles && PlayerControl.LocalPlayer.isPlayerRole("Joker")))
                    role.PlayerControl.nameText.Color = roleColor;
            }

            //Color of name plates in the voting hub should be the same as in-game
            foreach (var player in PlayerControl.AllPlayerControls)
                if (MeetingHud.Instance != null)
                    foreach (var playerVoteArea in MeetingHud.Instance.playerStates)
                        if (playerVoteArea.NameText != null && player.PlayerId == playerVoteArea.TargetPlayerId)
                            playerVoteArea.NameText.Color = player.nameText.Color;

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
                else if (PlayerControl.LocalPlayer.isPlayerRole("Medic") &&
                         (showShielded == (int) ShieldOptions.Medic || showShielded == (int) ShieldOptions.SelfAndMedic))
                {
                    GiveShieldedPlayerShield(shieldedPlayer);
                }
            }

            if (PlayerControl.LocalPlayer.Data.IsDead)
            {
                if (!PlayerControl.LocalPlayer.isPlayerRole("Engineer"))
                {
                    KillButton.gameObject.SetActive(false);
                    KillButton.renderer.enabled = false;
                    KillButton.isActive = false;
                    KillButton.SetTarget(null);
                    KillButton.enabled = false;
                    return;
                }
            }

            if (__instance.UseButton != null && PlayerControl.LocalPlayer.isPlayerRole("Medic") &&
                __instance.UseButton.isActiveAndEnabled)
            {
                KillButton.renderer.sprite = Main.Assets.shieldIco;
                KillButton.gameObject.SetActive(true);
                KillButton.isActive = true;
                KillButton.SetCoolDown(0f, 1f);
                if (DistLocalClosest < GameOptionsData.KillDistances[PlayerControl.GameOptions.KillDistance] &&
                    !PlayerControl.LocalPlayer.getModdedControl().UsedAbility)
                {
                    KillButton.SetTarget(PlayerTools.closestPlayer);
                    CurrentTarget = PlayerTools.closestPlayer;
                }
                else
                {
                    KillButton.SetTarget(null);
                    CurrentTarget = null;
                }
            }

            if (__instance.UseButton != null && PlayerControl.LocalPlayer.isPlayerRole("Officer") &&
                __instance.UseButton.isActiveAndEnabled)
            {
                KillButton.gameObject.SetActive(true);
                KillButton.isActive = true;
                KillButton.SetCoolDown(PlayerTools.getOfficerCD(), PlayerControl.GameOptions.KillCooldown + 15.0f);
                if (DistLocalClosest < GameOptionsData.KillDistances[PlayerControl.GameOptions.KillDistance])
                {
                    KillButton.SetTarget(PlayerTools.closestPlayer);
                    CurrentTarget = PlayerTools.closestPlayer;
                }
                else
                {
                    KillButton.SetTarget(null);
                    CurrentTarget = null;
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