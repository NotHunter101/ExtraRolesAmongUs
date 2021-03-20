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
using static ExtraRolesMod.ExtraRoles;

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
                Button = new CooldownButton(sprite: null, new Vector2(7.967f, 0f), Main.Config.OfficerCD, 0f, 10f);
                Button.OnUpdate += OfficerKillButton_OnUpdate;
                Button.OnClick += OfficerKillButton_OnClick;
            }
            Button.Visible = false;
        }


        private static void OfficerKillButton_OnUpdate(object sender, EventArgs e)
        {
            Button.Clickable = !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.FindClosestPlayer() != null;

            lastQ = Input.GetKeyUp(KeyCode.Q);

            if (Input.GetKeyDown(KeyCode.Q) && !lastQ && Button.IsUsable)
                Button.PerformClick();
        }

        public static void OfficerKillButton_OnClick(object sender, CancelEventArgs e)
        {
            var target = PlayerControl.LocalPlayer.FindClosestPlayer();
            var isTargetJoker = target.isPlayerRole(Role.Joker);
            var isTargetImpostor = target.Data.IsImpostor;
            var officerKillSetting = Main.Config.officerKillBehaviour;
            if (target.isPlayerImmortal())
            {
                if (Main.Config.officerShouldDieToShieldedPlayers)
                {
                    // suicide packet
                    SendOfficerKillRpc(PlayerControl.LocalPlayer);
                }
                BreakShield(false);
            }
            else if (officerKillSetting == OfficerKillBehaviour.OfficerSurvives // don't care who it is, kill them
                || isTargetImpostor // impostors always die
                || (officerKillSetting != OfficerKillBehaviour.Impostor && isTargetJoker)) // joker can die and target is joker
            {
                // kill target
                SendOfficerKillRpc(target);
            }
            else // officer dies
            {
                if (officerKillSetting == OfficerKillBehaviour.CrewDie)
                {
                    // kill target too
                    SendOfficerKillRpc(target);
                }
                // kill officer
                SendOfficerKillRpc(PlayerControl.LocalPlayer);
            }

        }
        private static void SendOfficerKillRpc(PlayerControl target)
        {
            PlayerControl.LocalPlayer.getModdedControl().LastAbilityTime = DateTime.UtcNow;
            var attacker = PlayerControl.LocalPlayer;
            Rpc<OfficerKillRpc>.Instance.Send(data: (Attacker: attacker, Target: target), immediately: true);
            Rpc<AttemptKillShieldedPlayerRpc>.Instance.Send(data: attacker.PlayerId, immediately: true);
        }
    }
}
