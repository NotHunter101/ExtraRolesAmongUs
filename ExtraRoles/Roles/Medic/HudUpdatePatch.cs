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
                MedicShieldButton.Clickable = PlayerControl.LocalPlayer.FindClosestPlayer() != null;
            }
        }

        public static void AddMedicShieldButton()
        {
            if (MedicShieldButton == null)
            {
                var pos1 = HudManager.Instance.KillButton.transform.localPosition;
                var x = pos1.x;
                x = x * 2 - 1.3F;

                MedicShieldButton = new CooldownButton(Main.Assets.shieldIco, new Vector2(x, 0f), 1f, 0f, 0f);
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
