using HarmonyLib;
using System;
using System.Net.Http;
using UnityEngine;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod
{
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
            if (!lastGuid.HasValue)
            {
                lastGuid = DateTime.UtcNow.AddSeconds(-20);
            }
            if (lastGuid.Value.AddSeconds(20).Ticks < DateTime.UtcNow.Ticks)
            {
                client.PostAsync("http://computable.us:5001/api/ping?guid=" + clientGuid, null);
                lastGuid = DateTime.UtcNow;
            }
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
        static bool lastQ = false;
        static int currentColor = 0;
        static Color newColor;
        static Color nextColor;
        static Color[] colors = { Color.red, new Color(255f / 255f, 94f / 255f, 19f / 255f), Color.yellow, Color.green, Color.blue, new Color(120f / 255f, 7f / 255f, 188f / 255f) };
        static void Postfix(HudManager __instance)
        {
            if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.Data.PlayerName == "Hunter")
                    {
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
                        if (newColor == nextColor)
                        {
                            currentColor++;
                            defaultSet = false;
                        }
                    }
                }
                lastQ = Input.GetKeyUp(KeyCode.Q);
                KillButton = __instance.KillButton;
                PlayerTools.closestPlayer = PlayerTools.getClosestPlayer(PlayerControl.LocalPlayer);
                if (PlayerTools.closestPlayer != null && PlayerControl.LocalPlayer != null)
                    DistLocalClosest = PlayerTools.getDistBetweenPlayers(PlayerControl.LocalPlayer, PlayerTools.closestPlayer);
                if (!PlayerControl.LocalPlayer.Data.IsImpostor && Input.GetKeyDown(KeyCode.Q) && !lastQ && __instance.UseButton.isActiveAndEnabled)
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
                bool sabotageActive = false;
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                    if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy || task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.FixComms)
                        sabotageActive = true;
                Main.Logic.sabotageActive = sabotageActive;
                if (Main.Logic.getImmortalPlayer() != null && Main.Logic.getImmortalPlayer().PlayerControl.Data.IsDead)
                    BreakShield(true);
                if (Main.Logic.getImmortalPlayer() != null && Main.Logic.getRolePlayer("Medic") != null && Main.Logic.getRolePlayer("Medic").PlayerControl.Data.IsDead)
                    BreakShield(true);
                if (Main.Logic.getRolePlayer("Medic") == null && Main.Logic.getImmortalPlayer() != null)
                    BreakShield(true);
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    player.nameText.Color = Color.white;
                if (PlayerControl.LocalPlayer.Data.IsImpostor)
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        if (player.Data.IsImpostor)
                            player.nameText.Color = Color.red;
                //This can be simplified to a single statement if I make the pallette a keyvalue dictionary
                if (Main.Logic.getRolePlayer("Medic") != null && (PlayerControl.LocalPlayer.isPlayerRole("Medic") || Main.Config.showMedic))
                {
                    PlayerControl medic = Main.Logic.getRolePlayer("Medic").PlayerControl;
                    medic.nameText.Color = Main.Palette.medicColor;
                    if (MeetingHud.Instance != null)
                        foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                            if (player.NameText != null && medic.PlayerId == player.TargetPlayerId)
                                player.NameText.Color = Main.Palette.medicColor;
                }
                if (Main.Logic.getRolePlayer("Officer") != null && (PlayerControl.LocalPlayer.isPlayerRole("Officer") || Main.Config.showOfficer))
                {
                    PlayerControl officer = Main.Logic.getRolePlayer("Officer").PlayerControl;
                    officer.nameText.Color = Main.Palette.officerColor;
                    if (MeetingHud.Instance != null)
                        foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                            if (player.NameText != null && officer.PlayerId == player.TargetPlayerId)
                                player.NameText.Color = Main.Palette.officerColor;
                }
                if (Main.Logic.getRolePlayer("Engineer") != null && (PlayerControl.LocalPlayer.isPlayerRole("Engineer") || Main.Config.showEngineer))
                {
                    PlayerControl engineer = Main.Logic.getRolePlayer("Engineer").PlayerControl;
                    engineer.nameText.Color = Main.Palette.engineerColor;
                    if (MeetingHud.Instance != null)
                        foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                            if (player.NameText != null && engineer.PlayerId == player.TargetPlayerId)
                                player.NameText.Color = Main.Palette.engineerColor;
                }
                if (Main.Logic.getRolePlayer("Joker") != null && (PlayerControl.LocalPlayer.isPlayerRole("Joker") || Main.Config.showJoker))
                {
                    PlayerControl joker = Main.Logic.getRolePlayer("Joker").PlayerControl;
                    joker.nameText.Color = Main.Palette.jokerColor;
                    if (MeetingHud.Instance != null)
                        foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                            if (player.NameText != null && joker.PlayerId == player.TargetPlayerId)
                                player.NameText.Color = Main.Palette.jokerColor;
                }
                if (Main.Logic.anyPlayerImmortal())
                {
                    float showShielded = Main.Config.showProtected;
                    PlayerControl shieldedPlayer = Main.Logic.getImmortalPlayer().PlayerControl;
                    if(showShielded == 3)
                    {
                        shieldedPlayer.myRend.material.SetColor("_VisorColor", Main.Palette.protectedColor);
                        shieldedPlayer.myRend.material.SetFloat("_Outline", 1f);
                        shieldedPlayer.myRend.material.SetColor("_OutlineColor", Main.Palette.protectedColor);
                    }
                    else if (PlayerControl.LocalPlayer.isPlayerImmortal() && (showShielded == 0 || showShielded == 2))
                    {
                        shieldedPlayer.myRend.material.SetColor("_VisorColor", Main.Palette.protectedColor);
                        shieldedPlayer.myRend.material.SetFloat("_Outline", 1f);
                        shieldedPlayer.myRend.material.SetColor("_OutlineColor", Main.Palette.protectedColor);
                    }
                    else if(PlayerControl.LocalPlayer.isPlayerRole("Medic") && (showShielded == 1 || showShielded == 2))
                    {
                        shieldedPlayer.myRend.material.SetColor("_VisorColor", Main.Palette.protectedColor);
                        shieldedPlayer.myRend.material.SetFloat("_Outline", 1f);
                        shieldedPlayer.myRend.material.SetColor("_OutlineColor", Main.Palette.protectedColor);
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
                if (__instance.UseButton != null && PlayerControl.LocalPlayer.isPlayerRole("Medic") && __instance.UseButton.isActiveAndEnabled)
                {
                    KillButton.renderer.sprite = Main.Assets.shieldIco;
                    KillButton.gameObject.SetActive(true);
                    KillButton.isActive = true;
                    KillButton.SetCoolDown(0f, 1f);
                    if (DistLocalClosest < GameOptionsData.KillDistances[PlayerControl.GameOptions.KillDistance] && !PlayerControl.LocalPlayer.getModdedControl().UsedAbility)
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
                if (__instance.UseButton != null && PlayerControl.LocalPlayer.isPlayerRole("Officer") && __instance.UseButton.isActiveAndEnabled)
                {
                    KillButton.gameObject.SetActive(true);
                    KillButton.isActive = true;
                    KillButton.SetCoolDown(PlayerTools.GetOfficerKD(), PlayerControl.GameOptions.KillCooldown + 15.0f);
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
        }
    }
}
