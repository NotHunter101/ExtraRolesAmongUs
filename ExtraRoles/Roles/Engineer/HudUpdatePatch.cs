using Essentials.UI;
using ExtraRolesMod;
using HarmonyLib;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod.Roles.Engineer
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class HudUpdatePatch
    {
        public static CooldownButton EngineerButton { get; private set; }

        public static void Postfix()
        {
            if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Ended)
            {
                EngineerButton?.Dispose();
                EngineerButton = null;
                return;
            }

            if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
                return;

            if (!PlayerControl.LocalPlayer.isPlayerRole(Role.Engineer))
                return;

            if (PlayerControl.LocalPlayer.Data.IsDead)
                return;
            if (EngineerButton == null)
            {
                AddEngineerButton();
            }
        }

        private static void AddEngineerButton()
        {
            var pos1 = HudManager.Instance.KillButton.transform.localPosition;
            var x = pos1.x;
            x = x * 2 - 1.3F;

            EngineerButton = new CooldownButton(Main.Assets.repairIco, new Vector2(x, 0f), 0f, 0f, 0f);
            EngineerButton.OnClick += EngineerButton_OnClick;
        }

        private static void EngineerButton_OnClick(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DestroyableSingleton<HudManager>.Instance.ShowMap((Action<MapBehaviour>)delegate (MapBehaviour m)
            {
                m.ShowInfectedMap();
                m.ColorControl.baseColor = Main.Logic.sabotageActive ? Color.gray : Main.Palette.engineerColor;
            });
        }
    }
}
