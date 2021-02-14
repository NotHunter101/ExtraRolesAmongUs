using HarmonyLib;
using System;
using System.IO;
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
                DistLocalClosest = PlayerTools.getDistBetweenPlayers(PlayerControl.LocalPlayer, PlayerTools.closestPlayer);
                if (!PlayerControl.LocalPlayer.Data.IsImpostor && Input.GetKeyDown(KeyCode.Q) && !lastQ && __instance.UseButton.isActiveAndEnabled)
                {
                    PerformKillPatch.Prefix();
                }
                if (MedicSettings.Protected != null && MedicSettings.Protected.PlayerId == PlayerControl.LocalPlayer.PlayerId && __instance.UseButton.isActiveAndEnabled)
                {
                    if (rend == null)
                    {
                        rend = new GameObject("Shield Icon");
                        rend.AddComponent<SpriteRenderer>().sprite = smallShieldIco;
                    }
                    int scale;
                    if (Screen.width > Screen.height)
                        scale = Screen.width / 800;
                    else
                        scale = Screen.height / 600;
                    rend.transform.localPosition = Camera.main.ScreenToWorldPoint(new Vector3(0 + (25 * scale), 0 + (25 * scale), -50f));
                    rend.SetActive(true);
                }
                if (EngineerSettings.Engineer != null && EngineerSettings.Engineer.PlayerId == PlayerControl.LocalPlayer.PlayerId && __instance.UseButton.isActiveAndEnabled)
                {
                    KillButton.gameObject.SetActive(true);
                    KillButton.isActive = true;
                    KillButton.SetCoolDown(0f, 1f);
                    KillButton.renderer.sprite = repairIco;
                    KillButton.renderer.color = Palette.EnabledColor;
                    KillButton.renderer.material.SetFloat("_Desat", 0f);
                }
                if (JokerSettings.Joker != null)
                    JokerSettings.ClearTasks();
                if (rend != null)
                    rend.SetActive(false);
                bool sabotageActive = false;
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                    if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy || task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.FixComms)
                        sabotageActive = true;
                EngineerSettings.sabotageActive = sabotageActive;
                if (MedicSettings.Protected != null && MedicSettings.Protected.Data.IsDead)
                    BreakShield(true);
                if (MedicSettings.Protected != null && MedicSettings.Medic != null && MedicSettings.Medic.Data.IsDead)
                    BreakShield(true);
                if (MedicSettings.Medic == null && MedicSettings.Protected != null)
                    BreakShield(true);
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    player.nameText.Color = Color.white;
                if (PlayerControl.LocalPlayer.Data.IsImpostor)
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        if (player.Data.IsImpostor)
                            player.nameText.Color = Color.red;
                if (MedicSettings.Medic != null)
                {
                    if (MedicSettings.Medic == PlayerControl.LocalPlayer || MedicSettings.showMedic)
                    {
                        MedicSettings.Medic.nameText.Color = ModdedPalette.medicColor;
                        if (MeetingHud.Instance != null)
                            foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                                if (player.NameText != null && MedicSettings.Medic.PlayerId == player.TargetPlayerId)
                                    player.NameText.Color = ModdedPalette.medicColor;
                    }
                }
                if (OfficerSettings.Officer != null)
                {
                    if (OfficerSettings.Officer == PlayerControl.LocalPlayer || OfficerSettings.showOfficer)
                    {
                        OfficerSettings.Officer.nameText.Color = ModdedPalette.officerColor;
                        if (MeetingHud.Instance != null)
                            foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                                if (player.NameText != null && OfficerSettings.Officer.PlayerId == player.TargetPlayerId)
                                    player.NameText.Color = ModdedPalette.officerColor;
                    }
                }
                if (EngineerSettings.Engineer != null)
                {
                    if (EngineerSettings.Engineer == PlayerControl.LocalPlayer || EngineerSettings.showEngineer)
                    {
                        EngineerSettings.Engineer.nameText.Color = ModdedPalette.engineerColor;
                        if (MeetingHud.Instance != null)
                            foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                                if (player.NameText != null && EngineerSettings.Engineer.PlayerId == player.TargetPlayerId)
                                    player.NameText.Color = ModdedPalette.engineerColor;
                    }
                }
                if (JokerSettings.Joker != null)
                {
                    if (JokerSettings.Joker == PlayerControl.LocalPlayer || JokerSettings.showJoker)
                    {
                        JokerSettings.Joker.nameText.Color = ModdedPalette.jokerColor;
                        if (MeetingHud.Instance != null)
                            foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                                if (player.NameText != null && JokerSettings.Joker.PlayerId == player.TargetPlayerId)
                                    player.NameText.Color = ModdedPalette.jokerColor;
                    }
                }
                if (MedicSettings.Protected != null)
                {
                    int showShielded = MedicSettings.showProtected;
                    // If everyone can see shielded
                    if(showShielded == 3)
                    {
                        MedicSettings.Protected.myRend.material.SetColor("_VisorColor", ModdedPalette.protectedColor);
                        MedicSettings.Protected.myRend.material.SetFloat("_Outline", 1f);
                        MedicSettings.Protected.myRend.material.SetColor("_OutlineColor", ModdedPalette.protectedColor);
                    }
                    // If I am protected and should see the shield
                    else if (PlayerControl.LocalPlayer == MedicSettings.Protected && (showShielded == 0 || showShielded == 2))
                    {
                        MedicSettings.Protected.myRend.material.SetColor("_VisorColor", ModdedPalette.protectedColor);
                        MedicSettings.Protected.myRend.material.SetFloat("_Outline", 1f);
                        MedicSettings.Protected.myRend.material.SetColor("_OutlineColor", ModdedPalette.protectedColor);
                    }
                    // If I am Medic and should see the shield
                    else if(PlayerControl.LocalPlayer == MedicSettings.Medic && (showShielded == 1 || showShielded == 2))
                    {
                        MedicSettings.Protected.myRend.material.SetColor("_VisorColor", ModdedPalette.protectedColor);
                        MedicSettings.Protected.myRend.material.SetFloat("_Outline", 1f);
                        MedicSettings.Protected.myRend.material.SetColor("_OutlineColor", ModdedPalette.protectedColor);
                    }
                }
                        
                if (PlayerControl.LocalPlayer.Data.IsDead)
                {
                    if (EngineerSettings.Engineer == null || EngineerSettings.Engineer.PlayerId != PlayerControl.LocalPlayer.PlayerId)
                    {
                        KillButton.gameObject.SetActive(false);
                        KillButton.renderer.enabled = false;
                        KillButton.isActive = false;
                        KillButton.SetTarget(null);
                        KillButton.enabled = false;
                        return;
                    }
                }
                if (MedicSettings.Medic != null && __instance.UseButton != null && MedicSettings.Medic.PlayerId == PlayerControl.LocalPlayer.PlayerId && __instance.UseButton.isActiveAndEnabled)
                {
                    KillButton.renderer.sprite = shieldIco;
                    KillButton.gameObject.SetActive(true);
                    KillButton.isActive = true;
                    KillButton.SetCoolDown(0f, 1f);
                    if (DistLocalClosest < GameOptionsData.KillDistances[PlayerControl.GameOptions.KillDistance] && MedicSettings.shieldUsed == false)
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
                if (OfficerSettings.Officer != null && __instance.UseButton != null && OfficerSettings.Officer.PlayerId == PlayerControl.LocalPlayer.PlayerId && __instance.UseButton.isActiveAndEnabled)
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
