using Essentials.UI;
using ExtraRolesMod.Officer;
using ExtraRolesMod.Rpc;
using ExtraRolesMod;
using HarmonyLib;
using InnerNet;
using Reactor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UnityEngine;


namespace ExtraRolesMod.Roles.Officer
{
    public static class OfficerKillButton
    {
        static bool lastQ = false;
        public static CooldownButton Button { get; private set; }

        public static void AddOfficerKillButton()
        {
            if (Button == null)
            {
                Button = new CooldownButton(sprite: null, new Vector2(7.967f, 0f), ExtraRoles.Config.OfficerCD, 0f, 10f);
                Button.OnUpdate += OfficerKillButton_OnUpdate;
                Button.OnClick += OfficerKillButton_OnClick;
            }
            Button.Visible = false;
        }


        private static void OfficerKillButton_OnUpdate(object sender, EventArgs e)
        {
            Button.Visible = PlayerControl.LocalPlayer.IsPlayerRole(Role.Officer);
            Button.Clickable = !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.FindClosestPlayer();

            lastQ = Input.GetKeyUp(KeyCode.Q);

            if (Input.GetKeyDown(KeyCode.Q) && !lastQ && Button.IsUsable)
                Button.PerformClick();
        }

        public static void OfficerKillButton_OnClick(object sender, CancelEventArgs e)
        {
            var target = PlayerControl.LocalPlayer.FindClosestPlayer();

            SendOfficerKillRpc(target);

        }
        private static void SendOfficerKillRpc(PlayerControl target)
        {
            PlayerControl.LocalPlayer.GetModdedControl().LastAbilityTime = DateTime.UtcNow;
            var attacker = PlayerControl.LocalPlayer;
            Rpc<OfficerKillRpc>.Instance.Send(data: (Attacker: attacker, Target: target), immediately: true);
        }
    }
}
