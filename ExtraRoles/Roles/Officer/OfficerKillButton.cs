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
            Button = new CooldownButton(sprite: null, new HudPosition(GameplayButton.OffsetX, 0, HudAlignment.BottomRight), ExtraRoles.Config.OfficerCD, 0f, 10f);
            Button.OnUpdate += OfficerKillButton_OnUpdate;
            Button.OnClick += OfficerKillButton_OnClick;
        }


        private static void OfficerKillButton_OnUpdate(object sender, EventArgs e)
        {
            Button.Visible = PlayerControl.LocalPlayer.HasRole(Role.Officer);
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
            var attacker = PlayerControl.LocalPlayer;
            Rpc<OfficerKillRpc>.Instance.Send(data: (Attacker: attacker, Target: target), immediately: true);
        }
    }
}
