using HarmonyLib;
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

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudUpdateManager
    {
        static bool lastQ = false;
        static void Postfix(HudManager __instance)
        {
            if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
            {
                if (!PlayerControl.LocalPlayer.Data.IsImpostor && Input.GetKeyDown(KeyCode.Q) && !lastQ)
                {
                    PerformKillPatch.Prefix();
                }
                lastQ = Input.GetKeyUp(KeyCode.Q);
                KillButton = __instance.KillButton;
                PlayerTools.closestPlayer = PlayerTools.getClosestPlayer(PlayerControl.LocalPlayer);
                DistLocalClosest = PlayerTools.getDistBetweenPlayers(PlayerControl.LocalPlayer, PlayerTools.closestPlayer);
                if (MedicSettings.Protected != null && __instance.UseButton != null && MedicSettings.Protected.PlayerId == PlayerControl.LocalPlayer.PlayerId && __instance.UseButton.isActiveAndEnabled)
                {
                    if (rend == null)
                    {
                        rend = new GameObject("Shield Icon", new Il2CppSystem.Type[] { SpriteRenderer.Il2CppType });
                        rend.GetComponent<SpriteRenderer>().sprite = smallShieldIco;
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
                    KillButton.SetCoolDown(0f, 30f);
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
                        KillButton.isActive = false;
                        KillButton.SetTarget(null);
                        return;
                    }
                }
                if (MedicSettings.Medic != null && __instance.UseButton != null && MedicSettings.Medic.PlayerId == PlayerControl.LocalPlayer.PlayerId && __instance.UseButton.isActiveAndEnabled)
                {
                    KillButton.renderer.sprite = shieldIco;
                    KillButton.gameObject.SetActive(true);
                    KillButton.isActive = true;
                    KillButton.SetCoolDown(0f, PlayerControl.GameOptions.KillCooldown + 15.0f);
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
