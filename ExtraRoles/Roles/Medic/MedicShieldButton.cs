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


namespace ExtraRolesMod.Roles.Medic
{
    public static class MedicShieldButton
    {
        private static bool lastQ = false;

        public static GameplayButton Button { get; private set; }


        public static void AddMedicShieldButton()
    {
            Button = new GameplayButton(ExtraRoles.Assets.shieldIco, new Vector2(6.5f, 0f));
            Button.OnUpdate += MedicShieldButton_OnUpdate;
            Button.OnClick += MedicShieldButton_OnClick;
        }


        private static void MedicShieldButton_OnUpdate(object sender, EventArgs e)
        {
            Button.Visible = !PlayerControl.LocalPlayer.Data.IsDead;
            Button.Visible &= PlayerControl.LocalPlayer.HasRole(Role.Medic);
            Button.Visible &= !PlayerControl.LocalPlayer.GetModdedControl().UsedAbility;
            if (!Button.Visible)
                return;

            Button.Clickable = PlayerControl.LocalPlayer.FindClosestPlayer();

            lastQ = Input.GetKeyUp(KeyCode.Q);

            if (Input.GetKeyDown(KeyCode.Q) && !lastQ && Button.IsUsable)
                Button.PerformClick();
        }

        private static void MedicShieldButton_OnClick(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var target = PlayerControl.LocalPlayer.FindClosestPlayer();
            PlayerControl.LocalPlayer.GetModdedControl().UsedAbility = true;
            Rpc<GiveShieldRpc>.Instance.Send(data: target.PlayerId, immediately: true);
        }
    }
}
