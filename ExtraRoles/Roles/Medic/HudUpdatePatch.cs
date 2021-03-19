using Essentials.UI;
using ExtraRolesMod.Rpc;
using ExtraRolesMod;
using HarmonyLib;
using InnerNet;
using Reactor;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ExtraRolesMod.ExtraRoles;

namespace ExtraRolesMod.Roles.Medic
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class HudUpdatePatch
    {
        private static bool lastQ = false;

        public static CooldownButton MedicShieldButton { get; private set; }

        public static void Postfix()
        {
            if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
                return;

            if (!PlayerControl.LocalPlayer.isPlayerRole(Role.Medic))
                return;

            if (PlayerControl.LocalPlayer.Data.IsDead)
                return;

            if (MedicShieldButton != null)
            {
                MedicShieldButton.Clickable = 
                    !PlayerControl.LocalPlayer.getModdedControl().UsedAbility && PlayerControl.LocalPlayer.FindClosestPlayer() != null;
            }

            lastQ = Input.GetKeyUp(KeyCode.Q);

            if (Input.GetKeyDown(KeyCode.Q) && !lastQ && HudManager.Instance.UseButton.isActiveAndEnabled && MedicShieldButton.Clickable)
                MedicShieldButton.PerformClick();
        }

        public static void AddMedicShieldButton()
        {
            if (MedicShieldButton == null)
            {
                MedicShieldButton = new CooldownButton(Main.Assets.shieldIco, new Vector2(6.5f, 0f), 1f, 0f, 0f);
                MedicShieldButton.OnClick += MedicShieldButton_OnClick;
            }
            MedicShieldButton.Visible = false;
        }

        private static void MedicShieldButton_OnClick(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var target = PlayerControl.LocalPlayer.FindClosestPlayer();
            PlayerControl.LocalPlayer.getModdedControl().UsedAbility = true;
            Rpc<GiveShieldRpc>.Instance.Send(data: target.PlayerId, immediately: true);
        }
    }
}
